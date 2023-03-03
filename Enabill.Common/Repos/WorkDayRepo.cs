using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public class WorkDayRepo : BaseRepo
	{
		#region WORK DAY SPECIFIC

		public static IEnumerable<WorkDay> GetWorkableDays(bool isWorkable, DateTime startDate, DateTime endDate) => DB.WorkDays
					.Where(wd => wd.IsWorkable == isWorkable && wd.WorkDate >= startDate.Date && wd.WorkDate <= endDate.Date);

		public static int GetNumberOfWorkableDays(bool isWorkable, DateTime startDate, DateTime endDate) => DB.WorkDays
					.Where(wd => wd.IsWorkable == isWorkable && wd.WorkDate >= startDate.Date && wd.WorkDate <= endDate.Date)
					.Count();

		public static WorkDay GetLastWorkdateByDate(DateTime date) => DB.WorkDays
					.Where(wd => wd.WorkDate <= date && wd.IsWorkable)
					.OrderByDescending(d => d.WorkDate)
					.FirstOrDefault();

		public static WorkDay GetLastWorkDay() => DB.WorkDays
			.OrderByDescending(d => d.WorkDate)
			.FirstOrDefault();
		
		internal static bool IsDayWorkable(DateTime date)
		{
			var wd = DB.WorkDays.SingleOrDefault(w => w.WorkDate == date.Date);

			if (wd == null)
				return !(date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday);

			return wd.IsWorkable;
		}

		#endregion WORK DAY SPECIFIC

		#region JSON LOOKUPS

		public static JsonLookup GetNumberOfWorkDays(DateTime periodFrom, DateTime periodTo)
		{
			int nrWorkDays = (from wd in DB.WorkDays
							  where wd.IsWorkable && wd.WorkDate >= periodFrom.Date && wd.WorkDate <= periodTo.Date
							  select wd.WorkDate).Count();

			return new JsonLookup()
			{
				id = nrWorkDays,
				label = nrWorkDays.ToString(),
				value = nrWorkDays.ToString()
			};
		}

		public static int GetTotalWorkDays(DateTime? periodFrom, DateTime? periodTo)
		{
			int totalWorkDays = 0;

			if (periodFrom == null || periodTo == null)
			{
				return totalWorkDays;
			}
			else
			{
				return totalWorkDays = (from wd in DB.WorkDays
										where wd.IsWorkable && wd.WorkDate >= periodFrom.Value && wd.WorkDate <= periodTo.Value
										select wd.WorkDate).Count();
			}
		}

		#endregion JSON LOOKUPS

		public static void GenerateWorkableDaysFor(int year, IEnumerable<Holiday> holidays)
		{
			for (var d = new DateTime(year, 1, 1); d < new DateTime(year + 1, 1, 1); d = d.AddDays(1)) 
			{
				DB.WorkDays.Add(new WorkDay (d, !(d.DayOfWeek == DayOfWeek.Sunday || d.DayOfWeek == DayOfWeek.Saturday || holidays.Any(x => x.Date == d))));
			}
			DB.SaveChanges();
		}

		public static void ChangeWorkableDay(WorkDayModel model)
		{
			var workDay = DB.WorkDays.SingleOrDefault(w => w.WorkDate == model.Date);
			workDay.SetIsWorkable(model.IsWorkable);
			DB.Entry(workDay).State = EntityState.Modified;
			DB.SaveChanges();
		}
	}
}