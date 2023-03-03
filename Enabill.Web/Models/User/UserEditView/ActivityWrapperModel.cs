using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class ActivityWrapperModel
	{
		#region INITIALIZATION

		public ActivityWrapperModel(Client client, Project project, Activity activity, User user)
		{
			this.ClientID = client.ClientID;
			this.ClientName = client.ClientName;
			this.ProjectID = project.ProjectID;
			this.ProjectName = project.ProjectName;
			this.ActivityID = activity.ActivityID;
			this.ActivityName = activity.ActivityName;
			this.RegionID = activity.RegionID;
			this.RegionName = RegionRepo.GetByID(activity.RegionID).RegionName;
			this.DepartmentID = activity.DepartmentID;
			this.DepartmentName = DepartmentRepo.GetByID(activity.DepartmentID).DepartmentName;

			this.CurrentAllocations = this.LoadCurrentAllocations(activity, user, DateTime.Today);
			this.MaxNumberOfCurrentAllocations = this.CurrentAllocations.Count;

			this.PastAllocations = this.LoadPastAllocations(activity, user, DateTime.Today);
			this.MaxNumberOfPastAllocations = this.CurrentAllocations.Count;

			this.FutureAllocations = this.LoadFutureAllocations(activity, user, DateTime.Today);
			this.MaxNumberOfFutureAllocations = this.CurrentAllocations.Count;

			this.MaxNumberOfAllocations = this.SetMaxNumberOfAllocations(this.MaxNumberOfCurrentAllocations, this.MaxNumberOfPastAllocations, this.MaxNumberOfFutureAllocations);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int ActivityID { get; internal set; }
		public int ClientID { get; internal set; }
		public int DepartmentID { get; internal set; }
		public int ProjectID { get; internal set; }
		public int RegionID { get; internal set; }
		public int MaxNumberOfAllocations { get; set; }
		public int MaxNumberOfCurrentAllocations { get; internal set; }
		public int MaxNumberOfFutureAllocations { get; internal set; }
		public int MaxNumberOfPastAllocations { get; internal set; }

		public string ActivityName { get; internal set; }
		public string ClientName { get; internal set; }
		public string DepartmentName { get; internal set; }
		public string ProjectName { get; internal set; }
		public string RegionName { get; internal set; }

		public List<UserActivity> CurrentAllocations { get; internal set; }
		public List<UserActivity> FutureAllocations { get; internal set; }
		public List<UserActivity> PastAllocations { get; internal set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<UserActivity> LoadCurrentAllocations(Activity activity, User user, DateTime refDate)
		{
			var model = new List<UserActivity>();
			model.AddRange(user.GetActivities().Where(a => a.ActivityID == activity.ActivityID).ToList());

			return model;
		}

		private List<UserActivity> LoadFutureAllocations(Activity activity, User user, DateTime refDate)
		{
			var model = new List<UserActivity>();
			model.AddRange(user.GetFutureActivities(refDate).Where(a => a.ActivityID == activity.ActivityID).ToList());

			return model;
		}

		private List<UserActivity> LoadPastAllocations(Activity activity, User user, DateTime refDate)
		{
			var model = new List<UserActivity>();
			model.AddRange(user.GetPastActivities(refDate).Where(a => a.ActivityID == activity.ActivityID).ToList());

			return model;
		}

		private int SetMaxNumberOfAllocations(int currentAllocations, int pastAllocations, int futureAllocations)
		{
			int max = currentAllocations;

			if (pastAllocations > max)
				max = pastAllocations;
			if (futureAllocations > max)
				max = futureAllocations;

			return max;
		}

		#endregion FUNCTIONS
	}
}