using System;

namespace Enabill.Models.Dto
{
	public class LeaveCycleExtendedReportModel
	{
		#region PROPERTIES

		//public User User;
		//public LeaveCycleBalance LeaveCycleBalance;

		public double ClosingBalance { get; set; }
		public double ManualAdjustment { get; set; }
		public double OpeningBalance { get; set; }
		public double Taken { get; set; }

		public string EmploymentType { get; set; }
		public string FullName { get; set; }
		public string LeaveType { get; set; }
		public string Manager { get; set; }
		public string PayrollRefNo { get; set; }
		public string Status { get; set; }

		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }
		public DateTime LastUpdated { get; set; }

		#endregion PROPERTIES
	}
}