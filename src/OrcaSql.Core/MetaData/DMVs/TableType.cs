using System;
using System.Collections.Generic;
using System.Linq;
using OrcaSql.Core.Engine;

namespace OrcaSql.Core.MetaData.DMVs
{
    public class TableType : Row
    {
        private const string CACHE_KEY = "DMV__Table_Type";

        private static readonly ISchema schema = new Schema(new[]
            {
                new DataColumn("Name", "sysname"),
                new DataColumn("SystemTypeID", "tinyint"),
                new DataColumn("UserTypeID", "int"),
                new DataColumn("SchemaID", "int"),
                new DataColumn("PrincipalID", "int", true),
                new DataColumn("MaxLength", "smallint"),
                new DataColumn("Precision", "tinyint"),
                new DataColumn("Scale", "tinyint"),
                new DataColumn("CollationName", "sysname", true),
                new DataColumn("IsNullable", "bit", true),
                new DataColumn("IsUserDefined", "bit"),
                new DataColumn("IsAssemblyType", "bit"),
                new DataColumn("DefaultObjectID", "int"),
                new DataColumn("RuleObjectID", "int"),
                new DataColumn("IsTableType", "bit"),
                new DataColumn("TypeTableObjectId", "int"),
                new DataColumn("IsMemoryOptimized", "bit")
            });

        public string Name { get { return Field<string>("Name"); } private set { this["Name"] = value; } }
        public byte SystemTypeID { get { return Field<byte>("SystemTypeID"); } private set { this["SystemTypeID"] = value; } }
        public int UserTypeID { get { return Field<int>("UserTypeID"); } private set { this["UserTypeID"] = value; } }
        public int SchemaID { get { return Field<int>("SchemaID"); } private set { this["SchemaID"] = value; } }
        public int? PrincipalID { get { return Field<int?>("PrincipalID"); } private set { this["PrincipalID"] = value; } }
        public short MaxLength { get { return Field<short>("MaxLength"); } private set { this["MaxLength"] = value; } }
        public byte Precision { get { return Field<byte>("Precision"); } private set { this["Precision"] = value; } }
        public byte Scale { get { return Field<byte>("Scale"); } private set { this["Scale"] = value; } }
        public string CollationName { get { return Field<string>("CollationName"); } private set { this["CollationName"] = value; } } // TODO
        public bool IsNullable { get { return Field<bool>("IsNullable"); } private set { this["IsNullable"] = value; } }
        public bool IsUserDefined { get { return Field<bool>("IsUserDefined"); } private set { this["IsUserDefined"] = value; } }
        public bool IsAssemblyType { get { return Field<bool>("IsAssemblyType"); } private set { this["IsAssemblyType"] = value; } }
        public int DefaultObjectID { get { return Field<int>("DefaultObjectID"); } private set { this["DefaultObjectID"] = value; } }
        public int RuleObjectID { get { return Field<int>("RuleObjectID"); } private set { this["RuleObjectID"] = value; } }
        public bool IsTableType { get { return Field<bool>("IsTableType"); } private set { this["IsTableType"] = value; } }
        public int TypeTableObjectId { get { return Field<int>("TypeTableObjectId"); } private set { this["TypeTableObjectId"] = value; } }
        public bool IsMemoryOptimized { get { return Field<bool>("IsMemoryOptimized"); } private set { this["IsMemoryOptimized"] = value; } }

        public TableType() : base(schema)
        { }

        public override Row NewRow()
        {
            return new TableType();
        }

        internal static IEnumerable<TableType> GetDmvData(Database db)
        {
            if (!db.ObjectCache.ContainsKey(CACHE_KEY))
            {
                db.ObjectCache[CACHE_KEY] =
                    (from t in
                            db.BaseTables
                                .SysScalarTypes //o.depid = t.id AND o.class = 36 AND o.depsubid = 0 -- SRC_TYPETOTABLE  
                     join o in db.BaseTables.SysSingleObjRefs on
                         new { t.id, @class = 36, depsubid = 0 } equals new { id = o.depid, o.@class, o.depsubid }
                     join obj in db.BaseTables.SysSchObjs on o.indepid equals obj.id
                     select new TableType
                     {
                         Name = t.name,
                         SystemTypeID = t.xtype,
                         UserTypeID = t.id,
                         SchemaID = t.schid,
                         MaxLength = t.length,
                         Precision = t.prec,
                         Scale = t.scale,
                         DefaultObjectID = t.dflt,
                         RuleObjectID = t.chk,
                         IsNullable = Convert.ToBoolean(1 - (t.status & 1)),
                         IsUserDefined = t.id > 256,
                         IsAssemblyType = t.xtype == 240,
                         IsTableType = t.xtype == 243,
                         TypeTableObjectId = o.indepid,
                         IsMemoryOptimized = Convert.ToBoolean(obj.status2 & 0x00000008),
                         PrincipalID = db.BaseTables.SysSingleObjRefs
                             .Where(o1 => o1.depid == t.id && o1.@class == 44 && o1.depsubid == 0)
                             .Select(o1 => (int?)o1.indepid)
                             .SingleOrDefault()
                     }).ToList();
            }

            return (IEnumerable<TableType>)db.ObjectCache[CACHE_KEY];
        }
    }
}