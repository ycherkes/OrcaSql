using System.Collections.Generic;
using System.Linq;
using OrcaSql.Core.Engine;

namespace OrcaSql.Core.MetaData.DMVs
{
	public class SysSchema : Row
	{
		private const string CACHE_KEY = "DMV_SysSchema";

        private static readonly ISchema schema = new Schema(new[]
        {
            new DataColumn("name", "sysname"),
            new DataColumn("schema_id", "int"),
            new DataColumn("principal_id", "int")
        });

        public string name
        {
            get => Field<string>("name");
            private set => this["name"] = value;
        }
		public int? principal_id
        {
            get => Field<int?>("principal_id");
            private set => this["principal_id"] = value;
        }
		public int schema_id
        {
            get => Field<int>("schema_id");
            private set => this["schema_id"] = value;
        }

		public SysSchema()
			: base(schema)
		{ }

		public override Row NewRow()
		{
			return new SysSchema();
		}

		internal static IEnumerable<SysSchema> GetDmvData(Database db)
		{
			if (!db.ObjectCache.ContainsKey(CACHE_KEY))
			{
                db.ObjectCache[CACHE_KEY] = (from cls in db.BaseTables.SysClsObjs
                                            join ors in db.BaseTables.SysSingleObjRefs 
                                            on new{ cls.id, @class = (int)cls.@class} equals new {id = ors.depid, ors.@class}
                                            where  cls.@class == 50
                                            select new SysSchema
                                            {
                                                name = cls.name,
                                                schema_id = cls.id,
                                                principal_id = ors.indepid
                                            }).ToList();
            }

			return (IEnumerable<SysSchema>)db.ObjectCache[CACHE_KEY];
		}
	}
}