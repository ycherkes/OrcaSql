using System;
using System.Collections.Generic;
using System.Linq;
using OrcaSql.Core.MetaData;
using OrcaSql.Core.MetaData.DMVs;
using Object = OrcaSql.Core.MetaData.DMVs.Object;
using Type = OrcaSql.Core.MetaData.DMVs.Type;

namespace OrcaSql.OSMS
{
    internal class DmvInfo
    {
        private readonly Dictionary<string, (Func<IEnumerable<Row>> getData, Func<Row> getSchema)> _dictionary;

        public DmvInfo(IDatabaseContext dbContext)
        {
            _dictionary = new Dictionary<string, (Func<IEnumerable<Row>>, Func<Row>)>
            {
                { "sys.columns",                            (() => dbContext.Database.Dmvs.Columns,                         () => dbContext.Database.Dmvs.GetSchemaRow<Column>())},
                { "sys.database_principals",                (() => dbContext.Database.Dmvs.DatabasePrincipals,              () => dbContext.Database.Dmvs.GetSchemaRow<DatabasePrincipal>())},
                { "sys.default_constraints",                (() => dbContext.Database.Dmvs.SysDefaultConstraints,           () => dbContext.Database.Dmvs.GetSchemaRow<SysDefaultConstraint>())},
                { "sys.foreign_keys",                       (() => dbContext.Database.Dmvs.ForeignKeys,                     () => dbContext.Database.Dmvs.GetSchemaRow<ForeignKey>())},
                { "sys.indexes",                            (() => dbContext.Database.Dmvs.Indexes,                         () => dbContext.Database.Dmvs.GetSchemaRow<Index>())},
                { "sys.index_columns",                      (() => dbContext.Database.Dmvs.IndexColumns,                    () => dbContext.Database.Dmvs.GetSchemaRow<IndexColumn>())},
                { "sys.objects",                            (() => dbContext.Database.Dmvs.Objects,                         () => dbContext.Database.Dmvs.GetSchemaRow<Object>())},
                { "sys.objects$",                           (() => dbContext.Database.Dmvs.ObjectsDollar,                   () => dbContext.Database.Dmvs.GetSchemaRow<ObjectDollar>())},
                { "sys.partitions",                         (() => dbContext.Database.Dmvs.Partitions,                      () => dbContext.Database.Dmvs.GetSchemaRow<Partition>())},
                { "sys.procedures",                         (() => dbContext.Database.Dmvs.Procedures,                      () => dbContext.Database.Dmvs.GetSchemaRow<Procedure>())},
                { "sys.schemas",                            (() => dbContext.Database.Dmvs.Schemas,                         () => dbContext.Database.Dmvs.GetSchemaRow<SysSchema>())},
                { "sys.sql_modules",                        (() => dbContext.Database.Dmvs.SqlModules,                      () => dbContext.Database.Dmvs.GetSchemaRow<SqlModule>())},
                { "sys.system_internals_allocation_units",  (() => dbContext.Database.Dmvs.SystemInternalsAllocationUnits,  () => dbContext.Database.Dmvs.GetSchemaRow<SystemInternalsAllocationUnit>())},
                { "sys.system_internals_partitions",        (() => dbContext.Database.Dmvs.SystemInternalsPartitions,       () => dbContext.Database.Dmvs.GetSchemaRow<SystemInternalsPartition>())},
                { "sys.system_internals_partition_columns", (() => dbContext.Database.Dmvs.SystemInternalsPartitionColumns, () => dbContext.Database.Dmvs.GetSchemaRow<SystemInternalsPartitionColumn>())},
                { "sys.tables",                             (() => dbContext.Database.Dmvs.Tables,                          () => dbContext.Database.Dmvs.GetSchemaRow<Table>())},
                { "sys.table_types",                        (() => dbContext.Database.Dmvs.TableTypes,                      () => dbContext.Database.Dmvs.GetSchemaRow<TableType>())},
                { "sys.types",                              (() => dbContext.Database.Dmvs.Types,                           () => dbContext.Database.Dmvs.GetSchemaRow<Type>())},
                { "sys.views",                              (() => dbContext.Database.Dmvs.Views,                           () => dbContext.Database.Dmvs.GetSchemaRow<View>())},
            };
        }

        public IEnumerable<string> Names => _dictionary.Keys;

        public IEnumerable<Row> GetData(string table)
        {
            if (!_dictionary.TryGetValue(table, out var dataWithSchema))
                throw new ArgumentOutOfRangeException(table);

            return dataWithSchema.getData().ToList();
        }

        public Row GetSchema(string table)
        {
            if (!_dictionary.TryGetValue(table, out var dataWithSchema))
                throw new ArgumentOutOfRangeException(table);

            return dataWithSchema.getSchema();
        }
    }
}
