using System;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class FlexiTimeMonthSummaryModel
	{
		#region INITIALIZATION

		public FlexiTimeMonthSummaryModel(User user, DateTime date, double openingBalance)
		{
			this.Date = date.Date;
			this.SetupModel(user, date, openingBalance);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool IsFlexiDay { get; set; }
		public bool IsLeaveDay { get; set; }

		public double DeltaHours { get; set; }
		public double FlexiTimeAdjustment { get; set; }
		public double GrossHours { get; set; }
		public double HoursRequired { get; set; }
		public double HoursWorked { get; set; }
		public double LeaveHoursTaken { get; set; }
		public double LunchHours { get; set; }
		public double NettHours { get; set; }
		public double OpeningBalance { get; set; }

		public string FlexiOrLeaveDayText { get; private set; }
		public string WorkDayClass { get; set; }

		public DateTime Date { get; set; }

		public Leave Leave { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private void SetupModel(User user, DateTime date, double openingBalance)
		{
			var dateFrom = date.ToFirstDayOfMonth();
			var dateTo = date.IsInCurrentMonth() ? DateTime.Today : date.ToLastDayOfMonth();

			//when  getting worksession set date to to next day as endate includes time which could result in last day being exclude
			var workSessions = user.GetWorkSessionsForDateSpan(dateFrom, dateTo.AddDays(1));
			var adjustments = user.GetFlexiBalanceAdjustments(dateFrom, dateTo, true);

			this.HoursWorked = workSessions.Sum(ws => ws.GrossTime);
			this.FlexiTimeAdjustment = adjustments.Sum(a => a.Adjustment);
			this.LunchHours = workSessions.Sum(ws => ws.LunchTime);
			int nrOfWorkDays = WorkDay.GetWorkableDays(true, dateFrom, dateTo).Count;
			this.LeaveHoursTaken = UserRepo.GetTotalLeaveDaysForUserForDates(user.UserID, ApprovalStatusType.Approved, dateFrom, dateTo) * user.WorkHoursOnDate(date);
			this.GrossHours = date.Date > DateTime.Today ? 0 : this.HoursWorked + this.LeaveHoursTaken + this.FlexiTimeAdjustment;
			this.NettHours = date.Date > DateTime.Today ? 0 : this.GrossHours - this.LunchHours;
			this.HoursRequired = date.Date > DateTime.Today ? 0 : user.WorkHoursOnDate(date) * nrOfWorkDays;
			this.DeltaHours = this.NettHours - this.HoursRequired;
		}

		#endregion FUNCTIONS
	}
}