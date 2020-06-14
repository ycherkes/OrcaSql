using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OrcaSql.Core.MetaData
{
	public class Schema : ISchema
	{
		private readonly List<DataColumn> _columns = new List<DataColumn>();
		private readonly HashSet<string> _columnNameCache = new HashSet<string>();
		
		public Schema(IEnumerable<DataColumn> columns)
		{
			_columns.AddRange(columns);

			foreach(var col in columns)
				_columnNameCache.Add(col.Name);
		}

		public ReadOnlyCollection<DataColumn> Columns => _columns.AsReadOnly();

        public bool HasColumn(string name)
		{
			return _columnNameCache.Contains(name);
		}
	}
}