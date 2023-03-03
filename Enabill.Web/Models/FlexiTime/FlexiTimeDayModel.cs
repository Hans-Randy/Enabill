using System;
using System.Linq;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class FlexiTimeDayModel
	{
		#region INITIALIZATION

		public FlexiTimeDayModel(User user, DateTime date, double openingBalance)
		{
			this.Date = date.Date;
			this.OpeningBalance = openingBalance;

			this.SetupModel(user, date, openingBalance);
			this.WorkDayClass = this.SetUpWorkDayClass(date);

			this.FlexiOrLeaveDayText = "";

			if (this.IsFlexiDay)
			{
				this.FlexiOrLeaveDayText = "Flexi";
			}
			else if (this.IsLeaveDay)
			{
				this.Leave = user.GetAnyLeaveForDate(date);
				this.FlexiOrLeaveDayText = ((LeaveTypeEnum)this.Leave.LeaveType).ToString();
			}
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

		public string WorkDayClass { get; set; }
		public string FlexiOrLeaveDayText { get; private set; }

		public DateTime Date { get; set; }

		public Leave Leave { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private void SetupModel(User user, DateTime date, double openingBalance)
		{
			var workSessions = user.GetWorkSessions(date);
			var adjustments = user.GetFlexiBalanceAdjustments(date, date, true);

			this.HoursWorked = workSessions.Sum(ws => ws.GrossTime);
			this.FlexiTimeAdjustment = adjustments.Sum(a => a.Adjustment);
			this.LunchHours = workSessions.Sum(ws => ws.LunchTime);
			this.IsFlexiDay = user.IsFlexiDayTakenOnDate(date);
			this.IsLeaveDay = user.IsAnyLeaveTakenOnDate(date);

			this.LeaveHoursTaken = date.IsWorkableDay() ? user.GetLeave(ApprovalStatusType.Approved, date, date).Sum(l => l.NumberOfHours ?? user.WorkHoursOnDate(date)) : 0;
			this.GrossHours = date.Date > DateTime.Today ? 0 : this.HoursWorked + this.LeaveHoursTaken + this.FlexiTimeAdjustment;
			this.NettHours = date.Date > DateTime.Today ? 0 : this.GrossHours - this.LunchHours;
			this.HoursRequired = date.Date > DateTime.Today ? 0 : date.IsWorkableDay() ? user.WorkHoursOnDate(date) : 0;
			this.DeltaHours = this.NettHours - this.HoursRequired;
		}

		public string SetUpWorkDayClass(DateTime wDay)
		{
			if (wDay == DateTime.Today)
				return "today";
			if (!wDay.IsWorkableDay())
				return "weekend";

			return string.Empty;
		}

		#endregion FUNCTIONS
	}
}