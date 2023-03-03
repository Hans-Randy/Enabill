using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class HolidayRepo : BaseRepo
	{
		#region WORK DAY SPECIFIC

		public static Holiday GetByID(int holidayID) => DB.Holidays
			.SingleOrDefault(h => h.HolidayID == holidayID);

		public static IEnumerable<Holiday> GetHolidays(int year) => DB.Holidays
			.Where(h => h.IsRepeated || h.Date.Year == year && h.IsRepeated == false && h.Status == StatusEnum.Enabled);

		public static void Save(Holiday holiday)
		{
			try
			{
				if (holiday.HolidayID == 0)
				{
					holiday.DateCreated = DateTime.Now;
					DB.Entry(holiday).State = EntityState.Added;
					DB.Holidays.Add(holiday);
				}
				else
				{
					holiday.DateUpdated = DateTime.Now;
					var CreatorOfHloidayToBeUpdated = DB.Holidays.Where(h => h.HolidayID == holiday.HolidayID).FirstOrDefault();
					holiday.CreatedBy = CreatorOfHloidayToBeUpdated.CreatedBy;
					holiday.DateCreated = CreatorOfHloidayToBeUpdated.DateCreated;
					holiday.Status = StatusEnum.Enabled;
					DB.Entry(CreatorOfHloidayToBeUpdated).State = EntityState.Detached;
					DB.Entry(holiday).State = EntityState.Modified;
				}

				DB.SaveChanges();
			}
			catch (Exception ex)
			{
				throw new EnabillDomainException("The holiday could not be added: " + ex.Message);
			}
		}

		public static void Delete(Holiday holiday)
		{
			try
			{
				holiday.DateDeleted = DateTime.Now;
				var CreatorOfHloidayToBeUpdated = DB.Holidays.Where(h => h.HolidayID == holiday.HolidayID).FirstOrDefault();
				holiday.CreatedBy = CreatorOfHloidayToBeUpdated.CreatedBy;
				holiday.DateCreated = CreatorOfHloidayToBeUpdated.DateCreated;
				holiday.DateUpdated = CreatorOfHloidayToBeUpdated.DateUpdated;
				holiday.Status = StatusEnum.Deleted;
				DB.Entry(CreatorOfHloidayToBeUpdated).State = EntityState.Detached;
				DB.Entry(holiday).State = EntityState.Modified;
				DB.SaveChanges();
			}
			catch (Exception ex)
			{
				throw new EnabillDomainException("The holiday could not be deleted: " + ex.Message);
			}
		}

		#endregion WORK DAY SPECIFIC

		#region JSON LOOKUPS

		#endregion JSON LOOKUPS
	}
}