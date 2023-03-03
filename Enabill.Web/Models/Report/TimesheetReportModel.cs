using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class TimesheetReportModel
	{
		#region INITIALIZATION

		public TimesheetReportModel(DateTime dateFrom, DateTime dateTo, User user)
		{
			this.TimesheetReportDateFrom = InputHistory.GetDateTime(HistoryItemType.TimesheetReportDateFrom, DateTime.Today.ToFirstDayOfMonth()).Value;
			this.TimesheetReportDateTo = InputHistory.GetDateTime(HistoryItemType.TimesheetReportDateTo, DateTime.Today.ToLastDayOfMonth()).Value;
			this.TimesheetReport = this.LoadTimesheetReportModel(dateFrom, dateTo, user);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public DateTime TimesheetReportDateFrom { get; private set; }
		public DateTime TimesheetReportDateTo { get; private set; }

		public List<UserTimeSplitReportModel> TimesheetReport { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<UserTimeSplitReportModel> LoadTimesheetReportModel(DateTime dateFrom, DateTime dateTo, User user)
		{
			//return a model for selected period and client
			var model = UserTimeSplitRepo.GetAll(dateFrom, dateTo, 0, "0", "0", "0", "0", user.UserID).ToList();

			return model;
		}

		#endregion FUNCTIONS
	}
}