using System;

namespace OrcaSql.Core.MetaData.BaseTables
{
	public class sysclsobj : Row
	{
		private static readonly ISchema schema = new Schema(new[]
		    {
		        new DataColumn("class", "tinyint"),
				new DataColumn("id", "int"),
				new DataColumn("name", "sysname"),
				new DataColumn("status", "int"),
				new DataColumn("type", "char(2)"),
				new DataColumn("intprop", "int"),
				new DataColumn("created", "datetime"),
				new DataColumn("modified", "datetime")
		    });

		public sysclsobj() : base(schema)
		{ }

		public override Row NewRow()
		{
			return new sysclsobj();
		}

		public short @class => Field<short>("class");
        public int id => Field<int>("id");
        public string name => Field<string>("name");
        public int status => Field<int>("status");
        public string type => Field<string>("type");
        public int intprop => Field<int>("intprop");
        public DateTime created => Field<DateTime>("created");
        public DateTime modified => Field<DateTime>("modified");
    }
}