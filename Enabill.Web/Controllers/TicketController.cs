using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Enabill.Emailing;
using Enabill.Models;
using Enabill.Repos;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class TicketController : BaseController
	{
		#region VIEW TICKETS

		[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
		public ActionResult Index(int? id)
		{
			int ticketTypeFilter = InputHistory.Get(HistoryItemType.TicketTypeFilter, 0);
			var dateFrom = InputHistory.GetDateTime(HistoryItemType.TicketDateFrom, DateTime.Today).Value;
			var dateTo = InputHistory.GetDateTime(HistoryItemType.TicketDateTo, DateTime.Today).Value;

			var user = Settings.Current.CurrentUser;
			int filterBy = user.HasRole(UserRoleType.TicketManager) || user.HasRole(UserRoleType.SystemAdministrator) ? 0 : 1;

			var model = new TicketIndexModel(user, TicketRepo.GetFirstOpenTicketDate(), DateTime.Today, 0, filterBy);
			var firstTicket = TicketRepo.GetByID(model.TicketList.FirstTicketID);
			this.SetViewData(firstTicket);

			if (id != null)
			{
				this.ViewTickets(InputHistory.Get(HistoryItemType.ClientID, 0), InputHistory.Get(HistoryItemType.StatusID, 0), (DateTime)InputHistory.GetDateTime(HistoryItemType.TicketDateFrom, dateFrom), (DateTime)InputHistory.GetDateTime(HistoryItemType.TicketDateTo, dateTo), InputHistory.Get(HistoryItemType.TicketTypeFilter, 0), InputHistory.Get(HistoryItemType.TicketFilterBy, filterBy), (int)id);
				this.ViewTicketLines(InputHistory.Get(HistoryItemType.TicketID, 0));
			}

			return this.View(model);
		}

		[HttpPost]
		public ActionResult RefreshTickets(DateTime dateFrom, DateTime dateTo, int ticketType, int filterBy)
		{
			var user = Settings.Current.CurrentUser;
			InputHistory.Set(HistoryItemType.TicketDateFrom, dateFrom);
			InputHistory.Set(HistoryItemType.TicketDateTo, dateTo);
			InputHistory.Set(HistoryItemType.TicketTypeFilter, ticketType);
			InputHistory.Set(HistoryItemType.TicketFilterBy, filterBy);
			var model = new TicketIndexModel(user, dateFrom, dateTo, ticketType, filterBy);
			var firstTicket = TicketRepo.GetByID(model.TicketList.FirstTicketID);
			this.SetViewData(firstTicket);

			return this.PartialView("ucIndex", model);
		}

		[HttpPost]
		public ActionResult ViewTickets(int clientID, int statusID, DateTime dateFrom, DateTime dateTo, int ticketType, int filterBy, int? id)
		{
			var user = Settings.Current.CurrentUser;

			if (id == null)
			{
				InputHistory.Set(HistoryItemType.ClientID, clientID);
				InputHistory.Set(HistoryItemType.StatusID, statusID);
				InputHistory.Set(HistoryItemType.TicketDateFrom, dateFrom);
				InputHistory.Set(HistoryItemType.TicketDateTo, dateTo);
				InputHistory.Set(HistoryItemType.TicketTypeFilter, ticketType);
				InputHistory.Set(HistoryItemType.TicketFilterBy, filterBy);
			}

			var model = new TicketIndexModel(user, dateFrom, dateTo, ticketType, filterBy);

			return this.PartialView("ucViewTickets", new TicketInstanceModel(this.CurrentUser, InputHistory.Get(HistoryItemType.ClientID, clientID), InputHistory.Get(HistoryItemType.StatusID, statusID), dateFrom, dateTo, ticketType, filterBy));
		}

		[HttpGet]
		public ActionResult ViewDetails(string ticketReference)
		{
			var ticket = TicketRepo.GetByReference(ticketReference);
			var model = new TicketLineModel(ticket.TicketID);
			this.SetViewData(ticket);

			return this.View("ViewDetails", model);
		}

		[HttpPost]
		public ActionResult ViewTicketLines(int id)
		{
			InputHistory.Set(HistoryItemType.TicketID, id);
			var ticket = TicketRepo.GetByID(id);
			this.SetViewData(ticket);

			return this.PartialView("ucExistingTicket", new TicketLineModel(id));
		}

		#endregion VIEW TICKETS

		#region CREATE TICKET

		[HttpPost]
		public ActionResult CreateTicket()
		{
			var firstClient = ClientRepo.GetByID(ProjectRepo.GetSupportEmails().First().ClientID);
			this.ViewData["Client"] = ClientRepo.GetClientsWithSupportProjects().Select(c => new SelectListItem() { Value = c.ClientID.ToString(), Text = c.ClientName });

			var firstProject = Project.GetSupportProjectsForClient(firstClient.ClientID).First();
			var projectItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "Please select Project" }
			};
			Project.GetSupportProjectsForClient(firstClient.ClientID).ToList().ForEach(p => projectItems.Add(new SelectListItem() { Value = p.ProjectID.ToString(), Text = p.ProjectName }));
			this.ViewData["Project"] = projectItems;

			this.ViewData["TicketType"] = TicketTypeRepo.GetAll().Select(s => new SelectListItem() { Value = s.TicketTypeID.ToString(), Text = s.TicketTypeName });
			this.ViewData["Priority"] = TicketPriorityRepo.GetAll().Select(s => new SelectListItem() { Value = s.TicketPriorityID.ToString(), Text = s.TicketPriorityName });

			var assignedUserItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "Please select User" }
			};

			this.ViewData["AssignedUser"] = assignedUserItems.Distinct();

			string subject = firstClient.ClientName + ':' + firstProject.ProjectName;

			return this.PartialView("ucNewTicket", subject);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult SubmitNewTicket(FormCollection form)
		{
			try
			{
				int clientID = int.Parse(form["Client"]);
				int projectID = int.Parse(form["Project"]);
				string subject = form["Subject"];
				string details = form["Details"];
				string ticketBody = form["PostText"];
				int ticketType = int.Parse(form["TicketType"]);
				int? priority = string.IsNullOrEmpty(form["Priority"]) ? 0 : int.Parse(form["Priority"]);
				int? effort = string.IsNullOrEmpty(form["Effort"]) ? 0 : int.Parse(form["Effort"]);
				int assignedUserID = int.Parse(form["AssignedUser"]);

				var ticket = new Ticket()
				{
					ClientID = clientID,
					ProjectID = projectID,
					TicketSubject = subject,
					TicketDetails = details,
					TicketStatus = (int)TicketStatusEnum.Open,
					TicketType = ticketType,
					Priority = priority ?? 0,
					Effort = effort ?? 0,
					FromAddress = this.CurrentUser.Email,
					DateCreated = DateTime.Now,
					DateModified = DateTime.Now,
					UserAssigned = assignedUserID,
					UserCreated = CurrentUserID,
					UserModified = CurrentUserID
				};

				ticket.Save();

				//Not sure what the to and from email should be set to on the ticketline
				//setting to=assigned user and from = no-reply as that is what it is
				string toEmail = UserRepo.GetByID(assignedUserID).Email;
				var ticketLine = ticket.AddTicketLine(TicketLineSourceType.EnabillPost, Settings.DefaultFromEmailAddress, toEmail, ticketBody, this.CurrentUser);
				ticket.AddTicketLog(ticket.TicketID, TicketLogType.Ticket, ticketLine.TicketLineID);

				EnabillEmails.NotifyUserofNewTicket(UserRepo.GetByID(assignedUserID), ticket);

				string emailAddresses = form["EmailAddresses"];

				if (!string.IsNullOrEmpty(emailAddresses))
					EnabillEmails.SendNewTicketManualEmail(emailAddresses, ticket);

				int ticketTypeFilter = InputHistory.Get(HistoryItemType.TicketTypeFilter, 0);
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.TicketDateFrom, DateTime.Today).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.TicketDateTo, DateTime.Today).Value;

				var model = new TicketIndexModel(this.CurrentUser, dateFrom, dateTo, ticketTypeFilter);
				var firstTicket = TicketRepo.GetByID(model.TicketList.FirstTicketID);
				this.SetViewData(firstTicket);

				return this.PartialView("ucIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion CREATE TICKET

		#region DELETE TICKET

		[HttpPost]
		public ActionResult ShowDeleteTicketView() => this.PartialView("ucDeleteTicket", "");

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult DeleteTicket(FormCollection form)
		{
			try
			{
				string ticketReferenceList = form["TicketReferenceList"];
				string deletedTickets = "";

				foreach (string reference in ticketReferenceList.Split(','))
				{
					var ticket = TicketRepo.GetByReference(reference.Trim());

					if (ticket != null)
					{
						deletedTickets += deletedTickets?.Length == 0 ? reference : "," + reference;
						ticket.IsDeleted = true;
						ticket.Save();
					}
				}

				if (deletedTickets.Trim() != "")
					EnabillEmails.NotifyUserofDeletedTicket(this.CurrentUser, deletedTickets);

				int ticketTypeFilter = InputHistory.Get(HistoryItemType.TicketTypeFilter, 0);
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.TicketDateFrom, DateTime.Today).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.TicketDateTo, DateTime.Today).Value;

				var model = new TicketIndexModel(this.CurrentUser, dateFrom, dateTo, ticketTypeFilter);
				var firstTicket = TicketRepo.GetByID(model.TicketList.FirstTicketID);
				this.SetViewData(firstTicket);

				return this.PartialView("ucIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion DELETE TICKET

		#region REACTIVATE TICKET

		[HttpPost]
		public ActionResult ShowReActivateTicketView() => this.PartialView("ucReActivateTicket", "");

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult ReActivateTicket(FormCollection form)
		{
			try
			{
				string ticketReactivateList = form["TicketReActivateList"];
				string reactivatedTickets = "";

				foreach (string reference in ticketReactivateList.Split(','))
				{
					var ticket = TicketRepo.GetByReference(reference.Trim());

					if (ticket != null)
					{
						reactivatedTickets += reactivatedTickets?.Length == 0 ? reference : "," + reference;
						ticket.IsDeleted = false;
						ticket.Save();
					}
				}

				if (reactivatedTickets.Trim() != "")
					EnabillEmails.NotifyUserofReactivatedTicket(this.CurrentUser, reactivatedTickets);

				int ticketTypeFilter = InputHistory.Get(HistoryItemType.TicketTypeFilter, 0);
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.TicketDateFrom, DateTime.Today).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.TicketDateTo, DateTime.Today).Value;

				var model = new TicketIndexModel(this.CurrentUser, dateFrom, dateTo, ticketTypeFilter);
				var firstTicket = TicketRepo.GetByID(model.TicketList.FirstTicketID);
				this.SetViewData(firstTicket);

				return this.PartialView("ucIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion REACTIVATE TICKET

		#region FIND TICKET

		[HttpPost]
		public ActionResult ShowFindTicketView() => this.PartialView("ucFindTicket", "");

		[HttpGet]
		public ActionResult FindTicket(string ticketReference)
		{
			var ticket = TicketRepo.GetByReference(ticketReference);
			string message = "Ticket " + ticketReference + "  not found.";

			if (ticket == null)
			{
				return this.RedirectToAction("Index");
			}
			else
			{
				var model = new TicketLineModel(ticket.TicketID);
				this.SetViewData(ticket);

				return this.View("ViewDetails", model);
			}
		}

		#endregion FIND TICKET

		#region EDIT AND CLOSE TICKET

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult SubmitTicketLine(FormCollection form)
		{
			try
			{
				int? id = form["TicketID"].ToInt();
				int? ticketStatus = form["TicketStatus"].ToInt();
				int? ticketType = form["TicketType"].ToInt();
				int? assignedUser = form["AssignedUser"].ToInt();
				int? project = form["Project"].ToInt();
				int? priority = form["Priority"].ToInt();
				string postText = form["PostText"];
				double? timeSpent = form["TimeSpent"].Replace(",", "").Replace(" ", "").ToDouble();
				string subject = form["TicketSubject"];
				string details = form["TicketDetails"];

				var ticket = TicketRepo.GetByID(id.Value);
				int previousAssignedUser = ticket.UserAssigned ?? 0;
				int previousTicketStatus = ticket.TicketStatus;
				string typeOfPost = "";

				if (assignedUser != 0)
				{
					ticket.UserAssigned = assignedUser;
				}

				ticket.TicketSubject = subject;
				ticket.TicketDetails = details;
				ticket.TicketStatus = ticketStatus.Value;
				ticket.TicketType = ticketType.Value;
				ticket.ProjectID = project.Value;
				ticket.Priority = priority.Value;
				ticket.Effort = 0; //not used on screen for now string.IsNullOrEmpty(effort.ToString()) ? 0 : effort;
				ticket.DateModified = DateTime.Now;
				ticket.UserModified = this.CurrentUserID;
				ticket.TimeSpent = timeSpent ?? 0;
				ticket.Save();

				int? ticketReferenceNumberID = ticket.TicketReference.Substring(3, 4).ToInt();
				var ticketReferenceNumber = TicketReferenceNumberRepo.GetByID(ticketReferenceNumberID ?? 0);

				if (ticketReferenceNumber != null)
				{
					ticketReferenceNumber.TicketSubject = ticket.TicketSubject;
					ticketReferenceNumber.Save();
				}

				if (assignedUser != previousAssignedUser & previousAssignedUser != 0)
				{
					var ticketAssignmentChange = new TicketAssignmentChange
					{
						TicketID = ticket.TicketID,
						FromUser = previousAssignedUser,
						ToUser = assignedUser.Value,
						DateCreated = DateTime.Now,
						Remark = postText,
						UserID = CurrentUserID
					};
					ticketAssignmentChange.Save();
					ticket.AddTicketLog(ticket.TicketID, TicketLogType.AssignmentChange, ticketAssignmentChange.TicketAssignmentChangeID);
					typeOfPost = "Assignment Change: " + (previousAssignedUser == 0 ? "None" : UserRepo.GetByID(previousAssignedUser).FullName) + " to " + UserRepo.GetByID(assignedUser.Value).FullName;
				}

				if (ticketStatus != previousTicketStatus)
				{
					var ticketStatusChange = new TicketStatusChange
					{
						TicketID = ticket.TicketID,
						FromStatus = previousTicketStatus,
						ToStatus = ticketStatus.Value,
						DateCreated = DateTime.Now,
						Remark = postText,
						UserID = CurrentUserID
					};
					ticketStatusChange.Save();
					ticket.AddTicketLog(ticket.TicketID, TicketLogType.StatusChange, ticketStatusChange.TicketStatusChangeID);
					string statusChange = "Status Change: " + (TicketStatusEnum)previousTicketStatus + " to " + (TicketStatusEnum)ticketStatus;
					typeOfPost = typeOfPost?.Length == 0 ? statusChange : typeOfPost + " and " + statusChange;
				}

				string toEmail = UserRepo.GetByID(assignedUser.Value).Email;
				var ticketLine = ticket.AddTicketLine(TicketLineSourceType.EnabillPost, Settings.DefaultFromEmailAddress, toEmail, postText, this.CurrentUser);

				foreach (string upload in this.Request.Files)
				{
					var fileStream = this.Request.Files[upload].InputStream;
					string fileName = Path.GetFileName(this.Request.Files[upload].FileName);
					int fileLength = this.Request.Files[upload].ContentLength;
					byte[] fileData = new byte[fileLength];
					fileStream.Read(fileData, 0, fileLength);
					if (fileName != "")
						ticketLine.AddAttachment(fileName, fileData);
				}

				if ((TicketStatusEnum)ticket.TicketStatus == TicketStatusEnum.Closed)
					EnabillEmails.NotifyUsersOfClosedTicket(this.CurrentUser, ticket);
				else
					EnabillEmails.NotifyUserofNewTicketLine(UserRepo.GetByID(assignedUser.Value), ticket, postText);

				var model = new TicketLineModel(id.Value);
				this.SetViewData(ticket);

				return this.View("ViewDetails", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult CloseTicket(int ticketID, string postText)
		{
			try
			{
				var ticket = TicketRepo.GetByID(ticketID);
				ticket.TicketStatus = (int)TicketStatusEnum.Closed;
				ticket.DateModified = DateTime.Now;
				ticket.UserModified = this.CurrentUserID;
				ticket.Save();

				string toEmail = ticket.ProjectID == 0 ? ClientRepo.GetByID(ticket.ClientID).SupportEmailAddress : ProjectRepo.GetByID(ticket.ProjectID).SupportEmailAddress;
				ticket.AddTicketLine(TicketLineSourceType.EnabillPost, Settings.DefaultFromEmailAddress, toEmail, postText, this.CurrentUser);

				EnabillEmails.NotifyUsersOfClosedTicket(this.CurrentUser, ticket);

				int ticketTypeFilter = InputHistory.Get(HistoryItemType.TicketTypeFilter, 0);
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.TicketDateFrom, DateTime.Today).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.TicketDateTo, DateTime.Today).Value;

				var model = new TicketLineModel(ticketID);
				this.SetViewData(ticket);

				return this.PartialView("ucViewDetails", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion EDIT AND CLOSE TICKET

		#region EMAIL NOTIFICATION

		[HttpPost]
		public ActionResult ShowSendEmailNotificationView(int id)
		{
			var model = TicketRepo.GetByID(id);

			return this.PartialView("ucEmailNotification", model);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult SendEmailNotification(FormCollection form)
		{
			try
			{
				string ticketReference = form["TicketReference"];
				string emailAddresses = form["EmailAddresses"];
				var ticket = TicketRepo.GetByReference(ticketReference);

				if ((TicketStatusEnum)ticket.TicketStatus == TicketStatusEnum.Closed)
					EnabillEmails.SendClosedTicketManualEmail(emailAddresses, ticket);
				else
					EnabillEmails.SendNewTicketManualEmail(emailAddresses, ticket);

				int ticketTypeFilter = InputHistory.Get(HistoryItemType.TicketTypeFilter, 0);
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.TicketDateFrom, DateTime.Today).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.TicketDateTo, DateTime.Today).Value;

				var model = new TicketIndexModel(this.CurrentUser, dateFrom, dateTo, ticketTypeFilter);
				var firstTicket = TicketRepo.GetByID(model.TicketList.FirstTicketID);
				this.SetViewData(firstTicket);

				return this.PartialView("ucIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion EMAIL NOTIFICATION

		#region VIEW DATA

		public ActionResult ViewAttachments(int ticketLineAttachmentID)
		{
			byte[] report = new byte[0];
			string fileName = "";

			try
			{
				var ticketLineAttachment = TicketLineAttachmentRepo.GetAttachmentByID(ticketLineAttachmentID);
				report = ticketLineAttachment.Attachment;
				fileName = ticketLineAttachment.AttachmentName;
			}
			catch (Exception ex)
			{
			}

			return this.File(report, "application/xls", fileName);
		}

		public void SetViewData(Ticket firstTicket)
		{
			int firstClientID = firstTicket != null ? firstTicket.ClientID : ProjectRepo.GetSupportEmails().First().ClientID;
			this.ViewData["Client"] = ClientRepo.GetClientsWithSupportProjects().Select(c => new SelectListItem() { Value = c.ClientID.ToString(), Text = c.ClientName });

			var project = firstTicket != null && firstTicket.ProjectID != 0 ? ProjectRepo.GetByID(firstTicket.ProjectID) : Project.GetSupportProjectsForClient(firstClientID).First();
			int projectID = firstTicket?.ProjectID ?? 0;

			var projectItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "Please select Project" }
			};
			Project.GetSupportProjectsForClient(firstClientID).ToList().ForEach(p => projectItems.Add(new SelectListItem() { Value = p.ProjectID.ToString(), Text = p.ProjectName, Selected = p.ProjectID == projectID }));
			this.ViewData["Project"] = projectItems;

			this.ViewData["TicketStatus"] = TicketStatusRepo.GetByAll().Select(s => new SelectListItem() { Value = s.TicketStatusID.ToString(), Text = s.TicketStatusName, Selected = s.TicketStatusID == (firstTicket?.TicketStatus ?? 0) });
			this.ViewData["TicketType"] = TicketTypeRepo.GetAll().Select(s => new SelectListItem() { Value = s.TicketTypeID.ToString(), Text = s.TicketTypeName, Selected = s.TicketTypeID == (firstTicket?.TicketType ?? 0) });
			this.ViewData["Priority"] = TicketPriorityRepo.GetAll().Select(s => new SelectListItem() { Value = s.TicketPriorityID.ToString(), Text = s.TicketPriorityName, Selected = s.TicketPriorityID == (firstTicket?.Priority ?? 0) });

			var ticketTypeFilterItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All" }
			};
			TicketTypeRepo.GetAll().OrderBy(d => d.TicketTypeName).ToList().ForEach(t => ticketTypeFilterItems.Add(new SelectListItem { Value = t.TicketTypeID.ToString(), Text = t.TicketTypeName, Selected = t.TicketTypeID == InputHistory.Get(HistoryItemType.TicketTypeFilter, 0) }));
			this.ViewData["TicketTypeFilter"] = ticketTypeFilterItems;

			var assignedUserItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "Please select User" }
			};
			int assignedUser = firstTicket?.UserAssigned.HasValue == true ? firstTicket.UserAssigned.Value : 0;

			if (projectID != 0)
				UserRepo.GetUsersAssignedToProject(projectID).OrderBy(u => u.UserName).ToList().ForEach(u => assignedUserItems.Add(new SelectListItem { Value = u.UserID.ToString(), Text = u.FullName, Selected = u.UserID == assignedUser }));

			//Add the user assigned to the ticket to the dropdown incase it is no longer actively linked to the project
			if (assignedUser != 0 && !assignedUserItems.Exists(u => u.Value == assignedUser.ToString()))
			{
				var currentAssignedUser = UserRepo.GetByID(assignedUser);
				assignedUserItems.Add(new SelectListItem { Value = currentAssignedUser.UserID.ToString(), Text = currentAssignedUser.FullName, Selected = currentAssignedUser.UserID == assignedUser });
			}

			//Add Ticket Project Managers who may not neccesarily be assigned to the project itself.
			foreach (var po in UserRepo.GetUsersByRole((int)UserRoleType.TicketProjectOwner).OrderBy(u => u.UserName).ToList())
			{
				if (!assignedUserItems.Exists(u => u.Value == po.UserID.ToString()))
					assignedUserItems.Add(new SelectListItem { Value = po.UserID.ToString(), Text = po.FullName, Selected = po.UserID == assignedUser });
			}

			this.ViewData["AssignedUser"] = assignedUserItems.Distinct();

			this.ViewData["FilterBy"] = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "Any user", Selected = 0 == InputHistory.Get(HistoryItemType.TicketFilterBy, 0) },
				new SelectListItem() { Value = "1", Text = "Me", Selected = 1 == InputHistory.Get(HistoryItemType.TicketFilterBy, 0) }
			};
		}

		[HttpPost]
		public ActionResult ProjectListLookup(int clientID)
		{
			var client = ClientRepo.GetByID(clientID);
			int firstProjectID = Project.GetSupportProjectsForClient(clientID).First().ProjectID;
			var list = new List<SelectListItem>();

			foreach (var project in Project.GetSupportProjectsForClient(clientID).ToList())
				list.Add(new SelectListItem { Value = project.ProjectID.ToString(), Text = project.ProjectName, Selected = project.ProjectID == firstProjectID });

			return this.Json(list);
		}

		[HttpPost]
		public ActionResult UserListLookup(int projectID)
		{
			var list = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "Please select User" }
			};

			foreach (var user in UserRepo.GetUsersAssignedToProject(projectID).ToList())
				list.Add(new SelectListItem { Value = user.UserID.ToString(), Text = user.FullName });

			//Add Ticket Project Managers who may not neccesarily be assigned to the project itself.
			foreach (var po in UserRepo.GetUsersByRole((int)UserRoleType.TicketProjectOwner).OrderBy(u => u.UserName).ToList())
			{
				if (!list.Exists(u => u.Value == po.UserID.ToString()))
					list.Add(new SelectListItem { Value = po.UserID.ToString(), Text = po.FullName });
			}

			list.Distinct();

			return this.Json(list);
		}
	}

	#endregion VIEW DATA
}