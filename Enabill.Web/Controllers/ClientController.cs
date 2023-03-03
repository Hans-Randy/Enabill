using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Enabill.Emailing;
using Enabill.Models;
using Enabill.Properties;
using Enabill.Repos;
using Enabill.Repository.Interfaces;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class ClientController : SearchableController
	{
		private IClientRepository clientRepository;

		protected override string GetSearchLabelText() => "clients";

		public ClientController()
		{
		}

		public ClientController(IClientRepository clientRepository)
		{
			this.clientRepository = clientRepository;
		}

		public override ActionResult Index(string q, bool? isActive)
		{
			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator))
				return this.View("CustomError", new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			this.ViewBag.q = InputHistory.Get(q, HistoryItemType.ClientSearchCriteria);
			this.ViewBag.Client_b = InputHistory.Get(isActive, HistoryItemType.ClientFilterBy, true);
			
			SetViewData(this.ViewBag.Client_b);

			List<Client> model = ClientRepo.FilterByAll(this.ViewBag.q, this.ViewBag.Client_b);

			SaveAllInput(HistoryItemType.ClientSearchCriteria, this.ViewBag.q, HistoryItemType.ClientFilterBy, this.ViewBag.Client_b);
	
			return this.View(model.OrderBy(c => c.ClientName).ToList());
		
		}

		public ActionResult IndexResult(string q, bool? isActive)
		{
			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator))
				return this.View("CustomError", new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			this.SetViewData(isActive);

			this.ViewBag.q = InputHistory.Get(q, HistoryItemType.ClientSearchCriteria);
			this.ViewBag.Client_b = InputHistory.Get(isActive, HistoryItemType.ClientFilterBy, true);
			List<Client> model = ClientRepo.FilterByAll(this.ViewBag.q, this.ViewBag.Client_b);
		
			SaveAllInput(HistoryItemType.ClientSearchCriteria, this.ViewBag.q, HistoryItemType.ClientFilterBy, this.ViewBag.Client_b);

			return this.View("Index", model);
		}

		[HttpPost]
		public ActionResult RefreshList(string q, bool isActive)
		{
			this.SetViewData(isActive);
			
			this.ViewBag.q = InputHistory.Get(q, HistoryItemType.ClientSearchCriteria);
			this.ViewBag.Client_b = InputHistory.Get(isActive, HistoryItemType.ClientFilterBy, true);
			List<Client> model = ClientRepo.FilterByAll(this.ViewBag.q, this.ViewBag.Client_b);

			SaveAllInput(HistoryItemType.ClientSearchCriteria, this.ViewBag.q, HistoryItemType.ClientFilterBy, this.ViewBag.Client_b);

			return this.PartialView("Index", model);
		}

		private void SetViewData(bool? isActive = true) => this.ViewData["StatusFilter"] = new List<SelectListItem>
			{
				new SelectListItem() { Value = "1", Text = "Active", Selected = isActive == true },
				new SelectListItem() { Value = "0", Text = "Inactive", Selected = isActive == false }

			};

		//tulisa
		private void SetViewDataList(Client model)=> this.AddAsDropdownSource(CurrencyTypeRepo.GetAll().OrderBy(r => r.CurrencyISO).ToList(), "CurrencyType", "CurrencyTypeID","CurrencyISO",model.CurrencyTypeID);
			
		

		[HttpGet]
		public ActionResult Details(int id)
		{
			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			var client = ClientRepo.GetByID(id);

			if (client == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoUser_Message));

			return this.View(new ClientEditModel(client));
		}

		[HttpGet]
		public ActionResult Create()
		{
			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			//tulisa
			var client = Client.GetNew("", true);
			client.CurrencyTypeID = 1;
			this.SetViewDataList(client);
	
			return this.View(client);
		}

		[HttpPost]
		public ActionResult Create(FormCollection collection)
		{

			try
			{
				var client = ClientRepo.GetByName(collection["ClientName"]);

				if (client != null)
					throw new EnabillConsumerException("A Client by this name already exists.");

					return this.Save(Client.GetNew("", true));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult Edit(int id)
		{
			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			var client = ClientRepo.GetByID(id);

			if (client == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoClient_Message));

			var model = new ClientEditModel(client);

			this.SetViewDataList(client);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult Edit(int id, FormCollection collection)
		{
			try
			{
				var client = ClientRepo.GetByID(id);

				if (client == null)
					return this.ErrorView(new ErrorModel(Resources.ERR_NoClient_Message));
				else
				{
					var client2 = ClientRepo.GetByName(collection["ClientName"]);

					if (client2 != null)
					{
						if (client2.ClientID != id)
						{
							throw new EnabillConsumerException("A Client by this name already exists.");
						}
					}

					this.SetViewDataList(client);

					//Default client support email address to be captured at client level
					//If a project has its own unique support email address, capture it on project level
					//An support email address cannot be referenced more than once.
					client.SupportEmailAddress = collection["SupportEmailAddress"];

					return this.Save(client);
				}
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		private ActionResult Save(Client client)
		{
			bool isNew = client.ClientID == 0;

			try
			{
				if (client.SupportEmailAddress != null && client.SupportEmailAddress.Trim() != "")
				{
					var supportEmail = ProjectRepo.GetBySupportEmailAddress(client.SupportEmailAddress);
					string errorMessage = "";

					//throw an exception if the support email is used on
					if (supportEmail != null)
					{
						if (supportEmail.ClientID != client.ClientID)
						{
							errorMessage = "Support Email Address, " + client.SupportEmailAddress + ", is already referenced on Client " + ClientRepo.GetByID(supportEmail.ClientID).ClientName + ". Please specify another support email address. Save cancelled.";
						}
						else if (supportEmail.ClientID == client.ClientID && supportEmail.ProjectID != 0)
						{
							errorMessage = "Support Email Address, " + client.SupportEmailAddress + ", is already referenced on Project " + ClientRepo.GetByID(supportEmail.ClientID).ClientName + ":" + ProjectRepo.GetByID(supportEmail.ProjectID).ProjectName + ". Please specify another support email address. Save cancelled.";
						}

						if (errorMessage != "")
							throw new EnabillConsumerException(errorMessage);
					}

					//check that its a valid support email address
					//if (string.Equals(client.SupportEmailAddress, "enabill@saratoga.co.za", StringComparison.OrdinalIgnoreCase) || (!(client.SupportEmailAddress.IndexOf("enabill@saratoga.co.za", StringComparison.OrdinalIgnoreCase) >= 0) && !(client.SupportEmailAddress.IndexOf("enabill@saratoga.co.za", StringComparison.OrdinalIgnoreCase) >= 0)))
					if (string.Equals(client.SupportEmailAddress, Enabill.Code.Constants.EMAILADDRESSSUPPORT, StringComparison.OrdinalIgnoreCase))
					{
						errorMessage = "<b>" + client.SupportEmailAddress + "</b> is not a valid Client Enabill Helpdesk Email Address as this the general support Email address. ";
					}
					else
					{
						if (!(client.SupportEmailAddress.IndexOf(Enabill.Code.Constants.EMAILDOMAIN, StringComparison.OrdinalIgnoreCase) >= 0))
						{
							errorMessage = "<b>" + client.SupportEmailAddress + $"</b> is not a valid Enabill Helpdesk Email Address. Must contain <b>'...{Enabill.Code.Constants.EMAILDOMAIN}'</b>. ";
						}
					}

					if (errorMessage != "")
						throw new EnabillConsumerException(errorMessage);
				}

				this.UpdateModel(client);

				client.Save(this.CurrentUser);

				if (!isNew)
					return this.PartialView("ucClientDetails", client);

				//Send email to accountants, notifying of new client
				EnabillEmails.NotifyAccountantsAboutNewClient(client);

				return this.Json(new
				{
					IsError = false,
					Description = "New client was saved successfully.",
					Url = "/Client/Edit/" + client.ClientID
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult ActivateClient(int clientID)
		{
			try
			{
				var client = ClientRepo.GetByID(clientID);

				if (client == null)
					throw new EnabillConsumerException(Resources.ERR_NoClient_Message);

				client.Activate(this.CurrentUser);

				return this.Json(new
				{
					IsError = false,
					Description = client.ClientName + " was activitated successfully."
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult DeactivateClient(int clientID)
		{
			try
			{
				var client = ClientRepo.GetByID(clientID);

				if (client == null)
					throw new EnabillConsumerException(Resources.ERR_NoClient_Message);

				client.Deactivate(this.CurrentUser);

				return this.Json(new
				{
					IsError = false,
					Description = $"{client.ClientName} was deactivitated successfully.<br/><br/>* Note that the Client will only be<br/>flagged as Inactive after the latest<br/>Project Confirmed End Date!"
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult ListContacts(int id)
		{
			var client = ClientRepo.GetByID(id);

			if (client == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoClientForRetrieveContactList_Message));

			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			var model = new ClientEditModel(client); // This model holds a list of contacts... a generic method is used to build up this view, and that is why the whole model is passed to the view

			return this.PartialView("ucContactList", model);
		}

		[HttpGet]
		public ActionResult CreateContact(int id) // id = clientID
		{
			var client = ClientRepo.GetByID(id);

			if (client == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoClientForContact_Message));

			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			var model = client.GetNewContact();
			var contactEditModel = new ContactEditModel(this.CurrentUser, client, model);

			return this.View("EditContact", contactEditModel);
		}

		[HttpPost]
		public ActionResult CreateContact(int id, FormCollection form) // id = clientID
		{
			try
			{
				var client = ClientRepo.GetByID(id);

				if (client == null)
					throw new EnabillConsumerException(Resources.ERR_NoClientForContactCreate_Message);

				var contact = client.GetNewContact();

				this.UpdateModel(contact, "Contact");

				client.SaveContact(this.CurrentUser, contact);

				return this.Content("1");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult SaveEditContact(FormCollection form) // id = clientID
		{
			try
			{
				int? clientId = form["Contact.ClientID"].ToInt();
				int? contactId = form["Contact.ContactID"].ToInt();

				if (clientId.HasValue && contactId.HasValue)
				{
					var client = ClientRepo.GetByID(clientId.Value);

					if (client == null)
						throw new EnabillConsumerException(Resources.ERR_NoClientForContactCreate_Message);

					var contact = client.GetContact(contactId.Value);
					this.UpdateModel(contact, "Contact");

					client.SaveContact(this.CurrentUser, contact);
				}

				return this.Content("1");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult DeleteContact(int id) // id = clientID
		{
			try
			{
				var contact = ContactRepo.GetByID(id);
				int clientID = contact.ClientID;
				var client = ClientRepo.GetByID(clientID);
				ContactRepo.DeleteContact(contact);
				var model = new ClientEditModel(client);

				return this.View("Edit", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult EditContact(int id, int clientID) // id = contactID
		{
			var client = ClientRepo.GetByID(clientID);
			if (client == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoClientForContact_Message));

			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			var model = new ContactEditModel(this.CurrentUser, client, id);

			if (model == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoContact_Message));

			return this.View(model);
		}

		[HttpGet]
		public ActionResult ListDepartmentCodes(int id)
		{
			var client = ClientRepo.GetByID(id);

			var model = new ClientEditModel(client);

			return this.PartialView("ucClientDepartmentCodes", model);
		}

		[HttpPost]
		public ActionResult EditDepartmentCode(int id, int clientID)
		{
			var client = ClientRepo.GetByID(clientID);

			if (client == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoClientForContact_Message));

			var model = ClientDepartmentCodeRepo.GetByID(id);

			return this.PartialView("EditClientDepartmentCode", model);
		}

		[HttpPost]
		public ActionResult CreateDepartmentCode(int id)
		{
			var client = ClientRepo.GetByID(id);

			if (client == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoClientForContact_Message));

			var model = new ClientDepartmentCode
			{
				ClientID = id,
				DepartmentCode = "",
				IsActive = false
			};

			return this.PartialView("EditClientDepartmentCode", model);
		}

		[HttpPost]
		public ActionResult SaveDepartmentCode(FormCollection form)
		{
			try
			{
				int id = int.Parse(form["ClientDepartmentCodeID"]);
				int clientID = int.Parse(form["ClientID"]);

				var client = ClientRepo.GetByID(clientID);

				if (client == null)
					throw new EnabillConsumerException(Resources.ERR_NoClientForContactUpdate_Message);

				var clientDepartmentCode = id == 0 ? new ClientDepartmentCode() : ClientDepartmentCodeRepo.GetByID(id);
				clientDepartmentCode.DepartmentCode = form["DepartmentCode"];
				clientDepartmentCode.IsActive = form["isActive"].Contains("true");
				clientDepartmentCode.ClientID = client.ClientID;
				clientDepartmentCode.ValidateSave();
				ClientDepartmentCodeRepo.Save(clientDepartmentCode);

				var model = new ClientEditModel(client);

				return this.PartialView("ucClientDepartmentCodes", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		public ActionResult DeleteDepartmentCode(int id)
		{
			try
			{
				var clientDepartmentCode = ClientDepartmentCodeRepo.GetByID(id);
				var client = ClientRepo.GetByID(clientDepartmentCode.ClientID);

				ClientDepartmentCodeRepo.Delete(clientDepartmentCode);

				var model = new ClientEditModel(client);

				return this.PartialView("ucClientDepartmentCodes", model);
			}
			catch (EnabillException ex)
			{
				return this.ReturnJsonException(ex);
			}
		}
	}
}