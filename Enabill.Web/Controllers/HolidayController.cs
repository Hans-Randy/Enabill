using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Properties;
using Enabill.Repos;
using Enabill.Web.Models;
using Enabill.Web.Models.Holiday;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class HolidayController : BaseController
	{
		public ActionResult Index(int? year)
		{
			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			if (!year.HasValue)
				year = DateTime.Today.Year;

			var holidays = this.GetHolidaysFor(year.Value);

			var nonWorkDays = WorkDayRepo
				.GetWorkableDays(false, new DateTime(year.Value, 1, 1), new DateTime(year.Value, 12, 31))
				.Where(x => x.WorkDate.DayOfWeek != DayOfWeek.Saturday && x.WorkDate.DayOfWeek != DayOfWeek.Sunday)
				.Select(x => x.WorkDate.ToString("M-d")).ToList();
			return this.View(new HolidayViewModel
			{
				IsWorkDaySaved = WorkDayRepo.GetLastWorkDay().WorkDate >= new DateTime(year.Value, 12, 31),
				Holiday = new HolidayModel(),
				SelectedYear = year.Value,
				HolidayList = holidays.Select(x => new HolidayLiteModel(x, year.Value)).OrderBy(x => x.Date).ToList(),
				CalendarHolidays = holidays.Select(x => x.Date.ToString("M-d")).ToList(),
				NonWorkDays = nonWorkDays
			});
		}

		private IEnumerable<Holiday> GetHolidaysFor(int year)
		{
			bool IsWorkDaySaved = WorkDayRepo.GetLastWorkDay().WorkDate >= new DateTime(year, 12, 31);


			var dbContextHolidays = HolidayRepo.GetHolidays(year);
			
			var currentYearHolidays = dbContextHolidays.Select(holiday => new Holiday
				{
					HolidayID = holiday.HolidayID,
					Date = new DateTime(year, holiday.Date.Month, holiday.Date.Day),
					HolidayName = holiday.HolidayName,
					IsRepeated = holiday.IsRepeated,
					IsFixedDate = holiday.IsFixedDate
				})
				.ToList();

			// easter fix;
			var easterDate = this.GoodFriday(year);
			currentYearHolidays.FirstOrDefault(x => x.HolidayID == 3).Date = easterDate;
			currentYearHolidays.FirstOrDefault(x => x.HolidayID == 4).Date = easterDate.AddDays(2); // Sunday is the actually holiday, but Monday holidays are now automated.

			var sundayHolidays = currentYearHolidays.Where(x => new DateTime(year, x.Date.Month, x.Date.Day).DayOfWeek == DayOfWeek.Sunday && !IsWorkDaySaved).ToList();
			foreach (var holiday in sundayHolidays)
			{
				currentYearHolidays.Add(new Holiday
				{
					HolidayID = 0,
					Date = new DateTime(year, holiday.Date.Month, holiday.Date.Day).AddDays(1),
					HolidayName = holiday.HolidayName + "'s public holiday",
					IsFixedDate = false,
					IsRepeated = false
				});

			}
			
			return currentYearHolidays;
		}

		[HttpPost]
		public JsonResult Create(int selectedYear, int holidayId, string holidayName, DateTime dateOfHoliday, bool isFixed, bool isRepeated)
		{
			try
			{
				var holiday = new Holiday
				{
					HolidayID = holidayId,
					HolidayName = holidayName,
					Date = new DateTime(selectedYear, dateOfHoliday.Month, dateOfHoliday.Day),
					IsRepeated = isFixed,
					IsFixedDate = isRepeated
				};

				if (holiday.HolidayID == 0)
				{
					holiday.Status = StatusEnum.Enabled;
					holiday.CreatedBy = this.CurrentUser.FullName;
				}
				else
				{
					holiday.LastModifiedBy = this.CurrentUser.FullName;
				}

				HolidayRepo.Save(holiday);

				return this.Json(new
				{
					IsError = false,
					Description = "New holiday was saved successfully.",
					Url = "/Holiday"
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex) as JsonResult;
			}
		}

		[HttpPost]
		public JsonResult Delete(HolidayModel model)
		{
			try
			{
				var holiday = HolidayRepo.GetByID(model.HolidayId);

				if(holiday == null)
					return this.Json(new
					{
						IsError = true,
						Description = "This holiday does not exist."
					});

				if (holiday.CanDelete())
				{
					holiday.DeletedBy = this.CurrentUser.FullName;
					HolidayRepo.Delete(holiday);
				}
				else
					return this.Json(new
					{
						IsError = true,
						Description = "This holiday can't be deleted."
					});

				return this.Json(new
				{
					IsError = false,
					Description = "Custom holiday was successfully removed."
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex) as JsonResult;
			}
		}

		[HttpPost]
		public JsonResult ApplyHolidays(int year)
		{
			try
			{
				var holidays = this.GetHolidaysFor(year).ToList();

				foreach (var holiday in holidays.Where(x => x.HolidayID == 0))
				{
					holiday.CreatedBy = this.CurrentUser.FullName;
					HolidayRepo.Save(holiday);
				}

				WorkDayRepo.GenerateWorkableDaysFor(year, holidays);

				return this.Json(new
				{
					IsError = false,
					Description = "New holiday was saved successfully.",
					Url = "/Holiday?year=" + year
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex) as JsonResult;
			}
		}

		[HttpPost]
		public JsonResult WorkDayChange(WorkDayModel model)
		{
			try
			{
				WorkDayRepo.ChangeWorkableDay(model);
				return this.Json(new
				{
					IsError = false,
					Description = "Workday updated to " + (model.IsWorkable?"IS workable":"NOT workable"),
					Url = "/Holiday?year=" + model.Date.Year
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex) as JsonResult;
			}
		}

		/*
		 * Web URL for Easter Sunday calculation: https://www.timeanddate.com/holidays/south-africa/easter-sunday
		 * From this date we can deduce Good Friday and Family Day dates;
		 */
		private DateTime GoodFriday(int year)
		{
			int a = year % 19;
			int b = year / 100;
			int c = year % 100;
			int d = b / 4;
			int e = b % 4;
			int f = (b + 8) / 25;
			int g = (b - f + 1) / 3;
			int h = (19 * a + b - d - g + 15) % 30;
			int i = c / 4;
			int k = c % 4;
			int l = (32 + 2 * e + 2 * i - h - k) % 7;
			int m = (a + 11 * h + 22 * l) / 451;
			int n = (h + l - 7 * m + 114) / 31;
			int p = (h + l - 7 * m + 114) % 31;
			
			var easterDate = new DateTime(year, n, p + 1).AddDays(-2);
			
			return easterDate;
		}
	}
}