using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class InvoiceEditModel
	{
		#region INITIALIZATION

		public InvoiceEditModel(Invoice invoice)
		{
			this.Invoice = invoice;
			this.Client = invoice.Client;
			this.InvoiceContactName = invoice.InvoiceContactID.HasValue ? ContactRepo.GetByID(invoice.InvoiceContactID.Value).ContactName : invoice.InvoiceContactName ?? string.Empty;
			this.InvoiceContactName = Enabill.Helpers.DecryptString(this.InvoiceContactName, Enabill.Code.Constants.PASSPHRASE);
			this.WorkAllocationsModel = invoice.GetWorkAllocationsExtendedModel();
			this.WorkAllocationExceptionModel = this.LoadWorkAllocationExceptionModel(invoice);
			this.InvoiceCategoryName = this.InvoiceSubCategoryName = string.Empty;
			this.ClientDepartmentCode = ClientDepartmentCodeRepo.GetAll().Where(s => s.IsActive).ToList();
			this.GLAccountCode = GLAccountRepo.GetAll().Where(s => s.IsActive).ToList();
			this.LoadModel(invoice);
			//RelatedInvoices = LoadClientRelatedInvoices(invoice);
			//LinkedInvoice = LoadLinkedInvoice(invoice);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public string InvoiceCategoryName { get; private set; }
		public string InvoiceContactName { get; private set; }
		public string InvoiceSubCategoryName { get; private set; }

		public Client Client { get; private set; }
		public Invoice Invoice { get; private set; }
		public Invoice LinkedInvoice { get; private set; }

		public List<ClientDepartmentCode> ClientDepartmentCode { get; set; }
		public List<GLAccount> GLAccountCode { get; set; }
		public List<Invoice> RelatedInvoices { get; private set; }
		public List<WorkAllocationExceptionModel> WorkAllocationExceptionModel { get; private set; }
		public List<WorkAllocationExtendedModel> WorkAllocationsModel { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private void LoadModel(Invoice invoice)
		{
			if (!invoice.IsOpen)
			{
				var subCat = InvoiceSubCategoryRepo.GetByID(invoice.InvoiceSubCategoryID);
				this.InvoiceCategoryName = subCat.InvoiceCategory.CategoryName;
				this.InvoiceSubCategoryName = subCat.SubCategoryName;
			}
		}

		private List<WorkAllocationExceptionModel> LoadWorkAllocationExceptionModel(Invoice invoice)
		{
			//List<WorkAllocationExceptionModel> userWorkAllocationExceptionList = new List<WorkAllocationExceptionModel>();
			var allWorkAllocationExceptionList = invoice.GetTimeCaptureExceptions();

			//Update Invoice approval status
			var activityIDs = invoice.GetActivitiesFromRule().Select(a => a.ActivityID).ToList();
			var unApprovedWorkSessions = new List<WorkSession>();
			unApprovedWorkSessions.AddRange(InvoiceRepo.GetUnApprovedWorkSessionsLinkedToInvoicePeriod(activityIDs, invoice.DateFrom, invoice.DateTo));

			invoice.IsTimeApproved = unApprovedWorkSessions.Count <= 0 && allWorkAllocationExceptionList.Count <= 0;

			invoice.Save(Settings.Current.CurrentUser);

			return allWorkAllocationExceptionList;
		}

		private List<Invoice> LoadRuleRelatedInvoices(Invoice invoice) =>
			//Retrieves a list of invoice related to the same ruleID linked to the selected invoice
			invoice.GetRuleRelatedInvoices(invoice);

		private List<Invoice> LoadClientRelatedInvoices(Invoice invoice) =>
			//Retrieves a list of invoice related to the same Client linked to the selected invoice
			invoice.GetClientRelatedInvoices(invoice);

		private Invoice LoadLinkedInvoice(Invoice invoice) => invoice.GetLinkedInvoice();

		#endregion FUNCTIONS
	}
}