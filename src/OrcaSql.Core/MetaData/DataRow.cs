using System.Collections.Generic;

namespace OrcaSql.Core.MetaData
{
	public class DataRow : Row
	{
        public int? ObjectId { get; }

        public DataRow(IEnumerable<DataColumn> columns, int? objectId = null)
			: base(new Schema(columns))
        {
            ObjectId = objectId;
        }

		public DataRow(ISchema schema) : base(schema)
		{ }

        public override Row NewRow()
		{
			return new DataRow(Schema);
		}
	}
}