using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models.Dto
{
	public class ActivityReportPrintModel
	{
		#region INITIALIZATION

		public ActivityReportPrintModel(DateTime dateFrom, DateTime dateTo, int managerID = 0)
		{
			this.ActivityReport = this.LoadActivityReportPrintModel(dateFrom, dateTo, managerID);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<UserTimeSplitReportModel> ActivityReport { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<UserTimeSplitReportModel> LoadActivityReportPrintModel(DateTime dateFrom, DateTime dateTo, int managerID) => UserTimeSplitRepo.GetAll(dateFrom, dateTo, 0, managerID, 0, 0, 0, 0, 0).ToList();

		#endregion FUNCTIONS
	}
}