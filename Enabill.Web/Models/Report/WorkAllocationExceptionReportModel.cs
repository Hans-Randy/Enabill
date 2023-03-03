using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class WorkAllocationExceptionReportModel
	{
		#region INITIALIZATION

		public WorkAllocationExceptionReportModel(DateTime dateFrom, DateTime dateTo, int divisionID)
		{
			this.WorkAllocationExceptionDateFrom = InputHistory.GetDateTime(HistoryItemType.WorkAllocationExceptionDateFrom, DateTime.Today.AddDays(-7)).Value;
			this.WorkAllocationExceptionDateTo = InputHistory.GetDateTime(HistoryItemType.WorkAllocationExceptionDateTo, DateTime.Today).Value;
			this.WorkAllocationExceptionReport = divisionID != 0 ? UserRepo.GetDivisionTimeCaptureExceptions(dateFrom, dateTo, divisionID).ToList() : UserRepo.GetAllTimeCaptureExceptions(dateFrom, dateTo).ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public DateTime WorkAllocationExceptionDateFrom { get; set; }
		public DateTime WorkAllocationExceptionDateTo { get; set; }

		public List<WorkAllocationExceptionModel> WorkAllocationExceptionReport { get; set; }

		#endregion PROPERTIES
	}
}