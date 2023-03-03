using System;

namespace Enabill.Models.Dto
{
	public class LeaveBalanceSummarisedReportModel
	{
		#region PROPERTIES

		public double Approved { get; set; }
		public double ClosingBalance { get; set; }
		public double Compassionate { get; set; }
		public double LeaveCredited { get; set; }
		public double LeaveTaken { get; set; }
		public double ManualAdjustment { get; set; }
		public double Maternity { get; set; }
		public double NextMonthApprovedLeave { get; set; }
		public double NextMonthPendingLeave { get; set; }
		public double OpeningBalance { get; set; }
		public double Pending { get; set; }
		public double Relocation { get; set; }
		public double Sick { get; set; }
		public double Study { get; set; }
		public double Unpaid { get; set; }

		public string EmploymentType { get; set; }
		public string FullName { get; set; }
		public string LeaveType { get; set; }
		public string Manager { get; set; }
		public string PayrollRefNo { get; set; }

		public DateTime BalanceDate { get; set; }
		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }

		#endregion PROPERTIES
	}
}