using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("vwUserTimeSplit")]
	public class UserTimeSplit
	{
		#region PROPERTIES

		[Key]
		public int WorkAllocationID { get; set; }

		public int ActivityID { get; set; }
		public int ClientID { get; set; }
		public int DivisionID { get; set; }
		public int ProjectID { get; set; }
		public int UserID { get; set; }

		public double HoursWorked { get; set; }

		public string ActivityName { get; set; }
		public string ClientName { get; set; }
		public string DepartmentName { get; set; }
		public string DivisionName { get; set; }
		public string FullName { get; set; }
		public string Period { get; set; }
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