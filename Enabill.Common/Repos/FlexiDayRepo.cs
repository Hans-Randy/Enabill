using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class FlexiDayRepo : BaseRepo
	{
		#region FLEXIDAY SPECIFIC

		public static FlexiDay GetByID(int flexiDay) => DB.FlexiDays
					.SingleOrDefault(fd => fd.FlexiDayID == flexiDay);

		public static void Save(FlexiDay flexiDay)
		{
			if (flexiDay.FlexiDayID == 0)
				DB.FlexiDays.Add(flexiDay);

			DB.SaveChanges();
		}

		public static void Delete(FlexiDay flexiDay)
		{
			DB.FlexiDays.Remove(flexiDay);
			DB.SaveChanges();
		}

		public static List<FlexiDay> GetFlexiDays(DateTime dateFrom, DateTime dateTo) => (from r in DB.FlexiDays
																						  where r.FlexiDate >= dateFrom && r.FlexiDate < dateTo
																						  select r).ToList();

		#endregion FLEXIDAY SPECIFIC
	}
}