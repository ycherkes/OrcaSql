using System.Collections.Generic;
using System.Linq;
using OrcaSql.Core.Engine;

namespace OrcaSql.Core.MetaData.DMVs
{
	public class SystemInternalsAllocationUnit : Row
	{
		private const string CACHE_KEY = "DMV_SystemInternalsAllocationUnit";

		private static readonly ISchema schema = new Schema(new[]
		    {
		        new DataColumn("AllocationUnitID", "bigint"),
				new DataColumn("Type", "tinyint"),
				new DataColumn("TypeDesc", "nvarchar", true),
				new DataColumn("ContainerID", "bigint"),
				new DataColumn("FilegroupID", "smallint"),
				new DataColumn("TotalPages", "bigint"),
				new DataColumn("UsedPages", "bigint"),
				new DataColumn("DataPages", "bigint"),
				new DataColumn("FirstPage", "binary(6)"),
				new DataColumn("RootPage", "binary(6)"),
				new DataColumn("FirstIamPage", "binary(6)")
		    });

		public long AllocationUnitID { get { return Field<long>("AllocationUnitID"); } private set { this["AllocationUnitID"] = value; } }
		public byte Type { get { return Field<byte>("Type"); } private set { this["Type"] = value; } }
		public string TypeDesc { get { return Field<string>("TypeDesc"); } private set { this["TypeDesc"] = value; } }
		public long ContainerID { get { return Field<long>("ContainerID"); } private set { this["ContainerID"] = value; } }
		public short FilegroupID { get { return Field<short>("FilegroupID"); } private set { this["FilegroupID"] = value; } }
		public long? TotalPages { get { return Field<long?>("TotalPages"); } private set { this["TotalPages"] = value; } } // TODO
		public long? UsedPages { get { return Field<long?>("UsedPages"); } private set { this["UsedPages"] = value; } } // TODO
		public long? DataPages { get { return Field<long?>("DataPages"); } private set { this["DataPages"] = value; } } // TODO
		public byte[] FirstPage { get { return Field<byte[]>("FirstPage"); } private set { this["FirstPage"] = value; } }
		public byte[] RootPage { get { return Field<byte[]>("RootPage"); } private set { this["RootPage"] = value; } }
		public byte[] FirstIamPage { get { return Field<byte[]>("FirstIamPage"); } private set { this["FirstIamPage"] = value; } }

        public PagePointer FirstPagePointer => new PagePointer(FirstPage);
        public PagePointer RootPagePointer => new PagePointer(RootPage);
        public PagePointer FirstIamPagePointer => new PagePointer(FirstIamPage);

        public SystemInternalsAllocationUnit() : base(schema)
		{ }

		public override Row NewRow()
		{
			return new SystemInternalsAllocationUnit();
		}

		internal static IEnumerable<SystemInternalsAllocationUnit> GetDmvData(Database db)
		{
			if (!db.ObjectCache.ContainsKey(CACHE_KEY))
			{
				db.ObjectCache[CACHE_KEY] = db.BaseTables.SysAllocUnits
                    .Where(au => au.auid != null)
					.Select(au => new SystemInternalsAllocationUnit
						{
							AllocationUnitID = (long)au.auid,
							Type = au.type,
							TypeDesc = db.BaseTables.SysPalValues
								.Where(ip => ip.@class == "AUTY" && ip.value == au.type)
								.Select(n => n.name)
								.FirstOrDefault(),
							ContainerID = au.ownerid,
							FilegroupID = au.fgid,
							FirstPage = au.pgfirst,
							RootPage = au.pgroot,
							FirstIamPage = au.pgfirstiam
						})
					.ToList();
			}

			return (IEnumerable<SystemInternalsAllocationUnit>)db.ObjectCache[CACHE_KEY];
		}
	}
}