using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OrcaSql.Core.MetaData
{
	/// <summary>
	/// Stores the actual data contained in a row, including a reference to the row schema.
	/// </summary>
	public abstract class Row
	{
		protected ISchema Schema;
		
		protected IDictionary<string, object> data;

		public ReadOnlyCollection<DataColumn> Columns => Schema.Columns;

        protected Row()
        {
            data = new Dictionary<string, object>();
        }

        protected Row(ISchema schema)
		{
			Schema = schema;
			data = new Dictionary<string, object>();
		}

		private void EnsureColumnExists(string name)
		{
			if(!Schema.HasColumn(name))
				throw new ArgumentOutOfRangeException("Column '" + name + "' does not exist.");
		}

		public T Field<T>(DataColumn col)
		{
			return Field<T>(col.Name);
		}

		public T Field<T>(string name)
		{
			EnsureColumnExists(name);

			// We need to handle nullables explicitly
			var t = typeof (T);
			var u = Nullable.GetUnderlyingType(t);
			
			if(u != null)
			{
				if (!data.ContainsKey(name) || data[name] == null)
					return default(T);

				return (T)Convert.ChangeType(data[name], u);
			}

			// This is ugly, but fast as columns will practically always be present.
			// Exceptions are... The exception.
			try
			{
				return (T)Convert.ChangeType(data[name], t);
			}
			catch (KeyNotFoundException)
			{
				return (T)Convert.ChangeType(null, t);
			}
		}

		public object this[string name]
		{
			get
			{
				EnsureColumnExists(name);

				return data.TryGetValue(name, out var value) ? value : null;
            }
			set
			{
				EnsureColumnExists(name);

				data[name] = value;
			}
		}

		public object this[DataColumn col]
		{
			get => this[col.Name];
            set => this[col.Name] = value;
        }

		public abstract Row NewRow();
	}
}