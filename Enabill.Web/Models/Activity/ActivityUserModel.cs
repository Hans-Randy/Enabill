using System.Collections.Generic;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Web.Models
{
	public class ActivityUserModel
	{
		#region INITIALIZATION

		public ActivityUserModel(Activity activity)
		{
			this.Activity = activity;
			this.UsersAssignedToActivity = activity.GetUsersAssigned();
			this.PastUsersAssignedToActivity = activity.GetPastUsersAssigned();
			this.FutureUsersAssignedToActivity = activity.GetFutureUsersAssigned();
			this.SetUpModel();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int RequiredRows { get; private set; }

		public Activity Activity;

		public List<ActivityUserDetail> FutureUsersAssignedToActivity;
		public List<ActivityUserDetail> PastUsersAssignedToActivity;
		public List<ActivityUserDetail> UsersAssignedToActivity;

		#endregion PROPERTIES

		#region FUNCTIONS

		private void SetUpModel()
		{
			int current = this.UsersAssignedToActivity.Count;
			int past = this.PastUsersAssignedToActivity.Count;
			int future = this.FutureUsersAssignedToActivity.Count;

			this.RequiredRows = current;

			if (this.RequiredRows < past)
				this.RequiredRows = past;

			if (this.RequiredRows < future)
				this.RequiredRows = future;

			if (this.RequiredRows == 0)
				this.RequiredRows = 1;
		}

		#endregion FUNCTIONS
	}
}