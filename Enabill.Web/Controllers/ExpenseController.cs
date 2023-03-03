using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
	public class ExpenseController : BaseController
	{
		private readonly IExpenseRepository expenseRepository;

		public ExpenseController()
		{
		}

		public ExpenseController(IExpenseRepository expenseRepository)
		{
			this.expenseRepository = expenseRepository;
		}

		[HttpGet]
		public ActionResult Index(int? id)
		{
			var user = this.CurrentUser;

			if (id.HasValue)
				user = UserRepo.GetByID(id.Value);

			if (user == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoUser_Message));

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			Settings.Current.ContextUser = user;

			var tmpDate = InputHistory.GetDateTime(HistoryItemType.ExpenseDate, DateTime.Today).Value;

			// Logic to retain month in drop-down if it has been selected
			if (tmpDate.Date == DateTime.Today.Date)
				this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.IsInCurrentMonth() });
			else
				this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key == tmpDate });

			SetViewData(this.ViewBag.Expense_b);

			var model = new ExpenseIndexModel(user, tmpDate);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult Index(FormCollection form)
		{
			try
			{
				var monthYear = form["MonthList"].ToDate();
				int? userID = form["User"].ToInt();
				var user = UserRepo.GetByID(userID.Value);

				if (user == null)
					return this.ErrorView(new ErrorModel(Resources.ERR_NoUser_Message));

				if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
					return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

				this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key == monthYear.Value });

				InputHistory.Set(HistoryItemType.ExpenseDate, monthYear.Value);

				var model = new ExpenseIndexModel(user, monthYear.Value);

				return this.View(model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#region CREATE EXPENSE

		[HttpGet]
		public ActionResult Create()
		{
			var user = this.ContextUser;

			if (user == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoUser_Message));

			if (this.CurrentUserID != user.UserID)
			{
				return this.ErrorView(new ErrorModel(Resources.ERR_CreateExpense_Message, true));
			}

			var expense = Expense.GetNew(user.UserID);

			this.SetViewDataLists(expense);

			var model = new ExpenseEditModel(user, expense);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult Create(FormCollection form) => this.Save(Expense.GetNew(this.ContextUserID));

		private ActionResult Save(Expense expense)
		{
			bool isNew = expense.ExpenseID == 0;

			try
			{
				this.UpdateModel(expense);

				expense.Save(this.CurrentUser);

				if (isNew)
					EnabillEmails.NotifyManagerOfExpense(this.ContextUser);

				return this.Json(new
				{
					IsError = false,
					redirectTo = this.Url.Action("Edit", "Expense", new { userID = expense.UserID, expenseID = expense.ExpenseID })
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion CREATE EXPENSE

		#region EDIT EXPENSE

		[HttpGet]
		public ActionResult Edit(int? userID, int expenseID, string fileName = "", string filePath = "", string callingPage = "")
		{
			//User
			var user = this.ContextUser;

			if (userID.HasValue && userID.Value != user.UserID)
				user = UserRepo.GetByID(userID.Value);

			if (user == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoUser_Message));

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			//Date
			var date = InputHistory.GetDateTime(HistoryItemType.ExpenseDate, DateTime.Today).Value;

			//Expense
			var expense = ExpenseRepo.GetByID(expenseID);

			//Original Expense Date
			InputHistory.Set(HistoryItemType.ExpenseOriginalDate, expense.ExpenseDate);

			//Attachments
			var listOfFiles = ExpenseRepo.GetListOfFilesByExpenseID(expenseID);

			this.SetViewDataLists(expense);

			var model = new ExpenseEditModel(user, expense, listOfFiles, fileName, filePath, callingPage);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult Edit(int id, FormCollection form)
		{
			var expense = ExpenseRepo.GetByID(id);
			var newExpenseDate = Convert.ToDateTime(form["ExpenseDate"]);

			if (newExpenseDate.ToString() != InputHistory.Get(HistoryItemType.ExpenseOriginalDate, true).ToString())
			{
				//Determine if there are any attachments and then move
				string[] fileList = ExpenseRepo.GetListOfFileNamesByExpenseID(id).Select(f => f).ToArray();

				if (fileList.Length > 0)
					this.FileMoveOrDelete(this.CurrentUserID, id, fileList, "MoveExpenseAttachments", newExpenseDate);
			}

			if (expense == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoExpense_Message));

			return this.Save(expense);
		}

		#endregion EDIT EXPENSE

		#region DELETE EXPENSE

		[HttpPost]
		public ActionResult DeleteExpense(int userID, int expenseID)
		{
			try
			{
				// Delete all the attachments first
				string[] fileList = ExpenseRepo.GetListOfFileNamesByExpenseID(expenseID).Select(f => f).ToArray();
				if (fileList.Length > 0)
					this.FileMoveOrDelete(userID, expenseID, fileList, "DeleteExpense");

				var expense = ExpenseRepo.GetByID(expenseID);
				ExpenseRepo.Delete(expense);

				var user = this.CurrentUser;

				user = UserRepo.GetByID(userID);

				if (user == null)
					return this.ErrorView(new ErrorModel(Resources.ERR_NoUser_Message));

				if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
					return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

				Settings.Current.ContextUser = user;

				var model = new ExpenseIndexModel(user, InputHistory.GetDateTime(HistoryItemType.ExpenseDate, DateTime.Today).Value);

				return this.PartialView("ucIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion DELETE EXPENSE

		#region LOOK UP AND VIEW DATA

		private void SetViewData(bool? locked = true) => this.ViewData["StatusFilter"] = new List<SelectListItem>
			{
				new SelectListItem() { Value = "1", Text = "Locked", Selected = locked == true },
				new SelectListItem() { Value = "0", Text = "Unlocked", Selected = locked == false }
			};

		private void SetViewDataLists(Expense model)
		{
			this.AddAsDropdownSource(ClientRepo.GetViewDataActiveClientsForUser(this.ContextUserID).OrderBy(c => c.ClientName).ToList(), "Client", model.ClientID);
			this.AddAsDropdownSource(ProjectRepo.GetViewDataActiveProjectsForUser(this.ContextUserID).OrderBy(c => c.ProjectName).ToList(), "Project", model.ProjectID);
			this.AddAsDropdownSource(ExpenseCategoryTypeRepo.GetAll().OrderBy(e => e.ExpenseCategoryTypeName).ToList(), "ExpenseCategoryType", model.ExpenseCategoryTypeID);
		}

		#endregion LOOK UP AND VIEW DATA

		#region FILE ADD

		[HttpPost]
		public ActionResult FileAdd(ExpenseAttachment expenseAttachmentModel, int userID, int expenseID, DateTime expenseDate)
		{
			var file = GetFileNameAndPath(expenseAttachmentModel, userID, expenseID, expenseDate);

			try
			{
				//Save the File to the Directory (Folder).
				expenseAttachmentModel.AttachmentFile.SaveAs(file.filePathAndName);
				this.ModelState.Clear();

				var expenseAttachment = ExpenseAttachment.GetNew(expenseID, file.filePath, file.fileName, file.mimeType);

				this.UpdateModel(expenseAttachment);

				//Save file details to database
				expenseAttachment.Save();

				this.Dispose();

				return this.RedirectToAction("Edit", "Expense", new { userID, expenseID });
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult FileMoveOrDelete(int userID, int expenseID, string[] fileList, string callingPage = "", DateTime newExpenseDate = default)
		{
			string filePath = "";
			string filePathAndName = "";

			var expenseAttachment = new ExpenseAttachment();

			this.ModelState.Clear();

			try
			{
				foreach (string fileName in fileList)
				{
					expenseAttachment = ExpenseAttachment.GetExpenseAttachmentByFileName(fileName);
					filePath = expenseAttachment.FilePath;
					filePathAndName = string.Format(filePath + fileName);

					if (callingPage == "MoveExpenseAttachments") //Move files
					{
						// Move the physical file to a new path
						if (System.IO.File.Exists(filePathAndName))
						{
							//Create a new file name and file path
							var file = GetFileNameAndPath(expenseAttachment, userID, expenseID, newExpenseDate, callingPage);

							//Move and rename
							System.IO.File.Move(filePathAndName, file.filePathAndName);

							//Insert new Expense Attachment and delete the old one
							var expenseAttachmentNew = ExpenseAttachment.GetNew(expenseID, file.filePath, file.fileName, file.mimeType);

							this.UpdateModel(expenseAttachmentNew);

							//Save file details to database
							expenseAttachmentNew.Save();

							//Delete the old attachment from the database
							this.UpdateModel(expenseAttachment);
							expenseAttachment.Delete();
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
						expenseAttachment.Delete();

						this.UpdateModel(expenseAttachment);
					}
				}

				expenseAttachment.Save();

				this.Dispose();

				if (callingPage?.Length == 0)
				{
					return this.Json(new
					{
						IsError = false,
						redirectTo = this.Url.Action("Edit", "Expense", new { userID, expenseID })
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

		private static (string fileName, string filePath, string filePathAndName, string mimeType) GetFileNameAndPath(ExpenseAttachment expenseAttachment, int userID, int expenseID, DateTime expenseDate, string callingPage = "")
		{
			string year = expenseDate.ToString("yyyy");
			string month = expenseDate.ToString("MM");
			string day = expenseDate.ToString("dd");
			string rootFolderPath = Enabill.Code.Constants.PATHEXPENSE.Replace("\\", "/");
			string yearFolderPath = string.Format($"{rootFolderPath}{year}/");
			string filePath = string.Format($"{yearFolderPath}{month}/");
			string extension = "";

			if (callingPage?.Length == 0)
			{
				extension = Path.GetExtension(expenseAttachment.AttachmentFile.FileName);
			}
			else
			{
				extension = Path.GetExtension(expenseAttachment.FileName);
			}

			string fileName = string.Format($"{userID}{expenseID}{year}{month}{day}{DateTime.Now.ToString("mmssfff")}");

			//Lines below check to make sure the extension matches the actual file type
			string filePathAndName = string.Format(filePath + fileName + extension);
			string mimeType = MimeTypes.GetContentType(filePathAndName);
			extension = string.Format($".{MimeTypes.GetExtension(mimeType, extension)}");

			if (extension == ".")
				throw new ExpenseAttachmentException("File extension does not match file type or file type not allowed.");

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

		#endregion FILE ADD

		[HttpPost]
		public ActionResult RefreshList(string q, bool locked)
		{
			this.SetViewData(locked);

			this.ViewBag.q = InputHistory.Get(q, HistoryItemType.ExpenseSearchCriteria);
			this.ViewBag.Expense_b = InputHistory.Get(locked, HistoryItemType.ExpenseFilterBy, true);

			List<Expense> model = ExpenseRepo.FilterByAll(this.ViewBag.q, this.ViewBag.Expense_b);

			return this.PartialView("Index", model);
		}
	}
}