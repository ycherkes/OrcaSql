using System;
using System.Text;
using System.Text.RegularExpressions;

namespace OrcaSql.Core.MetaData
{
	public class DataColumn
	{
        public short? VariableFixedLength;
		public int? ColumnID;
		public string Name;
		public ColumnType UnderlyingType;
        public byte Precision;
		public byte Scale;
		public string TypeString;
		public bool IsNullable;
		public bool IsIncluded;
		public bool IsVariableLength;
		public bool IsSparse;

        public DataColumn(string name, string type)
			: this(name, type, false)
		{ }

		public DataColumn(string name, string type, bool nullable)
		{
			Name = name;
			TypeString = type;
			IsNullable = nullable;

            if(type == null) return;

            var typeNameWithoutNull = type.Replace(", not null", "").Replace(", null", "");

            var match = Regex.Match(typeNameWithoutNull, "(?<=\\w+)\\((\\w+(\\(\\d+\\)*)*)\\)");

            var underlyingType = match.Success && !Regex.IsMatch(match.Groups[1].Value, "^\\d+|max$", RegexOptions.IgnoreCase) ? match.Groups[1].Value : typeNameWithoutNull;

            if (underlyingType.StartsWith("Computed"))
            {
                UnderlyingType = ColumnType.Computed;
                Type = ColumnType.Computed;
                return;
            }

            var typeString = underlyingType.Split('(')[0];

            switch (typeString)
			{
				case "bigint":
                    Type = UnderlyingType = ColumnType.BigInt;
					break;

				case "binary":
					UnderlyingType = ColumnType.Binary;
                    Type = ColumnType.Binary;
                    VariableFixedLength = GetVariableFixedLength(underlyingType);
                    break;

				case "bit":
                    Type = UnderlyingType = ColumnType.Bit;
					break;

				case "char":
                    Type = UnderlyingType = ColumnType.Char;
					VariableFixedLength = GetVariableFixedLength(underlyingType);
					break;

				case "datetime":
                    Type  = UnderlyingType = ColumnType.DateTime;
                    break;

                case "numeric":
				case "decimal":
					UnderlyingType = ColumnType.Decimal;

					var parts = underlyingType.Split('(')[1].Split(')')[0].Split(',');
					Precision = Convert.ToByte(parts[0].Trim());
					Scale = Convert.ToByte(parts[1].Trim());

					if (parts.Length == 3)
						IsVariableLength = Convert.ToBoolean(parts[2]);

                    Type = typeString == "numeric" ? ColumnType.Numeric : ColumnType.Decimal;

                    break;
                case "float":
                    Type = UnderlyingType = ColumnType.Float;
                    Precision = Convert.ToByte(underlyingType.Split('(')[1].Split(')')[0]);
                    break;
                case "real":
                    UnderlyingType = ColumnType.Float;
                    Type = ColumnType.Real;
                    Precision = 24;
                    break;
                case "image":
					UnderlyingType = ColumnType.VarBinary;
                    Type = ColumnType.VarBinary;
					IsVariableLength = true;
                    VariableFixedLength = -1;
					break;

				case "int":
                    Type = UnderlyingType = ColumnType.Int;
					break;

				case "money":
                    Type = UnderlyingType = ColumnType.Money;
					break;

				case "nchar":
                    Type = UnderlyingType = ColumnType.NChar;
					VariableFixedLength = GetVariableFixedLength(underlyingType, true);
                    break;

				case "ntext":
                    UnderlyingType = ColumnType.NVarchar;
                    Type = ColumnType.NText;
                    VariableFixedLength = -1;
                    IsVariableLength = true;
					break;

				case "nvarchar":
                    Type = UnderlyingType = ColumnType.NVarchar;
                    IsVariableLength = true;
                    VariableFixedLength = GetVariableFixedLength(underlyingType, true);
                    break;

                case "sysname":
                    Type = ColumnType.SysName;
                    UnderlyingType = ColumnType.NVarchar;
                    VariableFixedLength = 256;
					IsVariableLength = true;
                    break;

                case "smalldatetime":
                    Type = UnderlyingType = ColumnType.SmallDatetime;
					break;

                case "date":
                    Type = UnderlyingType = ColumnType.Date;
                    break;

                case "smallint":
                    Type = UnderlyingType = ColumnType.SmallInt;
					break;

				case "smallmoney":
                    Type = UnderlyingType = ColumnType.SmallMoney;
					break;

				case "text":
                    UnderlyingType = ColumnType.NVarchar;
                    Type = ColumnType.Text;
                    VariableFixedLength = -1;
                    IsVariableLength = true;
					break;

				case "tinyint":
					UnderlyingType = ColumnType.TinyInt;
					break;

				case "uniqueidentifier":
                    Type = UnderlyingType = ColumnType.UniqueIdentifier;
					break;

				case "uniquifier":
                    Type = UnderlyingType = ColumnType.Uniquifier;
					IsVariableLength = true;
					break;

				case "rid":
                    Type = UnderlyingType = ColumnType.RID;
					IsVariableLength = false;
					break;

                case "hierarchyid":
                    UnderlyingType = ColumnType.VarBinary;
                    Type = ColumnType.HierarchyId;
                    IsVariableLength = true;
                    break;
                case "geography":
                    UnderlyingType = ColumnType.VarBinary;
                    Type = ColumnType.Geography;
                    IsVariableLength = true;
                    break;
                case "geometry":
                    UnderlyingType = ColumnType.VarBinary;
                    Type = ColumnType.Geometry;
                    IsVariableLength = true;
                    break;
                case "timestamp":
                    UnderlyingType = ColumnType.Binary;
                    VariableFixedLength = 8;
                    Type = ColumnType.Timestamp;
                    break;
                case "xml":
                    UnderlyingType = ColumnType.VarBinary;
                    Type = ColumnType.Xml;
                    IsVariableLength = true;
                    break;

                case "varbinary":
					UnderlyingType = ColumnType.VarBinary;
                    Type = ColumnType.VarBinary;
                    IsVariableLength = true;
                    VariableFixedLength = GetVariableFixedLength(underlyingType);
                    break;

                case "time":
                    UnderlyingType = Type = ColumnType.Time;
                    Scale = Convert.ToByte(underlyingType.Split('(')[1].Split(')')[0]);
                    break;
                case "datetime2":
                    UnderlyingType = Type = ColumnType.DateTime2;
                    Scale = Convert.ToByte(underlyingType.Split('(')[1].Split(')')[0]);
                    break;
                case "datetimeoffset":
                    UnderlyingType = Type = ColumnType.DateTimeOffset;
                    Scale = Convert.ToByte(underlyingType.Split('(')[1].Split(')')[0]);
                    break;

                case "varchar":
                    Type = UnderlyingType = ColumnType.Varchar;
					IsVariableLength = true;
                    VariableFixedLength = GetVariableFixedLength(underlyingType);
                    break;

				case "sql_variant":
					UnderlyingType = ColumnType.Variant;
					IsVariableLength = true;
					break;

				default:
					throw new ArgumentException("Unsupported type: " + type);
			}
		}

        private static short GetVariableFixedLength(string underlyingType, bool isUnicode = false)
        {
            var unicodeMultiplier = isUnicode ? (short)2 : (short)1;

            if (!underlyingType.Contains("(")) return unicodeMultiplier;

            var size = underlyingType.Split('(')[1].Split(')')[0];

            if (string.Equals(size, "max", StringComparison.OrdinalIgnoreCase)) return -1;

            return (short)(Convert.ToInt16(size) * unicodeMultiplier );
        }

        public ColumnType Type { get; set; }

        /// <summary>
		/// Standard DataColumn to be used for uniquifier column
		/// </summary>
		public static DataColumn Uniquifier => new DataColumn("___Uniquifier", "uniquifier");

        /// <summary>
		/// Standard DataColumn to be used for rid column
		/// </summary>
		public static DataColumn RID => new DataColumn("___RID", "rid");

        public Encoding Encoding { get; set; }

        public override string ToString()
		{
			return Name + " " + TypeString;
		}
	}
}
