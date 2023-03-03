using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class InvoiceEditAdHoc
	{
		#region INITIALIZATION

		public InvoiceEditAdHoc(Invoice invoice, Client client)
		{
			this.Invoice = invoice;
			this.ClientName = string.IsNullOrEmpty(invoice.ClientName) ? client.ClientName : invoice.ClientName;
			this.ProjectName = invoice.ProjectID.HasValue && invoice.ProjectID != 0 ? invoice.GetProject().ProjectName : "None";
			this.InvoiceContactName = invoice.InvoiceContactID.HasValue ? ContactRepo.GetByID(invoice.InvoiceContactID.Value).ContactName : invoice.InvoiceContactName ?? string.Empty;
			this.InvoiceContactName = Enabill.Helpers.DecryptString(this.InvoiceContactName, Enabill.Code.Constants.PASSPHRASE);
			this.InvoiceSubCategoryName = InvoiceSubCategoryRepo.GetInvoiceSubCategoryExtendedName(invoice.InvoiceID);
			this.AdHocInvoiceContacts = this.SetupAdHocInvoiceContacts(invoice, client);
			this.ClientDepartmentCode = ClientDepartmentCodeRepo.GetAll().Where(s => s.IsActive).ToList();
			this.GLAccountCode = GLAccountRepo.GetAll().Where(s => s.IsActive).ToList();
			//InvoiceLines = LoadInvoiceLines(invoice);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public string ClientName { get; private set; }
		public string InvoiceContactName { get; private set; }
		public string InvoiceSubCategoryName { get; private set; }
		public string ProjectName { get; private set; }

		public Invoice Invoice { get; private set; }

		public List<ClientDepartmentCode> ClientDepartmentCode { get; set; }
		public List<ContactSelectModel> AdHocInvoiceContacts { get; private set; }
		public List<GLAccount> GLAccountCode { get; set; }
		//public List<InvoiceLineModel> InvoiceLines { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<ContactSelectModel> SetupAdHocInvoiceContacts(Invoice invoice, Client client)
		{
			var model = new List<ContactSelectModel>();

			foreach (var contact in client.GetContacts())
			{
				contact.ContactName = Enabill.Helpers.DecryptString(contact.ContactName, Enabill.Code.Constants.PASSPHRASE);

				if (contact.ContactID == invoice.InvoiceContactID)
					continue;

				model.Add(new ContactSelectModel { Contact = contact, IsSelected = false });
			}

			foreach (var contact in invoice.Contacts)
			{
				contact.ContactName = Enabill.Helpers.DecryptString(contact.ContactName, Enabill.Code.Constants.PASSPHRASE);

				if (contact.ContactID == invoice.InvoiceContactID)
					continue;

				model.Single(c => c.Contact == contact).IsSelected = true;
			}

			return model;
		}

		/*
		private List<InvoiceLineModel> LoadInvoiceLines(Invoice invoice)
		{
			List<InvoiceLineModel> model = new List<InvoiceLineModel>();

			invoice.GetInvoiceLines().ForEach(il => model.Add(new InvoiceLineModel(il)));

			return model;
		}
		*/

		#endregion FUNCTIONS
	}
}