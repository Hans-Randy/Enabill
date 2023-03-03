using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public abstract class ClientRepo : BaseRepo
	{
		#region CLIENT SPECIFIC

		public static Client GetByID(int clientID) => DB.Clients
					.SingleOrDefault(c => c.ClientID == clientID);

		public static Client GetByName(string clientName) => DB.Clients
					.SingleOrDefault(r => r.ClientName == clientName);

		public static Client GetClientByContactID(int contactID) => (from contact in DB.Contacts
																	 join client in DB.Clients on contact.ClientID equals client.ClientID
																	 where contact.ContactID == contactID
																	 select client
				   )
				   .SingleOrDefault();

		public static IEnumerable<Client> GetAll() => DB.Clients;

		public static IEnumerable<Client> GetAllActiveClients() => DB.Clients.AsQueryable()
					.Join(DB.Projects,
					c => c.ClientID,
					p => p.ClientID,
					(c, p) => new { c, p })
					.Where(
							pr => pr.c.IsActive
							|| pr.p.ConfirmedEndDate == null
							|| pr.p.ConfirmedEndDate > DateTime.Today
						  )
					.Select(pr => pr.c)
					.Distinct()
					.ToList();

		//public static IEnumerable<Client> GetAllActiveClientsForUser(User invoiceAdmin) => DB.Clients.AsQueryable()
		//			.Join(
		//				DB.Projects,
		//				client => client.ClientID,
		//				project => project.ClientID,
		//				(client, project) =>
		//					new
		//					{
		//						client = client,
		//						project = project
		//					}
		//			)
		//			.Join(
		//				DB.Invoices,
		//				temp0 => (int?)(temp0.project.ProjectID),
		//				invoice => invoice.ProjectID,
		//				(temp0, invoice) =>
		//					new
		//					{
		//						temp0 = temp0,
		//						invoice = invoice
		//					}
		//			)
		//			.Where(
		//				temp1 =>
		//				(
		//					temp1.temp0.client.IsActive
		//					&& temp1.temp0.project.InvoiceAdminID == invoiceAdmin.UserID
		//					&& (temp1.temp0.project.ConfirmedEndDate == null || temp1.temp0.project.ConfirmedEndDate > DateTime.Today || temp1.invoice.InvoiceStatusID < 8)
		//				)
		//			)
		//			.Select(temp1 => temp1.temp0.client)
		//			.Distinct()
		//			.ToList();

		public static IEnumerable<Client> GetAllActiveClientsForUser(User invoiceAdmin) => DB.Clients.AsQueryable()
			.Join(
				DB.Projects,
				client => client.ClientID,
				project => project.ClientID,
				(client, project) =>
					new
					{
						client = client,
						project = project
					}
			)
			.GroupJoin(
				DB.Invoices,
				temp0 => (int?)(temp0.project.ProjectID),
				invoice => invoice.ProjectID,
				(temp0, invoice) =>
					new
					{
						temp0 = temp0,
						invoice = invoice
					}
			)
			.SelectMany(
				temp1 => temp1.invoice.DefaultIfEmpty(),
				(temp1, i) =>
				new
				{
					temp0 = temp1.temp0,
					i = i
				}
			)
			.Where(
				temp1 =>
				(
					temp1.temp0.client.IsActive
					&& temp1.temp0.project.InvoiceAdminID == invoiceAdmin.UserID
					&& (temp1.temp0.project.ConfirmedEndDate == null || temp1.temp0.project.ConfirmedEndDate > DateTime.Today || temp1.i.InvoiceStatusID < 8)
				)
			)
			.Select(temp1 => temp1.temp0.client)
			.Distinct()
			.ToList();

		public static List<ClientViewData> GetViewDataActiveClients() => DB.Clients.Where(c => c.IsActive).Select(c => new ClientViewData { ClientID = c.ClientID, ClientName = c.ClientName }).OrderBy(c => c.ClientName).ToList();

		public static List<ClientViewData> GetViewDataActiveClientsForUser(int userID) => (from u in DB.Users
																						   join ua in DB.UserAllocations on u.UserID equals ua.UserID
																						   join a in DB.Activities on ua.ActivityID equals a.ActivityID
																						   join p in DB.Projects on a.ProjectID equals p.ProjectID
																						   join c in DB.Clients on p.ClientID equals c.ClientID
																						   where
																							   c.IsActive
																							   && u.UserID == userID
																							   && (p.ConfirmedEndDate == null || p.ConfirmedEndDate > DateTime.Now)
																						   select new ClientViewData
																						   {
																							   ClientName = c.ClientName,
																							   ClientID = c.ClientID
																						   }).Distinct().ToList();

		public static List<Client> FilterByAll(string searchFilter, bool isActive)
		{
			if (string.IsNullOrEmpty(searchFilter))
				searchFilter = string.Empty;

			return DB.Clients
					.Where(u => u.IsActive == isActive && (u.ClientName.Contains(searchFilter)
						|| u.RegisteredName.Contains(searchFilter)))
					.OrderBy(c => c.ClientName)
					.ToList();
		}

		internal static void Save(Client client)
		{
			if (client.ClientID == 0)
				DB.Clients.Add(client);

			DB.SaveChanges();
		}

		#endregion CLIENT SPECIFIC

		#region CONTACTS

		internal static IEnumerable<Contact> GetContacts(int clientID) => DB.Contacts
					.Where(c => c.ClientID == clientID);

		internal static void SaveContact(Contact contact)
		{
			string passPhrase = Code.Constants.PASSPHRASE;

			contact.ContactName = (contact.ContactName == null) ? null : Helpers.EncryptString(contact.ContactName, passPhrase);
			contact.Email = (contact.Email == null) ? null : Helpers.EncryptString(contact.Email, passPhrase);
			contact.CellphoneNo = (contact.CellphoneNo == null) ? null : Helpers.EncryptString(contact.CellphoneNo, passPhrase);
			contact.TelephoneNo = (contact.TelephoneNo == null) ? null : Helpers.EncryptString(contact.TelephoneNo, passPhrase);

			try
			{
				if (contact.ContactID == 0)
					DB.Contacts.Add(contact);
				DB.SaveChanges();
			}
			catch (Exception ex)
			{
				throw new EnabillDomainException("Could not save the contact: " + ex.Message);
			}
		}

		internal static void DeleteContact(Contact contact)
		{
			try
			{
				DB.Contacts.Remove(contact);
				DB.SaveChanges();
			}
			catch (Exception ex)
			{
				throw new EnabillDomainException("The contact could not be deleted: " + ex.Message);
			}
		}

		#endregion CONTACTS

		#region PROJECTS

		internal static IEnumerable<Project> GetProjects(int clientID) => DB.Projects
					.Where(p => p.ClientID == clientID);

		public static IEnumerable<Client> GetClientsWithSupportProjects() => (from c in DB.Clients
																			  join e in DB.SupportEmails on c.ClientID equals e.ClientID
																			  select c).Distinct();

		internal static IEnumerable<ProjectSelectModel> GetProjectsByBillingMethodType(int clientID, BillingMethodType billingMethodType, bool isUsedInAnInvoiceRule)
		{
			if (isUsedInAnInvoiceRule)
			{
				return from a in DB.Activities
					   join p in DB.Projects on a.ProjectID equals p.ProjectID
					   join c in DB.Clients on p.ClientID equals c.ClientID
					   join iag in DB.InvoiceRuleActivities on a.ActivityID equals iag.ActivityID into t_iag
					   where
						   p.BillingMethodID == (int)billingMethodType
						   && p.ClientID == clientID
					   from t in t_iag.DefaultIfEmpty()
					   where t.ActivityID != null
					   select new ProjectSelectModel
					   {
						   Project = p,
						   IsSelected = isUsedInAnInvoiceRule
					   };
			}
			else
			{
				IEnumerable<ProjectSelectModel> list = from a in DB.Activities
													   join p in DB.Projects on a.ProjectID equals p.ProjectID
													   join c in DB.Clients on p.ClientID equals c.ClientID
													   join iag in DB.InvoiceRuleActivities on a.ActivityID equals iag.ActivityID into t_iag
													   where
														   p.BillingMethodID == (int)billingMethodType
														   && p.ClientID == clientID
													   from t in t_iag.DefaultIfEmpty()
													   where t.ActivityID == null
													   select new ProjectSelectModel
													   {
														   Project = p,
														   IsSelected = isUsedInAnInvoiceRule
													   };

				return list.Distinct(new ProjectSelectModelComparer());
			}
		}

		#endregion PROJECTS

		#region ACTIVITIES

		internal static IEnumerable<ActivityDetail> GetActivitiesByBillingMethodType(int clientID, BillingMethodType billingMethodType)
		{
			var list = from a in DB.Activities
					   join p in DB.Projects on a.ProjectID equals p.ProjectID
					   join c in DB.Clients on p.ClientID equals c.ClientID
					   where
						   p.BillingMethodID == (int)billingMethodType
						   && p.ClientID == clientID
					   select new { activity = a, project = p, client = c };

			return list.Select(m => new ActivityDetail()
			{
				ActivityID = m.activity.ActivityID,
				ActivityName = m.activity.ActivityName,
				ClientID = m.client.ClientID,
				ClientName = m.client.ClientName,
				ProjectID = m.project.ProjectID,
				ProjectName = m.project.ProjectName,
				CanHaveNotes = m.activity.CanHaveNotes,
				MustHaveRemarks = m.activity.MustHaveRemarks,
				IsSelected = false
			});
		}

		internal static IEnumerable<ActivityDetail> GetActivitiesByBMForInvRule(int clientID, int? invRuleID, BillingMethodType billingMethodType)
		{
			var list = from a in DB.Activities
					   join p in DB.Projects on a.ProjectID equals p.ProjectID
					   join c in DB.Clients on p.ClientID equals c.ClientID
					   join iag in DB.InvoiceRuleActivities on a.ActivityID equals iag.ActivityID into t_iag
					   from t in t_iag.Where(r => (invRuleID.HasValue && r.InvoiceRuleID == invRuleID.Value) || (!invRuleID.HasValue)).DefaultIfEmpty()
					   where ((t.ActivityID == null && !invRuleID.HasValue) || (t.ActivityID != null && invRuleID.HasValue))
						   && p.BillingMethodID == (int)billingMethodType
						   && p.ClientID == clientID
					   select new
					   {
						   activity = a,
						   project = p,
						   client = c
					   };

			return list.Select(m => new ActivityDetail()
			{
				ActivityID = m.activity.ActivityID,
				ActivityName = m.activity.ActivityName,
				ClientID = m.client.ClientID,
				ClientName = m.client.ClientName,
				ProjectID = m.project.ProjectID,
				ProjectName = m.project.ProjectName,
				CanHaveNotes = m.activity.CanHaveNotes,
				MustHaveRemarks = m.activity.MustHaveRemarks,
				IsSelected = invRuleID.HasValue
			});
		}

		#endregion ACTIVITIES

		#region INVOICES

		internal static IEnumerable<Invoice> GetInvoices(int clientID, DateTime dateFrom, DateTime dateTo, int billingMethodBWTotal, int statusBWTotal) => from i in DB.Invoices
																																						   where i.ClientID == clientID
																																							   && i.InvoiceDate >= dateFrom
																																							   && i.InvoiceDate <= dateTo
																																							   && (i.BillingMethodID & billingMethodBWTotal) > 0
																																							   && (i.InvoiceStatusID & statusBWTotal) > 0
																																						   select i;

		internal static IEnumerable<Invoice> GetInvoicesForUser(int clientID, DateTime dateFrom, DateTime dateTo, int billingMethodBWTotal, int statusBWTotal, int userRequestingID) => (from u in DB.Users
																																														 join p in DB.Projects on u.UserID equals p.InvoiceAdminID
																																														 join c in DB.Clients on p.ClientID equals c.ClientID
																																														 join i in DB.Invoices on c.ClientID equals i.ClientID
																																														 where p.InvoiceAdminID == userRequestingID
																																															 && i.ClientID == clientID
																																															 && i.InvoiceDate >= dateFrom
																																															 && i.InvoiceDate <= dateTo
																																															 && (i.BillingMethodID & billingMethodBWTotal) > 0
																																															 && (i.InvoiceStatusID & statusBWTotal) > 0
																																															 && (p.ConfirmedEndDate == null || p.ConfirmedEndDate > dateFrom)
																																														 select i).Distinct();

		internal static IEnumerable<Invoice> GetInvoices(int clientID, int invoicePeriod, int billingMethodBWTotal, int statusBWTotal) => from i in DB.Invoices
																																		  where i.ClientID == clientID
																																			  && i.Period == invoicePeriod
																																			  && (i.BillingMethodID & billingMethodBWTotal) > 0
																																			  && (i.InvoiceStatusID & statusBWTotal) > 0
																																		  select i;

		internal static IEnumerable<Invoice> GetInvoicesForUser(int clientID, int invoicePeriod, DateTime firstDayOfPeriod, int billingMethodBWTotal, int statusBWTotal, int userRequestingID) => (from u in DB.Users
																																																   join p in DB.Projects on u.UserID equals p.InvoiceAdminID
																																																   join c in DB.Clients on p.ClientID equals c.ClientID
																																																   join i in DB.Invoices on c.ClientID equals i.ClientID
																																																   where p.InvoiceAdminID == userRequestingID
																																																	   && i.ClientID == clientID
																																																	   && i.Period == invoicePeriod
																																																	   && (i.BillingMethodID & billingMethodBWTotal) > 0
																																																	   && (i.InvoiceStatusID & statusBWTotal) > 0
																																																	   && (p.ConfirmedEndDate == null || p.ConfirmedEndDate > firstDayOfPeriod)
																																																   select i).Distinct();

		#endregion INVOICES

		#region INVOICE RULES

		public static Client GetClientByIDHasInvoiceRule(int clientID) => (from ir in DB.InvoiceRules
																		   join c in DB.Clients on ir.ClientID equals c.ClientID
																		   where ir.ClientID == clientID
																		   select c
																		  ).Distinct()
																		   .SingleOrDefault();

		public static IEnumerable<Client> GetAllWithInvoiceRules(bool status)
		{
			var today = DateTime.Today;

			if (status)
			{
				return (from ir in DB.InvoiceRules
						join c in DB.Clients on ir.ClientID equals c.ClientID
						where ir.DateTo > today || ir.DateTo == null
						select c
					   ).Distinct();
			}
			else
			{
				return (from ir in DB.InvoiceRules
						join c in DB.Clients on ir.ClientID equals c.ClientID
						where ir.DateTo <= today && ir.DateTo != null
						select c
					  ).Distinct();
			}
		}

		#endregion INVOICE RULES

		#region JSON LOOKUPS

		public static List<JsonLookup> AutoComplete(string partialClient, int topCount) => (from c in DB.Clients
																							where c.ClientName.StartsWith(partialClient)
																							select new JsonLookup
																							{
																								id = c.ClientID,
																								label = c.ClientName,
																								value = c.ClientName
																							}).Distinct()
																							  .OrderBy(c => c.label)
																							  .Take(topCount)
																							  .ToList();

		#endregion JSON LOOKUPS
	}
}