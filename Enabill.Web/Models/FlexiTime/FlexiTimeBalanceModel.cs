using System;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class FlexiTimeBalanceModel
	{
		#region INITIALIZATION

		public FlexiTimeBalanceModel(User user)
		{
			this.FlexiUser = user;
			this.LastBalanceDate = DateTime.Today.ToFirstDayOfMonth();
			this.MostRecentBalance = user.GetMostRecentFlexiBalanceHours();
			this.MostRecentBalanceClass = this.LoadClassForBalance(this.MostRecentBalance);
			this.HoursRequiredSinceLastBalance = WorkDayRepo.GetNumberOfWorkableDays(true, DateTime.Today.ToFirstDayOfMonth(), DateTime.Today) * user.WorkHours;
			this.HoursWorkedSinceLastBalance = user.GetWorkSessionsForDateSpan(DateTime.Today.ToFirstDayOfMonth(), DateTime.Today).Sum(ws => ws.TotalTime);
			this.HoursTakenAsLeave = user.GetLeaveHoursForPeriod(DateTime.Today.ToFirstDayOfMonth(), DateTime.Today).Sum(l => l.NumberOfDays * user.WorkHours);

			this.Difference = this.HoursWorkedSinceLastBalance - this.HoursRequiredSinceLastBalance + this.HoursTakenAsLeave;// +FlexiHoursAdjustmentSinceLastBalance;
			this.DifferenceString = string.Format($"{(this.Difference < 0 ? "Deficit" : "Surplus")} for period");
			this.DifferenceClass = this.LoadClassForBalance(this.Difference);

			this.FlexiBalanceToday = this.MostRecentBalance + this.Difference;
			this.FlexiBalanceTodayClass = this.LoadClassForBalance(this.FlexiBalanceToday);
			this.LastWorkSessionDate = user.GetLastWorkSession();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public double Difference { get; private set; }
		public double FlexiBalanceToday { get; private set; }
		public double FlexiHoursAdjustmentSinceLastBalance { get; private set; }
		public double HoursRequiredSinceLastBalance { get; private set; }
		public double HoursTakenAsLeave { get; private set; }
		public double HoursWorkedSinceLastBalance { get; private set; }
		public double MostRecentBalance { get; private set; }

		public string DifferenceClass { get; private set; }
		public string DifferenceString { get; private set; }
		public string FlexiBalanceTodayClass { get; private set; }
		public string LastWorkSessionDate { get; private set; }
		public string MostRecentBalanceClass { get; private set; }

		public DateTime LastBalanceDate { get; private set; }

		public User FlexiUser { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private string LoadClassForBalance(double balance) => balance < 0 ? "error" : string.Empty;

		public double CalculateHoursWorkedSinceLastBalance(User user)
		{
			double hoursWorked = 0;

			foreach (var day in Enabill.Helpers.GetDaysInDateSpan(this.LastBalanceDate, DateTime.Today))
			{
				hoursWorked += user.GetWorkSessions(day).Sum(ws => ws.TotalTime);
			}

			return hoursWorked;
		}

		private double LoadLeaveHours(User user) => user.GetLeaveTakenTotalInHoursForDateSpan(ApprovalStatusType.Approved, this.LastBalanceDate, DateTime.Today);

		#endregion FUNCTIONS
	}
}