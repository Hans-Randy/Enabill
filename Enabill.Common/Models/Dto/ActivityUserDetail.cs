using System;

namespace Enabill.Models.Dto
{
	/// <summary>
	/// Provide a detail class for the User's allocation to an Activity.
	/// </summary>
	public class ActivityUserDetail
	{
		#region PROPERTIES

		public int ActivityID { get; set; }
		public int UserAllocationID { get; set; }
		public int UserID { get; set; }

		public double ChargeRate { get; set; }

		public string ActivityName { get; set; }
		public string UserFullName { get; set; }

		public DateTime StartDate { get; set; }
		public DateTime? ConfirmedEndDate { get; set; }
		public DateTime? ScheduledEndDate { get; set; }

		#endregion PROPERTIES
	}
}