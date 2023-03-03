using System;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class TicketDetailModel
	{
		#region INITIALIZATION

		public TicketDetailModel(User user, DateTime dateFrom, DateTime dateTo, int clientID, int statusID, int ticketType = 0, int filterBy = 0)
		{
			this.DateFrom = InputHistory.Get(HistoryItemType.TicketDateFrom) == DateTime.MinValue ? dateFrom : InputHistory.Get(HistoryItemType.TicketDateFrom);
			this.DateTo = InputHistory.Get(HistoryItemType.TicketDateTo) == DateTime.MinValue ? dateTo : InputHistory.Get(HistoryItemType.TicketDateTo);
			this.TicketList = new TicketInstanceModel(user, clientID, statusID, dateFrom, dateTo, ticketType, filterBy);
			this.TicketLines = this.TicketList.Tickets.Count > 0 ? new TicketLineModel(this.TicketList.Tickets[0].TicketID) : new TicketLineModel(0);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public DateTime DateFrom { get; private set; }
		public DateTime DateTo { get; private set; }

		public TicketInstanceModel TicketList { get; private set; }
		public TicketLineModel TicketLines { get; private set; }

		#endregion PROPERTIES
	}
}