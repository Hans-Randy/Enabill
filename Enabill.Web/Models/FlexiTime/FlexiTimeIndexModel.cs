using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class FlexiTimeIndexModel
	{
		#region INITIALIZATION

		public FlexiTimeIndexModel(User user)
		{
			var monthDate = InputHistory.GetDateTime(HistoryItemType.FlexiDay, DateTime.Today.ToFirstDayOfMonth()).Value;
			var startDate = user.EmployStartDate > EnabillSettings.SiteStartDate ? user.EmployStartDate : EnabillSettings.SiteStartDate;

			this.User = user;
			this.FlexiDate = monthDate;
			this.LoadCalendar(user, monthDate);
			this.FlexiDays = user.GetFlexiDaysForDateSpan(monthDate.ToFirstDayOfMonth(), monthDate.ToLastDayOfMonth());
			this.FlexiManualAdjustments = new FlexiTimeManualAdjustmentModel(user, monthDate.ToFirstDayOfMonth(), monthDate.ToLastDayOfMonth());
			this.WorkAllocationExceptions = UserRepo.GetTimeCaptureExceptions(user.UserID, startDate, DateTime.Today).ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public double OpeningBalance { get; set; }
		public double ClosingBalance { get; set; }

		public DateTime FlexiDate { get; set; }

		public FlexiTimeManualAdjustmentModel FlexiManualAdjustments { get; set; }
		public User User { get; set; }

		public List<FlexiDay> FlexiDays { get; set; }
		public List<FlexiTimeDayModel> Calendar { get; set; }
		public List<WorkAllocationExceptionModel> WorkAllocationExceptions { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public void LoadCalendar(User user, DateTime monthDate)
		{
			var model = new List<FlexiTimeDayModel>();

			var monthBalance = user.GetFlexiBalance(monthDate);
			double openingBalance = 0;

			if (monthBalance == null)
				openingBalance = user.CalculateFlexiBalanceOnDate(DateTime.Today, true);
			else
				openingBalance = monthBalance.FinalBalance;

			this.OpeningBalance = openingBalance;

			monthDate = monthDate.ToFirstDayOfMonth();

			if (user.EmployStartDate.IsInSameMonth(monthDate))
				monthDate = user.ConfigureDate(monthDate);

			foreach (var date in Enabill.Helpers.GetDaysInDateSpan(monthDate, monthDate.ToLastDayOfMonth()))
			{
				var dayModel = new FlexiTimeDayModel(user, date, openingBalance);
				model.Add(dayModel);
				openingBalance += dayModel.DeltaHours;
			}

			this.Calendar = model;
			this.ClosingBalance = openingBalance;
		}

		public string SetUpWorkDayClass(DateTime wDay)
		{
			if (wDay == DateTime.Today)
				return "today";

			return wDay.DayOfWeek == 0 || (int)wDay.DayOfWeek == 6 ? "weekend" : string.Empty;
		}

		#endregion FUNCTIONS
	}
}