using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web.Mvc;
using Enabill.Emailing;
using Enabill.Models;
using Enabill.Properties;
using Enabill.Repos;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class ProjectController : SearchableController
	{
		protected override string GetSearchLabelText() => "projects";

		#region PROJECT MANAGEMENT

		public override ActionResult Index(string q, bool? isActive)
		{
			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator) && !this.CurrentUser.HasRole(UserRoleType.ProjectOwner))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			this.ViewBag.q = InputHistory.Get(q, HistoryItemType.ProjectSearchCriteria);
			this.ViewBag.Project_b = InputHistory.Get(isActive, HistoryItemType.ProjectFilterBy, true);

			SetViewData(this.ViewBag.Project_b);

			List<ProjectSearchResult> model = Project.Search(this.CurrentUser, this.ViewBag.q, this.ViewBag.Project_b);

			SaveAllInput(HistoryItemType.ProjectSearchCriteria, this.ViewBag.q, HistoryItemType.ProjectFilterBy, this.ViewBag.Project_b);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult RefreshList(string q, bool isActive)
		{
			this.SetViewData(isActive);

			this.ViewBag.q = InputHistory.Get(q, HistoryItemType.ProjectSearchCriteria);
			this.ViewBag.Project_b = InputHistory.Get(isActive, HistoryItemType.ProjectFilterBy, true);
			List<ProjectSearchResult> model = Project.Search(this.CurrentUser, this.ViewBag.q, this.ViewBag.Project_b);

			SaveAllInput(HistoryItemType.ProjectSearchCriteria, this.ViewBag.q, HistoryItemType.ProjectFilterBy, this.ViewBag.Project_b);

			return this.PartialView("Index", model);
		}

		[HttpPost]
		public ActionResult RefreshActivityList(int id, bool isActive)
		{
			this.ViewData["ActivityFilter"] = new List<SelectListItem>
			{
				new SelectListItem() { Value = "1", Text = "Active", Selected = isActive },
				new SelectListItem() { Value = "0", Text = "Inactive", Selected = !isActive }
			};

			var project = ProjectRepo.GetByID(id);
			if (project == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoProjectForActivityList_Message));

			var model = new ProjectEditModel(project, isActive);

			return this.PartialView("ucActivityList", model.ProjectActivities);
		}

		[HttpGet]
		public ActionResult Details(int id)
		{
			var project = ProjectRepo.GetByID(id);

			if (project == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoProject_Message));

			if (!this.CurrentUser.CanManage(project))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			var model = new ProjectEditModel(project, true);

			return this.View(model);
		}

		[HttpGet]
		public ActionResult Create()
		{
			var project = Project.GetNew();

			project.AmountTotal = "Project Value";
			this.SetViewDataLists(project);

			return this.View(project);
		}

		[HttpPost]
		public ActionResult Create(FormCollection form)
		{
			try
			{
				var project = Project.GetNew();

				//Currently a Support Email Address is unique to a project.
				//Would be best if the email address in stored at client level in my humble opinion and the correct project selected when editing the ticket.
				string supportEmailAddress = form["SupportEmailAddress"];

				if (supportEmailAddress.Trim() != "")
				{
					var supportEmail = ProjectRepo.GetBySupportEmailAddress(supportEmailAddress);
					string errorMessage = "";

					if (supportEmail != null)
					{
						if (supportEmail.ProjectID == 0)
							errorMessage = "Support Email Address, " + supportEmailAddress + ", is already referenced on Client " + ClientRepo.GetByID(supportEmail.ClientID).ClientName + ". Please specify another support email address. Save cancelled.";
						else
							errorMessage = "Support Email Address, " + supportEmailAddress + ", is already referenced on Project " + ClientRepo.GetByID(supportEmail.ClientID).ClientName + ":" + ProjectRepo.GetByID(supportEmail.ProjectID).ProjectName + ". Please specify another support email address. Save cancelled.";

						throw new EnabillConsumerException(errorMessage);
					}
				}

				project.SupportEmailAddress = supportEmailAddress;

				if (form["BillingMethodID"] == "")
				{
					string errorMessage = "A Billing Method is required for a Project. Save cancelled.";
					throw new BillingException(errorMessage);
				}

				this.UpdateModel(project);

				using (var ts = new TransactionScope())
				{
					project.Save(this.CurrentUser);

					if (form["CreateDefaultActivity"] != null)
						project.CreateDefaultActivity(this.CurrentUser);

					EnabillEmails.NotifyProjectOwnerOfNewProjectAssignedToHimHer(project);

					ts.Complete();
				}

				return this.Json(new
				{
					IsError = false,
					Description = "Project saved successfully",
					Url = "/Project/Edit/" + project.ProjectID
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult Edit(int id, int? clientID, string fileName = "", string filePath = "", string callingPage = "")
		{
			var project = ProjectRepo.GetByID(id);

			if (project == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoProject_Message));

			if (!this.CurrentUser.CanManage(project))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
			
			int currencyTypeID = ClientRepo.GetByID(project.ClientID).CurrencyTypeID;

			project.AmountTotal = "Project Value (" + CurrencyTypeRepo.GetByID(currencyTypeID).CurrencyISO.ToString() + ")";

			project.Currency = CurrencyTypeRepo.GetByID(currencyTypeID).CurrencyISO.ToString();

			this.SetViewDataLists(project);
			var model = new ProjectEditModel(project, true, fileName, filePath);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult Edit(int projectID, FormCollection form)
		{
			try
			{
				var project = ProjectRepo.GetByID(projectID);

				if (project == null)
					throw new EnabillConsumerException(Resources.ERR_NoProjectForUpdate_Message);

				if (project.InvoiceAdminID == 0 || !int.TryParse(form["InvoiceAdminID"], out int id))
					throw new EnabillConsumerException("Please select an Invoice administrator.");

				if (project.ProjectOwnerID == 0 || !int.TryParse(form["ProjectOwnerID"], out id))
					throw new EnabillConsumerException("Please select an Project Owner.");

				if (project.RegionID == 0 || !int.TryParse(form["RegionID"], out id))
					throw new EnabillConsumerException("Please select a Region.");

				if (project.BillingMethodID == 0 || !int.TryParse(form["BillingMethodID"], out id))
					throw new EnabillConsumerException("Please select a BillingMethod.");

				if (project.DepartmentID == 0 || !int.TryParse(form["DepartmentID"], out id))
					throw new EnabillConsumerException("Please select a Department.");

				// Default client support email address to be captured at client level
				// If a project has its own unique support email address, capture it on project level
				// A support email address cannot be referenced more than once.
				string supportEmailAddress = form["SupportEmailAddress"];

			
				if (supportEmailAddress.Trim() != "")
				{
					var supportEmail = ProjectRepo.GetBySupportEmailAddress(supportEmailAddress);
					string errorMessage = "";

					// Throw an exception if you find the support email address on another project other than the one you editing.
					if (supportEmail != null && supportEmail.ProjectID != projectID)
					{
						if (supportEmail.ProjectID == 0)
							errorMessage = "Support Email Address, " + supportEmailAddress + ", is already referenced on Client " + ClientRepo.GetByID(supportEmail.ClientID).ClientName + ". Please specify another support email address. Save cancelled.";
						else
							errorMessage = "Support Email Address, " + supportEmailAddress + ", is already referenced on Project " + ClientRepo.GetByID(supportEmail.ClientID).ClientName + ":" + ProjectRepo.GetByID(supportEmail.ProjectID).ProjectName + ". Please specify another support email address. Save cancelled.";

						throw new EnabillConsumerException(errorMessage);
					}

					// Check that its a valid support email address
					if (string.Equals(supportEmailAddress, Enabill.Code.Constants.EMAILADDRESSSUPPORT, StringComparison.OrdinalIgnoreCase))
					{
						errorMessage = "<b>" + supportEmailAddress + "</b> is not a valid Client Enabill Helpdesk Email Address as this is the general support Email address. ";

						throw new EnabillConsumerException(errorMessage);
					}
					else if (!(supportEmailAddress.IndexOf(Enabill.Code.Constants.EMAILDOMAIN, StringComparison.OrdinalIgnoreCase) >= 0))
					{
						errorMessage = "<b>" + supportEmailAddress + $"</b> is not a valid Enabill Helpdesk Email Address. Must contain <b>'...{Enabill.Code.Constants.EMAILDOMAIN}'</b>. ";

						throw new EnabillConsumerException(errorMessage);
					}

				}


				project.SupportEmailAddress = supportEmailAddress;

				//ToDo If Confirmed End Date checkbox checked, then add in date
				this.UpdateModel(project);
				project.Save(this.CurrentUser);

			
				this.SetViewDataLists(project);

				return this.PartialView("ucProjectDetail", project);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult EndProject(int id)
		{
			var project = ProjectRepo.GetByID(id);

			if (project == null)
				throw new EnabillConsumerException(Resources.ERR_NoProject_Message);

			try
			{
				project.EndProject(this.CurrentUser);

				project = ProjectRepo.GetByID(id);
				this.SetViewDataLists(project);

				return this.PartialView("ucProjectDetail", project);
			}
			catch (Exception ex)
			{
				return this.Content(ex.Message);
			}
		}

		private void SetViewDataLists(Project model)
		{
			this.ViewData["StatusFilter"] = new List<SelectListItem>
			{
				new SelectListItem() { Value = "1", Text = "Active" },
				new SelectListItem() { Value = "0", Text = "Inactive" }
			};

			this.AddAsDropdownSource(Enabill.Models.User.GetByRoleBW((int)UserRoleType.ProjectOwner), "ProjectOwnerUser", "UserID", "FullName", model.ProjectOwnerID);
			this.AddAsDropdownSource(Enabill.Models.User.GetByRoleBW((int)UserRoleType.InvoiceAdministrator), "InvoiceUser", "UserID", "FullName", model.InvoiceAdminID);

			this.AddAsDropdownSource(ClientRepo.GetAll().Where(r => r.IsActive).OrderBy(r => r.ClientName).ToList(), "Client", model.ClientID);
			this.AddAsDropdownSource(RegionRepo.GetAll().OrderBy(r => r.RegionName).ToList(), "Region", model.RegionID);
			this.AddAsDropdownSource(BillingMethodRepo.GetAll().OrderBy(r => r.BillingMethodName).ToList(), "BillingMethod", model.BillingMethodID);
			this.AddAsDropdownSource(DepartmentRepo.GetAll().OrderBy(d => d.DepartmentName).ToList(), "Department", model.DepartmentID);
		}

		#endregion PROJECT MANAGEMENT

		#region ACTIVITY MANAGEMENT

		[HttpGet]
		public ActionResult CreateActivity(int id)
		{
			var project = ProjectRepo.GetByID(id);
			

			if (project == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoProjectForActivity_Message));

			var model = project.GetNewActivity();

			this.SetViewDataLists(model);

			return this.View("EditActivity", model);
		}

		[HttpPost]
		public ActionResult CreateActivity(int id, FormCollection form)
		{
			try
			{
				var project = ProjectRepo.GetByID(id);
				
				if (project == null)
					throw new EnabillConsumerException(Resources.ERR_NoProjectForActivityCreate_Message);

				var activity = project.GetNewActivity();

				this.UpdateModel(activity);

				activity.ActivityName = activity.ActivityName.Trim();

				project.SaveActivity(this.CurrentUser, activity);

				if (!string.IsNullOrEmpty(form["isActive"]))
					project.SetActivityStatus(this.CurrentUser, activity, IsCheckBoxChecked(form["IsDeactivated"]));

				return this.Content("1");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		public ActionResult ListActivity(int id)
		{
			this.ViewData["ActivityFilter"] = new List<SelectListItem>
			{
				new SelectListItem() { Value = "1", Text = "Active" },
				new SelectListItem() { Value = "0", Text = "Inactive" }
			};

			var project = ProjectRepo.GetByID(id);
		
			if (project == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoProjectForActivityList_Message));

			var model = new ProjectEditModel(project, true);

			return this.PartialView("ucActivityList", model.ProjectActivities);
		}

		[HttpPost]
		public ActionResult EditActivity(int id, int activityID)
		{
			var project = ProjectRepo.GetByID(id);
			
			if (project == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoProject_Message));

			var activity = project.GetActivity(activityID);

			if (activity == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoActivityForUpdate_Message));

			this.SetViewDataLists(activity);

			return this.View(activity);
		}

		[HttpPost]
		public ActionResult SaveEditActivity(FormCollection form)
		{
			try
			{
				int id = int.Parse(form["ActivityID"]);
				int projectID = int.Parse(form["ProjectID"]);

				var project = ProjectRepo.GetByID(projectID);
			
				if (project == null)
					throw new EnabillConsumerException(Resources.ERR_NoProjectForActivityUpdate_Message);
				var activity = project.GetActivity(id);

				if (activity == null)
					throw new EnabillConsumerException(Resources.ERR_NoActivityForUpdate_Message);

				this.UpdateModel(activity);

				project.SetActivityStatus(this.CurrentUser, activity, IsCheckBoxChecked(form["isDeactivated"]));
				project.SaveActivity(this.CurrentUser, activity);

				return this.Content("1");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

			public ActionResult DeleteActivity(int id, int activityID)
		{
			try
			{
				var project = ProjectRepo.GetByID(id);
			
				if (project == null)
					throw new EnabillConsumerException(Resources.ERR_NoProjectForActivityDelete_Message);

				var activity = project.GetActivity(activityID);

				if (activity == null)
					throw new EnabillDomainException(Resources.ERR_NoActivityForDelete_Message);

				project.DeleteActivity(this.CurrentUser, activity);

				return this.ReturnJsonResult(false, "Activity was deleted successfully.");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		private void SetViewDataLists(Activity model)
		{
			this.AddAsDropdownSource(RegionRepo.GetAll().OrderBy(r => r.RegionName).ToList(), "Region", model.RegionID);
			this.AddAsDropdownSource(DepartmentRepo.GetAll().OrderBy(d => d.DepartmentName).ToList(), "Department", model.DepartmentID);
		}

		private void SetViewData(bool? isProjectActive = true) => this.ViewData["StatusFilter"] = new List<SelectListItem>
			{
				new SelectListItem() { Value = "1", Text = "Active", Selected = isProjectActive == true },
				new SelectListItem() { Value = "0", Text = "Inactive", Selected = isProjectActive == false }
			};

		public ActionResult ActivityLookup(string term)
		{
			var list = ActivityRepo.AutoComplete(term, 20);

			return this.Json(list, JsonRequestBehavior.AllowGet);
		}

		#endregion ACTIVITY MANAGEMENT

		#region USER ALLOCATION
	
		public ActionResult DisplayActivityUsers(int id)
		{
			var project = ProjectRepo.GetByID(id);
			
			int currencyTypeID = ClientRepo.GetByID(project.ClientID).CurrencyTypeID;

			project.Currency = CurrencyTypeRepo.GetByID(currencyTypeID).CurrencyISO.ToString();


			if (project == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoProject_Message));

			var model = new ProjectActivityUsersModel(project);

			return this.PartialView("ucActivityUsers", model);
		}

		[HttpPost]
		public ActionResult GetConfirmEndDateView(int projectID, int userAllocationID)
		{
			var project = ProjectRepo.GetByID(projectID);
			var userAllocation = project.GetUserAllocation(userAllocationID);

			DateTime? date = userAllocation.ScheduledEndDate ?? DateTime.Today;

			return this.PartialView("ucUserAllocationEndDate", date);
		}

		[HttpPost]
		public ActionResult SetConfirmedEndDate(int id, int userAllocationID, string confDate)
		{
			try
			{
				if (!DateTime.TryParse(confDate, out var confirmedEndDate))
					throw new EnabillConsumerException("An error was detected with the date supplied. Please retry.");

				var project = ProjectRepo.GetByID(id);

				if (project == null)
					throw new EnabillConsumerException(Resources.ERR_NoProjectForActivityUpdate_Message);

				var userAllocation = project.GetUserAllocation(userAllocationID);
				userAllocation.SetConfirmedEndDateOnUserAllocationForActivity(this.CurrentUser, confirmedEndDate);

				var model = new ProjectActivityUsersModel(project);

				return this.PartialView("ucActivityUsers", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult AllocateUsers(int id, FormCollection form)
		{
			try
			{
				var project = ProjectRepo.GetByID(id);
				
				int currencyTypeID = ClientRepo.GetByID(project.ClientID).CurrencyTypeID;
				project.Currency = CurrencyTypeRepo.GetByID(currencyTypeID).CurrencyISO.ToString();

				if (project == null)
					throw new EnabillConsumerException(Resources.ERR_NoProject_Message);

				string activityIDs = form["activityIDs"];
				string userIDs = form["userIDs"];
				double.TryParse(form["chargeRate"], out double chargeR);

				var startDate = DateTime.Parse(form["StartDate"]);
				var endDate = form["EndDate"].ToDate();
				bool isConfirmed = IsCheckBoxChecked(form["IsConfirmed"]);

				project.AssignUsersToActivities(this.CurrentUser, userIDs, activityIDs, chargeR, startDate, endDate, isConfirmed);

				var model = new ProjectActivityUsersModel(project);

				return this.PartialView("ucActivityUsers", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult UserAllocations(int? userAllocationID)
		{
			try
			{
				var user = this.ContextUser;

				if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
					return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

				UserAllocation model = null;

				if (userAllocationID.HasValue)
					model = UserRepo.GetUserAllocationByID(userAllocationID.Value);

				//SetViewData(model);

				return this.PartialView("ucAddEditUserAllocation", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult AddEditUserAllocation(FormCollection form)
		{
			var user = this.ContextUser;

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			try
			{
				var userAllocation = new UserAllocation();

				int? userAllocationID = int.Parse(form["UserAllocationID"]);

				if (userAllocationID.HasValue && userAllocationID.Value != 0)
					userAllocation = UserRepo.GetUserAllocationByID(userAllocationID.Value);

				var startDate = form["UserAllocationStartDate"].ToDate();

				if (startDate.HasValue)
					userAllocation.StartDate = startDate.Value;

				var endDate = form["UserAllocationEndDate"].ToDate();

				if (endDate.HasValue)
				{
					// Check that End Date is not before the Start Date
					if (endDate < startDate)
						throw new EnabillDomainException("User Allocation End Date cannot be prior to the Start Date.");

					if (IsCheckBoxChecked(form["EndDateIsConfirmed"]))
					{
						userAllocation.ConfirmedEndDate = endDate.Value;
					}
					else
					{
						userAllocation.ConfirmedEndDate = null;
						userAllocation.ScheduledEndDate = endDate.Value;
					}
				}
				else
				{
					userAllocation.ConfirmedEndDate = null;
					userAllocation.ScheduledEndDate = null;
				}

				userAllocation.ActivityID = int.Parse(form["Activity"]);
				userAllocation.ChargeRate = double.Parse(form["Rate"]);
				var project = ProjectRepo.GetByID(userAllocation.GetActivity().ProjectID);
				//tulisa
				int currencyTypeID = ClientRepo.GetByID(project.ClientID).CurrencyTypeID;
				project.Currency = CurrencyTypeRepo.GetByID(currencyTypeID).CurrencyISO.ToString();

				// End Date of User Allocation cannot be prior to the last date time was captured
				var workallocations = userAllocation.GetUser().GetLastWorkAllocationDateForUserForActivity(userAllocation.ActivityID).ToList();

				if (workallocations.Count > 0)
				{
					var dateTimeLastCaptured = workallocations.Max(wa => wa.DayWorked);

					if ((userAllocation.ConfirmedEndDate != null && dateTimeLastCaptured > userAllocation.ConfirmedEndDate) || (userAllocation.ScheduledEndDate != null && dateTimeLastCaptured > userAllocation.ScheduledEndDate))
						throw new EnabillDomainException("User Allocation End Date cannot be prior to the date user last captured time against this activity(" + dateTimeLastCaptured.ToExceptionDisplayString() + "). Please reallocate time to another activity before attempting to end this one.");
				}

				UserRepo.SaveUserAllocation(userAllocation);

				var model = new ProjectActivityUsersModel(userAllocation.GetActivity().GetProject());

				return this.PartialView("ucActivityUsers", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult DeleteUserAllocation(int userAllocationID)
		{
			try
			{
				var user = this.ContextUser;

				if (user == null)
					throw new UserManagementException(Resources.ERR_NoUser_Message);

				var userAllocation = UserRepo.GetUserAllocationByID(userAllocationID);
				var project = userAllocation.GetActivity().GetProject();
				//tulisa
				int currencyTypeID = ClientRepo.GetByID(project.ClientID).CurrencyTypeID;
				project.Currency = CurrencyTypeRepo.GetByID(currencyTypeID).CurrencyISO.ToString();

				if (userAllocation != null)
					project.DeleteUserAllocation(userAllocation);

				var model = new ProjectActivityUsersModel(userAllocation.GetActivity().GetProject());

				return this.PartialView("ucActivityUsers", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "Unknown error occurred.");
			}
		}

		#endregion USER ALLOCATION

		#region ATTACHMENT

		[HttpPost]
		public ActionResult FileAdd(ContractAttachment contractAttachmentModel, int clientID, int projectID, DateTime createdDate)
		{
			var file = GetFileNameAndPath(contractAttachmentModel, projectID, createdDate);

			try
			{
				//Save the File to the Directory (Folder).
				contractAttachmentModel.AttachmentFile.SaveAs(file.filePathAndName);
				this.ModelState.Clear();

				var contractAttachment = ContractAttachment.GetNew(clientID, projectID, file.filePath, file.fileName, file.mimeType);

				this.UpdateModel(contractAttachment);

				//Save file details to database
				contractAttachment.Save();

				this.Dispose();

				return new RedirectResult(this.Url.Action("Edit", new { id = projectID }) + "#tabs-4");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult FileMoveOrDelete(int clientID, int projectID, string[] fileList, string callingPage = "", DateTime newContractDate = default)
		{
			if (fileList == null)
			{
				return this.Json(new
				{
					IsError = false,
					redirectTo = this.Url.Action("Edit", "Project", new { id = projectID })
					//redirectTo = this.Url.Action(this.Url.Action("Edit", "Project", new { id = projectID }) + "#tabs-4")
				});
			}
			else
			{
				var contractAttachment = new ContractAttachment();

				this.ModelState.Clear();

				try
				{
					foreach (string fileName in fileList)
					{
						contractAttachment = ContractAttachment.GetContractAttachmentByFileName(fileName);
						string filePath = contractAttachment.FilePath;
						string filePathAndName = string.Format(filePath + fileName);
						if (callingPage == "MoveContractAttachments") //Move files
						{
							// Move the physical file to a new path
							if (System.IO.File.Exists(filePathAndName))
							{
								//Create a new file name and file path
								var file = GetFileNameAndPath(contractAttachment, projectID, newContractDate, callingPage);

								//Move and rename
								System.IO.File.Move(filePathAndName, file.filePathAndName);

								//Insert new Expense Attachment and delete the old one
								var contractAttachmentNew = ContractAttachment.GetNew(clientID, projectID, file.filePath, file.fileName, file.mimeType);

								this.UpdateModel(contractAttachmentNew);

								//Save file details to database
								contractAttachmentNew.Save();

								//Delete the old attachment from the database
								this.UpdateModel(contractAttachment);
								contractAttachment.Delete();
							}
						}
						else //Delete files
						{
							// Remove the physical file from path
							if (System.IO.File.Exists(filePathAndName))
							{
								System.IO.File.Delete(filePathAndName);
							}

							// Remove the file from the database
							contractAttachment.Delete();

							this.UpdateModel(contractAttachment);
						}
					}

					contractAttachment.Save();

					this.Dispose();

					if (callingPage?.Length == 0)
					{
						return this.Json(new
						{
							IsError = false,
							redirectTo = this.Url.Action("Edit", "Project", new { id = projectID })
							//redirectTo = this.Url.Action(this.Url.Action("Edit", "Project", new { id = projectID }) + "#tabs-4")
						});
					}
					else
					{
						return new EmptyResult();
					}
				}
				catch (Exception ex)
				{
					return this.ReturnJsonException(ex);
				}
			}
		}

		private static (string fileName, string filePath, string filePathAndName, string mimeType) GetFileNameAndPath(ContractAttachment contractAttachment, int projectID, DateTime createdDate, string callingPage = "")
		{
			string year = createdDate.ToString("yyyy");
			string month = createdDate.ToString("MM");
			string day = createdDate.ToString("dd");
			string rootFolderPath = Enabill.Code.Constants.PATHCONTRACT.Replace("\\", "/");
			string yearFolderPath = string.Format($"{rootFolderPath}{year}/");
			string filePath = string.Format($"{yearFolderPath}{month}/");
			string extension = "";

			if (callingPage?.Length == 0)
			{
				extension = Path.GetExtension(contractAttachment.AttachmentFile.FileName);
			}
			else
			{
				extension = Path.GetExtension(contractAttachment.FileName);
			}

			string fileName = string.Format($"{projectID}{year}{month}{day}{DateTime.Now.ToString("mmssfff")}");

			//Lines below check to make sure the extension matches the actual file type
			string filePathAndName = string.Format(filePath + fileName + extension);
			string mimeType = MimeTypes.GetContentType(filePathAndName);
			extension = string.Format($".{MimeTypes.GetExtension(mimeType, extension)}");

			if (extension == ".")
				throw new ContractAttachmentException("File extension does not match file type or file type not allowed.");

			fileName = string.Format(fileName + extension);
			filePathAndName = string.Format(filePath + fileName);

			//Check whether Folders exist
			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return (fileName, filePath, filePathAndName, mimeType);
		}

		public FilePathResult Image(string mimeType, string filePath, string fileName)
		{
			string extension = Path.GetExtension(fileName).TrimStart('.');
			string image = string.Format($"~/Content/Icons/{extension}.png") ?? string.Format("~/Content/Icons/_blank.png");

			return this.File(image, mimeType);
		}

		#endregion ATTACHMENT
	}
}