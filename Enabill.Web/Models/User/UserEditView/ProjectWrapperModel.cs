using System.Collections.Generic;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class ProjectWrapperModel
	{
		#region INITIALIZATION

		public ProjectWrapperModel(Client client, Project project, User user)
		{
			this.ClientID = client.ClientID;
			this.ClientName = client.ClientName;
			this.ProjectID = project.ProjectID;
			this.ProjectName = project.ProjectName;
			this.ActivityList = this.LoadActivityList(client, project, user);
			this.MaxNumberOfActivities = this.ActivityList.Count;
			this.MaxNumberOfAllocations = this.LoadActivityListAllocationCount(this.ActivityList);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int ClientID { get; internal set; }
		public int ProjectID { get; internal set; }
		public int MaxNumberOfActivities { get; internal set; }
		public int MaxNumberOfAllocations { get; internal set; }

		public string ClientName { get; internal set; }
		public string ProjectName { get; internal set; }

		public List<ActivityWrapperModel> ActivityList { get; internal set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<ActivityWrapperModel> LoadActivityList(Client client, Project project, User user)
		{
			var model = new List<ActivityWrapperModel>();

			UserAllocationRepo.GetNonLinkedActivities(user.UserID, project.ProjectID).ForEach(activity => model.Add(new ActivityWrapperModel(client, project, activity, user)));

			return model;
		}

		private int LoadActivityListAllocationCount(List<ActivityWrapperModel> activityList)
		{
			int returnValue = 0;

			activityList.ForEach(a => returnValue += a.MaxNumberOfAllocations);

			return returnValue;
		}

		#endregion FUNCTIONS
	}
}