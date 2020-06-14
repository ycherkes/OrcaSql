using System;
using System.Collections.Generic;
using System.Linq;
using OrcaSql.Core.MetaData;
using OrcaSql.Core.MetaData.BaseTables;

namespace OrcaSql.OSMS
{
    internal class BaseTableInfo
    {
        private readonly Dictionary<string, (Func<IEnumerable<Row>> getData, Func<Row> getSchema)> _dictionary;

        public BaseTableInfo(IDatabaseContext dbContext)
        {
            _dictionary = new Dictionary<string, (Func<IEnumerable<Row>>, Func<Row>)>
            {
                { "sys.sysallocunits",    (() => dbContext.Database.BaseTables.SysAllocUnits,    () => dbContext.Database.BaseTables.GetSchemaRow<sysallocunit>())},
                { "sys.syscolpars",       (() => dbContext.Database.BaseTables.SysColPars,       () => dbContext.Database.BaseTables.GetSchemaRow<syscolpar>())},
                { "sys.sysidxstats",      (() => dbContext.Database.BaseTables.SysIdxStats,      () => dbContext.Database.BaseTables.GetSchemaRow<sysidxstat>())},
                { "sys.sysclsobjs",       (() => dbContext.Database.BaseTables.SysClsObjs,       () => dbContext.Database.BaseTables.GetSchemaRow<sysclsobj>())},
                { "sys.sysiscols",        (() => dbContext.Database.BaseTables.SysIsCols,        () => dbContext.Database.BaseTables.GetSchemaRow<sysiscol>())},
                { "sys.sysobjvalues",     (() => dbContext.Database.BaseTables.SysObjValues,     () => dbContext.Database.BaseTables.GetSchemaRow<sysobjvalue>())},
                { "sys.sysowners",        (() => dbContext.Database.BaseTables.SysOwners,        () => dbContext.Database.BaseTables.GetSchemaRow<sysowner>())},
                { "sys.sysrowsets",       (() => dbContext.Database.BaseTables.SysRowsets,       () => dbContext.Database.BaseTables.GetSchemaRow<sysrowset>())},
                { "sys.sysrscols",        (() => dbContext.Database.BaseTables.SysRsCols,        () => dbContext.Database.BaseTables.GetSchemaRow<sysrscol>())},
                { "sys.sysscalartypes",   (() => dbContext.Database.BaseTables.SysScalarTypes,   () => dbContext.Database.BaseTables.GetSchemaRow<sysscalartype>())},
                { "sys.sysschobjs",       (() => dbContext.Database.BaseTables.SysSchObjs,       () => dbContext.Database.BaseTables.GetSchemaRow<sysschobj>())},
                { "sys.syssingleobjrefs", (() => dbContext.Database.BaseTables.SysSingleObjRefs, () => dbContext.Database.BaseTables.GetSchemaRow<syssingleobjref>())},
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
