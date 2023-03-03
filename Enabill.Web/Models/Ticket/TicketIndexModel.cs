using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class TicketIndexModel
	{
		#region INITIALIZATION

		public TicketIndexModel(User user, DateTime dateFrom, DateTime dateTo, int ticketType = 0, int filterBy = 0)
		{
			this.DateFrom = InputHistory.GetDateTime(HistoryItemType.TicketDateFrom, dateFrom).Value;
			this.DateTo = InputHistory.GetDateTime(HistoryItemType.TicketDateTo, dateTo).Value;
			this.TicketStatusList = TicketStatusRepo.GetByAll().ToList();
			this.TicketClientSummaryList = TicketRepo.GetTicketByClientAndStatusTotals(user, this.DateFrom, this.DateTo, InputHistory.Get(HistoryItemType.TicketTypeFilter, ticketType), InputHistory.Get(HistoryItemType.TicketFilterBy, filterBy)).ToList();
			this.SelectedClientID = this.TicketClientSummaryList.Count > 0 ? this.TicketClientSummaryList[0].ClientID : 0;
			this.SelectedStatusID = this.TicketClientSummaryList.Count > 0 ? this.TicketClientSummaryList[0].TicketStatusID : 0;
			this.TicketList = new TicketInstanceModel(user, InputHistory.Get(HistoryItemType.ClientID, this.SelectedClientID) == 0 ? this.SelectedClientID : InputHistory.Get(HistoryItemType.ClientID, this.SelectedClientID), InputHistory.Get(HistoryItemType.StatusID, this.SelectedStatusID) == 0 ? this.SelectedStatusID : InputHistory.Get(HistoryItemType.StatusID, this.SelectedStatusID), dateFrom, dateTo, InputHistory.Get(HistoryItemType.TicketTypeFilter, ticketType), InputHistory.Get(HistoryItemType.TicketFilterBy, filterBy));
			this.UnassignedTicketList = new TicketUnassignedModel();
			this.TicketLines = this.TicketList.Tickets.Count > 0 ? new TicketLineModel(this.TicketList.Tickets[0].TicketID) : new TicketLineModel(0);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int SelectedClientID { get; private set; }
		public int SelectedStatusID { get; private set; }

		public DateTime DateFrom { get; private set; }
		public DateTime DateTo { get; private set; }

		public TicketInstanceModel TicketList { get; private set; }
		public TicketLineModel TicketLines { get; private set; }
		public TicketUnassignedModel UnassignedTicketList { get; private set; }

		public List<TicketClientStatusTotalModel> TicketClientSummaryList { get; private set; }
		public List<TicketStatus> TicketStatusList { get; private set; }

		#endregion PROPERTIES
	}
}