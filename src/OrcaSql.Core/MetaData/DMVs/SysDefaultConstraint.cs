using System;
using System.Collections.Generic;
using System.Linq;
using OrcaSql.Core.Engine;

namespace OrcaSql.Core.MetaData.DMVs
{
	public class SysDefaultConstraint : Row
	{
		private const string CACHE_KEY = "DMV_SysDefaultConstraints";

        private static readonly ISchema schema = new Schema(new[]
        {
            new DataColumn("name", "sysname"),
            new DataColumn("object_id", "int"),
            new DataColumn("principal_id", "int"),
            new DataColumn("schema_id", "int"),
            new DataColumn("parent_object_id", "int"),
            new DataColumn("type", "char(2)"),
            new DataColumn("type_desc", "nvarchar"),
            new DataColumn("create_date", "datetime"),
            new DataColumn("modify_date", "datetime"),
            new DataColumn("is_ms_shipped", "bit"),
            new DataColumn("is_published", "bit"),
            new DataColumn("is_schema_published", "bit"),
            new DataColumn("parent_column_id", "int"),
            new DataColumn("definition", "nvarchar(max)"),
            new DataColumn("is_system_named", "bit")
        });

        public string Name
        {
            get => Field<string>("name");
            private set => this["name"] = value;
        }
		public int? PrincipalId
        {
            get => Field<int?>("principal_id");
            private set => this["principal_id"] = value;
        }
		public int SchemaId
        {
            get => Field<int>("schema_id");
            private set => this["schema_id"] = value;
        }

        public bool IsSystemNamed
        {
            get => Field<bool>("is_system_named");
            private set => this["is_system_named"] = value;
        }

        public string Definition
        {
            get => Field<string>("definition");
            private set => this["definition"] = value;
        }

        public int ParentColumnId
        {
            get => Field<int>("parent_column_id");
            private set => this["parent_column_id"] = value;
        }

        public bool IsSchemaPublished
        {
            get => Field<bool>("is_schema_published");
            private set => this["is_schema_published"] = value;
        }

        public bool IsPublished
        {
            get => Field<bool>("is_published");
            private set => this["is_published"] = value;
        }

        public bool IsMsShipped
        {
            get => Field<bool>("is_ms_shipped");
            private set => this["is_ms_shipped"] = value;
        }

        public DateTime ModifyDate
        {
            get => Field<DateTime>("modify_date");
            private set => this["modify_date"] = value;
        }

        public DateTime CreateDate
        {
            get => Field<DateTime>("create_date");
            private set => this["create_date"] = value;
        }

        public string TypeDesc
        {
            get => Field<string>("type_desc");
            private set => this["type_desc"] = value;
        }

        public string Type
        {
            get => Field<string>("type");
            private set => this["type"] = value;
        }

        public int ParentObjectId
        {
            get => Field<int>("parent_object_id");
            private set => this["parent_object_id"] = value;
        }

        public int ObjectId
        {
            get => Field<int>("object_id");
            private set => this["object_id"] = value;
        }

        public SysDefaultConstraint()
			: base(schema)
		{ }

		public override Row NewRow()
		{
			return new SysDefaultConstraint();
		}

		internal static IEnumerable<SysDefaultConstraint> GetDmvData(Database db)
		{
			if (!db.ObjectCache.ContainsKey(CACHE_KEY))
			{
                db.ObjectCache[CACHE_KEY] = (
                    from o in db.Dmvs.ObjectsDollar
                    join m in db.Dmvs.SqlModules on o.ObjectID equals m.ObjectID
                    where o.Type == "D"
                          && (o.ParentObjectID > 0 || (o.ParentObjectID & 0xe0000000) == 0xa0000000)
                    select new SysDefaultConstraint
                    {
                        Name = o.Name,
                        ObjectId = o.ObjectID,
                        PrincipalId = o.PrincipalID,
                        SchemaId = o.SchemaID,
                        ParentObjectId = o.ParentObjectID,
                        Type = o.Type,
                        TypeDesc = o.TypeDesc,
                        CreateDate = o.CreateDate,
                        ModifyDate = o.ModifyDate,
                        IsMsShipped = o.IsMSShipped,
                        IsPublished = o.IsPublished,
                        IsSchemaPublished = o.IsSchemaPublished,
                        ParentColumnId = o.Property,
                        Definition = m.Definition,
                        IsSystemNamed = o.IsSystemNamed
                    }).ToList();
            }

			return (IEnumerable<SysDefaultConstraint>)db.ObjectCache[CACHE_KEY];
		}
    }
}