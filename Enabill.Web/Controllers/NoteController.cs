using System;
using System.Web.Mvc;
using Enabill.Models;
using Enabill.Properties;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class NoteController : BaseController
	{
		//protected override string GetSearchLabelText() { return "notes"; }

		[HttpGet]
		public ActionResult Index() => this.ManageIndexRedirect();

		[HttpPost]
		public ActionResult Index(FormCollection form)
		{
			try
			{
				InputHistory.Set(HistoryItemType.NoteSearchActivityList, form["ActivityList"]);
				InputHistory.Set(HistoryItemType.NoteSearchUserList, form["UserList"]);
				InputHistory.Set(HistoryItemType.NoteSearchDateFrom, DateTime.Parse(form["DateFrom"]));
				InputHistory.Set(HistoryItemType.NoteSearchDateTo, DateTime.Parse(form["DateTo"]));
				InputHistory.Set(HistoryItemType.NoteSearchKeyWord, form["KeyWord"]);
				InputHistory.Set(HistoryItemType.NoteSearchListAmount, form["SearchAmt"]);

				string activityIDs = InputHistory.Get(HistoryItemType.NoteSearchActivityList, string.Empty);
				string userIDs = InputHistory.Get(HistoryItemType.NoteSearchUserList, string.Empty);
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.NoteSearchDateFrom, EnabillSettings.SiteStartDate).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.NoteSearchDateTo, DateTime.Today.AddDays(7)).Value;
				string keyWord = InputHistory.Get(HistoryItemType.NoteSearchKeyWord, string.Empty);
				int noteSearchAmt = InputHistory.Get(HistoryItemType.NoteSearchListAmount, 10000);

				var noteModel = Note.GetForSearchCriteria(this.CurrentUser, activityIDs, userIDs, dateFrom, dateTo, keyWord, noteSearchAmt);

				return this.PartialView("ucNoteList", noteModel);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		private ActionResult ManageIndexRedirect()
		{
			string activityIDs = InputHistory.Get(HistoryItemType.NoteSearchActivityList, string.Empty);
			string userIDs = InputHistory.Get(HistoryItemType.NoteSearchUserList, string.Empty);
			var dateFrom = InputHistory.GetDateTime(HistoryItemType.NoteSearchDateFrom, EnabillSettings.SiteStartDate).Value;
			var dateTo = InputHistory.GetDateTime(HistoryItemType.NoteSearchDateFrom, DateTime.Today.AddDays(7)).Value;
			string keyWord = InputHistory.Get(HistoryItemType.NoteSearchUserList, string.Empty);
			int noteSearchAmt = InputHistory.Get(HistoryItemType.NoteSearchUserList, 10000);

			if (this.CurrentUser.HasRole(UserRoleType.ProjectOwner) && this.CurrentUser.HasRole(UserRoleType.Manager))
				return this.CompleteOverview(activityIDs, userIDs, dateFrom, dateTo, keyWord, noteSearchAmt);

			if (this.CurrentUser.HasRole(UserRoleType.ProjectOwner))
				return this.ProjectOwnerOverview(activityIDs, dateFrom, dateTo, keyWord, noteSearchAmt);

			if (this.CurrentUser.HasRole(UserRoleType.Manager))
				return this.ManagerOverview(userIDs, dateFrom, dateTo, keyWord, noteSearchAmt);

			if (this.CurrentUser.HasRole(UserRoleType.TimeCapturing))
				return this.TimeCapturerOverview(activityIDs, dateFrom, dateTo, keyWord, noteSearchAmt);

			return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
		}

		private ActionResult CompleteOverview(string activityIDs, string userIDs, DateTime dateFrom, DateTime dateTo, string keyWord, int noteSearchAmt)
		{
			var noteModel = Note.GetForSearchCriteria(this.CurrentUser, activityIDs, userIDs, dateFrom, dateTo, keyWord, noteSearchAmt);

			var model = new NoteCompleteOverviewIndexModel(this.CurrentUser, noteModel);

			return this.View("CompleteOverview", model);
		}

		private ActionResult ProjectOwnerOverview(string activityIDs, DateTime dateFrom, DateTime dateTo, string keyWord, int noteSearchAmt)
		{
			var noteModel = Note.GetForSearchCriteria(this.CurrentUser, activityIDs, null, dateFrom, dateTo, keyWord, noteSearchAmt);

			var model = new NoteProjectOwnerOverviewIndexModel(this.CurrentUser, noteModel);

			return this.View("ProjectOwnerOverview", model);
		}

		private ActionResult ManagerOverview(string userIDs, DateTime dateFrom, DateTime dateTo, string keyWord, int noteSearchAmt)
		{
			var noteModel = Note.GetForSearchCriteria(this.CurrentUser, null, userIDs, dateFrom, dateTo, keyWord, noteSearchAmt);

			var model = new NoteManagerOverviewIndexModel(this.CurrentUser, noteModel);

			return this.View("ManagerOverview", model);
		}

		private ActionResult TimeCapturerOverview(string activityIDs, DateTime dateFrom, DateTime dateTo, string keyWord, int noteSearchAmt)
		{
			var noteModel = Note.GetForSearchCriteria(this.CurrentUser, activityIDs, null, dateFrom, dateTo, keyWord, noteSearchAmt);

			var model = new NoteOverviewIndexModel(this.CurrentUser, noteModel);

			return this.View("IndexOverview", model);
		}

		[HttpPost]
		public ActionResult QuickEditView(int workAllocationID)
		{
			try
			{
				var wa = WorkAllocation.GetByID(this.CurrentUser, workAllocationID);

				if (wa == null)
					throw new EnabillConsumerException("Either the note could not be found, or you do not have the required permissions to view it.");

				var model = wa.GetDetailedNote(this.CurrentUser);

				return this.PartialView("ucNoteQuickEdit", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult QuickEdit(int workAllocationID, string noteText, string calledFrom) //id = workAllocationID
		{
			try
			{
				var wa = WorkAllocation.GetByID(this.CurrentUser, workAllocationID);

				if (wa == null)
					return this.ErrorView(new ErrorModel(Resources.ERR_NoWorkAllocationForNoteCreateUpdate_Message));

				var note = wa.GetNote(this.CurrentUser);
				note.NoteText = noteText;

				wa.SaveNote(this.CurrentUser, note);

				if (calledFrom == "/Note/Index/")
				{
					return this.PartialView("ucNoteDetail", note.GetDetailed());
				}
				return this.Json(new
				{
					IsError = false,
					Description = "Changes to the note were saved successfully.",
					DayHasNotes = this.ContextUser.GetNotes(wa.DayWorked, wa.DayWorked).Count > 0
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}
	}
}