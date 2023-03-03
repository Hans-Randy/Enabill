using System;

namespace Enabill.Models.Dto
{
	public class MISAllocatedTimeModel
	{
		#region PROPERTIES

		public double CostToCompany { get; set; }
		public double HoursWorked { get; set; }

		public string BillingMethod { get; set; }
		public string Client { get; set; }
		public string Department { get; set; }
		public string Project { get; set; }
		public string Region { get; set; }
		public string Resource { get; set; }

		public DateTime DayWorked { get; set; }

		#endregion PROPERTIES
	}
}