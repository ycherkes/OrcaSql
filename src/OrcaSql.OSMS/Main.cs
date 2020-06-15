using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OrcaSql.Core.Engine;
using OrcaSql.Core.MetaData;
using OrcaSql.Core.MetaData.DMVs;
using OrcaSql.OSMS.Formatting;
using OrcaSql.Scripting;
using OrcaSql.SQLConnectionDialog;
using SQLConnectionDialog;
using DataColumn = OrcaSql.Core.MetaData.DataColumn;

namespace OrcaSql.OSMS
{
    public partial class Main : Form, IDatabaseContext
	{
        public Database Database { get; private set; }
        private DataColumn[] _sqlColumns;
        private ColumnFormatter[] _sqlColumnFormatters;
        private bool _autoResize;
        private readonly BaseTableInfo _baseTableInfo;
        private readonly DmvInfo _dmvInfo;
        private readonly PluginInstaller _pluginInstaller;

        public Main()
		{
			InitializeComponent();
            txtCode.Highlighting ="SQLSSMS";
            showTableTopNRowsMenuItem.Text = $"Select Top {ConfigurationManager.AppSettings["SelectTopNRowsCount"]} Rows";

            _pluginInstaller = new PluginInstaller("Plugins", OpenDatabaseFromStreams, this);

            Disposed += Main_Disposed;

            _baseTableInfo = new BaseTableInfo(this);
            _dmvInfo = new DmvInfo(this);
        }

