using System;
using Enabill.Repos;

namespace Enabill.Models
{
	/// <summary>
	/// Represents the current Activity for a user with it's charge rate.
	/// </summary>
	public class UserActivity
	{
		#region PROPERTIES

		public bool IsConfirmed { get; set; }

		public int ActivityID { get; set; }
		public int ClientID { get; set; }
		public int ProjectID { get; set; }
		public int UserAllocationID { get; internal set; }

		public double ChargeRate { get; set; }

		public string ActivityName { get; set; }
		public string ClientName { get; set; }
		public string ProjectName { get; set; }

		public DateTime StartDate { get; set; }
		public DateTime? ConfirmedEndDate { get; set; }
		public DateTime? ScheduledEndDate { get; set; }

		#endregion PROPERTIES

		#region ACTIVITY

		public Activity GetActivity() => ActivityRepo.GetByID(this.ActivityID);

		#endregion ACTIVITY
	}
}