using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class TicketInstanceModel
	{
		#region INITIALIZATION

		public TicketInstanceModel(User user, int clientID, int statusID, DateTime dateFrom, DateTime dateTo, int ticketType, int filterBy)
		{
			this.Client = clientID == 0 ? "" : ClientRepo.GetByID(clientID).ClientName;
			this.TicketStatus = statusID == 0 ? "" : TicketStatusRepo.GetByID(statusID).TicketStatusName;
			this.Tickets = TicketRepo.GetByClientAndStatus(user, clientID, statusID, dateFrom, dateTo, ticketType, filterBy).ToList();
			this.FirstTicketID = this.Tickets.Count > 0 ? this.Tickets[0].TicketID : 0;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int FirstTicketID { get; internal set; }

		public string Client { get; internal set; }
		public string TicketStatus { get; internal set; }

		public List<Ticket> Tickets { get; internal set; }

		#endregion PROPERTIES
	}
}