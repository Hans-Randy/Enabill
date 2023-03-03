using System;

namespace Enabill.Models
{
	public class UserWorkAllocation
	{
		#region PROPERTIES

		public int ActivityId { get; set; }
		public int ClientId { get; set; }
		public int DepartmentId { get; set; }
		public int DivisionId { get; set; }
		public int EmploymentTypeId { get; set; }
		public int ManagerId { get; set; }
		public int PercentageAllocation { get; set; }
		public int Period { get; set; }
		public int ProjectId { get; set; }
		public int RegionId { get; set; }
		public int UserId { get; set; }
		public int WorkAllocationId { get; set; }

		public double Capacity { get; set; }
		public double HoursWorked { get; set; }
		public double WorkHours { get; set; }
		public double TotalHours { get; set; }
		public double TotalHoursWorkable { get; set; }

		public string ActivityName { get; set; }
		public string ClientName { get; set; }
		public string DepartmentName { get; set; }
		public string DivisionName { get; set; }
		public string EmploymentType { get; set; }
		public string FullNameManager { get; set; }
		public string FullName { get; set; }
		public string ProjectName { get; set; }
		public string RegionName { get; set; }
		public string Remark { get; set; }
		public string TrainerName { get; set; }
		public string TrainingInstitute { get; set; }
		public string TrainingType { get; set; }
		public string UserName { get; set; }
		public DateTime DayWorked { get; set; }

		#endregion PROPERTIES
	}
}