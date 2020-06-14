using System;
using System.Linq;
using System.Text;
using OrcaSql.Core.Engine;
using OrcaSql.Core.MetaData;

namespace OrcaSql.Scripting
{
    public class CreateTableScriptWriter
    {
        private readonly Database _database;

        public CreateTableScriptWriter(Database database)
        {
            _database = database;
        }

        public string GetCreateExportTableScript(string tableName, DataRow schemaRow)
        {
            var createTableTemplate = $"CREATE TABLE [{tableName}](\r\n{{0}})";

            var columnsSql = schemaRow.Columns.Select(x => $"[{x.Name}] {GetTypeName(x, schemaRow.ObjectId)} {GetSizeString(x)} {(x.UnderlyingType == ColumnType.Computed ? string.Empty : "null")}");

            var createTableSql = string.Format(createTableTemplate, string.Join(",\r\n", columnsSql));

            return createTableSql;
        }

        private string GetTypeName(DataColumn column, int? tableObjectId)
        {
            switch (column.Type)
            {
                case ColumnType.Geography:
                case ColumnType.Geometry:
                case ColumnType.Xml:
                case ColumnType.HierarchyId:
                case ColumnType.Numeric:
                case ColumnType.Real:
                    return column.Type.ToString().ToLowerInvariant();
                case ColumnType.Computed:
                    {
                        var objVal = _database.BaseTables.SysObjValues.Single(x =>
                            x.objid == tableObjectId
                            && x.subobjid == column.ColumnID
                            && x.valclass == 2);

                        var definition = Encoding.ASCII.GetString(objVal.imageval);
                        return $"AS {definition}";
                    }
                default:
                    return column.UnderlyingType.ToString().ToLowerInvariant();
            }

        }

        private static string GetSizeString(DataColumn dataColumn)
        {
            switch (dataColumn.UnderlyingType)
            {
                case ColumnType.BigInt:
                case ColumnType.Bit:
                case ColumnType.Date:
                case ColumnType.DateTime:
                case ColumnType.Image:
                case ColumnType.Int:
                case ColumnType.Money:
                case ColumnType.NText:
                case ColumnType.RID:
                case ColumnType.SmallDatetime:
                case ColumnType.SmallInt:
                case ColumnType.SmallMoney:
                case ColumnType.Text:
                case ColumnType.TinyInt:
                case ColumnType.UniqueIdentifier:
                case ColumnType.Uniquifier:
                case ColumnType.Variant:
                case ColumnType.HierarchyId:
                case ColumnType.Xml:
                case ColumnType.Timestamp:
                case ColumnType.Geography:
                case ColumnType.Geometry:
                case ColumnType.Computed:
                    return string.Empty;
                case ColumnType.Float:
                    switch (dataColumn.Type)
                    {
                        case ColumnType.Real:
                            return string.Empty;
                        default:
                            return $"({dataColumn.Precision})";
                    }
                case ColumnType.Decimal:
                    return $"({dataColumn.Precision}, {dataColumn.Scale})";
                case ColumnType.Binary:
                case ColumnType.Char:
                case ColumnType.Varchar:
                    return GetVariableLengthString(dataColumn.VariableFixedLength);
                case ColumnType.NChar:
                case ColumnType.NVarchar:
                    return GetNVariableLengthString(dataColumn.VariableFixedLength);
                case ColumnType.VarBinary:
                    switch (dataColumn.Type)
                    {
                        case ColumnType.Geography:
                        case ColumnType.Geometry:
                        case ColumnType.Xml:
                        case ColumnType.HierarchyId:
                            return string.Empty;
                        default:
                            return GetVariableLengthString(dataColumn.VariableFixedLength);
                    }

                case ColumnType.Time:
                case ColumnType.DateTime2:
                case ColumnType.DateTimeOffset:
                    return $"({dataColumn.Scale})";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string GetVariableLengthString(short? variableFixedLength)
        {
            switch (variableFixedLength)
            {
                case null:
                    throw new NotSupportedException();
                case -1:
                    return "(max)";
                case 1:
                    return string.Empty;
                default:
                    return $"({variableFixedLength})";
            }
        }

        private static string GetNVariableLengthString(short? variableFixedLength)
        {
            switch (variableFixedLength)
            {
                case null:
                    throw new NotSupportedException();
                case -1:
                    return "(max)";
                case 1:
                    return string.Empty;
                default:
                    return $"({variableFixedLength/2})";
            }
        }
    }
}
