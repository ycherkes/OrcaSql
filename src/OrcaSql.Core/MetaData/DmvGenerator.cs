using System.Collections.Generic;
using OrcaSql.Core.Engine;
using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.MetaData
{
	public class DmvGenerator
	{
		private readonly Database db;

		internal DmvGenerator(Database db)
		{
			this.db = db;
		}

		public IEnumerable<Column> Columns => Column.GetDmvData(db);

        public IEnumerable<ForeignKey> ForeignKeys => ForeignKey.GetDmvData(db);

        public IEnumerable<Index> Indexes => Index.GetDmvData(db);

        public IEnumerable<IndexColumn> IndexColumns => IndexColumn.GetDmvData(db);

        public IEnumerable<Object> Objects => Object.GetDmvData(db);

        public IEnumerable<ObjectDollar> ObjectsDollar => ObjectDollar.GetDmvData(db);

        public IEnumerable<Partition> Partitions => Partition.GetDmvData(db);

        public IEnumerable<Type> Types => Type.GetDmvData(db);

        public IEnumerable<TableType> TableTypes => TableType.GetDmvData(db);

        public IEnumerable<Procedure> Procedures => Procedure.GetDmvData(db);

        public IEnumerable<SysSchema> Schemas => SysSchema.GetDmvData(db);

        public IEnumerable<SysDefaultConstraint> SysDefaultConstraints => SysDefaultConstraint.GetDmvData(db);

        public IEnumerable<View> Views => View.GetDmvData(db);

        public IEnumerable<Function> Functions => Function.GetDmvData(db);

        public IEnumerable<SqlModule> SqlModules => SqlModule.GetDmvData(db);

        public IEnumerable<DatabasePrincipal> DatabasePrincipals => DatabasePrincipal.GetDmvData(db);

        public IEnumerable<SystemInternalsAllocationUnit> SystemInternalsAllocationUnits => SystemInternalsAllocationUnit.GetDmvData(db);

        public IEnumerable<SystemInternalsPartition> SystemInternalsPartitions => SystemInternalsPartition.GetDmvData(db);

        public IEnumerable<SystemInternalsPartitionColumn> SystemInternalsPartitionColumns => SystemInternalsPartitionColumn.GetDmvData(db);

        public IEnumerable<Table> Tables => Table.GetDmvData(db);

        public Row GetSchemaRow<T>() where T: Row, new()
        {
            return new T();
        }
    }
}