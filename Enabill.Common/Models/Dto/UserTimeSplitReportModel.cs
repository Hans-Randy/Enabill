using System;

namespace Enabill.Models.Dto
{
	public class UserTimeSplitReportModel
	{
		#region PROPERTIES

		public int ActivityID { get; set; }
		public int ClientID { get; set; }
		public int DivisionID { get; set; }
		public int EmploymentTypeID { get; set; }
		public int ProjectID { get; set; }
		public int ManagerID { get; set; }
		public int PercentageAllocation { get; set; }
		public int UserID { get; set; }
		public int WorkAllocationID { get; set; }

		public double Capacity { get; set; }

		public double HoursWorked { get; set; }
		public double TotalHours { get; set; }
		public double TotalHoursWorkable { get; set; }

		public string ActivityName { get; set; }
		public string ClientName { get; set; }
		public string DepartmentName { get; set; }
		public string DivisionName { get; set; }
		public string FullName { get; set; }
		public string FullNameManager { get; set; }
		public string EmploymentType { get; set; }
		public string Period { get; set; }
		public string ProjectName { get; set; }
		public string RegionName { get; set; }
		public string Remark { get; set; }
		public string TrainerName { get; set; }
		public string TrainingInstitute { get; set; }
		public string TrainingType { get; set; }

		public DateTime DayWorked { get; set; }

		#endregion PROPERTIES
	}
}