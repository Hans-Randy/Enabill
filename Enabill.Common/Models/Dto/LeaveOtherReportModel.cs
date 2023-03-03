using System;

namespace Enabill.Models.Dto
{
	public class LeaveOtherReportModel
	{
		#region PROPERTIES

		public double Approved { get; set; }
		public double Pending { get; set; }

		public string EmploymentType { get; set; }
		public string FullName { get; set; }
		public string LeaveType { get; set; }
		public string Manager { get; set; }
		public string PayrollRefNo { get; set; }

		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }

		#endregion PROPERTIES
	}
}