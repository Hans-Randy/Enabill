using System;
using System.Collections.Generic;
using System.Linq;
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
	public class UserController : SearchableController
	{
		protected override string GetSearchLabelText() => "users";

		public override ActionResult Index(string q, bool? isActive)
		{
			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator) && !this.CurrentUser.HasRole(UserRoleType.Accountant) && !this.CurrentUser.HasRole(UserRoleType.ProjectOwner))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			this.ViewBag.q = InputHistory.Get(q, HistoryItemType.UserSearchCriteria);
			this.ViewBag.User_b = InputHistory.Get(isActive, HistoryItemType.UserFilterBy, true);

			SetViewData(this.ViewBag.User_b);

			List<User> model = Enabill.Models.User.FilterByName(this.CurrentUser, this.ViewBag.q, this.ViewBag.User_b);

			SaveAllInput(HistoryItemType.UserSearchCriteria, this.ViewBag.q, HistoryItemType.UserFilterBy, this.ViewBag.User_b);

			return this.View(model.OrderBy(u => u.FullName).ToList());
		}

		[HttpPost]
		public ActionResult RefreshList(string q, bool isActive)
		{
			this.SetViewData(isActive);

			this.ViewBag.q = InputHistory.Get(q, HistoryItemType.UserSearchCriteria);
			this.ViewBag.User_b = InputHistory.Get(isActive, HistoryItemType.UserFilterBy, true);
			List<User> model = Enabill.Models.User.FilterByName(this.CurrentUser, this.ViewBag.q, this.ViewBag.User_b);

			SaveAllInput(HistoryItemType.UserSearchCriteria, this.ViewBag.q, HistoryItemType.UserFilterBy, this.ViewBag.User_b);

			return this.View("Index", model.OrderBy(u => u.UserName).ToList());
		}

		#region CRUD

		[HttpGet]
		public ActionResult Create()
		{
			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			var user = Enabill.Models.User.GetNew();
			Settings.Current.ContextUser = user;
			this.SetViewDataLists(user);

			if (this.Request.IsAjaxRequest())
				return this.PartialView("ucUserDetail", user);

			return this.View(user);
		}

		[HttpPost]
		public ActionResult Create(FormCollection form)
		{
			var user = Enabill.Models.User.GetNew();
			bool isNew = true;

			try
			{
				this.TryUpdateModel(user);

				if (user.UserID > 0)
					isNew = false;

				user.FirstName = user.FirstName.Trim();
				user.LastName = user.LastName.Trim();
				user.Email = user.Email.ToLower().Trim();

				// Check if and Active user with the same Email already exists

				var user2 = UserRepo.GetActiveByEmail(user.Email);

				if (user2 != null)
					throw new UserManagementException("An active user with this Email address already exists.");

				// Leave
				if (!double.TryParse(form["AnnualLeaveTakeOn"], out double LeaveTakeOnBalance))
					throw new UserManagementException("A valid leave take-on amount must be set for a new user. Save cancelled.");

				if (!double.TryParse(form["FlexiBalanceTakeOn"], out double FlexiTimeTakeOnBalance))
					throw new UserManagementException("A valid FlexiTime take-on amount must be set for a new user. Save cancelled.");

				if (int.TryParse(form["ManagerID"], out int ManagerID))
					user.AssignManagerToUser(this.CurrentUser, ManagerID);
				else
					user.AssignManagerToUser(this.CurrentUser, null);

				user.Save(this.CurrentUser);
				Settings.Current.ContextUser = user;

				user.CreateInitialLeaveCycles(user);
				user.RecalculateLeaveAndFlexiBalances(this.CurrentUser, user, 100);

				// Assign the Time Capturer role by default
				user.AssignRole(this.CurrentUser, UserRoleType.TimeCapturing);

				// Assign default activities
				user.AssignDefaultActivities(this.CurrentUser, this.ContextUser);

				if (isNew)
					user.SetForgottenPasswordToken();

				EnabillEmails.NewAccountEmail(user);

				return this.Json(new
				{
					IsError = false,
					Description = string.Format($"User account for {user.FullName} was saved successfully."),
					Url = "/User/Edit/" + user.UserID
				});
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

			UserEditModel model = null;
			var user = UserRepo.GetByID(id);

			if (user == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoUser_Message));

			model = new UserEditModel(user, null);
			Settings.Current.ContextUser = user;
			this.SetViewDataLists(user);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult Edit(FormCollection form)
		{
			var user = UserRepo.GetByID(this.ContextUserID);

			try
			{
				// Employee End Date
				bool isValidEmployEndDate = DateTime.TryParse(form["EmployEndDate"], out var Temp) == true;

				if (isValidEmployEndDate)
				{
					if (!user.Deactivate(this.CurrentUser))
					{
						user.EmployEndDate = null;
					}
					else
					{
						user.EmployEndDate = form["EmployEndDate"].ToDate();
					}
				}
				else
				{
					user.EmployEndDate = null;
				}

				this.UpdateModel(user);

				if (int.TryParse(form["ManagerID"], out int ManagerID))
					user.AssignManagerToUser(this.CurrentUser, ManagerID);
				else
					user.AssignManagerToUser(this.CurrentUser, null);

				user.FirstName = user.FirstName.Trim();
				user.LastName = user.LastName.Trim();
				user.Email = user.Email.ToLower().Trim();
				user.CanLogin = user.IsActive;

				user.Save(this.CurrentUser);

				if (this.CurrentUserID == user.UserID)
				{
					Settings.Current.CurrentUser = user;
					Settings.Current.ResetCache();
				}

				//user.RecalculateLeaveAndFlexiBalances(this.CurrentUser, user, 100);

				// Update the start date of the user allocations assigned to user. Only isActive Activities and where there are no End dates.
				user.UpdateUserAllocationsStartDate(this.CurrentUser.FullName, user.UserID, user.EmployStartDate);

				this.SetViewDataLists(user);

				return this.PartialView("ucUserDetail", user);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult GetAllActivities(int? id)
		{
			var user = UserRepo.GetByID((int)id);

			return this.PartialView("ucActivities", new UserAllocationModel(user, user.UserID));
		}

		[HttpPost]
		public ActionResult Activate(int id)
		{
			try
			{
				var user = UserRepo.GetByID(id);

				if (user == null)
					throw new EnabillConsumerException(Resources.ERR_NoUser_Message);

				user.Activate(this.CurrentUser);
				this.SetViewDataLists(user);

				return this.PartialView("ucUserDetail", user);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion CRUD

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

		#region ACTIVITY ALLOCATIONS

		[HttpPost]
		public ActionResult AssignActivitiesToUser(FormCollection form)
		{
			try
			{
				var user = this.ContextUser;

				if (!double.TryParse(form["HourRate"], out double chargeRate))
					throw new EnabillConsumerException("An error was detected with the hour rate. Please revise the amount and then try again.");

				if (!DateTime.TryParse(form["StartDate"], out var startDate))
					throw new EnabillConsumerException("An error was detected with the start date. Please revise the start date then try again.");

				var endDate = form["EndDate"].ToDate();
				bool isConfirmed = IsCheckBoxChecked(form["IsConfirmed"]);

				using (var ts = new TransactionScope())
				{
					foreach (int activityID in form["activityIDs"].ToIntArray())
					{
						var activity = ActivityRepo.GetByID(activityID);

						var project = new Project();
						project.AssignUserToActivity(this.CurrentUser, user, activity, chargeRate, startDate, endDate, isConfirmed);
					}

					ts.Complete();
				}

				return this.PartialView("ucActivities", new UserAllocationModel(user, user.UserID));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult GetSetConfirmedEndDateOnUserAllocationView(int userAllocationID)
		{
			try
			{
				var userAllocation = UserAllocation.GetByID(this.CurrentUser, userAllocationID);
				var date = userAllocation.ScheduledEndDate ?? DateTime.Today;

				return this.PartialView("ucUserAllocationEndDate", date);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult SetConfirmedEndDateOnUserAllocation(int id, string date)
		{
			try
			{
				var userAllocation = UserAllocation.GetByID(this.CurrentUser, id);
				var user = userAllocation.GetUser();
				var newDate = date.ToDate();

				if (newDate == null)
					throw new EnabillDomainException("The new date received was not correctly formatted.");

				userAllocation.SetConfirmedEndDateOnUserAllocationForActivity(this.CurrentUser, newDate.Value);

				return this.PartialView("ucActivities", new UserAllocationModel(user, user.UserID));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult UserAllocations(int? id)
		{
			try
			{
				var user = this.ContextUser;

				if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
					return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

				UserAllocation model = null;

				if (id.HasValue)
					model = UserRepo.GetUserAllocationByID(id.Value);

				this.SetViewData(model);

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

				//2013-03-13 EndDate of UserAllocation cannot be prior to the last date time was captured
				var workallocations = userAllocation.GetUser().GetLastWorkAllocationDateForUserForActivity(userAllocation.ActivityID).ToList();

				if (workallocations.Count > 0)
				{
					var dateTimeLastCaptured = workallocations.Max(wa => wa.DayWorked);

					if ((userAllocation.ConfirmedEndDate != null && dateTimeLastCaptured > userAllocation.ConfirmedEndDate) || (userAllocation.ScheduledEndDate != null && dateTimeLastCaptured > userAllocation.ScheduledEndDate))
						throw new EnabillDomainException("UserAllocation EndDate cannot be prior to the date user last captured time against this activity(" + dateTimeLastCaptured.ToExceptionDisplayString() + "). Please reallocate time to another activity before attempting to end this one.");
				}

				UserRepo.SaveUserAllocation(userAllocation);

				var model = new UserAllocationModel(user, user.UserID);

				return this.PartialView("ucActivities", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult DeleteUserAllocation(int id)
		{
			try
			{
				var user = this.ContextUser;

				if (user == null)
					throw new UserManagementException(Resources.ERR_NoUser_Message);

				var userAllocation = UserRepo.GetUserAllocationByID(id);

				if (userAllocation != null)
					user.DeleteUserAllocation(userAllocation);

				var model = new UserAllocationModel(user, user.UserID);

				return this.PartialView("ucActivities", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "Unknown error occurred.");
			}
		}

		#endregion ACTIVITY ALLOCATIONS

		#region ROLE ALLOCATIONS

		[HttpPost]
		public ActionResult AssignRoles(int id, string roleIDs)
		{
			try
			{
				var user = UserRepo.GetByID(id);
				Settings.Current.ContextUser = user ?? throw new EnabillConsumerException(Resources.ERR_NoUser_Message);

				if (!string.IsNullOrEmpty(roleIDs))
				{
					foreach (int rid in roleIDs.ToIntArray())
					{
						user.AssignRole(this.CurrentUser, (UserRoleType)rid);

						//If the user is assigned as a Time Capturer, then add default activities
						if (((UserRoleType)rid).GetEnumDescriptionById() == "Time Capturer")
						{
							user.AssignDefaultActivities(this.CurrentUser, this.ContextUser);
						}
					}
				}

				return this.PartialView("ucRoles", new UserRoleModel(user));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult RemoveRoles(int id, string roleIDs)
		{
			try
			{
				var user = UserRepo.GetByID(id);

				Settings.Current.ContextUser = user ?? throw new EnabillConsumerException(Resources.ERR_NoUser_Message);

				if (roleIDs != null)
				{
					foreach (int rid in roleIDs.ToIntArray())
					{
						user.RemoveRole(this.CurrentUser, (UserRoleType)rid);

						//If the user is removed as a Time Capturer, then remove default activities
						if (((UserRoleType)rid).GetEnumDescriptionById() == "Time Capturer")
						{
							user.RemoveDefaultActivities(user);
						}
					}
				}

				if (user.UserID == this.CurrentUserID)
					Settings.Current.CurrentUser = UserRepo.GetByID(this.CurrentUserID);

				return this.PartialView("ucRoles", new UserRoleModel(user));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion ROLE ALLOCATIONS

		#region REPORT ALLOCATIONS

		[HttpPost]
		public ActionResult AssignReportsToUser(string selectedReports)
		{
			string[] reports = selectedReports.Split(',');

			try
			{
				var user = this.ContextUser;

				//Delete existing entries to prevent duplicate entries
				foreach (var existingEmail in ReportEmailRepo.GetUserReportList(user.UserID).ToList())
				{
					ReportEmailRepo.Delete(existingEmail);
				}

				if (selectedReports != "")
				{
					foreach (string report in reports)
					{
						string[] IDs = report.Split('|');
						int? reportID = IDs[0].ToInt();
						int? frequencyID = IDs[1].ToInt();

						var reportEmail = new ReportEmail
						{
							UserID = user.UserID,
							ReportID = reportID.Value,
							FrequencyID = frequencyID.Value
						};
						ReportEmailRepo.Save(reportEmail);
					}
				}

				return this.PartialView("ucReports", new UserReportEmailModel(user));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion REPORT ALLOCATIONS

		#region LEAVE MANUAL ADJUSTMENTS

		[HttpPost]
		public ActionResult LeaveManualAdjustment(int? id)
		{
			try
			{
				var user = this.ContextUser;

				if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
					return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

				LeaveManualAdjustment model = null;

				if (id.HasValue)
					model = user.GetLeaveManualAdjustmentByID(id.Value);

				this.SetViewData(model);

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

				var model = new UserLeaveManualAdjustmentModel(user);

				return this.PartialView("ucLeaveManualAdjustments", model);
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
					UserRepo.DeleteLeaveManualAdjustment(leaveManualAdjustment);

				var model = new UserLeaveManualAdjustmentModel(user);

				return this.PartialView("ucLeaveManualAdjustments", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "Unknown error occurred.");
			}
		}

		#endregion LEAVE MANUAL ADJUSTMENTS

		#region LOOK UP

		private void SetViewDataLists(User model)
		{
			this.AddAsDropdownSource(Enabill.Models.User.GetByRoleBW((int)UserRoleType.Manager), "ManagerUser", "UserID", "FullName", model.ManagerID);

			this.AddAsDropdownSource(RegionRepo.GetAll().OrderBy(r => r.RegionName).ToList(), "Region", model.RegionID);
			this.AddAsDropdownSource(BillableIndicatorRepo.GetAll().OrderBy(b => b.BillableIndicatorName).ToList(), "BillableIndicator", model.BillableIndicatorID);
			this.AddAsDropdownSource(BillingMethodRepo.GetAll().OrderBy(r => r.BillingMethodName).ToList(), "BillingMethod", model.BillableIndicatorID);
			this.AddAsDropdownSource(DivisionRepo.GetActive().OrderBy(d => d.DivisionName).ToList(), "Division", model.DivisionID);
			this.AddAsDropdownSource(UserRepo.GetAllEmploymentType().OrderBy(e => e.EmploymentTypeName).ToList(), "EmploymentType", model.EmploymentTypeID);
		}

		private void SetViewData(LeaveManualAdjustment lma)
		{
			this.ViewData["LeaveType"] = new SelectList(Enabill.Extensions.GetEnumSelectList<LeaveTypeEnum>(), "Value", "Text", null);
			var selectedDate = DateTime.Now.ToFirstDayOfMonth();
			selectedDate = lma != null ? lma.EffectiveDate : selectedDate;
			this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.Date == selectedDate.Date });
		}

		private void SetViewData(bool? isActive = true) => this.ViewData["StatusFilter"] = new List<SelectListItem>
			{
				new SelectListItem() { Value = "1", Text = "Active", Selected = isActive == true },
				new SelectListItem() { Value = "0", Text = "Inactive", Selected = isActive == false }
			};

		private void SetViewData(UserAllocation ua)
		{
			this.ViewData["Client"] = ClientRepo.GetAll().OrderBy(c => c.ClientName).Select(c => new SelectListItem { Value = c.ClientID.ToString(), Text = c.ClientName, Selected = c.ClientID == ua.GetActivity().GetProject().ClientID });
			this.ViewData["Project"] = ProjectRepo.GetAll().OrderBy(p => p.ProjectName).Select(p => new SelectListItem { Value = p.ProjectID.ToString(), Text = p.ProjectName, Selected = p.ProjectID == ua.GetActivity().ProjectID });
			this.ViewData["Activity"] = ActivityRepo.GetAll().OrderBy(a => a.ActivityName).Select(a => new SelectListItem { Value = a.ActivityID.ToString(), Text = a.ActivityName, Selected = a.ActivityID == ua.ActivityID });
		}

		public ActionResult Lookup(string term)
		{
			var list = UserRepo.AutoComplete(term, 20);

			return this.Json(list, JsonRequestBehavior.AllowGet);
		}

		#endregion LOOK UP
	}
}