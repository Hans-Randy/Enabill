using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using Enabill.Emailing;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Print;
using Enabill.Properties;
using Enabill.Repos;
using Enabill.Web.Code;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class InvoiceController : BaseController
	{
		#region INVOICE

		[HttpGet]
		public ActionResult Index(bool myClients = true)
		{
			if (!this.CurrentUser.CanViewInvoices())
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			int clientID = InputHistory.Get(HistoryItemType.InvoiceIndexClient, 0);

			// If user not in any of these roles, then user can only see their own invoices
			if (!this.CurrentUser.HasRole(UserRoleType.Accountant) && !this.CurrentUser.HasRole(UserRoleType.Manager) && !this.CurrentUser.HasRole(UserRoleType.ProjectOwner) && !this.CurrentUser.HasRole(UserRoleType.InvoiceAdministrator))
			{
				myClients = true;
				InputHistory.Set(HistoryItemType.InvoiceShowMyClientsCheckBox, false);
			}
			else
			{
				InputHistory.Set(HistoryItemType.InvoiceShowMyClientsCheckBox, true);
			}

			var items = new List<SelectListItem> { new SelectListItem() { Value = "0", Text = "All " + (myClients ? "My " : "") + "Clients" } };

			if (myClients)
				Client.GetAllActiveClientsForUser(this.CurrentUser).ForEach(c => items.Add(new SelectListItem() { Value = c.ClientID.ToString(), Text = c.ClientName, Selected = c.ClientID == clientID }));
			else
				Client.GetAllActiveClients().ForEach(c => items.Add(new SelectListItem() { Value = c.ClientID.ToString(), Text = c.ClientName, Selected = c.ClientID == clientID }));

			this.ViewData["ClientList"] = items;
			InputHistory.Set(HistoryItemType.InvoiceMyClients, myClients);

			if (this.CurrentUser.HasRole(UserRoleType.Accountant) && this.CurrentUser.HasRole(UserRoleType.InvoiceAdministrator))
			{
				InputHistory.Set(HistoryItemType.InvoiceStatusOpen, true);
				InputHistory.Set(HistoryItemType.InvoiceStatusInProgress, false);
				InputHistory.Set(HistoryItemType.InvoiceStatusReady, false);
				InputHistory.Set(HistoryItemType.InvoiceStatusComplete, false);
			}
			else if (this.CurrentUser.HasRole(UserRoleType.InvoiceAdministrator))
			{
				InputHistory.Set(HistoryItemType.InvoiceStatusOpen, true);
				InputHistory.Set(HistoryItemType.InvoiceStatusInProgress, false);
				InputHistory.Set(HistoryItemType.InvoiceStatusReady, false);
				InputHistory.Set(HistoryItemType.InvoiceStatusComplete, false);
			}

			var model = new InvoiceIndexModel(this.CurrentUser);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult Index(FormCollection form)
		{
			try
			{
				if (!this.CurrentUser.CanViewInvoices())
					return this.ErrorView(new ErrorModel("Access Denied", "You do not have the required permissions to view invoices.", true));

				int? clientID = form["ClientList"].ToInt();

				InputHistory.Set(HistoryItemType.InvoiceIndexClient, clientID.ToString());

				if (this.CurrentUserPreferences.InvoiceIndexDateSelector == 0) // dateRange
				{
					InputHistory.Set(HistoryItemType.InvoiceDateFrom, form["DateFrom"].ToDate() ?? DateTime.Today.ToFirstDayOfMonth().AddMonths(-1));
					InputHistory.Set(HistoryItemType.InvoiceDateTo, form["DateTo"].ToDate() ?? DateTime.Today.ToLastDayOfMonth());
				}
				else //if (CurrentUserPreferences.InvoiceIndexDateSelector == 1) // period
				{
					if (!int.TryParse(form["InvoicePeriod"], out int period) || !period.IsValidPeriod())
						throw new EnabillConsumerException("The selected period is invalid.");

					InputHistory.Set(HistoryItemType.InvoicePeriod, period);
				}

				InputHistory.Set(HistoryItemType.InvoiceStatusOpen, IsCheckBoxChecked(form[InvoiceStatusType.Open.ToString()]));
				InputHistory.Set(HistoryItemType.InvoiceStatusInProgress, IsCheckBoxChecked(form[InvoiceStatusType.InProgress.ToString()]));
				InputHistory.Set(HistoryItemType.InvoiceStatusReady, IsCheckBoxChecked(form[InvoiceStatusType.Ready.ToString()]));
				InputHistory.Set(HistoryItemType.InvoiceStatusComplete, IsCheckBoxChecked(form[InvoiceStatusType.Complete.ToString()]));

				InputHistory.Set(HistoryItemType.InvoiceTimeMaterial, IsCheckBoxChecked(form[BillingMethodType.TimeMaterial.ToString()]));
				InputHistory.Set(HistoryItemType.InvoiceFixedCost, IsCheckBoxChecked(form[BillingMethodType.FixedCost.ToString()]));
				InputHistory.Set(HistoryItemType.InvoiceSLA, IsCheckBoxChecked(form[BillingMethodType.SLA.ToString()]));
				InputHistory.Set(HistoryItemType.InvoiceTravel, IsCheckBoxChecked(form[BillingMethodType.Travel.ToString()]));
				InputHistory.Set(HistoryItemType.InvoiceAdHoc, IsCheckBoxChecked(form[BillingMethodType.AdHoc.ToString()]));
				InputHistory.Set(HistoryItemType.InvoiceMonthlyFixedCost, IsCheckBoxChecked(form[BillingMethodType.MonthlyFixedCost.ToString()]));

				var model = new InvoiceIndexModel(this.CurrentUser);

				return this.PartialView("ucInvoiceList", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult Edit(int id) // id = invoiceID
		{
			var invoice = Invoice.GetByID(this.CurrentUser, id);

			if (invoice == null)
				return this.RedirectToAction("Index");

			if (!this.CurrentUser.CanViewInvoice(invoice))
				return this.ErrorView(new ErrorModel("Access Denied", "You do not have the required permissions to manage invoices.", true));

			if (invoice.BillingMethodID == (int)BillingMethodType.AdHoc)
				return this.EditAdHocInvoice(invoice);

			var model = new InvoiceEditModel(invoice);

			this.SetViewDropData(invoice, invoice.Client);

			return this.View("Edit", model);
		}

		[HttpPost]
		public ActionResult Edit(FormCollection form)
		{
			try
			{
				var invoice = Invoice.GetByID(this.CurrentUser, int.Parse(form["InvoiceID"]));
				int? ticketRemarkOptionID = form["PrintTicketRemarkOptions"].ToInt();
				invoice.PrintLayoutTypeID = form["PrintLayoutTypeID"].ToInt();
				invoice.InvoiceDate = form["InvoiceDate"].ToDate();
				invoice.Period = int.Parse(form["Period"]);
				invoice.PrintTicketRemarkOptionID = ticketRemarkOptionID.Value;
				int ClientDepartmentCodeID = 0;

				if (form["ClientDepartmentCodeID"].ToInt() != null)
				{
					//ClientDepartmentCodeID = (int)form["ClientDepartmentCodeID"].ToInt();
					ClientDepartmentCodeID = 125; //Hard coded to Alacrity as the client
				}
				else
				{
					//ClientDepartmentCodeID = invoice.ClientDepartmentCodeID.Value;
					ClientDepartmentCodeID = 125; //Hard coded to Alacrity as the client
				}

				int GLAccountID = 0;

				if (form["GLAccountID"].ToInt() != null)
				{
					//GLAccountID = (int)form["GLAccountID"].ToInt();
					GLAccountID = 10; //Hard coded to Consulting Fees
				}
				else
				{
					//GLAccountID = invoice.GLAccountID.Value;
					GLAccountID = 10; //Hard coded to Consulting Fees
				}

				bool isInternal = false;

				if (form["IsInternal"] != null)
				{
					isInternal = form["IsInternal"].Contains("true");
				}
				else
				{
					isInternal = invoice.IsInternal;
				}

				invoice.IsInternal = isInternal;
				//invoice.InvoiceDate = invoiceDate;

				if (invoice == null)
					throw new EnabillException("Invoice could not be updated. Error occurred.");

				//this.TryUpdateModel(invoice);

				if (ClientDepartmentCodeID == 0)
				{
					//invoice.ClientDepartmentCodeID = null;
					invoice.ClientDepartmentCodeID = null;
				}
				else
				{
					//invoice.ClientDepartmentCodeID = ClientDepartmentCodeID;
					invoice.ClientDepartmentCodeID = 125;
				}

				if (GLAccountID == 0)
				{
					invoice.GLAccountID = null;
				}
				else
				{
					//invoice.GLAccountID = GLAccountID;
					invoice.GLAccountID = 10; //Hard coded to Consulting Fees
				}

				invoice.ClientAccountCode = invoice.Client == null ? "" : invoice.Client.AccountCode;

				if (invoice.IsOpen || invoice.IsInProgress)
				{
					invoice.VATRate = Convert.ToDouble(form["VATRate"].Replace(",", "").Replace(" ", ""));
					invoice.VATRate = invoice.VATRate > 0 ? invoice.VATRate : 0;
					invoice.DateFrom = form["DateFrom"].ToDate() ?? default;
					invoice.DateTo = form["DateTo"].ToDate() ?? default;
					invoice.Description = form["Description"];

					if (invoice.IsInProgress)
					{
						invoice.InvoiceAmountExclVAT = form["InvoiceAmountExclVAT"].Replace(",", "").Replace(" ", "").ToDouble() ?? 0;
						invoice.AccrualExclVAT = form["AccrualAmountExclVAT"].Replace(",", "").Replace(" ", "").ToDouble() ?? 0;
					}
				}

				this.TryUpdateModel(invoice, "Invoice");

				switch ((BillingMethodType)invoice.BillingMethodID)
				{
					case BillingMethodType.TimeMaterial:
						return this.SaveTimeMaterial(invoice);

					case BillingMethodType.SLA:
						return this.SaveSLA(invoice);

					case BillingMethodType.FixedCost:
						return this.SaveFixedCost(invoice);

					case BillingMethodType.MonthlyFixedCost:
						return this.SaveMonthlyFixedCost(invoice);

					case BillingMethodType.ActivityFixedCost:
						return this.SaveActivityFixedCost(invoice);

					default:
						throw new EnabillConsumerException("Error occurred. Please mail Gavin van Gent an enquire about this issue.");
				}
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult EditFromRule(int id)
		{
			var rule = InvoiceRule.GetByID(id);

			if (rule == null)
				return this.ErrorView(new ErrorModel("Specified Invoice Rule not found."));

			var inv = rule.GetLatestOpenInvoice();

			if (inv == null)
				return this.ErrorView(new ErrorModel("Invoices have not yet been created from this rule."));

			return this.Edit(inv.InvoiceID);
		}

		private ActionResult SaveTimeMaterial(Invoice invoice)
		{
			try
			{
				if (invoice.IsOpen || invoice.IsInProgress)
				{
					invoice.Save(this.CurrentUser, true);
				}
				else
				{
					invoice.Save(this.CurrentUser);
				}

				var model = new InvoiceEditModel(invoice);
				this.SetViewDropData(invoice, invoice.Client);

				return this.PartialView("ucEdit", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		private ActionResult SaveFixedCost(Invoice invoice)
		{
			try
			{
				if (invoice.IsOpen || invoice.IsInProgress)
					invoice.Save(this.CurrentUser, true);
				else
					invoice.Save(this.CurrentUser);

				var model = new InvoiceEditModel(invoice);
				this.SetViewDropData(invoice, invoice.Client);

				return this.PartialView("ucEdit", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		private ActionResult SaveMonthlyFixedCost(Invoice invoice)
		{
			try
			{
				if (invoice.IsOpen || invoice.IsInProgress)
					invoice.Save(this.CurrentUser, true);
				else
					invoice.Save(this.CurrentUser);

				var model = new InvoiceEditModel(invoice);
				this.SetViewDropData(invoice, invoice.Client);

				return this.PartialView("ucEdit", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		private ActionResult SaveActivityFixedCost(Invoice invoice)
		{
			try
			{
				if (invoice.IsOpen || invoice.IsInProgress)
					invoice.Save(this.CurrentUser, true);
				else
					invoice.Save(this.CurrentUser);

				var model = new InvoiceEditModel(invoice);
				this.SetViewDropData(invoice, invoice.Client);

				return this.PartialView("ucEdit", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		private ActionResult SaveSLA(Invoice invoice)
		{
			try
			{
				if (invoice.IsOpen || invoice.IsInProgress)
					invoice.Save(this.CurrentUser, true);
				else
					invoice.Save(this.CurrentUser);

				var model = new InvoiceEditModel(invoice);
				this.SetViewDropData(invoice, invoice.Client);

				return this.PartialView("ucEdit", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult Delete(int id)
		{
			try
			{
				var invoice = Invoice.GetByID(Settings.Current.CurrentUser, id);

				invoice.Delete(this.CurrentUser);

				return this.ReturnJsonResult(false, "Invoice deleted successfully");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		public ActionResult PrintPdf(int id)
		{
			var invoice = Invoice.GetByID(Settings.Current.CurrentUser, id);

			if (invoice == null)
				return null;

			invoice.InvoiceContactName = Enabill.Helpers.DecryptString(invoice.InvoiceContactName, Enabill.Code.Constants.PASSPHRASE);

			var printer = new PrintInvoicePDF(invoice);
			byte[] report = new byte[0];

			try
			{
				report = printer.Print(Settings.Current.CurrentUser);
			}
			catch (Exception ex)
			{
				var blr = new StringBuilder();
				blr.AppendLine(ex.Message)
					.Append(ex.StackTrace);
				return this.Content(blr.ToString());
			}

			string filename = (string.IsNullOrEmpty(invoice.ExternalInvoiceNo) ? "Invoice " + invoice.InvoiceID.ToString() : invoice.ExternalInvoiceNo) + ".pdf";

			return this.File(report, "application/pdf", filename);
		}

		public ActionResult PrintTimeSheet(int id)
		{
			var invoice = Invoice.GetByID(Settings.Current.CurrentUser, id);

			if (invoice == null)
				return null;

			var printer = new PrintInvoicePDF(invoice);
			byte[] report = new byte[0];

			try
			{
				report = printer.PrintTimeSheet(Settings.Current.CurrentUser);
			}
			catch (Exception ex)
			{
				var blr = new StringBuilder();
				blr.AppendLine(ex.Message)
					.Append(ex.StackTrace);
				return this.Content(blr.ToString());
			}

			string filename = (string.IsNullOrEmpty(invoice.ExternalInvoiceNo) ? "Timesheet " + invoice.InvoiceID.ToString() : invoice.ExternalInvoiceNo) + ".pdf";

			return this.File(report, "application/pdf", filename);
		}

		#endregion INVOICE

		#region GL ACCOUNTS

		//[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
		public ActionResult GLAccountIndex()
		{
			var model = new InvoiceGLAccountsModel();

			return this.View(model);
		}

		public ActionResult EditGLAccount(int id) // id = GLAccountID
		{
			var model = new InvoiceGLAccountEditModel(id);

			if (model == null)
				return this.RedirectToAction("GLAccountIndex");
			else
				return this.PartialView("ucEditGLAccount", model);
		}

		[HttpPost]
		public ActionResult CreateGLAccount()
		{
			var model = GLAccount.GetNew();

			if (model == null)
				return this.RedirectToAction("GLAccountIndex");
			else
				return this.PartialView("ucCreateGLAccount", model);
		}

		[HttpPost]
		public ActionResult SaveGLAccount(FormCollection form) // id = GLAccountID
		{
			try
			{
				int? id = form["GLAccountID"].ToInt();
				//bool? isActive = form["IsActive"].ToBool();

				var model = GLAccount.GetNew();

				if (id.HasValue)
					model = GLAccount.GetGLAccountByID(id ?? 0);
				else
					model.GLAccountCode = form["GLAccountCode"];

				model.GLAccountName = form["GLAccountName"];
				model.IsActive = (bool)form["IsActive"].ToBool();

				model.ValidateSave();
				GLAccountRepo.Save(model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}

			var modelGLAList = new InvoiceGLAccountsModel();

			return this.PartialView("ucGLAccountIndex", modelGLAList);
		}

		#endregion GL ACCOUNTS

		#region EXPORT

		[HttpPost]
		public ActionResult Export(int? period)
		{
			var sourceTable = new DataTable();
			var recipients = new List<string>
			{
				Enabill.Code.Constants.EMAILADDRESSACCOUNTS
			};

			var body = new StringBuilder();

			body.AppendLine("<h2>Good day,</h2>")
				.AppendLine("<br />")
				.AppendLine("<p>Please find attached.<p>")
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var invoiceData = InvoiceRepo.GetInvoicesByPeriod(period);
			var diskStore = new DiskFileStore();
			sourceTable = invoiceData.ToADOTable<InvoiceExportModel>();

			byte[] outputBuffer = null;

			string filePath = diskStore.GetPathName + "\\invoices" + period + ".csv";

			try
			{
				using (var stream = new MemoryStream())
				{
					using (var writer = new StreamWriter(filePath))
					{
						InvoiceExportToCSVWriter.WriteExportData(sourceTable, writer, true);
					}

					outputBuffer = stream.ToArray();
				}
			}
			catch (EnabillException ex)
			{
				this.ReturnJsonException(ex);
			}

			this.File(outputBuffer, "text/csv", filePath);

			try
			{
				const string subject = "Automated Email please do not reply";
				var model = new EnabillEmailModel(subject, recipients, body.ToString(), new string[] { filePath });

				EnabillEmails.SendEmail(model);
			}
			catch (EnabillException ex)
			{
				return this.ReturnJsonException(ex);
			}

			return this.RedirectToAction("Index");
		}

		#endregion EXPORT

		#region INVOICE STATUS

		[HttpPost]
		public ActionResult MoveToOpen(FormCollection form)
		{
			try
			{
				var invoice = Invoice.GetByID(this.CurrentUser, Convert.ToInt32(form["InvoiceID"]));

				if (invoice == null)
					throw new EnabillDomainException("The requested invoice could not be located in our records.");

				invoice.IsInternal = form["IsInternal"].Contains("true");
				invoice.OpenInvoice(this.CurrentUser);

				this.SetViewDropData(invoice, invoice.Client);

				if (invoice.IsAdHoc)
					return this.PartialView("ucEditAdHoc", new InvoiceEditAdHoc(invoice, invoice.Client));

				return this.PartialView("ucEdit", new InvoiceEditModel(invoice));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult MoveToInProgess(FormCollection form)
		{
			try
			{
				var invoice = Invoice.GetByID(this.CurrentUser, Convert.ToInt32(form["InvoiceID"]));

				if (invoice == null)
					throw new EnabillDomainException("The requested invoice could not be located in our records.");

				invoice.ClientAccountCode = invoice.Client == null ? " " : invoice.Client.AccountCode;
				invoice.IsInternal = form["IsInternal"].Contains("true");
				invoice.ExternalInvoiceNo = form["ExternalInvoiceNo"];
				invoice.MoveInvoiceToInProgress(this.CurrentUser);

				this.SetViewDropData(invoice, invoice.Client);

				if (invoice.IsAdHoc)
					return this.PartialView("ucEditAdHoc", new InvoiceEditAdHoc(invoice, invoice.Client));

				return this.PartialView("ucEdit", new InvoiceEditModel(invoice));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult MoveToReady(FormCollection form)
		{
			try
			{
				var invoice = Invoice.GetByID(this.CurrentUser, Convert.ToInt32(form["InvoiceID"]));

				if (invoice == null)
					throw new EnabillDomainException("The requested invoice could not be located in our records.");

				if (form["IsInternal"] != null)
				{
					invoice.IsInternal = form["IsInternal"].Contains("true");
				}

				invoice.ReadyInvoice(this.CurrentUser);

				this.SetViewDropData(invoice, invoice.Client);

				if (invoice.IsAdHoc)
					return this.PartialView("ucEditAdHoc", new InvoiceEditAdHoc(invoice, invoice.Client));

				return this.PartialView("ucEdit", new InvoiceEditModel(invoice));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult MoveToComplete(int id, string extNo)
		{
			try
			{
				var invoice = Invoice.GetByID(this.CurrentUser, id);

				if (invoice == null)
					throw new EnabillDomainException("The requested invoice could not be located in our records.");

				invoice.ExternalInvoiceNo = extNo;
				invoice.CompleteInvoice(this.CurrentUser);
				this.SetViewDropData();

				if (invoice.IsAdHoc)
					return this.PartialView("ucEditAdHoc", new InvoiceEditAdHoc(invoice, invoice.Client));

				return this.PartialView("ucEdit", new InvoiceEditModel(invoice));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult MoveInvoices(string[] invoiceIds)
		{
			try
			{
				if (invoiceIds == null)
					throw new EnabillDomainException("No invoices provided for the move.");

				foreach (string i in invoiceIds)
				{
					if (i.Contains('|'))
					{
						string[] invoiceDetail = i.Split('|');
						int.TryParse(invoiceDetail[0], out int invoiceId);
						int.TryParse(invoiceDetail[1], out int invoiceStatusType);

						var invoice = Invoice.GetByID(this.CurrentUser, invoiceId);

						if (invoice != null)
						{
							if ((InvoiceStatusType)invoice.InvoiceStatusID == InvoiceStatusType.Open)
							{
								invoice.ClientAccountCode = invoice.Client == null ? " " : invoice.Client.AccountCode;
								invoice.ClientDepartmentCodeID = 125;
								invoice.GLAccountID = 10;
								invoice.InvoiceAmountExclVAT = invoice.ProvisionalNettAmount;
								invoice.MoveInvoiceToInProgress(this.CurrentUser);
							}

							if ((InvoiceStatusType)invoiceStatusType == InvoiceStatusType.Ready)
							{
								invoice.ReadyInvoice(this.CurrentUser);
							}

							if ((InvoiceStatusType)invoiceStatusType == InvoiceStatusType.Complete)
							{
								invoice.CompleteInvoice(this.CurrentUser);
							}
						}
					}
				}

				var model = new InvoiceIndexModel(this.CurrentUser);

				return this.PartialView("ucInvoiceList", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion INVOICE STATUS

		#region WORK ALLOCATIONS

		public ActionResult WorkAllocations(int id) // id = invoiceID
		{
			if (!this.CurrentUser.CanViewInvoices())
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			var invoice = Invoice.GetByID(this.CurrentUser, id);

			if (invoice == null)
				return this.ErrorView(new ErrorModel("The requested invoice could not be found and therefore, the linked workallocations could not be displayed."));

			if (!this.CurrentUser.CanViewInvoice(invoice))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			var model = new InvoiceWorkAllocationModel(invoice);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult SaveWorkAllocations(int id, double invoiceCreditAmount, string hourlyRates, string linkedWorkAllocations, string credits)
		{
			try
			{
				var invoice = Invoice.GetByID(this.CurrentUser, id);

				using (var ts = new TransactionScope())
				{
					invoice.LinkWorkAllocationsAndUpdateCredits(this.CurrentUser, invoiceCreditAmount, linkedWorkAllocations, credits);
					WorkAllocation.UpdateHourlyRates(this.CurrentUser, hourlyRates);
					invoice.CalculateEstimateValues(this.CurrentUser);
					invoice.Save(this.CurrentUser);

					ts.Complete();
				}

				var model = new InvoiceWorkAllocationModel(invoice);

				return this.PartialView("ucWorkAllocations", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion WORK ALLOCATIONS

		#region ADHOC

		[HttpPost]
		public ActionResult SetupAdHocInvoice()
		{
			this.SetViewDropData();

			return this.PartialView("ucSetupAdHocCreation");
		}

		[HttpGet]
		public ActionResult CreateAdHocInvoice(int clientID, int contactID, int projectID = 0)
		{
			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator) && !this.CurrentUser.HasRole(UserRoleType.InvoiceAdministrator))
				return this.ErrorView(new ErrorModel("Access Denied", "You do not have the required permissions to create invoices", true));

			var client = ClientRepo.GetByID(clientID);

			if (client == null)
				throw new EnabillConsumerException("The selected client could not be found. Please retry.");

			var contact = client.GetContact(contactID);

			if (contact == null)
				throw new EnabillConsumerException("The selected contact could not be found. Please retry.");

			var project = client.GetProject(projectID);

			var invoice = Invoice.CreateInvoice(this.CurrentUser, BillingMethodType.AdHoc, client, contact, string.Empty, string.Empty, string.Empty, DateTime.Today.ToFirstDayOfMonth(), DateTime.Today, 0, 1, project);

			this.SetViewDropData(invoice, client);

			return this.View("EditAdHocInvoice", new InvoiceEditAdHoc(invoice, client));
		}

		private ActionResult EditAdHocInvoice(Invoice invoice)
		{
			this.SetViewDropData(invoice, invoice.Client);

			return this.View("EditAdHocInvoice", new InvoiceEditAdHoc(invoice, invoice.Client));
		}

		[HttpPost]
		public ActionResult EditAdHocInvoice(FormCollection form)
		{
			try
			{
				var client = ClientRepo.GetByID(int.Parse(form["ClientID"]));
				var contact = client.GetContact(int.Parse(form["ContactID"]));

				//int? ClientDepartmentCodeID = form["ClientDepartmentCodeID"].ToInt();
				int? ClientDepartmentCodeID = 125; //Hard coded to Alacrity as the client
												   //int? GLAccountID = form["GLAccountID"].ToInt();
				int? GLAccountID = 10; //Hard coded to Consulting Fees
				bool isInternal = form["Isinternal"].Contains("true");

				var project = int.TryParse(form["ProjectID"], out int projectID) ? client.GetProject(projectID) : null;

				if (client == null || contact == null)
					throw new EnabillConsumerException("An error occured when trying to locate the client and contact for the invoice. Please try again.");

				var invoice = Invoice.CreateInvoice(this.CurrentUser, BillingMethodType.AdHoc, client, contact, string.Empty, string.Empty, string.Empty, DateTime.Today.ToFirstDayOfMonth(), DateTime.Today, 0, 1, project);

				if (!int.TryParse(form["InvoiceID"], out int invoiceID))
					throw new EnabillConsumerException("An error occurred with the validation from the received page. Please refresh your page and retry.");

				if (invoiceID > 0)
					invoice = Invoice.GetByID(this.CurrentUser, invoiceID);

				//UpdateModel(invoice);
				if (invoice.IsOpen || invoice.IsInProgress)
				{
					invoice.InvoiceSubCategoryID = int.Parse(form["InvoiceSubCategoryID"]);

					if (!DateTime.TryParse(form["DateFrom"], out var tempDate))
						throw new EnabillConsumerException("An error has been detected with the \"Date From\" field, value not accepted.");

					invoice.DateFrom = tempDate;

					if (!DateTime.TryParse(form["DateTo"], out tempDate))
						throw new EnabillConsumerException("An error has been detected with the \"Date To\" field, value not accepted.");

					invoice.DateTo = tempDate;

					invoice.OrderNo = form["OrderNo"];
					invoice.Description = form["Description"];
					invoice.PrintOptionTypeID = int.Parse(form["PrintOptionTypeID"]);
					invoice.IsInternal = isInternal;

					string credits = form["PrintCredits"];

					if (credits.Contains("true"))
						invoice.PrintCredits = true;

					if (!DateTime.TryParse(form["InvoiceDate"], out tempDate))
						throw new EnabillConsumerException("An error has been detected with the \"InvoiceDate\" field, value not accepted.");

					invoice.InvoiceDate = tempDate;

					invoice.Period = int.Parse(form["Period"]);

					string invoiceVal = form["InvoiceAmountExclVAT"].Replace(",", "").Replace(" ", "");
					invoice.InvoiceAmountExclVAT = double.Parse(invoiceVal);

					string accrualVal = form["AccrualAmountExclVAT"].Replace(",", "").Replace(" ", "");
					invoice.AccrualExclVAT = double.Parse(accrualVal);

					invoice.ProjectID = projectID;
				}
				else if (invoice.IsReady)
				{
					invoice.ExternalInvoiceNo = form["ExternalInvoiceNo"];
				}

				if (ClientDepartmentCodeID == 0)
					invoice.ClientDepartmentCodeID = null;
				else
					invoice.ClientDepartmentCodeID = 125; //ClientDepartmentCodeID. Hard coded to temp

				if (GLAccountID == 0)
				{
					invoice.GLAccountID = null;
				}
				else
				{
					invoice.GLAccountID = 10; //GLAccountID. Hard coded to Consulting Fees
				}

				invoice.ClientAccountCode = invoice.Client == null ? " " : invoice.Client.AccountCode;
				invoice.Save(this.CurrentUser);

				invoice.AddContacts(form["ContactList"]);

				return this.Json(new
				{
					IsError = false,
					Description = "AdHoc invoice saved successfully.",
					url = "/Invoice/Edit/" + invoice.InvoiceID
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion ADHOC

		#region LOOKUPS

		private void SetViewDropData(Invoice invoice, Client client)
		{
			this.ViewData["InvoiceStatusID"] = new SelectList(Enabill.Extensions.GetEnumSelectList<InvoiceStatusType>(), "Value", "Text", invoice.InvoiceStatusID);
			this.ViewData["InvoiceSubCategoryID"] = InvoiceSubCategoryRepo.GetInvoiceSubCategoriesExtendedNames().Select(i => new SelectListItem { Text = i.Value, Value = i.Key.ToString(), Selected = i.Key == invoice.InvoiceSubCategoryID });
			this.ViewData["PrintOptionTypeID"] = PrintOptionRepo.GetPrintOptionsExtendedNames().Select(o => new SelectListItem { Text = o.Value, Value = o.Key.ToString(), Selected = o.Key == invoice.PrintOptionTypeID });
			this.ViewData["PrintLayoutTypeID"] = PrintOptionRepo.GetPrintLayoutsExtendedNames().Select(o => new SelectListItem { Text = o.Value, Value = o.Key.ToString(), Selected = o.Key == invoice.PrintLayoutTypeID });
			this.ViewData["PrintTicketRemarkOptions"] = PrintTicketRemarkOptionRepo.GetPrintTicketRemarksOptionsExtendedNames().Select(o => new SelectListItem { Text = o.Value, Value = o.Key.ToString(), Selected = o.Key == invoice.PrintTicketRemarkOptionID });
			// = ClientDepartmentCode.GetByClientID(invoice.InvoiceID, invoice.ClientID).Select(i => new SelectListItem { Text = i.DepartmentCode, Value = i.ClientDepartmentCodeID.ToString(), Selected = i.ClientDepartmentCodeID == invoice.ClientDepartmentCodeID });
			// ViewData["GLAccountID"] = GLAccount.GetAll().Select(i => new SelectListItem { Text = i.GLAccountCode, Value = i.GLAccountID.ToString(), Selected = i.GLAccountID == invoice.GLAccountID });

			var clientAccountCode = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "Not Applicable" }
			};
			ClientDepartmentCode.GetByClientID(invoice.ClientID).ForEach(p => clientAccountCode.Add(new SelectListItem { Text = p.DepartmentCode, Value = p.ClientDepartmentCodeID.ToString(), Selected = p.ClientDepartmentCodeID == invoice.ClientDepartmentCodeID }));
			this.ViewData["ClientDepartmentCodeID"] = clientAccountCode;

			var glAccountCode = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "Not Applicable" }
			};
			GLAccount.GetAll().ForEach(p => glAccountCode.Add(new SelectListItem { Text = p.GLAccountCode, Value = p.GLAccountID.ToString(), Selected = p.GLAccountID == invoice.GLAccountID }));
			this.ViewData["GLAccountID"] = glAccountCode;

			var items = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "Not Applicable" }
			};
			client.GetProjects().ForEach(p => items.Add(new SelectListItem { Value = p.ProjectID.ToString(), Text = p.ProjectName, Selected = p.ProjectID == invoice.ProjectID }));
			this.ViewData["ProjectID"] = items;
		}

		private void SetViewDropData()
		{
			var client = ClientRepo.GetAll().OrderBy(r => r.ClientName).FirstOrDefault();
			this.ViewData["ClientID"] = ClientRepo.GetAll().OrderBy(c => c.ClientName).Select(c => new SelectListItem { Value = c.ClientID.ToString(), Text = c.ClientName, Selected = c.ClientID == client.ClientID });
			var items = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "Not Applicable" }
			};
			client.GetProjects().ForEach(p => items.Add(new SelectListItem { Value = p.ProjectID.ToString(), Text = p.ProjectName }));
			this.ViewData["ProjectID"] = items;
			//this.ViewData["InvoiceContactID"] = client.GetContacts().Select(c => new SelectListItem { Value = c.ContactID.ToString(), Text = Enabill.Helpers.DecryptString(c.ContactName, Enabill.Code.Constants.PASSPHRASE) });
			this.ViewData["InvoiceContactID"] = client.GetContacts().Select(c => new SelectListItem { Value = c.ContactID.ToString(), Text = Enabill.Helpers.DecryptString(c.ContactName, "XyQEz%gyo*%o") }); //Replace with line above when code is merged
			this.ViewData["PrintOptionTypeID"] = PrintOptionRepo.GetPrintOptionsExtendedNames().Select(o => new SelectListItem { Text = o.Value, Value = o.Key.ToString() });
			this.ViewData["PrintLayoutTypeID"] = PrintOptionRepo.GetPrintLayoutsExtendedNames().Select(o => new SelectListItem { Text = o.Value, Value = o.Key.ToString() });
			this.ViewData["PrintTicketRemarkOptions"] = PrintTicketRemarkOptionRepo.GetPrintTicketRemarksOptionsExtendedNames().Select(o => new SelectListItem { Text = o.Value, Value = o.Key.ToString() });
		}

		[HttpPost]
		public ActionResult ContactListLookup(int clientID)
		{
			var client = ClientRepo.GetByID(clientID);

			var list = new List<SelectListItem>();

			foreach (var contact in client.GetContacts().ToList())
			{
				//contact.ContactName = Enabill.Helpers.DecryptString(contact.ContactName, Enabill.Code.Constants.PASSPHRASE);
				contact.ContactName = Enabill.Helpers.DecryptString(contact.ContactName, "XyQEz%gyo*%o"); //Replace with line above when code is merged
				list.Add(new SelectListItem { Value = contact.ContactID.ToString(), Text = contact.ContactName });
			}

			list.Sort((x, y) => x.Text.CompareTo(y.Text));

			return this.Json(list);
		}

		[HttpPost]
		public ActionResult ProjectListLookup(int clientID)
		{
			var client = ClientRepo.GetByID(clientID);

			var list = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "Not Applicable" }
			};

			foreach (var project in client.GetProjects().ToList())
				list.Add(new SelectListItem { Value = project.ProjectID.ToString(), Text = project.ProjectName });

			return this.Json(list);
		}

		[HttpPost]
		public ActionResult AddEditCreditView(double hours, double hourlyRate, double creditAmount)
		{
			try
			{
				this.ViewBag.Hours = hours;
				this.ViewBag.HourlyRate = hourlyRate;
				this.ViewBag.GrossAmount = hours * hourlyRate;
				this.ViewBag.CreditedAmount = creditAmount;

				return this.PartialView("ucManageInvoiceCredit");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion LOOKUPS

		#region TIME APPROVAL

		[HttpPost]
		public ActionResult ApproveTime(string selectedInvoicesToBeApproved)
		{
			try
			{
				foreach (int invoiceID in selectedInvoicesToBeApproved.ToIntArray())
				{
					var invoice = Invoice.GetByID(this.CurrentUser, invoiceID);

					if (invoice == null)
						throw new EnabillDomainException("The requested invoice could not be located in our records.");

					invoice.ApproveTime(this.CurrentUser);
				}

				return this.RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion TIME APPROVAL

		[HttpPost]
		public ActionResult ChangeDateSelector(int toChangeTo)
		{
			try
			{
				this.CurrentUser.ChangeInvoiceIndexDateSelector(this.CurrentUser, toChangeTo);
				Settings.Current.CurrentUser = UserRepo.GetByID(this.CurrentUserID);

				return this.Content("1");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}
	}
}