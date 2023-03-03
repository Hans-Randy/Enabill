using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class TicketTimeAllocationReportModel
	{
		#region INITIALIZATION

		public TicketTimeAllocationReportModel(DateTime dateFrom, DateTime dateTo)
		{
			this.TicketTimeAllocationReportDateFrom = InputHistory.GetDateTime(HistoryItemType.TicketTimeAllocationReportDateFrom, DateTime.Today.ToFirstDayOfMonth()).Value;
			this.TicketTimeAllocationReportDateTo = InputHistory.GetDateTime(HistoryItemType.TicketTimeAllocationReportDateTo, DateTime.Today.ToLastDayOfMonth()).Value;
			this.TicketTimeAllocationReport = this.LoadTicketTimeAllocationReportModel(dateFrom, dateTo);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public DateTime TicketTimeAllocationReportDateFrom { get; private set; }
		public DateTime TicketTimeAllocationReportDateTo { get; private set; }

		public List<TicketTimeAllocation> TicketTimeAllocationReport { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<TicketTimeAllocation> LoadTicketTimeAllocationReportModel(DateTime dateFrom, DateTime dateTo)
		{
			var model = TicketTimeAllocationRepo.GetAll(dateFrom, dateTo).ToList();

			return model;
		}

		#endregion FUNCTIONS
	}
}