using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class ProjectActivitiesModel
	{
		#region INITIALIZATION

		public ProjectActivitiesModel(Project project, bool isActive)
		{
			this.Project = project;
			this.ProjectActivities = new List<ActivityListModel>();
			this.LoadProjectActivities(project, isActive);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public Project Project { get; private set; }

		public List<ActivityListModel> ProjectActivities { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private void LoadProjectActivities(Project project, bool isActive) => project.GetActivities(isActive)
				.ForEach(a => this.ProjectActivities.Add(new ActivityListModel(a)));

		#endregion FUNCTIONS
	}
}