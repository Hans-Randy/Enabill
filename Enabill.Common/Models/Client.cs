using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("Clients")]
	public class Client
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ClientID { get; set; }

		[Required]
		public int CurrencyTypeID { get; set; }

		[Required]
		public bool IsActive { get; internal set; }

		// If true, means that Client has been Deactivated and once past the latest Confirmed End Date, the Client will be made Inactive
		[Required]
		public bool IsDeactivated { get; internal set; }

		//If the vat rate is not set, we will use the site default VAT rate
		//Will be saved as a value of 1,00 = 100% {ie 0.14 = 14%}
		public double? VATRate { get; set; }

		[MaxLength(50)]
		public string AccountCode { get; set; }

		[Required, MaxLength(100)]
		public string ClientName { get; set; }

		[Required, MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		[MaxLength(250)]
		public string PostalAddress1 { get; set; }

		[MaxLength(250)]
		public string PostalAddress2 { get; set; }

		[MaxLength(250)]
		public string PostalAddress3 { get; set; }

		[MaxLength(20)]
		public string PostalCode { get; set; }

		[MaxLength(100)]
		public string RegisteredName { get; set; }

		[MaxLength(128)]
		public string SupportEmailAddress { get; set; }

		[MaxLength(50)]
		public string VATNo { get; set; }

		public List<CurrencyType> GetAllCurrencyTypes() => CurrencyTypeRepo.GetAll()
			.OrderByDescending(c => c.CurrencyISO)
			.ToList();

		public CurrencyType GetCurrency(int currencyTypeID) => CurrencyTypeRepo.GetByID(currencyTypeID);

		#endregion PROPERTIES

		#region CLIENT
		public static Client GetClientByID(int clientID) => ClientRepo.GetByID(clientID);
					
		public static List<Client> GetAllActiveClients() => ClientRepo.GetAllActiveClients()
					.OrderBy(c => c.ClientName)
					.ToList();

		public static List<Client> GetAllActiveClientsForUser(User invoiceAdmin) => ClientRepo.GetAllActiveClientsForUser(invoiceAdmin)
					.OrderBy(c => c.ClientName)
					.ToList();

		public static Client GetNew(string clientName, bool isActive) => new Client
		{
			ClientName = clientName,
			IsActive = isActive,
			LastModifiedBy = "Sys"
		};

		public void Activate(User userActivating)
		{
			if (!userActivating.HasRole(UserRoleType.SystemAdministrator))
				throw new UserRoleException("You do not have the required permissions to activate a client. Action cancelled.");

			this.IsActive = true;
			this.IsDeactivated = false;
			this.Save(userActivating);
		}

		public void Deactivate(User userDeactivating)
		{
			if (!userDeactivating.HasRole(UserRoleType.SystemAdministrator))
				throw new UserRoleException("You do not have the required permissions to deactivate a client. Action cancelled.");

			var projectList = this.GetProjects();

			foreach (var project in projectList)
			{
				if (project.GetActivities(true).Any(a => a.IsActive))
					throw new ProjectManagementException("This client still has active activities. Please deactivate these activities before attempting to deactivate the client.");
			}

			if (projectList.Any(p => p.IsProjectActive))
				throw new ClientManagementException("This client still has active projects. Please deactivate these projects before attempting to deactivate the client.");

			//this.IsActive = false;
			this.IsDeactivated = true;
			this.Save(userDeactivating);
		}

		public void Save(User userSaving)
		{
			if (!userSaving.HasRole(UserRoleType.SystemAdministrator))
				throw new UserRoleException("You do not have the required permissions to create/edit a client. Save cancelled.");

			if (this.VATRate.HasValue)
			{
				//this.VATRate = this.VATRate.Value / 100;
				this.VATRate = this.VATRate.Value;
				this.VATRate = Math.Round(this.VATRate.Value, 2);

				if (this.VATRate.Value == EnabillSettings.DefaultVATRate)
					this.VATRate = null;
			}

			this.LastModifiedBy = userSaving.FullName;

			ClientRepo.Save(this);
		}

		public double GetVATRate(User userRequesting)
		{
			if (!userRequesting.HasRole(UserRoleType.SystemAdministrator) && userRequesting.CanManage(this))
				throw new ClientManagementException("You do not have the required permissions to view the VAT rate for this client.");

			return this.GetVATRate();
		}

		internal double GetVATRate() => this.VATRate ?? EnabillSettings.DefaultVATRate;

		#endregion CLIENT

		#region CONTACTS

		public Contact GetNewContact() => new Contact { ClientID = this.ClientID, IsActive = true, LastModifiedBy = "Sys Generated" };

		public void SaveContact(User userSaving, Contact contact)
		{
			if (!userSaving.HasRole(UserRoleType.SystemAdministrator))
			{
				if (contact.ContactID == 0)
					throw new UserRoleException("You do not have the required permissions to create a contact. Save cancelled.");
				else
					throw new UserRoleException("You do not have the required permissions to update a contact. Save cancelled.");
			}

			if (!this.IsActive)
				throw new ClientContactException("Contacts cannot be added for this client because the client is inactive. Save cancelled.");

			if (this.ClientID != contact.ClientID)
				throw new ClientContactException("The contact being saved does not belong to this client. Save cancelled.");

			contact.LastModifiedBy = userSaving.FullName;
			ClientRepo.SaveContact(contact);
		}

		public List<Contact> GetContacts() => ClientRepo.GetContacts(this.ClientID)
					.OrderBy(c => c.ContactName)
					.ToList();

		public List<ClientDepartmentCode> GetClientDepartmentCodes() => ClientDepartmentCodeRepo.GetAllByClientID(this.ClientID)
					.OrderBy(c => c.DepartmentCode)
					.ToList();

		public Contact GetContact(int contactID) => ContactRepo.GetByID(contactID);

		public void DeleteContact(User userDeleting, int contactID)
		{
			var contact = this.GetContact(contactID);
			this.DeleteContact(userDeleting, contact);
		}

		public void DeleteContact(User userDeleting, Contact contact)
		{
			if (contact == null || contact.ContactID == 0)
				return;

			if (!userDeleting.HasRole(UserRoleType.SystemAdministrator))
				throw new UserRoleException("You do not have the required permissions to delete a contact. Action cancelled.");

			if (contact.IsLinkedToAnInvoiceRule())
				throw new ClientContactException("This contact cannot be deleted as he/she is linked to an invoice rule as a default contact. Action cancelled");

			if (contact.IsLinkedToAnInvoiceRuleByCC())
				throw new ClientContactException("This contact cannot be deleted as he/she is linked to an invoice rule as a CC contact. Action cancelled");

			if (contact.IsLinkedToAnInvoice())
				throw new ClientContactException("This contact cannot be deleted as he/she is linked to an open invoice as a default contact. Action cancelled.");

			if (contact.IsLinkedToAnInvoiceByCC())
				throw new ClientContactException("This contact cannot be deleted as he/she is linked to an open invoice as a CC contact. Action cancelled.");

			ClientRepo.DeleteContact(contact);
		}

		#endregion CONTACTS

		#region PROJECTS

		public Project CreateNewProject() => new Project
		{
			ClientID = this.ClientID,
			StartDate = DateTime.Today.ToFirstDayOfMonth()
		};

		public List<Project> GetProjects() => ClientRepo.GetProjects(this.ClientID)
				.OrderBy(p => p.ProjectName)
				.ToList();

		public Project GetProject(int projectID) => ProjectRepo.GetByID(projectID);

		public List<ProjectSelectModel> GetProjects(BillingMethodType billingMethodType, bool isUsedInAnInvoiceRule) => ClientRepo.GetProjectsByBillingMethodType(this.ClientID, billingMethodType, isUsedInAnInvoiceRule)
				.OrderBy(p => p.Project.ProjectName)
				.ToList();

		#endregion PROJECTS

		#region ACTIVITIES

		internal List<ActivityDetail> GetActivities(BillingMethodType billingMethodType) => ClientRepo.GetActivitiesByBillingMethodType(this.ClientID, billingMethodType)
				.OrderBy(a => a.ActivityName)
				.ToList();

		public List<ActivityDetail> GetActivities(BillingMethodType billingMethodType, int? invoiceRuleID) => ClientRepo.GetActivitiesByBMForInvRule(this.ClientID, invoiceRuleID, billingMethodType)
				.OrderBy(a => a.ActivityName)
				.ToList();

		#endregion ACTIVITIES

		#region INVOICE

		public List<Invoice> GetInvoices(DateTime dateFrom, DateTime dateTo, int billingMethodBWTotal, int statusBWTotal)
		{
			if (billingMethodBWTotal == 0 || statusBWTotal == 0)
				return new List<Invoice>();

			return ClientRepo.GetInvoices(this.ClientID, dateFrom, dateTo, billingMethodBWTotal, statusBWTotal)
					.OrderByDescending(i => i.InvoiceDate)
					.ToList();
		}

		public List<Invoice> GetInvoicesForUser(User userRequesting, DateTime dateFrom, DateTime dateTo, int billingMethodBWTotal, int statusBWTotal)
		{
			if (billingMethodBWTotal == 0 || statusBWTotal == 0)
				return new List<Invoice>();

			return ClientRepo.GetInvoicesForUser(this.ClientID, dateFrom, dateTo, billingMethodBWTotal, statusBWTotal, userRequesting.UserID)
				.OrderByDescending(i => i.InvoiceDate)
				.ToList();
		}

		public List<Invoice> GetInvoices(int period, int billingMethodBWTotal, int statusBWTotal)
		{
			if (billingMethodBWTotal == 0 || statusBWTotal == 0)
				return new List<Invoice>();

			var t = ClientRepo.GetInvoices(this.ClientID, period, billingMethodBWTotal, statusBWTotal)
					.OrderByDescending(i => i.InvoiceDate)
					.ToList();

			return ClientRepo.GetInvoices(this.ClientID, period, billingMethodBWTotal, statusBWTotal)
					.OrderByDescending(i => i.InvoiceDate)
					.ToList();
		}

		public List<Invoice> GetInvoicesForUser(User userRequesting, int period, int billingMethodBWTotal, int statusBWTotal)
		{
			if (billingMethodBWTotal == 0 || statusBWTotal == 0)
				return new List<Invoice>();

			var firstDayOfPeriod = period.FirstDayOfPeriod();

			return ClientRepo.GetInvoicesForUser(this.ClientID, period, firstDayOfPeriod, billingMethodBWTotal, statusBWTotal, userRequesting.UserID)
					.OrderByDescending(i => i.InvoiceDate)
					.ToList();
		}

		#endregion INVOICE

		#region INVOICE RULES

		public static List<Client> GetAllWithInvoiceRules(bool status) => ClientRepo.GetAllWithInvoiceRules(status)
						.OrderBy(c => c.ClientName)
						.ToList();

		#endregion INVOICE RULES
	}
}