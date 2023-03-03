using System;
using System.Web.Mvc;
using Enabill.Models;
using Enabill.Repos;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	public class HomeController : BaseController
	{
		[HttpGet]
		public ActionResult Index()
		{
			if (this.CurrentUser != null)
			{
				if (this.CurrentUser.EmployStartDate.Date > DateTime.Now.Date)
					this.RedirectToAction("Logoff", "Account");
				else
					return this.RedirectToAction("Dashboard");
			}

			this.ViewBag.ShowForgottenPasswordLink = true;

			return this.View("LogOn", new LogOnModel() { IsLoginUnsuccessful = false, IsAccesDenied = false });
		}

		[Authorize]
		public ActionResult Dashboard()
		{
			Settings.Current.ContextUser = this.CurrentUser;

			if (this.CurrentUser == null)
				return this.RedirectToAction("Index");

			Settings.Current.CurrentUser = UserRepo.GetByID(this.CurrentUserID);
			Settings.Current.ResetCache();

			var model = new DashboardViewModel(this.CurrentUser);

			return this.View(model);
		}

		[Authorize, HttpPost]
		public ActionResult ToggleTable(int tableToToggle)
		{
			/*
			tableToToggle, 1 = My Timesheet
						   2 = My FlexiTime Balance
						   3 = My Leave Balance
						   4 = My Upcoming Leave
			*/

			var userPref = this.CurrentUser.GetUserPreference();
			userPref.ToggleColumn((CollapseColumnType)tableToToggle);
			Settings.Current.CurrentUser = this.CurrentUser;

			return this.ReturnJsonResult(false, "Toggle Complete.");
		}

		[Authorize]
		public ActionResult Reset() => this.RedirectToAction("Dashboard");

		[HttpGet]
		public ActionResult Passphrase(string cont = "Home", string ac = "Index")
		{
			this.ViewData["cont"] = cont;
			this.ViewData["ac"] = ac;

			return this.View();
		}

		[HttpPost]
		public ActionResult Passphrase(FormCollection form)
		{
			string cont = form["cont"];
			string ac = form["ac"];
			Settings.Current.Passphrase = form["Passphrase"];

			if (Settings.Current.PassphraseIsValid)
				return this.RedirectToAction(ac, cont);
			else
				return this.RedirectToAction("Passphrase", new { cont, ac });
		}

		[HttpPost]
		public ActionResult ApproveLeave(string leaveIDs)
		{
			try
			{
				User user = null;

				if (leaveIDs != "")
				{
					foreach (int id in leaveIDs.ToIntArray())
					{
						var leave = LeaveRepo.GetByID(id);
						leave.ApprovalStatus = (int)ApprovalStatusType.Approved;
						user = UserRepo.GetByID(leave.UserID);

						leave.ApproveLeaveRequest(this.CurrentUser);

						if (leave.LeaveType == (int)LeaveTypeEnum.Unpaid && user.EmploymentTypeID != (int)EmploymentTypeEnum.HourlyContractor)
							Emailing.EnabillEmails.NotifyUserOfUnpaidLeaveApproval(user, leave);
						else
							Emailing.EnabillEmails.NotifyUserOfLeaveApproval(this.CurrentUser, user, leave);

						LeaveRepo.Save(leave);
					}
				}
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
			return this.RedirectToAction("Dashboard");
		}
	}
}