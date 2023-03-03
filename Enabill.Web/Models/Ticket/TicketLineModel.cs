using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class TicketLineModel
	{
		#region INITIALIZATION

		public TicketLineModel(int ticketID)
		{
			this.Ticket = ticketID == 0 ? null : TicketRepo.GetByID(ticketID);
			this.TicketLines = TicketLineRepo.GetLinesForTicket(ticketID).ToList();
			this.TicketLineAttachments = TicketLineAttachmentRepo.GetTicketAttachments(ticketID);
			this.ClientName = ticketID != 0 ? ClientRepo.GetByID(this.Ticket.ClientID).ClientName : "";
			this.ProjectName = ticketID != 0 && this.Ticket.ProjectID != 0 ? ProjectRepo.GetByID(this.Ticket.ProjectID).ProjectName : "";
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public string ClientName { get; internal set; }
		public string ProjectName { get; internal set; }

		public Ticket Ticket { get; internal set; }

		public List<TicketLine> TicketLines { get; internal set; }
		public List<TicketLineAttachment> TicketLineAttachments { get; internal set; }

		#endregion PROPERTIES
	}
}