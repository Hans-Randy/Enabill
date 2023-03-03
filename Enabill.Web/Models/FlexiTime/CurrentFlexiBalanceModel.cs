using System;
using System.Linq;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class CurrentFlexiBalanceModel
	{
		#region INITIALIZATION

		public CurrentFlexiBalanceModel(User user)
		{
			this.LastBalanceDate = user.GetMostRecentFlexiBalanceDate();
			this.MostRecentBalance = user.GetMostRecentFlexiBalanceHours();
			this.MostRecentBalanceClass = this.LoadClassForBalance(this.MostRecentBalance);
			this.HoursRequiredSinceLastBalance = user.CalculateHoursRequiredForDateSpan(this.LastBalanceDate, DateTime.Today);
			this.HoursWorkedSinceLastBalance = this.CalculateHoursWorkedSinceLastBalance(user);
			this.HoursTakenAsLeave = this.LoadLeaveHours(user);
			this.FlexiHoursAdjustmentSinceLastBalance = user.GetFlexiBalanceAdjustments(this.LastBalanceDate, DateTime.Today, true).Sum(fba => fba.Adjustment);
			this.FlexiBalanceToday = user.CalculateFlexiBalanceOnDate(DateTime.Today, true);
			this.CurrentBalanceClass = this.LoadClassForBalance(this.FlexiBalanceToday);

			this.Difference = this.HoursWorkedSinceLastBalance - this.HoursRequiredSinceLastBalance + this.HoursTakenAsLeave + this.FlexiHoursAdjustmentSinceLastBalance;
			this.DifferenceString = string.Format($"{(this.Difference < 0 ? "Deficit" : "Surplus")} for period");
			this.DifferenceClass = this.LoadClassForBalance(this.Difference);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public double Difference { get; private set; }
		public double FlexiBalanceToday { get; private set; }
		public double FlexiHoursAdjustmentSinceLastBalance { get; private set; }
		public double MostRecentBalance { get; private set; }
		public double HoursRequiredSinceLastBalance { get; private set; }
		public double HoursTakenAsLeave { get; private set; }
		public double HoursWorkedSinceLastBalance { get; private set; }

		public string CurrentBalanceClass { get; private set; }
		public string DifferenceClass { get; private set; }
		public string DifferenceString { get; private set; }
		public string MostRecentBalanceClass { get; private set; }

		public DateTime LastBalanceDate { get; private set; }

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