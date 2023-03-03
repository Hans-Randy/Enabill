using Enabill.Models;

namespace Enabill.Web.Models
{
	public class ActivitySearchModel
	{
		#region INITIALIZATION

		public ActivitySearchModel(Project project, Activity activity)
		{
			this.ClientID = project.ClientID;
			this.ClientName = project.GetClient().ClientName;
			this.ProjectID = project.ProjectID;
			this.ProjectName = project.ProjectName;
			this.ActivityID = activity.ActivityID;
			this.ActivityName = activity.ActivityName;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool IsSelected { get; internal set; }

		public int ActivityID { get; private set; }
		public int ClientID { get; internal set; }
		public int ProjectID { get; private set; }

		public string ActivityName { get; private set; }
		public string ClientName { get; private set; }
		public string ProjectName { get; private set; }

		#endregion PROPERTIES
	}
}