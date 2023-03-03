using System;

namespace Enabill.Models.Dto
{
	public class WorkAllocationExceptionModel
	{
		#region PROPERTIES

		public double AllocationDifference { get; set; }

		public string ExceptionDetail { get; set; }
		public string FullName { get; set; }
		public string UserName { get; set; }

		public DateTime EmploymentDate { get; set; }
		public DateTime WorkDate { get; set; }

		#endregion PROPERTIES
	}
}