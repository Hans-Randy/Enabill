using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class ActivityListModel
	{
		#region INITIALIZATION

		public ActivityListModel(Activity activity)
		{
			this.Activity = activity;
			this.RegionName = RegionRepo.GetByID(activity.RegionID).RegionName;
			this.DepartmentName = DepartmentRepo.GetByID(activity.DepartmentID).DepartmentName;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public string DepartmentName { get; set; }
		public string RegionName { get; set; }

		public Activity Activity { get; set; }

		#endregion PROPERTIES
	}
}