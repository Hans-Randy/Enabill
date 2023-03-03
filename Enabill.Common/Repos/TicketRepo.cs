using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public class TicketRepo : BaseRepo
	{
		internal static void Save(Ticket ticket)
		{
			if (ticket.TicketID <= 0)
				DB.Tickets.Add(ticket);

			DB.SaveChanges();
		}

		public static DateTime GetFirstOpenTicketDate()
		{
			var openTickets = from t in DB.Tickets
							  where t.TicketStatus != (int)TicketStatusEnum.Closed && !t.IsDeleted
							  orderby t.DateCreated
							  select t;

			if (openTickets != null)
				return openTickets.Min(i => i.DateCreated);
			else
				return DateTime.Today.AddDays(-14);
		}

		public static Ticket GetByID(int ticketID) => DB.Tickets.Where(i => i.TicketID == ticketID).SingleOrDefault();

		public static IEnumerable<string> GetAssociatedProjectTickets(int clientID, int projectID) => DB.Tickets.Where(t => t.ClientID == clientID && t.ProjectID == projectID && !t.IsDeleted).Select(t => t.TicketReference);

		public static Ticket GetByReference(string reference) => DB.Tickets.Where(i => i.TicketReference == reference).SingleOrDefault();

		public static List<TicketClientStatusTotalModel> GetTicketByClientAndStatusTotals(User user, DateTime dateFrom, DateTime dateTo, int ticketType, int filterBy)
		{
			var projects = user.HasRole(UserRoleType.SystemAdministrator) || user.HasRole(UserRoleType.TicketManager) ? ProjectRepo.GetAllProjectIDs() : user.HasRole(UserRoleType.ProjectOwner) ? UserRepo.GetProjectIDsLinkedToProjectOwner(user.UserID) : UserRepo.GetProjectIDsLinkedToUser(user.UserID);
			dateTo = dateTo.AddDays(1);

			if (ticketType != 0)
			{
				if (filterBy == 0)
				{
					return (from t in DB.Tickets
							join s in DB.TicketStatus on t.TicketStatus equals s.TicketStatusID
							join c in DB.Clients on t.ClientID equals c.ClientID
							where t.DateCreated >= dateFrom
								  && t.DateCreated <= dateTo
								  && t.TicketType == ticketType
								  && !t.IsDeleted
								  && projects.Contains(t.ProjectID)
							group t by new { c.ClientID, c.ClientName, s.TicketStatusID, s.TicketStatusName } into tGroup
							orderby tGroup.Key.ClientID, tGroup.Key.ClientName, tGroup.Key.TicketStatusID, tGroup.Key.TicketStatusName

							select new TicketClientStatusTotalModel()
							{
								ClientID = tGroup.Key.ClientID,
								ClientName = tGroup.Key.ClientName,
								TicketStatusID = tGroup.Key.TicketStatusID,
								TicketStatusName = tGroup.Key.TicketStatusName,
								NumberOfTickets = tGroup.Select(t => t.TicketID).Count()
							}
							   ).ToList();
				}
				else
				{
					return (from t in DB.Tickets
							join s in DB.TicketStatus on t.TicketStatus equals s.TicketStatusID
							join c in DB.Clients on t.ClientID equals c.ClientID
							where t.DateCreated >= dateFrom
								  && t.DateCreated <= dateTo
								  && t.TicketType == ticketType
								  && t.UserAssigned == user.UserID
								  && !t.IsDeleted
							group t by new { c.ClientID, c.ClientName, s.TicketStatusID, s.TicketStatusName } into tGroup
							orderby tGroup.Key.ClientID, tGroup.Key.ClientName, tGroup.Key.TicketStatusID, tGroup.Key.TicketStatusName

							select new TicketClientStatusTotalModel()
							{
								ClientID = tGroup.Key.ClientID,
								ClientName = tGroup.Key.ClientName,
								TicketStatusID = tGroup.Key.TicketStatusID,
								TicketStatusName = tGroup.Key.TicketStatusName,
								NumberOfTickets = tGroup.Select(t => t.TicketID).Count()
							}
							  ).ToList();
				}
			}
			else if (filterBy == 0)
			{
				return (from t in DB.Tickets
						join s in DB.TicketStatus on t.TicketStatus equals s.TicketStatusID
						join c in DB.Clients on t.ClientID equals c.ClientID
						where t.DateCreated >= dateFrom
							  && t.DateCreated <= dateTo
							  && !t.IsDeleted
							  && projects.Contains(t.ProjectID)
						group t by new { c.ClientID, c.ClientName, s.TicketStatusID, s.TicketStatusName } into tGroup
						orderby tGroup.Key.ClientID, tGroup.Key.ClientName, tGroup.Key.TicketStatusID, tGroup.Key.TicketStatusName

						select new TicketClientStatusTotalModel()
						{
							ClientID = tGroup.Key.ClientID,
							ClientName = tGroup.Key.ClientName,
							TicketStatusID = tGroup.Key.TicketStatusID,
							TicketStatusName = tGroup.Key.TicketStatusName,
							NumberOfTickets = tGroup.Select(t => t.TicketID).Count()
						}
						).ToList();
			}
			else
			{
				return (from t in DB.Tickets
						join s in DB.TicketStatus on t.TicketStatus equals s.TicketStatusID
						join c in DB.Clients on t.ClientID equals c.ClientID
						where t.DateCreated >= dateFrom
							  && t.DateCreated <= dateTo
							  && t.UserAssigned == user.UserID
							  && !t.IsDeleted
						group t by new { c.ClientID, c.ClientName, s.TicketStatusID, s.TicketStatusName } into tGroup
						orderby tGroup.Key.ClientID, tGroup.Key.ClientName, tGroup.Key.TicketStatusID, tGroup.Key.TicketStatusName

						select new TicketClientStatusTotalModel()
						{
							ClientID = tGroup.Key.ClientID,
							ClientName = tGroup.Key.ClientName,
							TicketStatusID = tGroup.Key.TicketStatusID,
							TicketStatusName = tGroup.Key.TicketStatusName,
							NumberOfTickets = tGroup.Select(t => t.TicketID).Count()
						}
									   ).ToList();
			}
		}

		public static IEnumerable<Ticket> GetUnassginedTickets()
		{
			const int ticketStatusID = (int)TicketStatusEnum.Closed;

			return (from t in DB.Tickets
					where t.TicketStatus != ticketStatusID && t.UserAssigned == null
								  && !t.IsDeleted
					select t).ToList();
		}

		public static IEnumerable<Ticket> GetByClientAndStatus(User user, int clientID, int statusID, DateTime dateFrom, DateTime dateTo, int ticketType, int filterBy)
		{
			var projects = user.HasRole(UserRoleType.SystemAdministrator) || user.HasRole(UserRoleType.TicketManager) ? ProjectRepo.GetAllProjectIDs() : user.HasRole(UserRoleType.ProjectOwner) ? UserRepo.GetProjectIDsLinkedToProjectOwner(user.UserID) : UserRepo.GetProjectIDsLinkedToUser(user.UserID);
			dateTo = dateTo.AddDays(1);

			if (ticketType != 0)
			{
				if (filterBy == 0)
				{
					return from t in DB.Tickets
						   join s in DB.TicketStatus on t.TicketStatus equals s.TicketStatusID
						   join c in DB.Clients on t.ClientID equals c.ClientID
						   where c.ClientID == clientID && t.TicketStatus == statusID
								 && t.TicketType == ticketType
								 && t.DateCreated >= dateFrom
								 && t.DateCreated <= dateTo
								 && !t.IsDeleted
								 && projects.Contains(t.ProjectID)
						   select t;
				}
				else
				{
					return from t in DB.Tickets
						   join s in DB.TicketStatus on t.TicketStatus equals s.TicketStatusID
						   join c in DB.Clients on t.ClientID equals c.ClientID
						   where c.ClientID == clientID && t.TicketStatus == statusID
								 && t.TicketType == ticketType
								 && t.DateCreated >= dateFrom
								 && t.DateCreated <= dateTo
								 && t.UserAssigned == user.UserID
								 && !t.IsDeleted
						   select t;
				}
			}
			else if (filterBy == 0)
			{
				return from t in DB.Tickets
					   join s in DB.TicketStatus on t.TicketStatus equals s.TicketStatusID
					   join c in DB.Clients on t.ClientID equals c.ClientID
					   where c.ClientID == clientID && t.TicketStatus == statusID
							 && t.DateCreated >= dateFrom
							 && t.DateCreated <= dateTo
							 && !t.IsDeleted
							 && projects.Contains(t.ProjectID)
					   select t;
			}
			else
			{
				return from t in DB.Tickets
					   join s in DB.TicketStatus on t.TicketStatus equals s.TicketStatusID
					   join c in DB.Clients on t.ClientID equals c.ClientID
					   where c.ClientID == clientID && t.TicketStatus == statusID
							 && t.DateCreated >= dateFrom
							 && t.DateCreated <= dateTo
							 && t.UserAssigned == user.UserID
							 && !t.IsDeleted
					   select t;
			}
		}
	}
}