using System;
using System.Linq;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class CurrentTimeBalanceModel
	{
		#region INITIALIZATION

		public CurrentTimeBalanceModel(User user)
		{
			this.CurrentDayTotal = this.LoadDayTotal(user);
			this.CurrentDayBalance = this.CurrentDayTotal - user.WorkHours;
			this.CurrentDayBalanceClass = this.LoadClassForBalance(this.CurrentDayBalance);

			this.MonthTotal = this.LoadMonthTotal(user);  //total hours worked
			this.MonthLeave = this.LoadMonthLeave(user);  //total leave taken/approved
			this.MonthGoalToDate = this.LoadMonthGoalToDate(user) - this.MonthLeave;    //total goal hours
			this.MonthBalance = this.MonthTotal - this.MonthGoalToDate;
			this.MonthBalanceClass = this.LoadClassForBalance(this.MonthBalance);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public double CurrentDayBalance { get; private set; }
		public double CurrentDayTotal { get; private set; }
		public double MonthBalance { get; private set; }
		public double MonthGoalToDate { get; private set; }
		public double MonthLeave { get; private set; }
		public double MonthTotal { get; private set; }

		public string CurrentDayBalanceClass { get; private set; }
		public string MonthBalanceClass { get; private set; }
		public string MonthBalanceName { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private string LoadClassForBalance(double balance) => balance < 0 ? "error" : "information";

		private double LoadDayTotal(User user)
		{
			var today = user.GetWorkAllocations(DateTime.Today);
			double balance = today.Sum(w => w.HoursWorked);

			return balance;
		}

		private double LoadMonthTotal(User user)
		{
			var monthStart = DateTime.Now.ToFirstDayOfMonth();
			var monthEnd = DateTime.Now.ToLastDayOfMonth();
			var month = user.GetWorkAllocations(monthStart, monthEnd);
			double balance = month.Sum(w => w.HoursWorked);

			return balance;
		}

		private double LoadMonthLeave(User user)
		{
			var monthStart = DateTime.Now.ToFirstDayOfMonth();
			var monthEnd = DateTime.Now.ToLastDayOfMonth();
			double leave = user.GetLeaveTakenTotalInHoursForDateSpan(ApprovalStatusType.Approved, monthStart, monthEnd);

			return leave;
		}

		private double LoadMonthGoalToDate(User user)
		{
			var monthStart = DateTime.Now.ToFirstDayOfMonth();
			var monthEnd = DateTime.Now;

			var workDays = WorkDay.GetWorkableDays(true, monthStart, monthEnd);

			double goal = (workDays.Count * user.WorkHours);

			return goal;
		}

		#endregion FUNCTIONS
	}
}