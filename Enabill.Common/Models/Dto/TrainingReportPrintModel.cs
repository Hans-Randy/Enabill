using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models.Dto
{
	public class TrainingReportPrintModel
	{
		#region INITIALIZATION

		public TrainingReportPrintModel(DateTime dateFrom, DateTime dateTo, int clientID, string projectName, string activityName, string employmentType, string department = "0")
		{
			this.TrainingReport = this.LoadTrainingReportPrintModel(dateFrom, dateTo, clientID, projectName, activityName, employmentType, department);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<UserTimeSplitReportModel> TrainingReport { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<UserTimeSplitReportModel> LoadTrainingReportPrintModel(DateTime dateFrom, DateTime dateTo, int clientID, string projectName, string activityName, string employmentType, string department = "0") => UserTimeSplitRepo.GetAll(dateFrom, dateTo, clientID, projectName, activityName, employmentType, department).ToList();

		#endregion FUNCTIONS
	}
}