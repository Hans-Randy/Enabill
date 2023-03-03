using System;

namespace Enabill.Models.Dto
{
	public class UserLeaveGeneralModel
	{
		#region PROPERTIES

		public bool IsPartial { get; set; }

		public int ApprovalStatusID { get; set; }
		public int EmployeeID { get; set; }
		public int EmploymentTypeID { get; set; }
		public int LeaveTypeID { get; set; }
		public int ManagerID { get; set; }

		public double NumberOfDays { get; set; }

		public string ApprovalStatus { get; set; }
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