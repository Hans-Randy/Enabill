using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models.Dto
{
	public class ProjectManagerActivityReportModel
	{
		#region INITIALIZATION

		public ProjectManagerActivityReportModel(DateTime dateFrom, DateTime dateTo, int projectManagerID)
		{
			this.ProjectActivityReport = this.LoadProjectActivityReportModel(dateFrom, dateTo, projectManagerID);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<ProjectActivityTimeReportModel> ProjectActivityReport { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<ProjectActivityTimeReportModel> LoadProjectActivityReportModel(DateTime dateFrom, DateTime dateTo, int projectManagerID) => WorkAllocationRepo.GetProjectActivityTimeForProjectManager(dateFrom, dateTo, projectManagerID).ToList();

		#endregion FUNCTIONS
	}
}