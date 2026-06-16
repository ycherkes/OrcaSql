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
				if (!data.TryGetValue(name, out var value))
					return default(T);

				value = ForceDeferredValue(value);

				if (value == null)
					return default(T);

				if (value is T typedNullableValue)
					return typedNullableValue;

				return (T)Convert.ChangeType(value, u);
			}

			// This is ugly, but fast as columns will practically always be present.
			// Exceptions are... The exception.
			try
			{
				var value = GetValue(name);

				if (value is T typedValue)
					return typedValue;

				return (T)Convert.ChangeType(value, t);
			}
			catch (KeyNotFoundException)
			{
				return (T)Convert.ChangeType(null, t);
			}
		}

		public object GetRawValue(string name)
		{
			EnsureColumnExists(name);

			return data.TryGetValue(name, out var value) ? value : null;
		}

		public object GetRawValue(DataColumn col)
		{
			return GetRawValue(col.Name);
		}

		private object GetValue(string name)
		{
			if (!data.TryGetValue(name, out var value))
				throw new KeyNotFoundException();

			return ForceDeferredValue(value);
		}

		private static object ForceDeferredValue(object value)
		{
			return value is IDeferredValue deferredValue ? deferredValue.Force() : value;
		}

		public object this[string name]
		{
			get
			{
				EnsureColumnExists(name);

				return data.TryGetValue(name, out var value) ? ForceDeferredValue(value) : null;
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
