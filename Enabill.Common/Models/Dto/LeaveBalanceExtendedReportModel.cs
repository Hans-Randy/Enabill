using System;

namespace Enabill.Models.Dto
{
	public class LeaveBalanceExtendedReportModel
	{
		#region PROPERTIES

		public int ApprovalStatus;
		public int LeaveTypeID;
		public int UserID;

		public double HoursTaken;
		public double LeaveCredited;
		public double LeaveTaken;
		public double ManualAdjustment;
		public double NormalHours;
		public double NumberOfDays;
		public double OpeningBalance;

		public string FullName;
		public string LeaveTypeName;
		public string Manager;
		public string PayrollRefNo;
		public string Remark;

		public DateTime BalanceDate;
		public DateTime LeavePeriodEndDate;
		public DateTime LeavePeriodStartDate;
		public DateTime WorkDate;

		#endregion PROPERTIES
	}
}