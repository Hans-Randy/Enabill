using System;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class CurrentLeaveBalanceModel
	{
		#region INITIALIZATION

		public CurrentLeaveBalanceModel(User user)
		{
			this.StartDate = DateTime.Today;
			this.EndDate = DateTime.Today;

			this.AnnualLeaveOpeningBalance = 0;
			this.AnnualLeaveClosingBalance = 0;

			this.LoadModel(user);
			this.AnnualLeaveOpeningBalanceClass = this.LoadClassForBalance(this.AnnualLeaveOpeningBalance);
			this.AnnualLeaveClosingBalanceClass = this.LoadClassForBalance(this.AnnualLeaveClosingBalance);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public double AnnualLeaveClosingBalance { get; set; }
		public double AnnualLeaveOpeningBalance { get; set; }
		public double AnnualLeaveTaken { get; set; }
		public double OtherLeaveTaken { get; set; }

		public string AnnualLeaveClosingBalanceClass { get; set; }
		public string AnnualLeaveOpeningBalanceClass { get; set; }

		public DateTime EndDate { get; set; }
		public DateTime StartDate { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private double GetLeaveAmount(User user, LeaveTypeEnum leaveType) => user.GetLeaveTakenTotalInDaysForDateSpan(leaveType, ApprovalStatusType.Approved, this.StartDate, this.EndDate);

		private string LoadClassForBalance(double balance) => balance < 0 ? "error" : string.Empty;

		private void LoadModel(User user)
		{
			var date = DateTime.Today.ToFirstDayOfMonth();
			date = user.ConfigureDate(date);

			/*LeaveBalance leaveBalance = user.GetLeaveBalance(LeaveTypeEnum.Annual, date);
			while (leaveBalance == null && user.IsDateValidForUser(date))
			{
				date = date.AddMonths(-1);
				date = user.ConfigureDate(date);
				leaveBalance = user.GetLeaveBalance(LeaveTypeEnum.Annual, date);
			}*/
			var leaveBalance = user.GetLeaveBalancePriorToDate(LeaveTypeEnum.Annual, date);

			this.StartDate = date;

			this.AnnualLeaveTaken = this.GetLeaveAmount(user, LeaveTypeEnum.Annual);

			double sickLeaveTaken = this.GetLeaveAmount(user, LeaveTypeEnum.Sick);
			double compassionateLeaveTaken = this.GetLeaveAmount(user, LeaveTypeEnum.Compassionate);
			double studyLeaveTaken = this.GetLeaveAmount(user, LeaveTypeEnum.Study);
			double maternityLeaveTaken = this.GetLeaveAmount(user, LeaveTypeEnum.Maternity);
			double relocationLeaveTaken = this.GetLeaveAmount(user, LeaveTypeEnum.Relocation);
			double unpaidLeaveTaken = this.GetLeaveAmount(user, LeaveTypeEnum.Unpaid);
			this.OtherLeaveTaken = sickLeaveTaken + compassionateLeaveTaken + studyLeaveTaken + maternityLeaveTaken + relocationLeaveTaken + unpaidLeaveTaken;

			if (leaveBalance == null)
				return;

			this.AnnualLeaveOpeningBalance = leaveBalance.Balance;
			this.AnnualLeaveClosingBalance = leaveBalance.Balance - this.AnnualLeaveTaken;
		}

		#endregion FUNCTIONS
	}
}