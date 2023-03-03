using System;
using System.Linq;
using System.Web.Mvc;
using Enabill.Emailing;
using Enabill.Properties;
using Enabill.Repos;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class ManagerController : BaseController
	{
		[HttpGet]
		public ActionResult Index()
		{
			if (this.CurrentUser.HasRole(UserRoleType.Manager))
				return this.View(new ManagerIndexModel(this.CurrentUser));

			return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
		}

		[HttpPost]
		public MvcHtmlString CalculateFlexiBalance(int userIDs)
		{
			string value = string.Empty;
			var user = UserRepo.GetByID(userIDs);
			value = UserDetailModel.PopulateFlexiBalance(user, true);
			string val = value + " - Calculate Flexi Balance";

			return MvcHtmlString.Create(string.Format($"<a href='#' onclick=\"{string.Empty}\">{val}</a>"));
		}

		#region FLEXIDAY

		/* OLD
		[HttpPost]
		public ActionResult BookFlexiDayView()
		{
			ViewData["UserList"] = CurrentUser.GetStaffOfManager(UserRoleType.TimeCapturing).Select(u => new SelectListItem { Value = u.UserID.ToString(), Text = u.FullName });
			return PartialView("ucBookFlexiDay");
		}
		*/

		/* OLD
		[HttpPost]
		public ActionResult SubmitFlexiDay(int userID, string date)
		{
			//This action is not being used, if it is to be used, remember to add the remark textbox to view etc
			string remark = "";

			try
			{
				if (CurrentUser == null)
					throw new EnabillException("You are not logged in. Please proceed to the login screen.");

				DateTime flexiDate = DateTime.Parse(date);
				if (!flexiDate.IsWorkableDay())
					throw new FlexiDayException("You cannot book a FlexiDay on a weekend or public holiday.");

				User user = UserRepo.GetByID(userID);
				if (user == null)
					throw new EnabillException("The user could not be found. FlexiDay booking cancelled.");

				FlexiDay flexiDay = user.BookFlexiDay(CurrentUser, remark, flexiDate);
				EnabillEmails.NotifyUserOfBookedFlexiDayByManager(CurrentUser, user, flexiDay);

				return PartialView("ucRecentFlexiDays", new ManagerIndexModel(CurrentUser).RecentFlexiDays);
			}
			catch (Exception ex)
			{
				return ReturnJsonException(ex);
			}
		}
		*/

		/* OLD
		[HttpPost]
		public ActionResult RemoveFlexiDay(int flexiDayID)
		{
			try
			{
				if (CurrentUser == null)
					throw new EnabillException("You are not logged in. Please proceed to the login screen.");

				FlexiDay flexiDay = FlexiDayRepo.GetByID(flexiDayID);
				if (flexiDay == null)
					throw new FlexiDayException("The FlexiDay being deleted could not be found.");

				User user = UserRepo.GetByID(flexiDay.UserID);
				user.RemoveFlexiDay(CurrentUser, flexiDay);

				return PartialView("ucRecentFlexiDays", new ManagerIndexModel(CurrentUser).RecentFlexiDays);
			}
			catch (Exception ex)
			{
				return ReturnJsonException(ex);
			}
		}
		*/

		#endregion FLEXIDAY

		#region FLEXI BALANCE

		[HttpPost]
		public ActionResult UpdateFlexiBalanceView()
		{
			this.ViewData["UserList"] = this.CurrentUser.GetStaffOfManager()
									.Where(u => u.HasRole(UserRoleType.TimeCapturing))
									.ToList()
									.Select(u => new SelectListItem { Value = u.UserID.ToString(), Text = u.FullName });

			return this.PartialView("ucUpdateFlexiBalance");
		}

		[HttpPost]
		public ActionResult UpdateFlexiBalance(int userID, string movement, string date)
		{
			try
			{
				if (this.CurrentUser == null)
					throw new EnabillException("You are not logged in. Please proceed to the login screen.");

				double adjustment = double.Parse(movement);
				var adjustmentDate = DateTime.Parse(date);

				var user = UserRepo.GetByID(userID);

				if (user == null)
					throw new EnabillException("The user could not be found. Update cancelled.");

				user.AdjustFlexiBalance(this.CurrentUser, adjustment, "test", adjustmentDate);

				return this.PartialView("ucFlexiBalances", new ManagerIndexModel(this.CurrentUser).FlexiBalance);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion FLEXI BALANCE

		#region LEAVE

		/*
		 * This Action lives on the LeaveController and all the logic is there, this action should never
		 * be used again, but if it is, then make sure to update the code here as this action is not
		 * being used.
		*/
		/*
		[HttpPost]
		public ActionResult ManageLeave(int leaveID, bool approved)
		{
			try
			{
				if (CurrentUser == null)
					throw new EnabillException("You are not logged in. Please proceed to the login screen.");

				Leave leave = LeaveRepo.GetByID(leaveID);
				if (leave == null)
					throw new LeaveException("This leave could not be found. Please try again.");

				if (approved)
				{
					leave.ApproveLeaveRequest(CurrentUser);
					//EnabillEmails TODO: send email to user from site about user approving/declining leave
				}
				else
				{
					leave.DeclineLeaveRequest(CurrentUser);
				}

				return PartialView("ucRecentLeave", new ManagerIndexModel(CurrentUser).RecentLeave);
			}
			catch (Exception ex)
			{
				return ReturnJsonException(ex);
			}
		}

		*/

		[HttpPost]
		public ActionResult GetBookLeaveDayForUserPartial()
		{
			this.SetLeaveViewData();

			return this.PartialView("ucBookLeaveDayForUser");
		}

		[HttpPost]
		public ActionResult GetBookLeaveDaysForUserPartial()
		{
			this.SetLeaveViewData();

			return this.PartialView("ucBookLeaveDaysForUser");
		}

		[HttpPost]
		public ActionResult BookLeaveForUser(int id, FormCollection form)//id is used to know which view was used to capture leave, 1 = ucBookLeaveDayForUser, 2 = ucBookLeaveDaysForUser
		{
			try
			{
				if (id == 1)
				{
					this.BookSingleLeaveDayForUser(form);
				}
				else if (id == 2)
				{
					this.BookMultipleLeaveDaysForUser(form);
				}
				else
				{
					throw new EnabillConsumerException("Error");
				}

				return this.PartialView("ucIndex", new ManagerIndexModel(this.CurrentUser));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		private void BookSingleLeaveDayForUser(FormCollection form)
		{
			var user = UserRepo.GetByID(int.Parse(form["UserID"]));

			if (user == null)
				throw new EnabillConsumerException("Error.");

			var leaveType = (LeaveTypeEnum)int.Parse(form["LeaveTypeID"]);
			var startDate = DateTime.Parse(form["LeaveDate"]);
			int hours = 0;

			if (leaveType == LeaveTypeEnum.BirthDay)
			{
				hours = int.Parse(form["BirthdayHours"]);
			}
			else
			{
				hours = int.Parse(form["Hours"]);
			}

			string remark = form["LeaveRemark"];

			var leave = user.ApplyForLeave(this.CurrentUser, leaveType, startDate, startDate, hours, remark);
			leave.ApproveLeaveRequest(this.CurrentUser);

			EnabillEmails.NotifyUserOfLeaveBookedByManager(this.CurrentUser, user, leave);
		}

		private void BookMultipleLeaveDaysForUser(FormCollection form)
		{
			var user = UserRepo.GetByID(int.Parse(form["UserID"]));

			if (user == null)
				throw new EnabillConsumerException("Error.");

			var leaveType = (LeaveTypeEnum)int.Parse(form["LeaveTypeID"]);
			var startDate = DateTime.Parse(form["LeaveDateFrom"]);
			var endDate = DateTime.Parse(form["LeaveDateTo"]);
			string remark = form["LeaveRemark"];

			var leave = user.ApplyForLeave(this.CurrentUser, leaveType, startDate, endDate, null, remark);
			leave.ApproveLeaveRequest(this.CurrentUser);

			EnabillEmails.NotifyUserOfLeaveBookedByManager(this.CurrentUser, user, leave);
		}

		#endregion LEAVE

		#region LEAVE LOOK UPS

		private void SetLeaveViewData()
		{
			this.ViewData["UserID"] = this.CurrentUser.GetStaffOfManager().Select(u => new SelectListItem() { Value = u.UserID.ToString(), Text = u.FullName });
			this.ViewData["LeaveTypeID"] = new SelectList(Enabill.Extensions.GetEnumSelectList<LeaveTypeEnum>(), "Value", "Text", null);
			this.ViewData["Hours"] = Helpers.GetDropDownOfNumbers(1, Convert.ToInt32(this.ContextUser.WorkHours)).Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Key.ToString() });
		}

		#endregion LEAVE LOOK UPS

		#region PROFILES

		[HttpGet]
		public ActionResult Profile(int? id)
		{
			var user = this.CurrentUser;

			if (id.HasValue)
				user = UserRepo.GetByID(id.Value);

			if (user == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoUser_Message));

			if (!this.CurrentUser.CanManage(user) && this.CurrentUserID != user.UserID && !this.CurrentUser.HasRole(UserRoleType.Accountant))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			Settings.Current.ContextUser = user;
			var model = new UserProfileModel(user);

			return this.View(model);
		}

		#endregion PROFILES
	}
}