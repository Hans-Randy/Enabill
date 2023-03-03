using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class FinPeriodRepo : BaseRepo
	{
		public static void Save(FinPeriod period)
		{
			if (period.FinPeriodID == 0)
			{
				DB.FinPeriods.Add(period);
				period.FinPeriodID = int.Parse(period.DateFrom.Year.ToString() + period.DateFrom.Month.ToString("00"));
			}

			DB.SaveChanges();
		}

		public static IEnumerable<FinPeriod> GetAll(string getAll)
		{
			var startDate = DateTime.Today.AddMonths(-1).ToFirstDayOfMonth();
			var endDate = DateTime.Today.ToLastDayOfMonth();

			if (getAll == "No" || string.IsNullOrEmpty(getAll))
			{
				//Although future periods have been captured only show the previous month and the current month to avoid mistakes and long list
				return DB.FinPeriods.Where(p => (p.DateFrom >= startDate && p.DateTo <= endDate) || p.IsCurrent);
			}
			else if (getAll == "lastYearFinPeriod")
			{
				startDate = new DateTime(2013, 10, 1);
				return DB.FinPeriods.Where(x => (x.DateFrom >= startDate && x.DateTo <= endDate) || x.IsCurrent);
			}
			else
			{
				return DB.FinPeriods;
			}
		}

		public static FinPeriod GetCurrentFinPeriod() => DB.FinPeriods.Where(p => p.IsCurrent).SingleOrDefault();

		public static FinPeriod GetFinPeriodByID(int finPeriod) => DB.FinPeriods.Where(p => p.FinPeriodID == finPeriod).SingleOrDefault();
	}
}