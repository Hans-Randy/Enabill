using System;

namespace Enabill.Models.Dto
{
	public class EncentivizeModel
	{
		#region PROPERTIES

		public int PointsAwarded { get; set; }

		public string Attended { get; set; }
		public string EmailAddress { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }

		#endregion PROPERTIES
	}

	public class EncentivizeModelExtended
	{
		#region PROPERTIES

		public int UserID { get; set; }

		public double HoursDiff { get; set; }
		public double HoursWorkedAllocationsWithLeave { get; set; }
		public double HoursWorkedWorkSession { get; set; }

		public string EmailAddress { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }

		public DateTime DayWorked { get; set; }

		#endregion PROPERTIES
	}
}