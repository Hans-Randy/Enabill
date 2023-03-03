using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class ProjectActivityUsersModel
	{
		#region INITIALIZATION

		public ProjectActivityUsersModel(Project project)
		{
			this.Project = project;
			this.LoadActivities();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public Project Project { get; private set; }

		public List<ActivityUserModel> Activities { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private void LoadActivities()
		{
			this.Activities = new List<ActivityUserModel>();
			this.Project.GetActivities()
				.ForEach(a => this.Activities.Add(new ActivityUserModel(a)));
		}

		#endregion FUNCTIONS
	}
}