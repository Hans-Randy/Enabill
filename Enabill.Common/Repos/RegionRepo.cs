using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class RegionRepo : BaseRepo
	{
		#region REGION SPECIFIC
		public static Region GetByID(int regionID) => DB.Regions.SingleOrDefault(r => r.RegionID == regionID);
		public static Region GetByName(string regionName) => DB.Regions
					.SingleOrDefault(r => r.RegionName == regionName);
		public static IEnumerable<Region> GetAll() => DB.Regions;
		public static IEnumerable<Status> GetRegionStatusList() => new List<Status>
			{
				new Status() { StatusID = 1, StatusName = "Active" },
				new Status() { StatusID = 0, StatusName = "Inactive" }
			};
		private static int GetMaxRegionID() 
		{
			int regionID = Search("", false, true).OrderByDescending(o => o.RegionID).Take(1).First().RegionID;
			return regionID + 1;
		}
		internal static void Save(Region region)
		{
			if (region.RegionID == 0)
			{
				region.RegionID = GetMaxRegionID();
				DB.Regions.Add(region);
			}
			DB.SaveChanges();
		}
		internal static void Delete(Region region)
		{
			try
			{
				DB.Regions.Remove(region);
				DB.SaveChanges();
			}
			catch
			{
				throw new NullReferenceException("The region could not be found in the records");
			}
		}
		public static Region GetNew() => new Region(){};
		internal static List<RegionSearchResult> Search(string q,bool isActive, bool showAllRegions)
		{
			if (q == null)
				q = string.Empty;

			var list = from r in DB.Regions
					   where  (r.RegionName.Contains(q) || r.RegionShortCode.Contains(q))
					  select new RegionSearchResult()
					  {
						  RegionID = r.RegionID,
						  RegionName = r.RegionName,
						  RegionShortCode = r.RegionShortCode,
						  IsActive = r.IsActive
					  };

			var resultList = showAllRegions ? list.OrderBy(r => r.RegionName).ToList() : list.OrderBy(r => r.RegionName).Where(r => r.IsActive == isActive).ToList();	
			return resultList;
		}
		#endregion REGION SPECIFIC
	}
}