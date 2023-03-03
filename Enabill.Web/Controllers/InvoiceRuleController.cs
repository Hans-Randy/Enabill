using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using Enabill.Models;
using Enabill.Properties;
using Enabill.Repos;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class InvoiceRuleController : BaseController
	{
		public ActionResult Index(FormCollection form)
		{
			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator) && !this.CurrentUser.HasRole(UserRoleType.InvoiceAdministrator))
				return this.ErrorView(new ErrorModel("Access Denied", "You do not have the required permissions to view this page.", true));

			bool hasRules = true;

			if (this.Request.HttpMethod == "POST")
				hasRules = IsCheckBoxChecked(form["hasRules"]);

			InputHistory.Set(HistoryItemType.InvRuleClientHasRule, hasRules);

			int? statusID = form["StatusFilter"].ToInt();
			bool status = true;

			status = !statusID.HasValue || statusID.Value != 0;

			InputHistory.Set(HistoryItemType.InvRuleStatus, status);
			InputHistory.Set(HistoryItemType.StatusFilter, status);

			int? clientID = form["ClientList"].ToInt();

			if (clientID.HasValue)
			{
				InputHistory.Set(HistoryItemType.StatusFilter, status);
				InputHistory.Set(HistoryItemType.InvRuleIndexClient, clientID.ToString());
				InputHistory.Set(HistoryItemType.InvRuleTimeMaterial, IsCheckBoxChecked(form[BillingMethodType.TimeMaterial.ToString()]));
				InputHistory.Set(HistoryItemType.InvRuleFixedCost, IsCheckBoxChecked(form[BillingMethodType.FixedCost.ToString()]));
				InputHistory.Set(HistoryItemType.InvRuleSLA, IsCheckBoxChecked(form[BillingMethodType.SLA.ToString()]));
				InputHistory.Set(HistoryItemType.InvRuleTravel, IsCheckBoxChecked(form[BillingMethodType.Travel.ToString()]));
				InputHistory.Set(HistoryItemType.InvRuleMonthlyFixedCost, IsCheckBoxChecked(form[BillingMethodType.MonthlyFixedCost.ToString()]));
				InputHistory.Set(HistoryItemType.InvRuleActivityFixedCost, IsCheckBoxChecked(form[BillingMethodType.ActivityFixedCost.ToString()]));
			}
			else
			{
				clientID = 0;
			}

			var items = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All Clients" }
			};

			Client.GetAllActiveClients().ForEach(c => items.Add(new SelectListItem() { Value = c.ClientID.ToString(), Text = c.ClientName, Selected = c.ClientID == InputHistory.Get(HistoryItemType.InvRuleIndexClient, 0) }));
			this.ViewData["ClientList"] = items;

			this.ViewData["StatusFilter"] = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "Inactive", Selected = !InputHistory.Get(HistoryItemType.StatusFilter, false) },
				new SelectListItem() { Value = "1", Text = "Active", Selected = InputHistory.Get(HistoryItemType.StatusFilter, true) }
			};

			return this.View(new InvoiceRuleIndexModel(clientID.Value));
		}

		[HttpPost]
		public ActionResult RuleDefaults()
		{
			// Clients
			var items = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "--Select a Client--" }
			};

			Client.GetAllActiveClients().ForEach(c => items.Add(new SelectListItem() { Value = c.ClientID.ToString(), Text = c.ClientName, Selected = c.ClientID == 0 }));

			this.ViewData["ClientRuleList"] = items;

			// Projects
			this.ViewData["ProjectList"] = new List<SelectListItem>();

			//this.ViewData["ProjectList"] = new List<SelectListItem>
			//{
			//	new SelectListItem() { Value = "0", Text = "--Select a Project--" }
			//};

			return this.PartialView("ucSetUpInvoiceRuleDefaults");
		}

		[HttpPost]
		public ActionResult RuleClientProject(int clientID, int billingMethodID, int projectID)
		{
			this.ViewData["ClientRuleList"] = Client.GetAllActiveClients().Where(c => c.IsActive).Select(c => new SelectListItem { Value = c.ClientID.ToString(), Text = c.ClientName, Selected = c.ClientID == clientID });

			this.ViewData["ProjectList"] = new List<SelectListItem>();

			return this.PartialView("ucSetUpInvoiceRuleDefaults");
		}

		[HttpGet]
		public ActionResult Create(int clientID, int projectID) //id = ClientID
		{
			if (!this.CurrentUser.HasRole(UserRoleType.ProjectOwner) && !this.CurrentUser.HasRole(UserRoleType.InvoiceAdministrator))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			// Client
			var client = ClientRepo.GetByID(clientID);

			if (client == null)
				return this.ErrorView(new ErrorModel("Client could not be found", "The selected client could not be found. Please refresh your browser and try again."));

			// Project
			var project = ProjectRepo.GetByID(projectID);

			if (project == null)
				return this.ErrorView(new ErrorModel("Project could not be found."));

			// Billing Method
			int billingMethod = ProjectRepo.GetBillingMethodByProjectID(projectID).BillingMethodID;

			if (billingMethod < 1)
				return this.ErrorView(new ErrorModel("Billing Method for Project could not be found."));
			else if (billingMethod == 32) // Non-billable
			{
				return this.ErrorView(new ErrorModel("Project is non-billable."));
			}

			var invRule = InvoiceRule.GetDefaultRule(this.CurrentUser, client, project, (BillingMethodType)billingMethod);
			var model = new InvoiceRuleModel(invRule, client, project);
			this.SetViewDataLists(invRule, client);

			this.ViewData["ProjectID"] = projectID;

			switch (invRule.BillingMethodID)
			{
				case (int)BillingMethodType.FixedCost:
					return this.View("CreateFixedCostRule", model);

				case (int)BillingMethodType.MonthlyFixedCost:
					return this.View("CreateMonthlyFixedCostRule", model);

				case (int)BillingMethodType.ActivityFixedCost:
					return this.View("ActivityFixedCostRule", model);

				case (int)BillingMethodType.TimeMaterial:
					return this.View("TimeMaterialRule", model);

				case (int)BillingMethodType.SLA:
					return this.View("SLARule", model);

				default:
					throw new InvoiceRuleException("Not valid");
			}
		}

		[HttpGet]
		public ActionResult Edit(int id) //id = InvRuleID
		{
			if (!this.CurrentUser.HasRole(UserRoleType.ProjectOwner) && !this.CurrentUser.HasRole(UserRoleType.InvoiceAdministrator))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			var invRule = InvoiceRule.GetByID(id);

			if (invRule == null)
				return this.ErrorView(new ErrorModel("The selected Invoice Rule could not be found."));

			var model = new InvoiceRuleModel(invRule, invRule.Client, invRule.Project);
			this.SetViewDataLists(invRule, invRule.Client);

			switch (invRule.BillingMethodID)
			{
				case (int)BillingMethodType.FixedCost:
					return this.View("EditFixedCostRule", model);

				case (int)BillingMethodType.MonthlyFixedCost:
					return this.View("EditMonthlyFixedCostRule", model);

				case (int)BillingMethodType.ActivityFixedCost:
					return this.View("ActivityFixedCostRule", model);

				case (int)BillingMethodType.TimeMaterial:
					return this.View("TimeMaterialRule", model);

				case (int)BillingMethodType.SLA:
					return this.View("SLARule", model);

				default:
					throw new InvoiceRuleException("Not valid");
			}
		}

		[HttpGet]
		public ActionResult EditFromInvoice(int id)
		{
			var invoice = Invoice.GetByID(this.CurrentUser, id);

			if (invoice?.InvoiceRuleID.HasValue != true)
				return this.ErrorView(new ErrorModel("The selected Invoice Rule could not be found."));

			return this.Edit(invoice.InvoiceRuleID.Value);
		}

		[HttpPost]
		public ActionResult Delete(int id)
		{
			try
			{
				var rule = InvoiceRule.GetByID(id);
				rule.Delete(this.CurrentUser);

				return this.ReturnJsonResult(false, "Invoice rule deleted successfully.");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#region TIME AND MATERIAL

		[HttpPost]
		public ActionResult CreateTMRule(FormCollection form)//id = invoiceRuleID
		{
			try
			{
				var client = ClientRepo.GetByID(int.Parse(form["ClientID"]));
				var project = ProjectRepo.GetByID(int.Parse(form["ProjectID"]));

				if (client == null)
					throw new EnabillConsumerException("An unknown error occurred while trying to locate the selected client. Please try again.");

				var invRule = InvoiceRule.GetDefaultRule(this.CurrentUser, client, project, BillingMethodType.TimeMaterial);
				this.TryUpdateModel(invRule);

				using (var ts = new TransactionScope())
				{
					invRule.Save(this.CurrentUser);
					invRule.AddActivities(form["ActivityList"]);
					invRule.AddBccContacts(form["ContactList"]);

					ts.Complete();
				}

				//once invoice is created, return the new url to nav to
				return this.Json(new
				{
					IsError = false,
					url = "/InvoiceRule/Edit/" + invRule.InvoiceRuleID
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "An unknown error was detected. Please retry.");
			}
		}

		[HttpPost]
		public ActionResult EditTMRule(int id, FormCollection form) //id = invoiceRuleID
		{
			try
			{
				var invRule = InvoiceRule.GetByID(id);
				int? ticketRemarkOptionID = form["PrintTicketRemarkOptions"].ToInt();
				invRule.PrintTicketRemarkOptionID = ticketRemarkOptionID.Value;

				if (invRule == null)
					throw new EnabillConsumerException("An unknown error occurred while trying to locate the selected invoice rule. Please try again.");

				this.TryUpdateModel(invRule);

				using (var ts = new TransactionScope())
				{
					invRule.Save(this.CurrentUser);
					invRule.UpdateActivities(form["ActivityList"]);
					invRule.UpdateBccContacts(form["ContactList"]);

					ts.Complete();
				}

				return this.Json(new
				{
					IsError = false,
					url = "/InvoiceRule/Edit/" + invRule.InvoiceRuleID
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "Unknown error was detected. Please try again.");
			}
		}

		#endregion TIME AND MATERIAL

		#region FIXED COST

		[HttpPost]
		public ActionResult CreateFixedCostRule(FormCollection form)//id = invoiceRuleID
		{
			try
			{
				var client = ClientRepo.GetByID(int.Parse(form["ClientID"]));
				var project = ProjectRepo.GetByID(int.Parse(form["ProjectID"]));

				if (client == null || project == null || client.ClientID != project.ClientID)
					throw new EnabillConsumerException("Errors were detected with the selected client and the selected project. Please try again.");
				var invRule = InvoiceRule.GetDefaultRule(this.CurrentUser, client, project, BillingMethodType.FixedCost);

				this.TryUpdateModel(invRule);
				//throw new EnabillConsumerException("An unknown error occured while trying to valid the information inserted. Please revise your inputs.");

				string invoiceVal = form["InvoiceAmountExclVAT"].Replace(",", "").Replace(" ", "");
				invRule.InvoiceAmountExclVAT = double.Parse(invoiceVal);
			

				using (var ts = new TransactionScope())
				{
					invRule.Save(this.CurrentUser);
					invRule.GenerateInvoiceRuleLines(this.CurrentUser);
					invRule.AddBccContacts(form["ContactList"]);

					ts.Complete();
				}

				return this.Json(new
				{
					IsError = false,
					url = "/InvoiceRule/Edit/" + invRule.InvoiceRuleID
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult EditFixedCostRule(int id, FormCollection form) //id = invoiceRuleID
		{
			try
			{
				var invRule = InvoiceRule.GetByID(id);
				int? ticketRemarkOptionID = form["PrintTicketRemarkOptions"].ToInt();
				invRule.PrintTicketRemarkOptionID = ticketRemarkOptionID.Value;

				if (invRule == null)
					throw new EnabillConsumerException("An error occurred when trying to retrieve the selected invoice rule. Please try again.");

				if (!this.TryUpdateModel(invRule))
					throw new EnabillConsumerException("An unknown error occured while trying to valid the information inserted. Please revise your inputs.");

				if (invRule.InvoiceRuleID == 0)
				{
					string invoiceVal = form["InvoiceAmountExclVAT"].Replace(",", "").Replace(" ", "");
					invRule.InvoiceAmountExclVAT = double.Parse(invoiceVal);
					invRule.AccrualPeriods = int.Parse(form["AccrualPeriods"]);
				}

				using (var ts = new TransactionScope())
				{
					invRule.Save(this.CurrentUser);
					invRule.UpdateBccContacts(form["ContactList"]);

					ts.Complete();
				}

				return this.Json(new
				{
					IsError = false,
					url = "/InvoiceRule/Edit/" + invRule.InvoiceRuleID
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult SaveFixedCostLines(int id, FormCollection form) //id = invRuleID
		{
			try
			{
				var invRule = InvoiceRule.GetByID(id);

				if (invRule == null)
					throw new EnabillConsumerException("An unknown error occurred while trying to located the selected invoice rule. Please try again.");

				var lines = invRule.InvoiceRuleLines.ToList();

				foreach (var line in lines)
				{
					line.CustomerAmount = double.Parse(form["CustAmt|" + line.InvoiceRuleLineID]);
					line.AccrualAmount = double.Parse(form["AccrualAmt|" + line.InvoiceRuleLineID]);
				}

				invRule.SaveInvoiceRuleLines(lines);

				return this.ReturnJsonResult(false, "Lines were updated successfully.");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion FIXED COST

		#region MONTHLY FIXED COST

		[HttpPost]
		public ActionResult CreateMonthlyFixedCostRule(FormCollection form)
		{
			try
			{
				var client = ClientRepo.GetByID(int.Parse(form["ClientID"]));
				var project = ProjectRepo.GetByID(int.Parse(form["ProjectID"]));

				if (client == null || project == null || client.ClientID != project.ClientID)
					throw new EnabillConsumerException("Errors were detected with the selected client and the selected project. Please try again.");

				var invRule = InvoiceRule.GetDefaultRule(this.CurrentUser, client, project, BillingMethodType.MonthlyFixedCost);

				this.TryUpdateModel(invRule);

				string invoiceVal = form["InvoiceAmountExclVAT"].Replace(",", "").Replace(" ", "");
				invRule.InvoiceAmountExclVAT = double.Parse(invoiceVal);

				using (var ts = new TransactionScope())
				{
					invRule.Save(this.CurrentUser);
					invRule.AddBccContacts(form["ContactList"]);

					ts.Complete();
				}

				return this.Json(new
				{
					IsError = false,
					url = "/InvoiceRule/Edit/" + invRule.InvoiceRuleID
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult EditMonthlyFixedCostRule(int id, FormCollection form) //id = invoiceRuleID
		{
			try
			{
				var invRule = InvoiceRule.GetByID(id);
				int? ticketRemarkOptionID = form["PrintTicketRemarkOptions"].ToInt();
				invRule.PrintTicketRemarkOptionID = ticketRemarkOptionID.Value;

				if (invRule == null)
					throw new EnabillConsumerException("An error occurred when trying to retrieve the selected invoice rule. Please try again.");

				this.TryUpdateModel(invRule);
				//throw new EnabillConsumerException("An unknown error occured while trying to valid the information inserted. Please revise your inputs.");

				string invoiceVal = form["InvoiceAmountExclVAT"].Replace(",", "").Replace(" ", "");
				invRule.InvoiceAmountExclVAT = double.Parse(invoiceVal);

				using (var ts = new TransactionScope())
				{
					invRule.Save(this.CurrentUser);
					invRule.UpdateBccContacts(form["ContactList"]);
					ts.Complete();
				}

				return this.Json(new
				{
					IsError = false,
					url = "/InvoiceRule/Edit/" + invRule.InvoiceRuleID
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion MONTHLY FIXED COST

		#region ACTIVITY FIXED COST

		[HttpPost]
		public ActionResult CreateActivityFixedCostRule(FormCollection form)
		{
			try
			{
				var client = ClientRepo.GetByID(int.Parse(form["ClientID"]));
				var project = ProjectRepo.GetByID(int.Parse(form["ProjectID"]));

				if (client == null)
					throw new EnabillConsumerException("Errors were detected with the selected client. Please try again.");

				var invRule = InvoiceRule.GetDefaultRule(this.CurrentUser, client, project, BillingMethodType.ActivityFixedCost);

				this.TryUpdateModel(invRule);

				using (var ts = new TransactionScope())
				{
					invRule.Save(this.CurrentUser);
					invRule.UpdateActivities(form["ActivityList"]);
					invRule.UpdateBccContacts(form["ContactList"]);

					ts.Complete();
				}

				return this.Json(new
				{
					IsError = false,
					url = "/InvoiceRule/Edit/" + invRule.InvoiceRuleID
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult EditActivityFixedCostRule(int id, FormCollection form) //id = invoiceRuleID
		{
			try
			{
				var invRule = InvoiceRule.GetByID(id);

				if (invRule == null)
					throw new EnabillConsumerException("An error occurred when trying to retrieve the selected invoice rule. Please try again.");

				this.TryUpdateModel(invRule);
				//throw new EnabillConsumerException("An unknown error occured while trying to valid the information inserted. Please revise your inputs.");

				using (var ts = new TransactionScope())
				{
					invRule.Save(this.CurrentUser);
					invRule.UpdateActivities(form["ActivityList"]);
					invRule.UpdateBccContacts(form["ContactList"]);
					ts.Complete();
				}

				return this.Json(new
				{
					IsError = false,
					url = "/InvoiceRule/Edit/" + invRule.InvoiceRuleID
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion ACTIVITY FIXED COST

		#region SLA

		[HttpPost]
		public ActionResult CreateSLARule(FormCollection form)//id = invoiceRuleID
		{
			try
			{
				var client = ClientRepo.GetByID(int.Parse(form["ClientID"]));
				var project = ProjectRepo.GetByID(int.Parse(form["ProjectID"]));

				if (client == null)
					throw new EnabillConsumerException("An unknown error occurred while trying to locate the selected client. Please try again.");

				var invRule = InvoiceRule.GetDefaultRule(this.CurrentUser, client, project, BillingMethodType.SLA);

				this.TryUpdateModel(invRule);
				//throw new EnabillConsumerException("An unknown error occured while trying to valid the information inserted. Please revise your inputs.");

				string invoiceVal = form["InvoiceAmountExclVAT"].Replace(",", "").Replace(" ", "");
				invRule.InvoiceAmountExclVAT = double.Parse(invoiceVal);

				using (var ts = new TransactionScope())
				{
					invRule.Save(this.CurrentUser);
					invRule.AddActivities(form["ActivityList"]);
					invRule.AddBccContacts(form["ContactList"]);

					ts.Complete();
				}

				return this.Json(new
				{
					IsError = false,
					url = "/InvoiceRule/Edit/" + invRule.InvoiceRuleID
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult EditSLARule(int id, FormCollection form) //id = invoiceRuleID
		{
			try
			{
				var invRule = InvoiceRule.GetByID(id);
				int? ticketRemarkOptionID = form["PrintTicketRemarkOptions"].ToInt();
				invRule.PrintTicketRemarkOptionID = ticketRemarkOptionID.Value;

				if (invRule == null)
					throw new EnabillConsumerException("An error occurred when trying to locate the selected invoice rule. Please try again.");

				this.TryUpdateModel(invRule);
				//throw new EnabillConsumerException("An unknown error occured while trying to valid the information inserted. Please revise your inputs.");

				invRule.InvoiceAdditionalHours = IsCheckBoxChecked(form["InvoiceAdditionalHours"]);
				invRule.InvoiceAmountExclVAT = double.Parse(form["InvoiceAmountExclVAT"]);
				invRule.Save(this.CurrentUser);
				invRule.UpdateActivities(form["ActivityList"]);
				invRule.UpdateBccContacts(form["ContactList"]);

				return this.Json(new
				{
					IsError = false,
					url = "/InvoiceRule/Edit/" + invRule.InvoiceRuleID
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion SLA

		#region INVOICE

		[HttpPost]
		public ActionResult CreateInvoiceFromRule(int id) // id = invoiceRuleID
		{
			var invRule = InvoiceRule.GetByID(id);

			if (invRule == null)
				return new HttpNotFoundResult();

			try
			{
				var invoice = invRule.CreateInvoice(this.CurrentUser, DateTime.Now);
				invoice.ProcessInvoice();

				return this.Json(new
				{
					IsError = false,
					url = "/Invoice/Edit/" + invoice.InvoiceID
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion INVOICE

		#region VIEW SETUP

		public void SetViewDataLists(InvoiceRule invRule, Client client)
		{
			const int billingMethodIDs = (int)BillingMethodType.FixedCost
								 + (int)BillingMethodType.MonthlyFixedCost
								 + (int)BillingMethodType.TimeMaterial
								 //+ (int)BillingMethodType.Travel
								 + (int)BillingMethodType.SLA;

			this.ViewData["BillingMethodID"] = BillingMethodRepo.GetListByID(billingMethodIDs)
					.Select(b => new SelectListItem { Value = b.BillingMethodID.ToString(), Text = b.BillingMethodName });

			this.ViewData["DefaultContactID"] = client.GetContacts()
					.Select(c => new SelectListItem { Value = c.ContactID.ToString(), Text = c.ContactName, Selected = c.ContactID == invRule.DefaultContactID });

			this.ViewData["InvoiceDay"] = PopulateInvoiceDate()
					.Select(d => new SelectListItem { Text = d.Value, Value = d.Key.ToString(), Selected = d.Key == (invRule.InvoiceDay ?? 1) });

			this.ViewData["InvoiceSubCategoryID"] = InvoiceSubCategoryRepo.GetInvoiceSubCategoriesExtendedNames()
					.Select(isc => new SelectListItem { Text = isc.Value, Value = isc.Key.ToString(), Selected = isc.Key == invRule.InvoiceSubCategoryID });

			this.ViewData["PrintOptionTypeID"] = PrintOptionRepo.GetPrintOptionsExtendedNames()
					.Select(o => new SelectListItem { Text = o.Value, Value = o.Key.ToString(), Selected = o.Key == invRule.PrintOptionTypeID });

			this.ViewData["PrintLayoutTypeID"] = PrintOptionRepo.GetPrintLayoutsExtendedNames()
					.Select(o => new SelectListItem { Text = o.Value, Value = o.Key.ToString(), Selected = o.Key == invRule.PrintLayoutTypeID });

			this.ViewData["PrintTicketRemarkOptions"] = PrintTicketRemarkOptionRepo.GetPrintTicketRemarksOptionsExtendedNames().Select(o => new SelectListItem { Text = o.Value, Value = o.Key.ToString(), Selected = o.Key == invRule.PrintTicketRemarkOptionID });
		}

		private static Dictionary<int, string> PopulateInvoiceDate()
		{
			var days = new Dictionary<int, string>();

			for (int k = 1; k <= 31; k++)
				days.Add(k, k == 31 ? "Last Day of Month" : k.ToString());

			return days;
		}

		#endregion VIEW SETUP

		#region LOOKUPS

		[HttpPost]
		public JsonResult ProjectListLookup(int clientID)
		{
			var client = ClientRepo.GetByID(clientID);

			//var list = new List<SelectListItem>({ Value = "0", Text = "--Select a Project--" });

			//list.Add(new SelectListItem { Value = "0", Text = "--Select a Project--" };

			var list = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "--Select a Project--" }
			};

			foreach (var project in client.GetProjects().Where(p => p.ClientID == clientID))
				list.Add(new SelectListItem { Value = project.ProjectID.ToString(), Text = project.ProjectName });

			return this.Json(list);
		}

		[HttpPost]
		public JsonResult BillingMethodLookup(int projectID)
		{
			string billingMethodName = ProjectRepo.GetBillingMethodByProjectID(projectID).BillingMethodName;

			return this.Json(billingMethodName);
		}

		[HttpPost]
		public ActionResult GetNewLines(int id, string amount, int periods)
		{
			try
			{
				var invRule = InvoiceRule.GetByID(id);

				if (invRule == null)
					throw new EnabillConsumerException("An error occured when trying to locate the current invoice rule. Please try again.");

				invRule.InvoiceAmountExclVAT = double.Parse(amount);
				invRule.AccrualPeriods = periods;

				using (var ts = new TransactionScope())
				{
					invRule.Save(this.CurrentUser);
					invRule.GenerateInvoiceRuleLines(this.CurrentUser);
					ts.Complete();
				}

				return this.PartialView("ucFixedCostLines", invRule.InvoiceRuleLines);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion LOOKUPS
	}
}