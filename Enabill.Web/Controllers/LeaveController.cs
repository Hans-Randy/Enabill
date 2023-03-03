using System;
using System.Collections.Generic;
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
	public class LeaveController : BaseController
	{
		public ActionResult Index(int? id, FormCollection form)
		{
			var user = this.CurrentUser;
			if (id.HasValue)
			{
				var tempUser = UserRepo.GetByID(id.Value);

				if (tempUser == null)
					return this.ErrorView(new ErrorModel(Resources.ERR_NoUser_Message));

				user = tempUser;
			}

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			var sD = InputHistory.GetDateTime(HistoryItemType.LeaveStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(-6)).Value;

			if (form["StartDate"].ToDate().HasValue)
				sD = form["StartDate"].ToDate().Value.ToFirstDayOfMonth();
			InputHistory.Set(HistoryItemType.LeaveStartDate, sD);

			var eD = InputHistory.GetDateTime(HistoryItemType.LeaveEndDate, DateTime.Today.ToLastDayOfMonth()).Value;

			if (form["EndDate"].ToDate().HasValue)
				eD = form["EndDate"].ToDate().Value.ToLastDayOfMonth();

			InputHistory.Set(HistoryItemType.LeaveEndDate, eD);

			var model = new LeaveIndexModel(user);
			Settings.Current.ContextUser = user;

			return this.View(model);
		}

		#region BOOK LEAVE

		[HttpPost]
		public ActionResult BookLeave(int id, FormCollection form)
		{
			try
			{
				var user = this.ContextUser;

				if (id == 1) //Booking a single day
				{
					this.RequestLeaveDay(user, form);
				}
				else if (id == 2) // Bokking for multiple days
				{
					this.RequestLeaveDays(user, form);
				}
				else
				{
					throw new EnabillDomainException();
				}

				var model = new LeaveIndexModel(user);

				return this.PartialView("ucIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		private void RequestLeaveDay(User user, FormCollection form)
		{
			DateTime startDate;

			try
			{
				startDate = DateTime.Parse(form["LeaveDate"]);
			}
			catch
			{
				throw new EnabillException("Errors have been detected with the leave date received");
			}

			int leaveType, hours;
			string remark = "";

			try
			{
				remark = form["LeaveRemark"];
				leaveType = int.Parse(form["LeaveType"]);

				if (leaveType == (int)LeaveTypeEnum.BirthDay)
				{
					hours = int.Parse(form["BirthdayHours"]);
				}
				else
				{
					hours = int.Parse(form["Hours"]);
				}
			}
			catch
			{
				throw new EnabillDomainException("An error has occurred. Please try again.");
			}

			if (!Enum.IsDefined(typeof(LeaveTypeEnum), leaveType))
				throw new EnabillException("Errors have been detected with the leave type received.");

			var leave = user.ApplyForLeave(this.CurrentUser, (LeaveTypeEnum)leaveType, startDate, startDate, hours, remark);

			List<LeaveCycleBalance> leaveCycleWhereLimitReached = null;

			if (leave.LeaveType == (int)LeaveTypeEnum.Sick || leave.LeaveType == (int)LeaveTypeEnum.Compassionate)
			//if (leave.LeaveType != (int)LeaveTypeEnum.Annual || leave.LeaveType != (int)LeaveTypeEnum.Unpaid)
			{
				double taken = leave.NumberOfDays < 1 ? leave.NumberOfDays : 1;
				leaveCycleWhereLimitReached = user.LeaveCycleWhereLimitReached((LeaveTypeEnum)leave.LeaveType, leave.DateFrom, leave.DateTo, taken);
			}

			if (leaveCycleWhereLimitReached?.Count() > 0)
				EnabillEmails.NotifyManagerLeaveBalanceLimitExceeded(user, leave, leaveCycleWhereLimitReached);
			else
				EnabillEmails.NotifyManagerOfLeaveRequest(user, leave);
		}

		private void RequestLeaveDays(User user, FormCollection form)
		{
			DateTime startDate;
			DateTime endDate;
			string remark = "";

			try
			{
				startDate = DateTime.Parse(form["LeaveDateFrom"]);
				endDate = DateTime.Parse(form["LeaveDateTo"]);
				remark = form["LeaveRemark"];
			}
			catch
			{
				throw new EnabillException("Errors have been detected with the dates received");
			}

			int leaveType = int.Parse(form["LeaveType"]);

			if (!Enum.IsDefined(typeof(LeaveTypeEnum), leaveType))
				throw new EnabillException("Errors have been detected with the leave type received.");

			List<LeaveCycleBalance> leaveCycleWhereLimitReached = null;

			var leave = user.ApplyForLeave(this.CurrentUser, (LeaveTypeEnum)leaveType, startDate, endDate, null, remark);

			if (leave.LeaveType == (int)LeaveTypeEnum.Sick || leave.LeaveType == (int)LeaveTypeEnum.Compassionate)
			//if (leave.LeaveType != (int)LeaveTypeEnum.Annual || leave.LeaveType != (int)LeaveTypeEnum.Unpaid)
			{
				double taken = leave.NumberOfDays < 1 ? leave.NumberOfDays : 1;
				leaveCycleWhereLimitReached = user.LeaveCycleWhereLimitReached((LeaveTypeEnum)leave.LeaveType, leave.DateFrom, leave.DateTo, taken);
			}

			if (leaveCycleWhereLimitReached?.Count() > 0)
				EnabillEmails.NotifyManagerLeaveBalanceLimitExceeded(user, leave, leaveCycleWhereLimitReached);
			else
				EnabillEmails.NotifyManagerOfLeaveRequest(user, leave);
		}

		#region UPDATE LEAVE

		[HttpPost]
		public ActionResult UpdateLeave(int id, FormCollection form)
		{
			try
			{
				var user = this.ContextUser;
				var leave = user.GetLeave(id);

				if (leave.IsPartialDay)
				{
					this.UpdatePartialLeave(user, leave, form);
				}
				else
				{
					this.UpdateFullDayLeave(user, leave, form);
				}

				return this.PartialView("ucIndex", new LeaveIndexModel(this.ContextUser));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		private void UpdateFullDayLeave(User user, Leave leave, FormCollection form)
		{
			DateTime startDate;
			DateTime endDate;
			string remark;

			try
			{
				startDate = DateTime.Parse(form["LeaveDateFrom"]);
				endDate = DateTime.Parse(form["LeaveDateTo"]);
				remark = form["LeaveRemark"];
			}
			catch
			{
				throw new EnabillException("Errors have been detected with the dates received");
			}

			this.UpdateModel(leave);
			leave.DateFrom = startDate;
			leave.DateTo = endDate;
			leave.Remark = remark;
			user.UpdateLeave(this.CurrentUser, leave, null);
		}

		private void UpdatePartialLeave(User user, Leave leave, FormCollection form)
		{
			DateTime startDate;
			DateTime endDate;
			int hours;
			string remark;

			try
			{
				startDate = DateTime.Parse(form["LeaveDate"]);
				endDate = startDate;
				hours = int.Parse(form["Hours"]);
				remark = form["LeaveRemark"];
			}
			catch
			{
				throw new EnabillException("Errors have been detected with the dates received");
			}

			leave.DateFrom = startDate;
			leave.DateTo = endDate;
			leave.Remark = remark;
			user.UpdateLeave(this.CurrentUser, leave, hours);
		}

		#endregion UPDATE LEAVE

		#endregion BOOK LEAVE

		#region MANAGE LEAVE

		[HttpPost]
		public ActionResult ManageLeave(int leaveID, bool approved)
		{
			try
			{
				var user = this.ContextUser;

				var leave = LeaveRepo.GetByID(leaveID);

				if (leave == null)
				{
					throw new LeaveException("This leave could not be found. Please try again.");
				}

				if (user.UserID != leave.UserID)
				{
					throw new EnabillConsumerException("The context user and the user for which the leave is being managed are not the same. Action cancelled.");
				}

				if (approved)
				{
					leave.ApproveLeaveRequest(this.CurrentUser);

					if (leave.LeaveType == (int)LeaveTypeEnum.Unpaid && user.EmploymentTypeID != (int)EmploymentTypeEnum.HourlyContractor)
						EnabillEmails.NotifyUserOfUnpaidLeaveApproval(user, leave);
				}
				else
				{
					leave.DeclineLeaveRequest(this.CurrentUser);
				}

				EnabillEmails.NotifyUserOfLeaveApproval(this.CurrentUser, user, leave);

				return this.PartialView("ucIndex", new LeaveIndexModel(user));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult WithdrawLeave(int leaveID)
		{
			try
			{
				this.ContextUser.WithdrawLeave(this.CurrentUser, leaveID);

				return this.PartialView("ucIndex", new LeaveIndexModel(this.ContextUser));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "Unknown error occurred. Please retry withdrawing the leave.");
			}
		}

		#endregion MANAGE LEAVE

		#region LOOK UPS

		[HttpPost]
		public ActionResult GetBookLeaveDaysPartial()
		{
			this.SetViewData(null);

			return this.PartialView("ucBookLeaveDays");
		}

		[HttpPost]
		public ActionResult GetBookLeaveDayPartial()
		{
			this.SetViewData(null, true);

			return this.PartialView("ucBookLeaveDay");
		}

		[HttpPost]
		public ActionResult GetLeaveManagementView(int id) //Id = leaveID
		{
			try
			{
				var user = this.ContextUser;
				var leave = user.GetLeave(id);

				if (leave == null)
					throw new EnabillConsumerException("Error");
				this.SetViewData(leave);

				return this.PartialView("ucManageLeave", new UserLeaveModel(user, leave));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		private void SetViewData(Leave leave, bool isPartial = false)
		{
			var leaveTypes = Enabill.Extensions.GetEnumSelectList<LeaveTypeEnum>();

			if ((EmploymentTypeEnum)this.ContextUser.EmploymentTypeID != EmploymentTypeEnum.HourlyContractor && leaveTypes.Any(c => c.Value == "128"))
			{
				int noWorkDayLeave = leaveTypes.IndexOf(leaveTypes.First(c => c.Value == "128"));
				leaveTypes.RemoveAt(noWorkDayLeave);
			}

			if ((EmploymentTypeEnum)this.ContextUser.EmploymentTypeID != EmploymentTypeEnum.Permanent || !isPartial || this.ContextUser.ExternalRef == Enabill.Code.Constants.COMPANYNAME)
			{
				if (leaveTypes.Any(c => c.Value == "256"))
				{
					int birthDayLeave = leaveTypes.IndexOf(leaveTypes.First(c => c.Value == "256"));
					leaveTypes.RemoveAt(birthDayLeave);
				}
			}

			this.ViewData["LeaveType"] = new SelectList(leaveTypes, "Value", "Text", null);
			this.ViewData["Hours"] = Helpers.GetDropDownOfNumbers(1, Convert.ToInt32(this.ContextUser.WorkHours - 1)).Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Key.ToString(), Selected = leave == null ? m.Key == 0 : m.Key == leave.GetNumberOfHours });
			this.ViewData["BirthdayHours"] = Convert.ToInt32(Math.Ceiling(this.ContextUser.WorkHours / 2)); //Use Ceiling to round up.
		}

		#endregion LOOK UPS

		#region LEAVE MANUAL ADJUSTMENTS

		[HttpPost]
		public ActionResult LeaveManualAdjustment(int? leaveManualAdjustmentID)
		{
			try
			{
				var user = this.ContextUser;

				if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
					return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

				LeaveManualAdjustment model = null;

				if (leaveManualAdjustmentID.HasValue)
					model = user.GetLeaveManualAdjustmentByID(leaveManualAdjustmentID.Value);

				this.SetViewDataManualAdjustment(model);

				return this.PartialView("ucAddLeaveManualAdjustment", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult AddEditLeaveManualAdjustment(FormCollection form)
		{
			var user = this.ContextUser;

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			try
			{
				var leaveManualAdjustment = new LeaveManualAdjustment();

				int? leaveManualAdjustmentID = int.Parse(form["LeaveManualAdjustmentID"]);

				if (leaveManualAdjustmentID.HasValue && leaveManualAdjustmentID.Value != 0)
					leaveManualAdjustment = user.GetLeaveManualAdjustmentByID(leaveManualAdjustmentID.Value);

				//DateTime? effectiveDate = form["MonthList"].ToDate();
				var effectiveDate = DateTime.Now.ToString("yyyy MMMM").ToDate();
				leaveManualAdjustment.EffectiveDate = effectiveDate.Value;
				leaveManualAdjustment.UserID = user.UserID;
				leaveManualAdjustment.LeaveTypeID = int.Parse(form["LeaveType"]);
				leaveManualAdjustment.ManualAdjustment = Math.Round(float.Parse(form["ManualAdjustment"]), 2);
				leaveManualAdjustment.Remark = !string.IsNullOrEmpty(form["Remark"]) ? form["Remark"] : " ";
				UserRepo.SaveLeaveManualAdjustment(leaveManualAdjustment);
				user.UpdateLeaveCycleBalanceManualAdjustment(leaveManualAdjustment);

				return this.PartialView("ucIndex", new LeaveIndexModel(user));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult DeleteLeaveManualAdjustment(int id)
		{
			try
			{
				var user = this.ContextUser;

				if (user == null)
					throw new UserManagementException(Resources.ERR_NoUser_Message);

				var leaveManualAdjustment = user.GetLeaveManualAdjustmentByID(id);

				if (leaveManualAdjustment != null)
				{
					UserRepo.DeleteLeaveManualAdjustment(leaveManualAdjustment);
					//LeaveCycleBalance leaveCycleBalance = LeaveCycleBalanceRepo.GetLeaveCycleBalanceForUserDate(user.UserID, (LeaveTypeEnum)leaveManualAdjustment.LeaveTypeID, leaveManualAdjustment.EffectiveDate);
					var leaveCycleBalance = LeaveCycleBalanceRepo.GetLeaveCycleBalanceForUserPeriod(user.UserID, (LeaveTypeEnum)leaveManualAdjustment.LeaveTypeID, leaveManualAdjustment.EffectiveDate);
					user.UpdateLeaveCycleBalanceManualAdjustment(leaveCycleBalance, user.UserID, (LeaveTypeEnum)leaveManualAdjustment.LeaveTypeID);
				}

				return this.PartialView("ucIndex", new LeaveIndexModel(user));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "Unknown error occurred.");
			}
		}

		private void SetViewDataManualAdjustment(LeaveManualAdjustment lma)
		{
			var leaveTypes = Enabill.Extensions.GetEnumSelectList<LeaveTypeEnum>();
			var employmentType = (EmploymentTypeEnum)this.ContextUser.EmploymentTypeID;

			if (employmentType != EmploymentTypeEnum.HourlyContractor)
			{
				if (leaveTypes.Any(c => c.Value == "128"))
				{
					int noWorkDayLeave = leaveTypes.IndexOf(leaveTypes.First(c => c.Value == "128"));
					leaveTypes.RemoveAt(noWorkDayLeave);
				}

				if (leaveTypes.Any(c => c.Value == "8"))
				{
					int studyLeave = leaveTypes.IndexOf(leaveTypes.First(c => c.Value == "8"));
					leaveTypes.RemoveAt(studyLeave);
				}

				if (leaveTypes.Any(c => c.Value == "16"))
				{
					int maternityLeave = leaveTypes.IndexOf(leaveTypes.First(c => c.Value == "16"));
					leaveTypes.RemoveAt(maternityLeave);
				}

				if (leaveTypes.Any(c => c.Value == "32"))
				{
					int relocationLeave = leaveTypes.IndexOf(leaveTypes.First(c => c.Value == "32"));
					leaveTypes.RemoveAt(relocationLeave);
				}

				if (leaveTypes.Any(c => c.Value == "64"))
				{
					int unpaidLeave = leaveTypes.IndexOf(leaveTypes.First(c => c.Value == "64"));
					leaveTypes.RemoveAt(unpaidLeave);
				}
			}

			if (lma != null)
			{
				this.ViewData["LeaveType"] = new SelectList(leaveTypes, "Value", "Text", lma.LeaveTypeID, null);
			}
			else
			{
				this.ViewData["LeaveType"] = new SelectList(leaveTypes, "Value", "Text", null);
			}

			var selectedDate = DateTime.Now.ToFirstDayOfMonth();
			selectedDate = lma != null ? lma.EffectiveDate : selectedDate;
			this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.Date == selectedDate.Date });
		}

		#endregion LEAVE MANUAL ADJUSTMENTS
	}
}