using System;
using System.Linq;
using System.Web.Mvc;
using Enabill.Emailing;
using Enabill.Models;
using Enabill.Properties;
using Enabill.Repos;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class FlexiTimeController : BaseController
	{
		public ActionResult Index(int? id)
		{
			var user = this.CurrentUser;

			if (id.HasValue)
				user = UserRepo.GetByID(id.Value);

			if (user == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoUser_Message));

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
			{
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
			}

			if (!user.IsFlexiTimeUser)
			{
				if (!user.HasRole(UserRoleType.TimeCapturing))
					return this.ErrorView(new ErrorModel(string.Format(Resources.ERR_NonTimeCapturerUser_Title, user.FullName), string.Format(Resources.ERR_NonTimeCapturerUser_Message, user.FullName, user.FirstName)));
				else
					return this.ErrorView(new ErrorModel(string.Format(Resources.ERR_HourlyContractedUser_Title, user.FullName), string.Format(Resources.ERR_HourlyContractedUser_Message, user.FullName, user.FirstName)));
			}

			var sD = InputHistory.GetDateTime(HistoryItemType.FlexiDay, DateTime.Today.ToFirstDayOfMonth()).Value;
			sD = sD.ToFirstDayOfMonth();
			sD = user.ConfigureDate(sD);
			InputHistory.Set(HistoryItemType.FlexiDay, sD);

			var model = new FlexiTimeIndexModel(user);

			Settings.Current.ContextUser = user;

			return this.View(model);
		}

		[HttpGet]
		public ActionResult ShowUsersFlexi(int userID, DateTime selectedMonth)
		{
			var user = UserRepo.GetByID(userID);
			InputHistory.Set(HistoryItemType.FlexiDay, selectedMonth);
			var model = new FlexiTimeIndexModel(user);
			Settings.Current.ContextUser = user;

			return this.View("Index", model);
		}

		[HttpPost]
		public ActionResult Month(string date)
		{
			try
			{
				var user = this.ContextUser;

				var dt = DateTime.Parse(date);
				dt = user.ConfigureDate(dt);
				InputHistory.Set(HistoryItemType.FlexiDay, dt);

				var model = new FlexiTimeIndexModel(user);

				return this.PartialView("ucIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult DisplayFlexiDayBooking()
		{
			try
			{
				var user = this.ContextUser;
				var date = InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value;

				InputHistory.Set(HistoryItemType.WorkDay, date);
				var model = new TimeDayModel(user, date);

				return this.PartialView("ucBookFlexiDay", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult BookFlexiDate(string remark)
		{
			try
			{
				var date = InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value;
				var user = this.ContextUser;

				if (!WorkDay.IsDayWorkable(date))
					throw new EnabillConsumerException("The selected date is either a public holiday or a weekend. Action cancelled.");

				double flexiBalance = user.CalculateFlexiBalanceOnDate(date, true);

				if (flexiBalance < user.WorkHours)
					throw new EnabillConsumerException("Insufficient hours to book a flexi day.");

				var flexiday = user.BookFlexiDay(this.CurrentUser, remark, date);

				if (flexiday.ApprovalStatusID == (int)ApprovalStatusType.Pending)
				{
					EnabillEmails.NotifyManagerOfFlexiDayAuthorisation(this.ContextUser, flexiday);
					var noException = new Exception();

					return this.ReturnJsonException(noException, "Please note that you have booked a flexi day in excess of the 2 day monthly limit. The request has been emailed to your manager for authorisation.");
				}
				else
				{
					if (this.ContextUserID == this.CurrentUserID)
						EnabillEmails.NotifyManagerOfBookedFlexiDay(this.ContextUser, flexiday);
					else
						EnabillEmails.NotifyUserOfBookedFlexiDayByManager(this.CurrentUser, this.ContextUser, flexiday);
				}

				return this.PartialView("ucDay", new TimeDayModel(this.ContextUser, date));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "Error occurred. Flexiday booking unsuccessful.");
			}
		}

		[HttpPost]
		public ActionResult RemoveFlexiDay(int flexiDayID)
		{
			try
			{
				this.ContextUser.RemoveFlexiDay(this.CurrentUser, flexiDayID);

				return this.PartialView("ucIndex", new FlexiTimeIndexModel(this.ContextUser));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "FlexiDay could not be removed. Unknown error occurred.");
			}
		}

		[HttpPost]
		public ActionResult AdjustFlexiBalanceView(int id)
		{
			var user = this.ContextUser;
			var flexiBalanceAdjustment = new FlexiBalanceAdjustment();

			if (id != 0)
			{
				flexiBalanceAdjustment = UserRepo.GetFlexiBalanceManualAdjustmentByID(id);
			}
			else
			{
				flexiBalanceAdjustment.FlexiBalanceAdjustmentID = 0;
				flexiBalanceAdjustment.ImplementationDate = DateTime.Today;
				flexiBalanceAdjustment.Remark = "";
			}

			return this.PartialView("ucAdjustFlexiBalance", flexiBalanceAdjustment);
		}

		[HttpPost]
		public ActionResult AdjustFlexiBalance(int flexiAdjustmentID, string date, string movement, string remark)
		{
			try
			{
				var user = this.ContextUser;

				if (!DateTime.TryParse(date, out var impDate))
					throw new EnabillConsumerException("Please select an effective date.");

				if (!double.TryParse(movement, out double adjustment))
					throw new EnabillConsumerException("Error detected with the adjustment amount. Please try again");

				if (flexiAdjustmentID == 0)
				{
					user.AdjustFlexiBalance(this.CurrentUser, adjustment, remark, impDate);
				}
				else
				{
					var flexiBalanceAdjustment = UserRepo.GetFlexiBalanceManualAdjustmentByID(flexiAdjustmentID);
					var prevImplementationDate = flexiBalanceAdjustment.ImplementationDate;

					flexiBalanceAdjustment.Adjustment = adjustment;
					flexiBalanceAdjustment.Remark = remark;
					flexiBalanceAdjustment.DateAdjusted = impDate;
					flexiBalanceAdjustment.ImplementationDate = impDate;
					flexiBalanceAdjustment.LastModifiedBy = this.CurrentUser.FullName;
					UserRepo.SaveFlexiBalanceAdjustment(flexiBalanceAdjustment);
					//Update the user time from earliest month of change
					user.ExecuteFlexiBalanceProcess(this.CurrentUser, impDate > prevImplementationDate ? prevImplementationDate : impDate);
				}

				return this.PartialView("ucIndex", new FlexiTimeIndexModel(user));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult DeleteFlexiBalanceAdjustment(int id)
		{
			try
			{
				var user = this.ContextUser;

				if (user == null)
					throw new UserManagementException(Resources.ERR_NoUser_Message);

				var flexiBalanceAdjustment = user.GetFlexiBalanceManualAdjustmentByID(id);
				var implementationDate = flexiBalanceAdjustment.ImplementationDate;

				if (flexiBalanceAdjustment != null)
					UserRepo.DeleteFlexiBalanceManualAdjustment(flexiBalanceAdjustment);

				//Update the user time from implemtationdate till current
				user.ExecuteFlexiBalanceProcess(this.CurrentUser, implementationDate);

				return this.PartialView("ucIndex", new FlexiTimeIndexModel(user));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "Unknown error occurred.");
			}
		}

		[HttpPost]
		public ActionResult SelectDay(string dateString)
		{
			try
			{
				var user = this.ContextUser;
				var date = InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value;

				if (dateString.ToDate().HasValue)
					date = dateString.ToDate().Value;

				InputHistory.Set(HistoryItemType.WorkDay, date);
				var model = new TimeDayModel(user, date);

				this.SetViewDataLists(model);

				return this.PartialView("ucDay", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		private void SetViewDataLists(TimeDayModel model) => this.ViewData["ExtraAllocation"] = model.ExtraAllocations
					  .Select(res => new SelectListItem { Text = res.ClientName + " - " + res.ProjectName + " - " + res.ActivityName, Value = res.ActivityID.ToString() });

		#region MANAGE FLEXIDAYS

		[HttpPost]
		public ActionResult ManageFlexiDay(int flexiDayID, bool approved)
		{
			try
			{
				var user = this.ContextUser;

				var flexiDay = FlexiDayRepo.GetByID(flexiDayID);

				if (flexiDay == null)
					throw new FlexiDayException("FlexiDay could not be found. Please try again.");

				if (user.UserID != flexiDay.UserID)
					throw new EnabillConsumerException("The context user and the user for which the flexiDay is being managed are not the same. Action cancelled.");

				if (approved)
					flexiDay.ApproveFlexiDayRequest(this.CurrentUser);
				else
					flexiDay.DeclineFlexiDayRequest(this.CurrentUser);

				EnabillEmails.NotifyUserOfFlexiDayApproval(this.CurrentUser, user, flexiDay);

				return this.PartialView("ucIndex", new FlexiTimeIndexModel(user));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult UpdateFlexiDay(int id, FormCollection form)
		{
			string remark = form["FlexiDayRemark"];

			var flexiDay = FlexiDayRepo.GetByID(id);

			this.UpdateModel(flexiDay);
			flexiDay.Remark = remark;
			FlexiDayRepo.Save(flexiDay);

			return this.PartialView("ucIndex", new FlexiTimeIndexModel(this.ContextUser));
		}

		[HttpPost]
		public ActionResult WithdrawFlexiDay(int flexiDayID)
		{
			try
			{
				var flexiDay = FlexiDayRepo.GetByID(flexiDayID);
				flexiDay.WithdrawFlexiDay(flexiDayID);

				return this.PartialView("ucIndex", new FlexiTimeIndexModel(this.ContextUser));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "Unknown error occurred. Please retry withdrawing the leave.");
			}
		}

		[HttpPost]
		public ActionResult GetFlexiDayManagementView(int id)
		{
			try
			{
				var user = this.ContextUser;
				var flexiDay = user.GetFlexiDay(id);

				if (flexiDay == null)
					throw new EnabillConsumerException("Error");

				return this.PartialView("ucManageFlexiDay", new FlexiDayModel(user, flexiDay));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		//private void SetViewData(FlexiDay flexiDaye)
		//{
		//    ViewData["LeaveType"] = new SelectList(Enabill.Web.Extensions.GetEnumSelectList<LeaveTypeEnum>(), "Value", "Text", null);
		//    ViewData["Hours"] = Helpers.GetDropDownOfNumbers(1, Convert.ToInt32(ContextUser.WorkHours - 1)).Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Key.ToString(), Selected = (leave == null ? m.Key == 0 : m.Key == leave.GetNumberOfHours) });
		//}

		#endregion MANAGE FLEXIDAYS
	}
}