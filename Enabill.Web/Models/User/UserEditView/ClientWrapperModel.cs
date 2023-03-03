using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class ClientWrapperModel
	{
		#region INITIALIZATION

		public ClientWrapperModel(Client client, User user)
		{
			this.ClientID = client.ClientID;
			this.ClientName = client.ClientName;
			this.ProjectList = this.LoadProjectList(client, user);
			this.MaxNumberOfAllocations = this.LoadMaxNumberOfAllocations(this.ProjectList);
			this.MaxNumberOfActivities = this.LoadMaxNumberOfActivities(this.ProjectList);
			this.MaxNumberOfProjects = this.ProjectList.Count;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int ClientID { get; internal set; }
		public int MaxNumberOfActivities { get; internal set; }
		public int MaxNumberOfAllocations { get; internal set; }
		public int MaxNumberOfProjects { get; internal set; }

		public string ClientName { get; internal set; }

		public List<ProjectWrapperModel> ProjectList { get; internal set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<ProjectWrapperModel> LoadProjectList(Client client, User user)
		{
			var model = new List<ProjectWrapperModel>();

			client.GetProjects().ForEach(project => model.Add(new ProjectWrapperModel(client, project, user)));

			return model;
		}

		private int LoadMaxNumberOfAllocations(List<ProjectWrapperModel> projectList)
		{
			int returnValue = 0;

			projectList.ForEach(p => returnValue += p.MaxNumberOfAllocations);

			return returnValue;
		}

		private int LoadMaxNumberOfActivities(List<ProjectWrapperModel> projectList)
		{
			int returnValue = 0;

			projectList.ForEach(p => returnValue += p.MaxNumberOfActivities);

			return returnValue;
		}

		#endregion FUNCTIONS
	}
}