using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class ActivityReportModel
	{
		#region INITIALIZATION

		public ActivityReportModel(DateTime dateFrom, DateTime dateTo, int clientID, string projectName, string activityName, string employmentType)
		{
			this.ActivityReportDateFrom = InputHistory.GetDateTime(HistoryItemType.ActivityReportDateFrom, DateTime.Today.ToFirstDayOfMonth()).Value;
			this.ActivityReportDateTo = InputHistory.GetDateTime(HistoryItemType.ActivityReportDateTo, DateTime.Today.ToLastDayOfMonth()).Value;
			this.TrainingReportDateFrom = InputHistory.GetDateTime(HistoryItemType.TrainingReportDateFrom, DateTime.Today.ToFirstDayOfMonth()).Value;
			this.TrainingReportDateTo = InputHistory.GetDateTime(HistoryItemType.TrainingReportDateTo, DateTime.Today.ToLastDayOfMonth()).Value;
			this.ActivityReport = this.LoadActivityReportModel(dateFrom, dateTo, clientID, projectName, activityName, employmentType);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public DateTime ActivityReportDateFrom { get; private set; }
		public DateTime ActivityReportDateTo { get; private set; }
		public DateTime TrainingReportDateFrom { get; private set; }
		public DateTime TrainingReportDateTo { get; private set; }

		public List<UserTimeSplitReportModel> ActivityReport { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<UserTimeSplitReportModel> LoadActivityReportModel(DateTime dateFrom, DateTime dateTo, int clientID, string projectName, string activityName, string employmentType)
		{
			List<UserTimeSplitReportModel> model;
			return UserTimeSplitRepo.GetAll(dateFrom, dateTo, clientID, projectName, activityName, employmentType).ToList();
		}

		#endregion FUNCTIONS
	}
}