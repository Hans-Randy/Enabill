using System;
using System.Linq;
using System.Web.Mvc;
using Enabill.Emailing;
using Enabill.Models;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class FeedbackController : BaseController
	{
		public ActionResult Index()
		{
			var user = Settings.Current.SystemUser;

			var model = new FeedbackIndexModel(Settings.Current.SystemUser, null);
			this.SetViewData();

			return this.View(model);
		}

		[HttpPost]
		public ActionResult QuickView(int feedbackThreadID)
		{
			var thread = new FeedbackThread();
			if (feedbackThreadID > 0)
				thread = FeedbackThread.GetByID(Settings.Current.SystemUser, feedbackThreadID);

			if (thread.FeedbackThreadID > 0)
			{
				return this.PartialView("ucExistingThread", new FeedbackInstanceModel(Settings.Current.SystemUser, thread));
			}
			else
			{
				this.SetViewData();

				return this.PartialView("ucNewThread", new FeedbackInstanceModel(Settings.Current.SystemUser, thread));
			}
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult SubmitNewThread(FormCollection form)
		{
			try
			{
				int feedBackTypeID = int.Parse(form["FeedbackTypeID"]);
				int feedBackUrgencyTypeID = int.Parse(form["FeedbackUrgencyTypeID"]);
				string threadSubject = form["Subject"];
				string postText = form["PostText"];

				var thread = this.CurrentUser.CreateNewThread(feedBackTypeID, feedBackUrgencyTypeID, threadSubject, postText);

				EnabillEmails.NotifyFeedbackAdminOfNewThread(this.CurrentUser, thread);

				return this.PartialView("ucIndex", new FeedbackIndexModel(this.CurrentUser, thread));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult SubmitThreadPost(int feedbackThreadID, string postText)
		{
			try
			{
				var thread = FeedbackThread.GetByID(this.CurrentUser, feedbackThreadID);

				if (thread == null)
					throw new EnabillConsumerException("Error detected. Please try again.");

				thread.AddPost(this.CurrentUser, postText);

				return this.PartialView("ucIndex", new FeedbackIndexModel(this.CurrentUser, thread));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult CloseThread(int threadID)
		{
			try
			{
				var thread = FeedbackThread.GetByID(this.CurrentUser, threadID);

				thread.CloseThread(this.CurrentUser);
				EnabillEmails.NotifyUsersOfClosedThread(this.CurrentUser, thread);

				this.SetViewData();

				return this.PartialView("ucIndex", new FeedbackIndexModel(this.CurrentUser, null));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		public void SetViewData()
		{
			this.ViewData["FeedbackTypeID"] = FeedbackType.GetAll().Select(t => new SelectListItem() { Value = t.FeedbackTypeID.ToString(), Text = t.FeedbackTypeName });
			this.ViewData["FeedbackUrgencyTypeID"] = FeedbackUrgencyType.GetAll().Select(t => new SelectListItem() { Value = t.FeedbackUrgencyTypeID.ToString(), Text = t.FeedbackUrgencyTypeName });
		}
	}
}