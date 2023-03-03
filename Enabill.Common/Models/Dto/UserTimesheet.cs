using System;

namespace Enabill.Models.Dto
{
	public class UserTimesheet
	{
		#region PROPERTIES

		public int Id { get; set; }
		public int UserID { get; set; }

		public DateTime DateCreated { get; set; }
		public DateTime DateUpdated { get; set; }
		public DateTime TimesheetEndDate { get; set; }
		public DateTime TimesheetStartDate { get; set; }

		public TimesheetSchedule Schedule { get; set; }

		#endregion PROPERTIES
	}
}