using System;

namespace Enabill.Models.Dto
{
	public class ActivityExtendedReportModel
	{
		#region PROPERTIES

		public double HoursAllocated { get; set; }

		public string Activity { get; set; }
		public string Client { get; set; }
		public string Department { get; set; }
		public string FullName { get; set; }
		public string Project { get; set; }
		public string Remarks { get; set; }
		public string TrainerName { get; set; }
		public string TrainingInstitute { get; set; }
		public string TrainingType { get; set; }
		public string UserName { get; set; }

		public DateTime Date { get; set; }

		#endregion PROPERTIES
	}
}