		public Main(string[] fileNames)
			: this()
		{
			if (fileNames?.Any() != true) 
				return;
			
			var badFileNames = fileNames.Where(fileName => !File.Exists(fileName)).ToArray();
			
			if (badFileNames.Any())
			{
				var msg = new StringBuilder("The following files specified on the command line do not exist:");
				msg.AppendLine();
				foreach (var fileName in badFileNames) { msg.AppendFormat("\t{0}{1}", fileName, Environment.NewLine); }
				msg.Append("OrcaSql.OSMS must edit");
				MessageBox.Show(msg.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
				Close();
				Environment.Exit(4);
			}
			
			try
			{
				Database = new Database(fileNames);
				RefreshTreeView();
			}
			catch (Exception ex)
			{
				LogException(ex);
			}
		}

        private void Main_Disposed(object sender, EventArgs e)
        {
            Database?.Dispose();
            _pluginInstaller.UninstallPlugins();
        }

		private static void LogException(Exception ex)
		{
			File.AppendAllText("ErrorLog.txt",
				DateTime.Now +
				Environment.NewLine +
				"----------" +
				Environment.NewLine +
				ex +
				Environment.NewLine +
				Environment.NewLine);

			var msg =
				"An exception has occurred:\r\n\r\n" +
				ex.Message +
				"\r\n\r\nTo help improve OrcaSql, I would appreciate if you would send the ErrorLog.txt file to me at ycherkes@outlook.com" +
				"\r\n\r\nThe error log does not contain any sensitive information, feel free to check it to be 100% certain. The ErrorLog.txt file is located in the same directory as the OrcaSql Studio application.";

			MessageBox.Show(msg, "Uh oh!", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private void OpenToolStripMenuItem1_Click(object sender, EventArgs e)
		{
            openDatabaseDialog.Filter = "SQL Server data files|*.mdf;*.ndf";
            openDatabaseDialog.Multiselect = true;
            var result = openDatabaseDialog.ShowDialog();

            if (result != DialogResult.OK) return;

            try
            {
                var files = openDatabaseDialog.FileNames;
                Database?.Dispose();
                Database = new Database(files);
                
                RefreshControls();
                RefreshTreeView();

            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void RefreshControls()
        {
            grid.DataSource = null;
            grid.Columns.Clear();
            txtCode.Text = string.Empty;
            gridStatusRows.Text = string.Empty;
        }

        private void RefreshTreeView()
		{
            var rootNode = new TreeNode(Database.Name) {ContextMenu = databaseMenu};


            // Add base tables
			AddBaseTablesNode(rootNode);

			// Add DMVs
			AddDmvNodes(rootNode);

			// Add tables
			AddTablesNode(rootNode);

			// Add programmability
			AddProgrammabilityNode(rootNode);

            // Refresh treeview
            //treeview.ShowNodeToolTips = true;
			treeview.Nodes.Clear();
            treeview.Nodes.Add(rootNode);
		}

		private void AddProgrammabilityNode(TreeNode rootNode)
		{
			var prgRootNode = rootNode.Nodes.Add("Programmability");

			AddStoredProceduresNode(prgRootNode);
            AddViewsNode(prgRootNode);
            AddFunctionsNode(prgRootNode);
            AddTypesNode(prgRootNode);
        }

        private void AddTypesNode(TreeNode prgRootNode)
        {
            if(Database.Dmvs.Types.All(x => !x.IsUserDefined)) return;
            var typesNode = prgRootNode.Nodes.Add("Types");
            AddDataTypes(typesNode);
            AddTableTypes(typesNode);
        }

        private void AddTableTypes(TreeNode typesNode)
        {
            var schemas = Database.Dmvs.Schemas.ToDictionary(x => x.schema_id);

            var types = Database.Dmvs.TableTypes.Where(x => x.IsTableType && x.IsUserDefined)
                .Select(x => new { t = x, Name = schemas[x.SchemaID].name + "." + x.Name })
                .OrderBy(t => t.Name).ToArray();

            if(types.Length == 0) return;

            var tableNode = typesNode.Nodes.Add("User-Defined Table Types");

            foreach (var type in types)
            {
                var typeNode = tableNode.Nodes.Add(type.Name);
                //typeNode.ContextMenu = tableTypeMenu;
                // Add columns
                var typeColumnsNode = typeNode.Nodes.Add("Columns");
                var columns = Database.Dmvs.Columns
                    .Where(c => c.ObjectID == type.t.TypeTableObjectId)
                    .OrderBy(c => c.Name)
                    .ToArray();

                foreach (var c in columns)
                {
                    var mainColumn = Database.Dmvs.Columns.Single(x => x.ColumnID == c.ColumnID && x.ObjectID == c.ObjectID);

                    var typeName = DatabaseMetaData.GetColumnTypeName(Database.Dmvs, mainColumn);

                    typeColumnsNode.Nodes.Add($"{c.Name} ({typeName})");
                }

                // Add indexes
                var tableIndexesNode = typeNode.Nodes.Add("Indexes");
                var indexes = Database.Dmvs.Indexes
                    .Where(i => i.ObjectID == type.t.TypeTableObjectId && i.IndexID > 0)
                    .OrderBy(i => i.Name);

                foreach (var i in indexes)
                {
                    var indexNode = tableIndexesNode.Nodes.Add(i.Name);

                    // Add index columns
                    var indexColumns = Database.Dmvs.IndexColumns
                        .Where(ic => ic.ObjectID == type.t.TypeTableObjectId && ic.IndexID == i.IndexID);

                    foreach (var ic in indexColumns)
                    {
                        var tableTypeColumn = columns.Single(c => c.ColumnID == ic.ColumnID);
                        var typeName = DatabaseMetaData.GetColumnTypeName(Database.Dmvs, tableTypeColumn);

                        indexNode.Nodes.Add($"{tableTypeColumn.Name} ({typeName})");
                    }
                }
            }
        }

        private void AddDataTypes(TreeNode typesNode)
        {
            var schemas = Database.Dmvs.Schemas.ToDictionary(x => x.schema_id);

            var types = Database.Dmvs.Types.Where(x => !x.IsTableType && x.IsUserDefined)
                .Select(x => new { Name = $"{schemas[x.SchemaID].name}.{x.Name}", Description = DatabaseMetaData.GetTypeName(Database.Dmvs, x, true) })
                .OrderBy(t => t.Name).ToArray();

            if(types.Length == 0) return;

            var tableNode = typesNode.Nodes.Add("User-Defined Data Types");

            foreach (var type in types)
            {
                /*var typeNode =*/ tableNode.Nodes.Add($"{type.Name} ({type.Description})");
                //typeNode.ContextMenu = dataTypeMenu;
            }
        }

        private void AddFunctionsNode(TreeNode prgRootNode)
        {
            if(Database.Dmvs.Functions.All(x => x.IsMSShipped)) return;

            var functionsNode = prgRootNode.Nodes.Add("Functions");
            AddTableValuedFunctions(functionsNode);
            AddScalarValuedFunctions(functionsNode);
        }

        private void AddScalarValuedFunctions(TreeNode functionsNode)
        {
            var schemas = Database.Dmvs.Schemas.ToDictionary(x => x.schema_id);

            var functions = Database.Dmvs.Functions.Where(x => !x.IsMSShipped && x.Type == "FN")
                .Select(x => new {t = x, Name = schemas[x.SchemaID].name + "." + x.Name})
                .OrderBy(t => t.Name).ToArray();

            if(functions.Length == 0) return;

            var scalarNode = functionsNode.Nodes.Add("Scalar-valued Functions");

            foreach (var function in functions)
            {
                var functionNode = scalarNode.Nodes.Add(function.Name);
                functionNode.ContextMenu = functionMenu;
            }
        }

        private void AddTableValuedFunctions(TreeNode functionsNode)
        {
            var schemas = Database.Dmvs.Schemas.ToDictionary(x => x.schema_id);

            var functions = Database.Dmvs.Functions.Where(x => !x.IsMSShipped && new[]{"TF", "IF"}.Contains(x.Type))
                .Select(x => new { t = x, Name = $"{schemas[x.SchemaID].name}.{x.Name}"})
                .OrderBy(t => t.Name).ToArray();

            if(functions.Length == 0) return;

            var tableNode = functionsNode.Nodes.Add("Table-valued Functions");

            foreach (var function in functions)
            {
                var functionNode = tableNode.Nodes.Add(function.Name);
                functionNode.ContextMenu = tableFunctionMenu;
            }
        }

        public void AddViewsNode(TreeNode prgRootNode)
		{
			var schemas = Database.Dmvs.Schemas.ToDictionary(x => x.schema_id);

            var views = Database.Dmvs.Views.Where(x => !x.IsMSShipped)
                .Select(x => new { t = x, Name = $"{schemas[x.SchemaID].name}.{x.Name}"})
                .OrderBy(t => t.Name).ToArray();

            if(views.Length == 0) return;

            var viewsNode = prgRootNode.Nodes.Add("Views");

            foreach (var view in views)
			{
				var viewNode = viewsNode.Nodes.Add(view.Name);
				viewNode.ContextMenu = viewMenu;
			}
		}

		private void AddStoredProceduresNode(TreeNode prgRootNode)
		{
			var schemas = Database.Dmvs.Schemas.ToDictionary(x => x.schema_id);

            var procedures = Database.Dmvs.Procedures.Where(x => !x.IsMSShipped)
                .Select(x => new { t = x, Name = $"{schemas[x.SchemaID].name}.{x.Name}"})
                .OrderBy(t => t.Name).ToArray();

            if(procedures.Length == 0) return;

            var proceduresNode = prgRootNode.Nodes.Add("Stored Procedures");

            foreach (var proc in procedures)
			{
				var procNode = proceduresNode.Nodes.Add(proc.Name);
				procNode.ContextMenu = procedureMenu;
			}
		}

		private void AddBaseTablesNode(TreeNode rootNode)
		{
			var baseTableNode = rootNode.Nodes.Add("Base Tables");

            AddNodes(_baseTableInfo.Names, baseTableNode, baseTableMenu);
        }

        private static void AddNodes(IEnumerable<string> nodeNames, TreeNode parentNode, ContextMenu contextMenu)
        {
            var nodeWithContextMenu = new TreeNode {ContextMenu = contextMenu};

            var nodes = nodeNames.Select(x => CloneNodeWithText(nodeWithContextMenu, x)).ToArray();

            parentNode.Nodes.AddRange(nodes);
        }

        private static TreeNode CloneNodeWithText(TreeNode nodeToClone, string text)
        {
            var clonedNode = (TreeNode)nodeToClone.Clone();
            clonedNode.Text = text;
            return clonedNode;
        }

        private void AddDmvNodes(TreeNode rootNode)
		{
			var dmvNode = rootNode.Nodes.Add("DMVs");

            AddNodes(_dmvInfo.Names, dmvNode, dmvMenu);
		}

		private void AddTablesNode(TreeNode rootNode)
		{
            var schemas = Database.Dmvs.Schemas.ToDictionary(x => x.schema_id);

            var tablesWithRowCount = (from table in Database.Dmvs.Tables.Where(x => !x.IsMSShipped)
                join partition in Database.Dmvs.Partitions on table.ObjectID equals partition.ObjectID
                where partition.IndexID < 2
                group partition by new { Name = $"{schemas[table.SchemaID].name}.{table.Name}", table.ObjectID}
                into g
                orderby g.Key.Name
                select new {g.Key.Name, g.Key.ObjectID, RowCount = g.Sum(x => x.Rows)}).ToArray();


            if(tablesWithRowCount.Length == 0) return;

            var tableRootNode = rootNode.Nodes.Add("Tables");

            foreach (var t in tablesWithRowCount)
			{
				var tableNode = tableRootNode.Nodes.Add(t.Name);
                //tableNode.ToolTipText = $"{t.RowCount} rows";
                tableNode.Tag = t.RowCount;

                tableNode.ContextMenu = tableMenu;

				// Add columns
				var tableColumnsNode = tableNode.Nodes.Add("Columns");
				var columns = Database.Dmvs.Columns
					.Where(c => c.ObjectID == t.ObjectID)
                    .ToArray();

				foreach (var c in columns)
				{
					var mainColumn = Database.Dmvs.Columns.Single(x => x.ColumnID == c.ColumnID && x.ObjectID == c.ObjectID);

                    var typeName = DatabaseMetaData.GetColumnTypeName(Database.Dmvs, mainColumn);

                    tableColumnsNode.Nodes.Add($"{c.Name} ({typeName})");
				}

				// Add indexes
				var tableIndexesNode = tableNode.Nodes.Add("Indexes");
				var indexes = Database.Dmvs.Indexes
					.Where(i => i.ObjectID == t.ObjectID && i.IndexID > 0)
					.OrderBy(i => i.Name);

				foreach (var i in indexes)
				{
					var indexNode = tableIndexesNode.Nodes.Add(i.Name);

					// Add index columns
					var indexColumns = Database.Dmvs.IndexColumns
						.Where(ic => ic.ObjectID == t.ObjectID && ic.IndexID == i.IndexID);

					foreach (var ic in indexColumns)
                    {
                        var tableColumn = columns.Single(c => c.ColumnID == ic.ColumnID);
                        var typeName = DatabaseMetaData.GetColumnTypeName(Database.Dmvs, tableColumn);

                        indexNode.Nodes.Add($"{tableColumn.Name} ({typeName})");
					}
				}
			}
		}

		private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void ShowTableTopNRowsMenuItem_Click(object sender, EventArgs e)
        {
            var tableNameParts = treeview.SelectedNode.Text.Split('.');
            var tableName = tableNameParts.Last();
            var schema = Database.Dmvs.Schemas.FirstOrDefault(x => x.name == tableNameParts[0]);

            LoadTable(tableName, schema?.schema_id, (long)treeview.SelectedNode.Tag);
		}

		private void LoadTable(string table, int? schemaId, long rowCount)
		{
			try
            {
                var takeRowCount = 1000;
                if(int.TryParse(ConfigurationManager.AppSettings["SelectTopNRowsCount"], out var configRowCount)) takeRowCount = configRowCount ;
                var scanner = new DataScanner(Database);
				var rows = scanner.ScanTable(table, schemaId, false).Take(takeRowCount);
                var schemaRow = scanner.GetEmptyDataRow(table, schemaId);
                ShowRows(rows, schemaRow, table, rowCount);
            }
			catch (Exception ex)
			{
				LogException(ex);
			}
		}

		private void ShowRows(IEnumerable<Row> rows, Row schemaRow, string tableName, long rowCount = -1)
		{
			grid.DataSource = null;
            _sqlColumns = null;
            _sqlColumnFormatters = null;
            
            if (schemaRow == null) return;

            var tbl = new DataTable();

            _sqlColumns = schemaRow.Columns.Where(x => x.UnderlyingType != ColumnType.Uniquifier && x.UnderlyingType != ColumnType.RID).ToArray();
            _sqlColumnFormatters = _sqlColumns.Select(x => ColumnFormatters.GetFormatter(tableName, x)).ToArray();

            if (bool.TryParse(ConfigurationManager.AppSettings["AutoResizeColumns"], out var autoResizeColumns)) _autoResize = autoResizeColumns;

            foreach (var col in _sqlColumns)
            {
                if (new[] { ColumnType.VarBinary, ColumnType.Binary }.Contains(col.UnderlyingType))
                {
                    tbl.Columns.Add(col.Name, typeof(byte[]));
                }
                else switch (col.UnderlyingType)
                {
                    case ColumnType.Int:
                        tbl.Columns.Add(col.Name, typeof(int));
                        break;
                    case ColumnType.SmallInt:
                        tbl.Columns.Add(col.Name, typeof(short));
                        break;
                    case ColumnType.TinyInt:
                        tbl.Columns.Add(col.Name, typeof(byte));
                        break;
                    case ColumnType.BigInt:
                        tbl.Columns.Add(col.Name, typeof(long));
                        break;
                    case ColumnType.Date:
                    case ColumnType.DateTime:
                    case ColumnType.DateTime2:
                    case ColumnType.SmallDatetime:
                        tbl.Columns.Add(col.Name, typeof(DateTime));
                        break;
                    case ColumnType.Time:
                    tbl.Columns.Add(col.Name, typeof(TimeSpan));
                        break;
                    case ColumnType.DateTimeOffset:
                        tbl.Columns.Add(col.Name, typeof(DateTimeOffset));
                        break;
                    case ColumnType.UniqueIdentifier:
                        tbl.Columns.Add(col.Name, typeof(Guid));
                        break;
                    default:
                        tbl.Columns.Add(col.Name);
                        break;
                }
            }

            foreach (var scannedRow in rows)
            {
                var row = tbl.NewRow();

                foreach (var col in scannedRow.Columns)
                    row[col.Name] = scannedRow[col] ?? DBNull.Value;

                tbl.Rows.Add(row);
            }

            grid.Columns.Clear();
            grid.AutoGenerateColumns = false;

            var listItemProps = ListBindingHelper.GetListItemProperties(tbl);
            var columnCollection = GetCollectionOfBoundDataGridViewColumns(listItemProps, _sqlColumns, tableName);
            grid.Columns.AddRange(columnCollection.ToArray());
            grid.DataSource = tbl;
            gridStatusRows.Text = $"{grid.Rows.Count} Rows {(rowCount > -1 && rowCount != grid.Rows.Count ? "of " + rowCount : string.Empty)}";
			txtCode.Visible = false;
			grid.Visible = true;
        }

        private void Grid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            var rowNumber = 1;
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;
                row.HeaderCell.Value = $"{rowNumber++}";
            }

            grid.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders);

            if (!_autoResize) return;

            foreach (DataGridViewColumn column in grid.Columns)
            {
                var sqlColumn = _sqlColumns.Single(x => string.Equals(x.Name, column.Name, StringComparison.OrdinalIgnoreCase));
                if(!sqlColumn.IsVariableLength || (sqlColumn.VariableFixedLength < 50 && sqlColumn.VariableFixedLength > 0))
                    grid.AutoResizeColumn(column.Index, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        public IReadOnlyCollection<DataGridViewColumn> GetCollectionOfBoundDataGridViewColumns(PropertyDescriptorCollection props, DataColumn[] sqlColumns, string tableName)
        {
            if (props == null)
                return null;

            var list = new List<DataGridViewColumn>();

            for (var index = 0; index < props.Count; ++index)
            {
                if (typeof(IList).IsAssignableFrom(props[index].PropertyType) && !TypeDescriptor
                        .GetConverter(typeof(Image)).CanConvertFrom(props[index].PropertyType)) continue;

                var sqlColumn = sqlColumns.Single(x => string.Equals(x.Name, props[index].Name, StringComparison.OrdinalIgnoreCase));

                var viewColumn = new DataGridViewTextBoxColumn
                {
                    DataPropertyName = props[index].Name,
                    Name = props[index].Name,
                    HeaderText = !string.IsNullOrEmpty(props[index].DisplayName)
                        ? props[index].DisplayName
                        : props[index].Name,
                    ValueType = props[index].PropertyType,
                    ReadOnly = props[index].IsReadOnly
                };

                var formatter = _sqlColumnFormatters[index];

                if (formatter != null && formatter.UseCellFormat)
                    formatter.ApplyFormat(sqlColumn.Precision, sqlColumn.Scale)(viewColumn.DefaultCellStyle);

                list.Add(viewColumn);
            }
            return list;
        }

        private void TreeView_MouseUp(object sender, MouseEventArgs e)
		{
			// Make sure right clicking a node also selects it
			if (e.Button == MouseButtons.Right)
				treeview.SelectedNode = treeview.GetNodeAt(e.X, e.Y);
		}

        private void ShowDmvRowsMenuItem_Click(object sender, EventArgs e)
		{
		    try
            {
                var schema = _dmvInfo.GetSchema(treeview.SelectedNode.Text);
                var rows = _dmvInfo.GetData(treeview.SelectedNode.Text);
                ShowRows(rows, schema, treeview.SelectedNode.Text);
		    }
		    catch (Exception ex)
		    {
		        LogException(ex);
		    }
		}

		private void BaseTableSelectAllRowsMenuItem_Click(object sender, EventArgs e)
		{
		    try
            {
                var schema = _baseTableInfo.GetSchema(treeview.SelectedNode.Text);
                var rows = _baseTableInfo.GetData(treeview.SelectedNode.Text);
                ShowRows(rows, schema, treeview.SelectedNode.Text);
		    }
		    catch (Exception ex)
		    {
		        LogException(ex);
		    }
		}

		private void ShowProcedureCodeMenuItem_Click(object sender, EventArgs e)
		{
            var procedureNameParts = treeview.SelectedNode.Text.Split('.');
            var procedureName = procedureNameParts.Last();
            var schema = Database.Dmvs.Schemas.FirstOrDefault(x => x.name == procedureNameParts[0]);
            ShowProcedureCode(procedureName, schema);
		}

		private void ShowProcedureCode(string procedureName, SysSchema schema)
		{
            
            // Get procedure ID
            var objId = Database.Dmvs.Procedures
				.Where(p => p.Name == procedureName && schema.schema_id == p.SchemaID)
				.Select(p => p.ObjectID)
				.Single();

			// Get definition from sql_modules
			var definition = Database.Dmvs.SqlModules
				.Where(m => m.ObjectID == objId)
				.Select(m => m.Definition)
				.Single();

			// Set code
			txtCode.Text = definition;
            txtCode.Refresh();
            grid.Visible = false;
			txtCode.Visible = true;
            gridStatusRows.Text = string.Empty;
        }

		private void ShowViewCodeMenuItem_Click(object sender, EventArgs e)
		{
            var viewNameParts = treeview.SelectedNode.Text.Split('.');
            var viewName = viewNameParts.Last();
            var schema = Database.Dmvs.Schemas.FirstOrDefault(x => x.name == viewNameParts[0]);
            ShowViewCode(viewName, schema);
		}

		private void ShowViewCode(string viewName, SysSchema schema)
		{
			// Get view ID
			var objId = Database.Dmvs.Views
				.Where(p => p.Name == viewName && schema.schema_id == p.SchemaID)
				.Select(p => p.ObjectID)
				.Single();

			// Get definition from sql_modules
			var definition = Database.Dmvs.SqlModules
				.Where(m => m.ObjectID == objId)
				.Select(m => m.Definition)
				.Single();

			// Set code
			txtCode.Text = definition;
            txtCode.Refresh();
			grid.Visible = false;
			txtCode.Visible = true;
            gridStatusRows.Text = string.Empty;
        }

        private static Color NullBackgroundColor { get; } = ColorTranslator.FromHtml("#FFFFE1");

        private async void ExportDataMenuItem_Click(object sender, EventArgs e)
        {
            var tableNameParts = treeview.SelectedNode.Text.Split('.');
            var tableName = tableNameParts.Last();
            var newTableName = tableName + DateTime.Now.ToString("_yyyyMMddHHmmss");

            var dlg = new SqlConnectionDialog
            {
                SaveHelper = {SaveMethod = connectionString => ConnectionString = connectionString},
                Title = "Specify connection and target table name",
                ConnectionString = ConnectionString,
                TableName = newTableName
            };
            var dialogResult = dlg.Show();
            if(dialogResult != DialogResult.OK) return;

            var bulkCopyBatchSize = 1000_000;
            if (int.TryParse(ConfigurationManager.AppSettings["BulKCopyBatchSize"], out var configBulkCopyBatchSize)) bulkCopyBatchSize = configBulkCopyBatchSize;
            var bulkCopyNotifyAfter = 1000;
            if (int.TryParse(ConfigurationManager.AppSettings["BulKCopyNotifyAfter"], out var configBulkCopyNotifyAfter)) bulkCopyNotifyAfter = configBulkCopyNotifyAfter;

            var schema = Database.Dmvs.Schemas.FirstOrDefault(x => x.name == tableNameParts[0]);

            var scanner = new DataScanner(Database);
            var schemaRow = scanner.GetEmptyDataRow(tableName, schema?.schema_id);
            var rows = scanner.ScanTable(tableName, schema?.schema_id, false);

            var createTableSql = new CreateTableScriptWriter(Database).GetCreateExportTableScript(dlg.TableName, schemaRow);

            using (var connection = new SqlConnection(dlg.ConnectionString))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = createTableSql;
                    await command.ExecuteNonQueryAsync();

                    var dataReader = new TableReader(rows, schemaRow);

                    using (var bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.BatchSize = bulkCopyBatchSize;
                        bulkCopy.DestinationTableName = dlg.TableName;
                        bulkCopy.BulkCopyTimeout = (int) TimeSpan.FromHours(1).TotalSeconds;
                        bulkCopy.SqlRowsCopied += BulkCopyOnSqlRowsCopied;
                        bulkCopy.NotifyAfter = bulkCopyNotifyAfter;
                        // Just to exclude Computed columns from the mappings
                        foreach (var columnIdx in schemaRow.Columns
                            .Select((c, i) => new
                            {
                                c.Type,
                                Index = i
                            }).Where(x => x.Type != ColumnType.Computed)
                            .Select(x => x.Index))
                        {
                            bulkCopy.ColumnMappings.Add(columnIdx, columnIdx);
                        }

                        await bulkCopy.WriteToServerAsync(dataReader);
                        bulkCopy.SqlRowsCopied -= BulkCopyOnSqlRowsCopied;
                    }
                }

            }

            gridStatusRows.Text = $"Export to table {dlg.TableName} finished.";
        }

        private void BulkCopyOnSqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            gridStatusRows.Text = $"Exported {e.RowsCopied} rows";
        }

        public string ConnectionString
        {
            get
            {
                var cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                return cfg.ConnectionStrings.ConnectionStrings["ExportDataConnectionString"].ConnectionString;
            }
            set
            {
                var cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (cfg.ConnectionStrings.ConnectionStrings["ExportDataConnectionString"].ConnectionString == value) return;

                cfg.ConnectionStrings.ConnectionStrings["ExportDataConnectionString"].ConnectionString = value;
                cfg.Save(ConfigurationSaveMode.Minimal, true);
                ConfigurationManager.RefreshSection("connectionStrings");
            }
        }

        private void ShowFunctionCodeMenuItem_Click(object sender, EventArgs e)
        {
            var functionNameParts = treeview.SelectedNode.Text.Split('.');
            var functionName = functionNameParts.Last();
            var schema = Database.Dmvs.Schemas.FirstOrDefault(x => x.name == functionNameParts[0]);
            ShowScalarFunctionCode(functionName, schema);
        }

        private void ShowScalarFunctionCode(string functionName, SysSchema schema)
        {

            // Get procedure ID
            var objId = Database.Dmvs.Functions
                .Where(p => p.Name == functionName && schema.schema_id == p.SchemaID && p.Type == "FN")
                .Select(p => p.ObjectID)
                .Single();

            // Get definition from sql_modules
            var definition = Database.Dmvs.SqlModules
                .Where(m => m.ObjectID == objId)
                .Select(m => m.Definition)
                .Single();

            // Set code
            txtCode.Text = definition;
            txtCode.Refresh();
            grid.Visible = false;
            txtCode.Visible = true;
            gridStatusRows.Text = string.Empty;
        }

        private void ShowTableFunctionCodeMenuItem_Click(object sender, EventArgs e)
        {
            var functionNameParts = treeview.SelectedNode.Text.Split('.');
            var functionName = functionNameParts.Last();
            var schema = Database.Dmvs.Schemas.FirstOrDefault(x => x.name == functionNameParts[0]);
            ShowTableFunctionCode(functionName, schema);
        }

        private void ShowTableFunctionCode(string functionName, SysSchema schema)
        {

            // Get procedure ID
            var objId = Database.Dmvs.Functions
                .Where(p => p.Name == functionName && schema.schema_id == p.SchemaID && new[] { "TF", "IF" }.Contains(p.Type))
                .Select(p => p.ObjectID)
                .Single();

            // Get definition from sql_modules
            var definition = Database.Dmvs.SqlModules
                .Where(m => m.ObjectID == objId)
                .Select(m => m.Definition)
                .Single();

            // Set code
            txtCode.Text = definition;
            txtCode.Refresh();
            grid.Visible = false;
            txtCode.Visible = true;
            gridStatusRows.Text = string.Empty;
        }

        private void ShowDataTypeCodeMenuItem_Click(object sender, EventArgs e)
        {
            var dataTypeNameParts = treeview.SelectedNode.Text.Split('.');
            var dataTypeName = dataTypeNameParts.Last();
            var schema = Database.Dmvs.Schemas.FirstOrDefault(x => x.name == dataTypeNameParts[0]);
            ShowDataTypeCode(dataTypeName, schema);
        }

        private void ShowDataTypeCode(string dataTypeName, SysSchema schema)
        {
            // Get procedure ID
            //var objId = _db.Dmvs.Types
            //    .Where(p => p.Name == dataTypeName && schema.schema_id == p.SchemaID && !p.IsTableType && p.IsUserDefined)
            //    .Select(p => p.ObjectID)
            //    .Single();

            //// Get definition from sql_modules
            //var definition = _db.Dmvs.SqlModules
            //    .Where(m => m.ObjectID == objId)
            //    .Select(m => m.Definition)
            //    .Single();

            //// Set code
            //txtCode.Text = definition;
            //txtCode.Refresh();
            //grid.Visible = false;
            //txtCode.Visible = true;
        }

        private void ShowTableTypeCodeMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == DBNull.Value)
            {
                e.CellStyle.BackColor = NullBackgroundColor;
                e.CellStyle.ForeColor = DefaultForeColor;
                e.Value = "NULL";
                e.FormattingApplied = true;
                return;
            }

            var formatter = _sqlColumnFormatters[e.ColumnIndex];

            if (formatter == null || !formatter.UseStringValue) return;

            e.Value = formatter.GetStringValue(e.Value);
            e.FormattingApplied = true;
        }

        private void GenerateRestorePageScriptMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new SqlConnectionDialog
            {
                SaveHelper = { SaveMethod = connectionString => ConnectionString = connectionString },
                Title = "Specify connection and page pointer",
                ConnectionString = ConnectionString,
                Mode =  Mode.PageId
            };

            var dialogResult = dlg.Show();
            if (dialogResult != DialogResult.OK) return;

            var restorePageScriptWriter = new RestorePageScriptWriter(Database);

           var resultScript = restorePageScriptWriter.CreateRestoreScriptWithRollback(dlg.ConnectionString, dlg.PagePointer);

            // Set code
            txtCode.Text = resultScript;
            txtCode.Refresh();
            grid.Visible = false;
            txtCode.Visible = true;
            gridStatusRows.Text = string.Empty;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            _pluginInstaller.ImportPlugins();
            _pluginInstaller.InstallPlugins(menuStrip1);
        }

        private void OpenDatabaseFromStreams(Func<IEnumerable<Stream>> dataFileStreamsGetter)
        {
            try
            {
                Database?.Dispose();

                Database = new Database(dataFileStreamsGetter());

                RefreshControls();
                RefreshTreeView();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
    }
}