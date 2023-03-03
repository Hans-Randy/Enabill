using System;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class LeaveBalanceMonthModel
	{
		#region INITIALIZATION

		public LeaveBalanceMonthModel(User user, DateTime monthDate, bool isInitialRecord = false)
		{
			this.Date = monthDate;
			this.MonthName = string.Format($"{monthDate.Year} {monthDate.ToMonthName()}");
			this.LoadMonthModel(user, monthDate);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool EntryExists { get; private set; }

		public double AnnualLeaveBalance { get; private set; }
		public double AnnualLeaveCredited { get; private set; }
		public double AnnualLeaveOpeningBalance { get; private set; }
		public double AnnualLeaveTaken { get; private set; }
		public double CompassionateLeaveTaken { get; private set; }
		public double ManualAdjustment { get; private set; }
		public double MaternityLeaveTaken { get; private set; }
		public double RelocationLeaveTaken { get; private set; }
		public double SickLeaveTaken { get; private set; }
		public double StudyLeaveTaken { get; private set; }
		public double UnpaidLeaveTaken { get; private set; }

		public string MonthName { get; private set; }

		public DateTime Date { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private double GetLeaveAmount(User user, LeaveTypeEnum leaveType, DateTime monthDate) => user.GetLeaveTakenTotalInDaysForDateSpan(leaveType, ApprovalStatusType.Approved, monthDate, monthDate.ToLastDayOfMonth());

		private void LoadMonthModel(User user, DateTime monthDate)
		{
			var date = monthDate;

			this.AnnualLeaveTaken = this.GetLeaveAmount(user, LeaveTypeEnum.Annual, date);
			this.SickLeaveTaken = this.GetLeaveAmount(user, LeaveTypeEnum.Sick, date);
			this.CompassionateLeaveTaken = this.GetLeaveAmount(user, LeaveTypeEnum.Compassionate, date);
			this.StudyLeaveTaken = this.GetLeaveAmount(user, LeaveTypeEnum.Study, date);
			this.MaternityLeaveTaken = this.GetLeaveAmount(user, LeaveTypeEnum.Maternity, date);
			this.RelocationLeaveTaken = this.GetLeaveAmount(user, LeaveTypeEnum.Relocation, date);
			this.UnpaidLeaveTaken = this.GetLeaveAmount(user, LeaveTypeEnum.Unpaid, date);

			this.EntryExists = false;
			this.AnnualLeaveCredited = 0;
			this.AnnualLeaveBalance = 0;
			this.AnnualLeaveOpeningBalance = 0;
			this.ManualAdjustment = 0;

			var leaveBalance = user.GetLeaveBalance(LeaveTypeEnum.Annual, date);

			if (leaveBalance != null)
			{
				this.EntryExists = true;
				this.AnnualLeaveOpeningBalance = leaveBalance.Balance;

				if (monthDate.IsInCurrentMonth())
				{
					//LeaveCredited only credited at the end of a month
					this.AnnualLeaveBalance = (leaveBalance.Balance + leaveBalance.ManualAdjustment) - this.AnnualLeaveTaken;
					this.ManualAdjustment = leaveBalance.ManualAdjustment;
				}
				else
				{
					this.AnnualLeaveCredited = leaveBalance.LeaveCredited;
					this.ManualAdjustment = leaveBalance.ManualAdjustment;
					this.AnnualLeaveBalance = (leaveBalance.Balance + this.AnnualLeaveCredited + leaveBalance.ManualAdjustment) - this.AnnualLeaveTaken;
					Console.WriteLine(monthDate.Month + " " + monthDate.Year + " ( " + leaveBalance.Balance + " + " + this.AnnualLeaveCredited + " + " + leaveBalance.ManualAdjustment + " ) - " + this.AnnualLeaveTaken + " = " + this.AnnualLeaveBalance);
				}
			}
		}

		#endregion FUNCTIONS
	}
}