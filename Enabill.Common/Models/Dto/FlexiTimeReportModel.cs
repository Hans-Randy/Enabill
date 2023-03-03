using System;

namespace Enabill.Models.Dto
{
	public class FlexiTimeReportModel
	{
		#region PROPERTIES

		public int BalanceAuditTrailChanges { get; set; }
		public int UserID { get; set; }

		public double PendingApprovalsToDate { get; set; }
		public double PreviousMonthsClosingBalance { get; set; }
		public double SelectedMonthsClosingBalance { get; set; }
		public double SelectedMonthsDelta { get; set; }
		public double SelectedMonthsOpeningBalance { get; set; }
		public double WorkAllocationExceptionsToDate { get; set; }

		public string EmploymentType { get; set; }
		public string FullName { get; set; }
		public string LastWorkSessionDate { get; set; }
		public string Manager { get; set; }

		public DateTime PreviousMonth { get; set; }
		public DateTime SelectedMonth { get; set; }

		#endregion PROPERTIES
	}
}