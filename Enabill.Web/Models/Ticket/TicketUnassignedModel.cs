using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class TicketUnassignedModel
	{
		#region INITIALIZATION

		public TicketUnassignedModel()
		{
			this.UnassignedTickets = TicketRepo.GetUnassginedTickets().ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<Ticket> UnassignedTickets { get; internal set; }

		#endregion PROPERTIES
	}
}