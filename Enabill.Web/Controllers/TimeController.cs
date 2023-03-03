using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Enabill.Emailing;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Properties;
using Enabill.Repos;
using Enabill.Repository.Interfaces;
using Enabill.Web.Models;
using ServiceStack;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class TimeController : BaseController
	{
		private IUserRepository userRepository;

		public TimeController()
		{
		}

		public TimeController(IUserRepository userRepository)
		{
			this.userRepository = userRepository;
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
			{
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
			}

			if (!user.HasRole(UserRoleType.TimeCapturing))
			{
				if (user.UserID == this.CurrentUserID)
					return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
				else // if (user.UserID != CurrentUserID)
					return this.ErrorView(new ErrorModel(string.Format(Resources.ERR_NonTimeCapturerUser_Message, user.FullName, user.FirstName), true));
			}

			Settings.Current.ContextUser = user;
			this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.IsInCurrentMonth() });

			var model = new TimeIndexModel(user, InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value);

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
				{
					return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
				}

				if (!user.HasRole(UserRoleType.TimeCapturing))
				{
					if (user.UserID == this.CurrentUserID)
						return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
					else
						return this.ErrorView(new ErrorModel(string.Format(Resources.ERR_NonTimeCapturerUser_Message, user.FullName, user.FirstName), true));
				}

				this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key == monthYear.Value });

				var model = new TimeIndexModel(user, monthYear.Value);

				return this.View(model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#region DAY

		[HttpGet]
		public ActionResult Day(int? id, string dateString, string callingPage = "")
		{
			var user = this.ContextUser;
			DateTime date;

			var userPref = user.GetUserPreference();
			if (!userPref.DayView)
				return this.RedirectToAction("Week", new { id, dateString, userPref.ThreeState, callingPage });

			if (id.HasValue && id.Value != user.UserID)
				user = UserRepo.GetByID(id.Value);

			if (user == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoUser_Message));

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
			{
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
			}

			Settings.Current.ContextUser = user;

			if (dateString.ToDate().HasValue)
				date = dateString.ToDate().Value;
			else
				date = DateTime.Today;

			InputHistory.Set(HistoryItemType.WorkDay, date);

			var model = new TimeDayModel(user, date, callingPage);
			this.SetViewDataLists(model);

			return this.View(model);
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

		[HttpPost]
		public ActionResult SaveNextDay(DateTime dayToCopy, string previous)
		{
			try
			{
				var user = this.ContextUser;
				bool isPrevious = previous == "true";
				var date = InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value;

				UserRepo.SaveNextWorkDay(user.UserID, dayToCopy, isPrevious);

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

		private double HoursWorkedOnDay(ActivityPostModel log, DateTime date)
		{
			switch (date.DayOfWeek)
			{
				case DayOfWeek.Monday:
					return log.Monday;

				case DayOfWeek.Tuesday:
					return log.Tuesday;

				case DayOfWeek.Wednesday:
					return log.Wednesday;

				case DayOfWeek.Thursday:
					return log.Thursday;

				case DayOfWeek.Friday:
					return log.Friday;

				case DayOfWeek.Saturday:
					return log.Saturday;

				default:
					return log.Sunday;
			}
		}

		#endregion DAY

		#region WEEK

		[HttpGet]
		public ActionResult Week(int? id, string dateString, string threeState, string callingPage = "")
		{
			var user = this.ContextUser;
			DateTime date;

			if (id.HasValue && id.Value != user.UserID)
				user = UserRepo.GetByID(id.Value);

			if (user == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoUser_Message));

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			Settings.Current.ContextUser = user;

			if (dateString.ToDate().HasValue)
				date = dateString.ToDate().Value;
			else
				date = DateTime.Today;

			date = date.AddDays(-(int)date.DayOfWeek + 1); // reset date to earliest monday of the week you selected;
			InputHistory.Set(HistoryItemType.WorkDay, date);

			var userPref = user.GetUserPreference();

			var model = new TimeWeekModel(user, date, userPref.ThreeState, callingPage);

			this.SetViewDataLists(model);

			return this.View(model);
		}

		[HttpPost]
		public JsonResult Week(FormCollection formData)
		{
			bool mustHaveRemarks = formData["mustHaveRemarks"].ToBool() == true;
			int? activityId = formData["activityId"].ToInt();
			int workSessionCount = 0;
			double hrsLogged = 0;
			string remark = "";
			string threeState = formData["filter"];

			if (activityId == null)
				throw new Exception("An error has occured. No values specified for Activity and Hours logged!");

			if (formData["column"] == "Remark")
			{
				hrsLogged = formData["editedDayTime"].ToDouble() ?? 0;
				remark = formData["value"];
			}
			else
			{
				hrsLogged = formData["value"].ToDouble() ?? 0;
				remark = formData["remark"];
			}

			if (hrsLogged > 0 && mustHaveRemarks && remark.IsNullOrEmpty())
			{
				string error = "A remark is required.";

				return this.Json(error);
			}

			var date = (DateTime)formData["startDate"].ToDate(); // this will always be monday's date;
			var day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), formData["editedDay"]);
			var user = this.ContextUser;
			date = date.AddDays(formData["editedDay"] != "Sunday" ? (int)day - 1 : 6); // get the date of the day values entered in for;

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
			{
				string error = "ERROR";

				return this.Json(error);
			}

			try
			{
				var workSessions = user.GetWorkSessions(date);
				workSessionCount = workSessions.Count;

				if (workSessionCount == 0)
				{
					var userPref = user.GetUserPreference();
					double workDayHoursBefore = UserRepo.GetWorkSessionsForDate(user.UserID, date).ToList().Sum(ws => ws.TotalTime);
					double flexiBalanceBefore = user.GetFlexiBalance(date.AddMonths(1).ToFirstDayOfMonth()) == null ? 0 : user.GetFlexiBalance(date.AddMonths(1).ToFirstDayOfMonth()).FinalBalance;

					var d = userPref.DefaultWorkSessionStartTime;
					var startTime = new DateTime(date.Year, date.Month, date.Day, d.Value.Hour, d.Value.Minute, d.Value.Second);
					d = userPref.DefaultWorkSessionEndTime;
					double lunchTime = userPref.DefaultLunchDuration;
					var endTime = startTime.AddHours(hrsLogged + lunchTime);

					user.CaptureWorkSession(this.CurrentUser, startTime, endTime, lunchTime);
					user.CreateBalanceAuditTrail("Worksession", date, user, workDayHoursBefore, flexiBalanceBefore, this.CurrentUser, "added");

					var workSessionList = user.GetWorkSessions(date);
					var userWorkAllocationExceptionList = new List<WorkAllocationExceptionModel>();

					foreach (var ws in workSessionList)
					{
						ws.WorkSessionStatusID = (int)WorkSessionStatusType.UnApproved;
						user.UpdateWorkSession(this.CurrentUser, ws);
					}
				}

				string ticketReference = null; //form["Ticket" + ID];  must set this as well; ????

				var wa = new WorkAllocation
				{
					UserID = user.UserID,
					ActivityID = (int)activityId,
					DayWorked = date,
					Remark = remark,
					HoursWorked = hrsLogged,
					WorkAllocationType = (int)WorkAllocationType.UserCreated
				};

				var waExistOnDate = user.GetWorkAllocationsForActivityForDate((int)activityId, date).SingleOrDefault();

				if (waExistOnDate != null)
					user.DeleteWorkAllocation(user, waExistOnDate);

				string waPreviousTicketReference = wa.TicketReference;
				wa.TicketReference = ticketReference;

				if (wa.GetActivity().ActivityName != "Training" && hrsLogged > 0)
				{
					//Only save non training allocations here as the AddTrainingDetails will take care of saving the training allocations
					user.SaveWorkAllocation(this.CurrentUser, wa);

					//Update the ticket, previous wa reference, timespent with the allocatedHours
					if (!string.IsNullOrEmpty(waPreviousTicketReference))
					{
						var previousTicket = TicketRepo.GetByReference(waPreviousTicketReference);
						previousTicket.TimeSpent = WorkAllocationRepo.GetAllocatedHoursByTicketReference(waPreviousTicketReference);

						previousTicket.Save();
					}
				}

				if (workSessionCount > 0) // For the week view, need to dynamically change the work session time span
				{
					this.AutoSizeWorkSession(workSessionCount, workSessions, user, date);
				}
			}
			catch (Exception e)
			{
			}

			string res = "OK";

			return this.Json(res);
		}

		[HttpPost]
		public JsonResult WeekPaste(WeekPasteDataModel model)
		{
			var user = this.ContextUser;

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
				throw new Exception("An error has occured. No manage rights!");

			var startDate = model.StartDate;

			if (startDate == null)
				throw new Exception("An error has occured. No values specified for Start Date!");

			try
			{
				int workSessionCount = 0;
				var userPref = user.GetUserPreference();
				var defaultStartTime = userPref.DefaultWorkSessionStartTime;
				var defaultEndTime = userPref.DefaultWorkSessionEndTime;
				double lunchTime = userPref.DefaultLunchDuration;

				for (int i = 0; i < 7; i++)
				{
					var date = startDate.AddDays(i);
					var workSessions = user.GetWorkSessions(date);

					foreach (var log in model.Logs)
					{
						if (log.ActivityId == 0 || log.ActivityId != 0 && this.HoursWorkedOnDay(log, date) == 0)
							continue; // groupings have ActivityId = 0; This can be skipped;

						if (workSessions.Count == 0)
						{
							double workDayHoursBefore = UserRepo.GetWorkSessionsForDate(user.UserID, date).ToList().Sum(ws => ws.TotalTime);
							double flexiBalanceBefore = user.GetFlexiBalance(date.AddMonths(1).ToFirstDayOfMonth()) == null ? 0 : user.GetFlexiBalance(date.AddMonths(1).ToFirstDayOfMonth()).FinalBalance;
							var startTime = new DateTime(date.Year, date.Month, date.Day, defaultStartTime.Value.Hour, defaultStartTime.Value.Minute, defaultStartTime.Value.Second);
							var endTime = new DateTime(date.Year, date.Month, date.Day, defaultEndTime.Value.Hour, defaultEndTime.Value.Minute, defaultEndTime.Value.Second);

							user.CaptureWorkSession(this.CurrentUser, startTime, endTime, lunchTime);
							user.CreateBalanceAuditTrail("Worksession", date, user, workDayHoursBefore, flexiBalanceBefore, this.CurrentUser, "added");

							var workSessionList = user.GetWorkSessions(date);
							var userWorkAllocationExceptionList = new List<WorkAllocationExceptionModel>();

							foreach (var ws in workSessionList)
							{
								ws.WorkSessionStatusID = (int)WorkSessionStatusType.UnApproved;
								user.UpdateWorkSession(this.CurrentUser, ws);
							}

							workSessions = user.GetWorkSessions(date);
						}

						double hoursWorkedOnDay = this.HoursWorkedOnDay(log, date);

						if (hoursWorkedOnDay == 0)
							continue;

						var waExistOnDate = user.GetWorkAllocationsForActivityForDate(log.ActivityId, date).SingleOrDefault();
						string remarkDay = this.GetRemarkDay(date.DayOfWeek, log);

						if (waExistOnDate != null)
						{
							if (waExistOnDate.HoursWorked == hoursWorkedOnDay && waExistOnDate.Remark == remarkDay)
								continue;

							user.DeleteWorkAllocation(user, waExistOnDate);
						}

						var wa = new WorkAllocation
						{
							UserID = user.UserID,
							ActivityID = log.ActivityId,
							DayWorked = date,
							Remark = remarkDay,
							HoursWorked = hoursWorkedOnDay,
							WorkAllocationType = (int)WorkAllocationType.UserCreated
						};

						string ticketReference = null;
						string waPreviousTicketReference = wa.TicketReference;

						wa.TicketReference = ticketReference;

						var activity = wa.GetActivity();

						if (activity.ActivityName == "Training")
							continue;

						user.SaveWorkAllocation(this.CurrentUser, wa);

						workSessionCount = workSessions.Count;

						if (workSessionCount > 0) // For the week view, need to dynamically change the work session time span
						{
							this.AutoSizeWorkSession(workSessionCount, workSessions, user, date);
						}

						if (string.IsNullOrEmpty(waPreviousTicketReference))
							continue;

						var previousTicket = TicketRepo.GetByReference(waPreviousTicketReference);
						previousTicket.TimeSpent = WorkAllocationRepo.GetAllocatedHoursByTicketReference(waPreviousTicketReference);

						previousTicket.Save();
					}
				}
			}
			catch (Exception e)
			{
				throw new Exception(e.InnerException.Message);
			}

			return this.Json("OK");
		}

		public ActionResult WeekCheckboxUpdate(int[] activityIDs, bool isHidden)
		{
			var user = this.ContextUser;

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
			{
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
			}

			try
			{
				foreach (int activityID in activityIDs)
				{
					var userAllocation = UserRepo.GetUserAllocationByUserAndActivityID(activityID, this.CurrentUserID);

					userAllocation.IsHidden = isHidden;

					UserRepo.SaveUserAllocation(userAllocation);
				}
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}

			return this.Json("OK");
		}

		protected string GetRemarkDay(DayOfWeek dayOfWeek, ActivityPostModel log)
		{
			switch (dayOfWeek)
			{
				case DayOfWeek.Monday:
					return log.RemarkMonday;

				case DayOfWeek.Tuesday:
					return log.RemarkTuesday;

				case DayOfWeek.Wednesday:
					return log.RemarkWednesday;

				case DayOfWeek.Thursday:
					return log.RemarkThursday;

				case DayOfWeek.Friday:
					return log.RemarkFriday;

				case DayOfWeek.Saturday:
					return log.RemarkSaturday;

				case DayOfWeek.Sunday:
					return log.RemarkSunday;

				default:
					return "";
			}
		}

		protected void AutoSizeWorkSession(int workSessionCount, List<WorkSession> workSessions, User user, DateTime date)
		{
			// Work Session and Work Allocations
			var waAllOnDate = user.GetWorkAllocations(date);
			double waTotal = 0;
			double wsTotal = 0;

			// Work Allocations
			if (waAllOnDate.Count > 0)
				waTotal = waAllOnDate.Sum(h => h.HoursWorked);

			// Work Sessions
			foreach (var ws in workSessions)
			{
				if (waTotal == 0)
				{
					// Delete work session
					user.DeleteWorkSession(user, ws.WorkSessionID);
					workSessionCount -= 1;
				}
				else
				{
					// Number of hours in work session
					wsTotal += ws.EndTime.Subtract(ws.StartTime).TotalHours - ws.LunchTime;
				}
			}

			if (waTotal > 0 && workSessionCount > 0)
			{
				workSessions.Sort((x, y) => -1 * x.EndTime.CompareTo(y.EndTime)); // -1 used to sort descending. The last work session time should be extended, otherwise extendng a previous work session end time may overlap with the following one
				double wsLunchTotal = 0;

				foreach (var ws in workSessions)
				{
					wsLunchTotal += ws.LunchTime;

					if (workSessionCount > 1)
					{
						// Delete work session
						user.DeleteWorkSession(user, ws.WorkSessionID);
					}
					else
					{
						// Change combined lunch time and end time for last remaining work session
						ws.LunchTime = wsLunchTotal;
						ws.WorkSessionStatusID = (int)WorkSessionStatusType.UnApproved;
						ws.EndTime = ws.StartTime.AddHours(waTotal + wsLunchTotal);
						user.UpdateWorkSession(this.CurrentUser, ws);
						break;
					}

					workSessionCount -= 1;
				}
			}
		}

		[HttpPost]
		public ActionResult SaveNextWeek(DateTime currentDay, string previous)
		{
			try
			{
				bool isPrevious = previous == "true";
				var mondayOfCurrentWeek = currentDay.AddDays(-(int)currentDay.DayOfWeek + (int)DayOfWeek.Monday);
				var date = InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value;
				var user = this.ContextUser;

				UserRepo.SaveNextWeek(user.UserID, this.CurrentUser.FullName, mondayOfCurrentWeek, isPrevious);

				date = date.AddDays(-(int)date.DayOfWeek + 1); // reset date to earliest monday of the week you selected;
				InputHistory.Set(HistoryItemType.WorkDay, date);

				var model = new TimeWeekModel(user, date, "");
				this.SetViewDataLists(model);

				return this.Json(new
				{
					IsError = false,
					redirectTo = this.Url.Action("Week", "Time", new { user.UserID, date })
				});
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion WEEK

		#region MONTH

		[HttpPost]
		public ActionResult Month(string date)
		{
			try
			{
				var user = this.ContextUser;

				var dt = DateTime.Parse(date);
				dt = user.ConfigureDate(dt).ToFirstDayOfMonth();

				if (dt.IsInCurrentMonth())
					dt = DateTime.Today;

				InputHistory.Set(HistoryItemType.WorkDay, dt);

				var model = new TimeIndexModel(user, dt);

				return this.PartialView("ucIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion MONTH

		#region WORK SESSION

		[HttpPost]
		public ActionResult AddWS(FormCollection form)
		{
			try
			{
				var user = this.ContextUser;
				var workDate = InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value;
				double workDayHoursBefore = UserRepo.GetWorkSessionsForDate(user.UserID, workDate).ToList().Sum(ws => ws.TotalTime);
				double flexiBalanceBefore = user.GetFlexiBalance(workDate.AddMonths(1).ToFirstDayOfMonth()) == null ? 0 : user.GetFlexiBalance(workDate.AddMonths(1).ToFirstDayOfMonth()).FinalBalance;

				var startTime = form["StartTime"].ToDate();
				var endTime = form["EndTime"].ToDate();
				double LunchTime = form["LunchTime"].Replace(",", "").Replace(" ", "").ToDouble() ?? 0;

				user.CaptureWorkSession(this.CurrentUser, startTime.Value, endTime.Value, LunchTime);
				//Create Audit trail entry if past workallocation changed
				user.CreateBalanceAuditTrail("Worksession", workDate, user, workDayHoursBefore, flexiBalanceBefore, this.CurrentUser, "added");

				//Update the WorkSessionStatus to UnApproved if no exceptions else to exceptions if exceptions
				var workSessionList = user.GetWorkSessions(workDate);
				var userWorkAllocationExceptionList = new List<WorkAllocationExceptionModel>();

				foreach (var ws in workSessionList)
				{
					ws.WorkSessionStatusID = !user.IsTimeCaptured(workDate, workDate, this.CurrentUser, out userWorkAllocationExceptionList) ? 3 : 1;
					user.UpdateWorkSession(this.CurrentUser, ws);
				}

				var model = new TimeDayModel(user, workDate);
				this.SetViewDataLists(model);

				return this.PartialView("ucDay", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "An unknown error occurred. Please resubmit your work session.");
			}
		}

		[HttpPost]
		public ActionResult ApproveWS(FormCollection form)
		{
			try
			{
				var workDate = InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value;

				var DateFrom = form["DateFrom"].ToDate();
				var DateTo = form["DateTo"].ToDate();
				int? userID = form["UserList"].ToInt();
				var user = UserRepo.GetByID(userID.Value);
				int? invoiceID = form["InvoiceID"].ToInt();
				string callingPage = form["CallingPage"];

				foreach (var ws in UserRepo.GetWorkSessionsForDateSpan(userID.Value, DateFrom.Value, DateTo.Value.AddDays(1)).ToList())
				{
					if (ws.WorkSessionStatusID == 1)
					{
						//only unapproved can be approved, ensure that we exclude exceptions(3) that fall within the same date range
						user.ApproveWorkSession(ws, this.CurrentUser);
					}
				}

				this.userRepository.InsertUserNonWorkSessions(DateFrom.Value, DateTo.Value, userID.Value);

				var userList = UserRepo.GetAll().ToList();
				var items = new List<SelectListItem>();

				foreach (var timeUser in userList)
				{
					items.Add(new SelectListItem() { Value = timeUser.UserID.ToString(), Text = timeUser.FullName, Selected = timeUser.UserID == user.UserID });
				}

				this.ViewData["UserList"] = items;

				var model = new TimeApproveIndexModel(user, DateFrom.Value, DateTo.Value, callingPage, invoiceID.Value);

				return this.PartialView("ucApproveIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "An unknown error occurred. Please resubmit your work session.");
			}
		}

		[HttpPost]
		public ActionResult UnApproveWS(FormCollection form)
		{
			try
			{
				var workDate = InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value;

				var DateFrom = form["DateFrom"].ToDate();
				var DateTo = form["DateTo"].ToDate();
				int? userID = form["UserList"].ToInt();
				var user = UserRepo.GetByID(userID.Value);
				int? invoiceID = form["InvoiceID"].ToInt();
				string callingPage = form["CallingPage"];

				foreach (var ws in UserRepo.GetWorkSessionsForDateSpan(userID.Value, DateFrom.Value, DateTo.Value.AddDays(1)).ToList())
				{
					if (ws.WorkSessionStatusID == 2)
					{
						//only update approved to unapproved. exclude exceptions that fall within the same date range
						user.UnApproveWorkSession(ws, this.CurrentUser);
					}
				}

				foreach (var nws in UserRepo.GetNonWorkSessionsForDateSpan(userID.Value, DateFrom.Value, DateTo.Value.AddDays(1)).ToList())
				{
					UserRepo.DeleteNonWorkSession(nws);
				}

				var userList = UserRepo.GetAll().ToList();
				var items = new List<SelectListItem>();

				foreach (var timeUser in userList)
				{
					items.Add(new SelectListItem() { Value = timeUser.UserID.ToString(), Text = timeUser.FullName, Selected = timeUser.UserID == user.UserID });
				}

				this.ViewData["UserList"] = items;

				var model = new TimeApproveIndexModel(user, DateFrom.Value, DateTo.Value, callingPage, invoiceID.Value);

				return this.PartialView("ucApproveIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "An unknown error occurred. Please resubmit your work session.");
			}
		}

		[HttpPost]
		public ActionResult DelWS(int wsID)
		{
			try
			{
				var user = this.ContextUser;

				if (user == null)
					throw new UserManagementException(Resources.ERR_NoUser_Message);

				var workSession = user.GetWorkSession(wsID);
				double workDayHoursBefore = UserRepo.GetWorkSessionsForDate(user.UserID, workSession.StartTime).ToList().Sum(ws => ws.TotalTime);
				double flexiBalanceBefore = user.GetFlexiBalance(workSession.StartTime.AddMonths(1).ToFirstDayOfMonth()) == null ? 0 : user.GetFlexiBalance(workSession.StartTime.AddMonths(1).ToFirstDayOfMonth()).FinalBalance;

				if (workSession == null)
					throw new UserWorkSessionException(Resources.ERR_NoWorkSession_Message);

				var workDate = workSession.WorkDate;
				user.DeleteWorkSession(this.CurrentUser, workSession);

				//Create Audit trail entry if past workallocation changed
				user.CreateBalanceAuditTrail("Worksession", workDate, user, workDayHoursBefore, flexiBalanceBefore, this.CurrentUser, "deleted");

				//Update the WorkSessionStatus to UnApproved if no exceptions else to exceptions if exceptions
				var workSessionList = user.GetWorkSessions(workDate);
				var userWorkAllocationExceptionList = new List<WorkAllocationExceptionModel>();

				foreach (var ws in workSessionList)
				{
					ws.WorkSessionStatusID = !user.IsTimeCaptured(workDate, workDate, this.CurrentUser, out userWorkAllocationExceptionList) ? 3 : 1;
					user.UpdateWorkSession(this.CurrentUser, ws);
				}

				var model = new TimeDayModel(user, workDate);
				this.SetViewDataLists(model);

				return this.PartialView("ucDay", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "Unknown error occurred. Please retry removing the work session.");
			}
		}

		[HttpPost]
		public ActionResult GetEditWorkSession(int workSessionID)
		{
			try
			{
				var user = this.ContextUser;
				var workSession = user.GetWorkSession(workSessionID);

				return this.PartialView("ucWorkSessionEdit", workSession);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult UpdateWorkSession(int workSessionID, DateTime startDate, DateTime endDate, string lunch)
		{
			try
			{
				var user = this.ContextUser;
				var workSession = user.GetWorkSession(workSessionID);
				double workDayHoursBefore = UserRepo.GetWorkSessionsForDate(user.UserID, workSession.StartTime).ToList().Sum(ws => ws.TotalTime);
				double flexiBalanceBefore = user.GetFlexiBalance(workSession.StartTime.AddMonths(1).ToFirstDayOfMonth()) == null ? 0 : user.GetFlexiBalance(workSession.StartTime.AddMonths(1).ToFirstDayOfMonth()).FinalBalance;

				double.TryParse(lunch, out double lunchDouble);
				var userWorkAllocationExceptionList = new List<WorkAllocationExceptionModel>();

				workSession.StartTime = startDate;
				workSession.EndTime = endDate;
				workSession.LunchTime = lunchDouble;

				workSession.WorkSessionStatusID = !user.IsTimeCaptured(startDate, startDate, this.CurrentUser, out userWorkAllocationExceptionList) ? 3 : 1;
				user.UpdateWorkSession(this.CurrentUser, workSession);

				//Create Audit trail entry if past workallocation changed
				user.CreateBalanceAuditTrail("Worksession", workSession.StartTime, user, workDayHoursBefore, flexiBalanceBefore, this.CurrentUser, "updated");

				var model = new TimeDayModel(user, workSession.WorkDate);
				this.SetViewDataLists(model);

				return this.PartialView("ucDay", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion WORK SESSION

		#region WORK ALLOCATIONS

		[HttpPost]
		public ActionResult SaveDayAllocation(FormCollection form)
		{
			var user = this.ContextUser;
			var workDay = InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value;

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
			{
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
			}

			try
			{
				foreach (var wAllocation in new TimeDayModel(user, workDay).WorkAllocations)
				{
					string ID = wAllocation.WorkAllocation.WorkAllocationID.ToString() + "__" + wAllocation.Activity.ActivityID;

					if (!double.TryParse(form["ActHours" + ID], out double hoursWorked))
						continue;

					string remark = form["ActRemark" + ID];

					if (string.IsNullOrEmpty(remark))
						remark = null;

					string ticketReference = form["Ticket" + ID];

					if (string.IsNullOrEmpty(ticketReference))
						ticketReference = null;

					WorkAllocation wa;

					if (wAllocation.WorkAllocation.WorkAllocationID != 0)
						wa = user.GetWorkAllocation(wAllocation.WorkAllocation.WorkAllocationID);
					else
						wa = user.GetWorkAllocationsForActivityForDate(wAllocation.Activity.ActivityID, workDay).SingleOrDefault();

					if (wa == null && hoursWorked == 0)
						continue;

					if (wa != null && hoursWorked == 0)
					{
						user.DeleteWorkAllocation(this.CurrentUser, wa);
						continue;
					}

					if (wa == null)
						wa = new WorkAllocation();

					wa.UserID = user.UserID;
					wa.ActivityID = wAllocation.Activity.ActivityID;
					wa.DayWorked = workDay;
					wa.Remark = remark;
					wa.HoursWorked = hoursWorked;
					wa.WorkAllocationType = (int)WorkAllocationType.UserCreated;

					string waPreviousTicketReference = wAllocation.WorkAllocation.TicketReference;

					wa.TicketReference = ticketReference;

					if (wa.GetActivity().ActivityName != "Training")
					{
						//Only save non training allocations here as the AddTrainingDetails will take care of saving the training allocations
						user.SaveWorkAllocation(this.CurrentUser, wa);

						//Update the ticket, current wa reference, timespent with the allocatedHours
						if (!string.IsNullOrEmpty(ticketReference))
						{
							var ticket = TicketRepo.GetByReference(ticketReference);
							ticket.TimeSpent = WorkAllocationRepo.GetAllocatedHoursByTicketReference(ticketReference);
							ticket.Save();
						}

						//Update the ticket, previous wa reference, timespent with the allocatedHours
						if (!string.IsNullOrEmpty(waPreviousTicketReference))
						{
							var previousTicket = TicketRepo.GetByReference(waPreviousTicketReference);
							previousTicket.TimeSpent = WorkAllocationRepo.GetAllocatedHoursByTicketReference(waPreviousTicketReference);
							previousTicket.Save();
						}
					}
				}

				//Update the WorkSessionStatus to UnApproved if no exceptions else to exceptions if exceptions
				var workSessionList = user.GetWorkSessions(workDay);
				var userWorkAllocationExceptionList = new List<WorkAllocationExceptionModel>();

				foreach (var ws in workSessionList)
				{
					ws.WorkSessionStatusID = !user.IsTimeCaptured(workDay, workDay, this.CurrentUser, out userWorkAllocationExceptionList) ? 3 : 1;
					user.UpdateWorkSession(this.CurrentUser, ws);
				}

				string approval = form["ApprovalInd"];

				var model = new TimeDayModel(user, workDay, approval);
				this.SetViewDataLists(model);

				return this.PartialView("ucDay", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult AllocateExtraTime(int activityID)
		{
			try
			{
				var activity = Activity.GetByID(activityID);
				var workDay = InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value;
				var user = this.ContextUser;
				var model = new TimeDayModel(user, workDay);

				var wa = WorkAllocation.GetNew(this.CurrentUser, user, activity, workDay);
				var workAllocationExtendedModel = WorkAllocation.GetExtendedDetail(wa) ?? WorkAllocation.CreateNewExtendedModel(user, activity, workDay);
				this.SetViewDataTrainingCategories(workAllocationExtendedModel);

				return this.PartialView("ucAllocateExtraTime", wa);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult AddExtraAllocation(FormCollection form)
		{
			var user = this.ContextUser;

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
			{
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
			}

			try
			{
				var workDay = InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value;
				var wa = WorkAllocation.GetNew(this.CurrentUser, user, int.Parse(form["ActivityID"]), workDay);
				string ID = wa.WorkAllocationID.ToString() + "__" + wa.ActivityID;

				wa.HoursWorked = double.Parse(form["HoursWorked"]);
				wa.Remark = form["Remark"];
				wa.TicketReference = form["Ticket" + ID];
				wa.TrainingCategoryID = form["TrainingCategoryList" + ID].ToInt() ?? 0;
				wa.TrainerName = string.IsNullOrEmpty(form["TrainerName" + ID]) ? null : form["TrainerName" + ID];
				wa.TrainingInstitute = string.IsNullOrEmpty(form["TrainingInstitute" + ID]) ? null : form["TrainingInstitute" + ID];
				user.SaveWorkAllocation(this.CurrentUser, wa);

				//Update the WorkSessionStatus to UnApproved if no exceptions else to exceptions if exceptions
				var workSessionList = user.GetWorkSessions(workDay);
				var userWorkAllocationExceptionList = new List<WorkAllocationExceptionModel>();

				foreach (var ws in workSessionList)
				{
					ws.WorkSessionStatusID = !user.IsTimeCaptured(workDay, workDay, this.CurrentUser, out userWorkAllocationExceptionList) ? 3 : 1;
					user.UpdateWorkSession(this.CurrentUser, ws);
				}

				var model = new TimeDayModel(user, workDay);
				this.SetViewDataLists(model);

				return this.PartialView("ucDay", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult TrainingDetails(int workAllocationID, int activityID, double hoursWorked, string remark)
		{
			try
			{
				var user = this.ContextUser;

				if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
				{
					return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
				}

				var workDay = InputHistory.GetDateTime(HistoryItemType.WorkDay, null).Value;
				WorkAllocation wa = null;
				WorkAllocationExtendedModel wae = null;

				if (workAllocationID == 0)
				{
					wa = WorkAllocation.GetNew(this.CurrentUser, user, activityID, workDay);
					wae = WorkAllocation.CreateNewExtendedModel(this.ContextUser, ActivityRepo.GetByID(activityID), workDay);
				}
				else
				{
					wa = WorkAllocation.GetByID(workAllocationID);
					wae = WorkAllocation.GetExtendedDetail(wa);
				}

				wae.WorkAllocation.HoursWorked = hoursWorked;

				if (!string.IsNullOrEmpty(remark))
					wae.WorkAllocation.Remark = remark;
				else
					wa.Remark = "";

				this.SetViewDataTrainingCategories(wae);

				return this.PartialView("ucTrainingDetails", wae);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult AddTrainingDetails(FormCollection form)
		{
			var user = this.ContextUser;

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
			{
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
			}

			try
			{
				var workDay = InputHistory.GetDateTime(HistoryItemType.WorkDay, null).Value;

				WorkAllocation wa = null;

				if (int.Parse(form["WorkAllocationID"]) == 0)
				{
					wa = WorkAllocation.GetNew(this.CurrentUser, user, int.Parse(form["ActivityID"]), workDay);
				}
				else
				{
					wa = WorkAllocation.GetByID(int.Parse(form["WorkAllocationID"]));
				}

				string ID = wa.WorkAllocationID.ToString() + "__" + wa.ActivityID;
				wa.HoursWorked = double.Parse(form["Hours"]);
				wa.Remark = !string.IsNullOrEmpty(form["Remark"]) ? form["Remark"] : " ";
				wa.TrainingCategoryID = form["TrainingCategoryList" + ID].ToInt() ?? 0;
				wa.TrainerName = string.IsNullOrEmpty(form["TrainerName" + ID]) ? null : form["TrainerName" + ID];
				wa.TrainingInstitute = string.IsNullOrEmpty(form["TrainingInstitute" + ID]) ? null : form["TrainingInstitute" + ID];
				user.SaveWorkAllocation(this.CurrentUser, wa);

				//Update the WorkSessionStatus to UnApproved if no exceptions else to exceptions if exceptions
				var workSessionList = user.GetWorkSessions(workDay);
				var userWorkAllocationExceptionList = new List<WorkAllocationExceptionModel>();

				foreach (var ws in workSessionList)
				{
					ws.WorkSessionStatusID = !user.IsTimeCaptured(workDay, workDay, this.CurrentUser, out userWorkAllocationExceptionList) ? 3 : 1;
					user.UpdateWorkSession(this.CurrentUser, ws);
				}

				var model = new TimeDayModel(user, workDay);
				this.SetViewDataLists(model);

				return this.PartialView("ucDay", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult DeleteAllocation(int id)
		{
			var user = this.ContextUser;

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
			{
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
			}

			try
			{
				var wa = WorkAllocation.GetByID(this.CurrentUser, id);
				var workDay = wa.DayWorked;

				user.DeleteWorkAllocation(this.CurrentUser, wa);

				//Update the WorkSessionStatus to UnApproved if no exceptions else to exceptions if exceptions
				var workSessionList = user.GetWorkSessions(workDay);
				var userWorkAllocationExceptionList = new List<WorkAllocationExceptionModel>();

				foreach (var ws in workSessionList)
				{
					ws.WorkSessionStatusID = !user.IsTimeCaptured(workDay, workDay, this.CurrentUser, out userWorkAllocationExceptionList) ? 3 : 1;
					user.UpdateWorkSession(this.CurrentUser, ws);
				}

				var model = new TimeDayModel(user, workDay);
				this.SetViewDataLists(model);

				return this.PartialView("ucDay", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion WORK ALLOCATIONS

		#region NOTE

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult SaveNote(int workAllocationID, string noteText)
		{
			var user = this.ContextUser;

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
			{
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
			}

			try
			{
				var workAllocation = user.GetWorkAllocation(workAllocationID);
				var note = workAllocation.GetNote(this.CurrentUser);

				note.NoteText = noteText;
				workAllocation.SaveNote(this.CurrentUser, note);

				var model = new TimeDayModel(user, workAllocation.DayWorked);
				this.SetViewDataLists(model);

				return this.PartialView("ucDay", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion NOTE

		#region TIMESHEET

		[HttpGet]
		public ActionResult ApproveIndex(int? id, DateTime? workDay, string callingPage = "", int statusID = 0, int invoiceID = 0)
		{
			try
			{
				string strDateFrom = string.Empty;
				string strDateTo = string.Empty;
				int? userID = id ?? 0;

				if (userID == 0)
				{
					userID = InputHistory.Get(HistoryItemType.TimeApprovalUser, 0);
				}

				InputHistory.Set(HistoryItemType.TimeApprovalUser, userID.Value);

				var user = UserRepo.GetByID(userID.Value);

				bool validFromDate = false;
				bool validToDate = false;
				var enZA = new CultureInfo("en-ZA");
				validFromDate = DateTime.TryParseExact(strDateFrom, "yyyy-MM-dd", enZA, DateTimeStyles.None, out var dateFrom);
				validToDate = DateTime.TryParseExact(strDateTo, "yyyy-MM-dd", enZA, DateTimeStyles.None, out var dateTo);

				if (!workDay.HasValue && !validFromDate && !validToDate)
				{
					//dateFrom = InputHistory.GetDateTime(HistoryItemType.ApprovalDateFrom, DateTime.Today).Value;
					//dateTo = InputHistory.GetDateTime(HistoryItemType.ApprovalDateTo, DateTime.Today).Value;

					dateFrom = InputHistory.GetDateTime(HistoryItemType.ApprovalDateFrom, DateTime.Today.ToFirstDayOfMonth()).Value;
					dateTo = InputHistory.GetDateTime(HistoryItemType.ApprovalDateTo, DateTime.Today.ToLastDayOfMonth()).Value;
				}
				else if (!validFromDate && !validToDate)
				{
					dateFrom = workDay.Value.ToFirstDayOfMonth();
					dateTo = workDay.Value.ToLastDayOfMonth();

					InputHistory.Set(HistoryItemType.ApprovalDateFrom, dateFrom);
					InputHistory.Set(HistoryItemType.ApprovalDateTo, dateTo);
				}

				int approvalStatus = statusID == 0 ? InputHistory.Get(HistoryItemType.ApprovalStatus, 1) : statusID;

				var statusItems = new List<SelectListItem>();
				this.ViewData["StatusList"] = new SelectList(Enabill.Extensions.GetEnumSelectList<WorkSessionStatusType>(), "Value", "Text", approvalStatus);

				var userList = UserRepo.GetAll().ToList();
				var items = new List<SelectListItem>();

				foreach (var timeUser in userList)
				{
					items.Add(new SelectListItem() { Value = timeUser.UserID.ToString(), Text = timeUser.FullName, Selected = timeUser.UserID == user.UserID });
				}

				this.ViewData["UserList"] = items;

				var model = new TimeApproveIndexModel(user, dateFrom, dateTo, callingPage, invoiceID);

				return this.View(model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult ApproveIndex(FormCollection form)
		{
			try
			{
				int? userID = form["UserList"].ToInt();

				InputHistory.Set(HistoryItemType.ApprovalDateFrom, form["DateFrom"].ToDate() ?? DateTime.Today.ToFirstDayOfMonth());
				InputHistory.Set(HistoryItemType.ApprovalDateTo, form["DateTo"].ToDate() ?? DateTime.Today.ToLastDayOfMonth());

				int? invoiceID = form["InvoiceID"].ToInt();
				string callingPage = form["CallingPage"];

				var user = UserRepo.GetByID(userID.Value);

				InputHistory.Set(HistoryItemType.TimeApprovalUser, user.UserID);

				var userList = UserRepo.GetAll().ToList();
				var items = new List<SelectListItem>();

				foreach (var timeUser in userList)
				{
					items.Add(new SelectListItem() { Value = timeUser.UserID.ToString(), Text = timeUser.FullName, Selected = timeUser.UserID == user.UserID });
				}

				this.ViewData["UserList"] = items;

				var model = new TimeApproveIndexModel(user, InputHistory.GetDateTime(HistoryItemType.ApprovalDateFrom, DateTime.Today).Value, InputHistory.GetDateTime(HistoryItemType.ApprovalDateTo, DateTime.Today).Value, callingPage, invoiceID.Value);

				return this.View(model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		//[HttpGet]
		public ActionResult ApproveUserIndex(UserWorkDayStatus status = UserWorkDayStatus.All)
		{
			var model = new TimeApproveModel(this.userRepository);
			var dtFrom = new DateTime();
			var dtTo = new DateTime();
			//bool validFromDate = false;
			//bool validToDate = false;
			var enZA = new CultureInfo("en-ZA");

			dtFrom = InputHistory.GetDateTime(HistoryItemType.ApprovalDateFrom, DateTime.Today.ToFirstDayOfMonth()).Value;
			dtTo = InputHistory.GetDateTime(HistoryItemType.ApprovalDateTo, DateTime.Today.ToLastDayOfMonth()).Value;

			this.Response.Cache.SetNoStore();
			this.Response.Cache.AppendCacheExtension("no-cache");
			this.Response.Expires = 0;

			model.GetUsersWorkSessions(dtFrom, dtTo, this.CurrentUserID, status);

			return this.View(model);
		}

		[HttpGet]
		public ActionResult GetApproveUserIndex(string dateFrom = "", string dateTo = "", int managerID = 0, UserWorkDayStatus status = UserWorkDayStatus.All)
		{
			var model = new TimeApproveModel(this.userRepository);
			var dtFrom = new DateTime();
			var dtTo = new DateTime();
			bool validFromDate = false;
			bool validToDate = false;
			var enZA = new CultureInfo("en-ZA");
			validFromDate = DateTime.TryParseExact(dateFrom, "yyyy-MM-dd", enZA, DateTimeStyles.None, out dtFrom);
			validToDate = DateTime.TryParseExact(dateTo, "yyyy-MM-dd", enZA, DateTimeStyles.None, out dtTo);

			if (!validFromDate)
			{
				dtFrom = DateTime.Now.ToFirstDayOfMonth();
			}

			if (!validToDate)
			{
				if (validFromDate)
				{
					dtTo = dtFrom.ToLastDayOfMonth();
				}
				else
				{
					dtTo = DateTime.Now;
				}
			}

			InputHistory.Set(HistoryItemType.ApprovalDateFrom, dtFrom);
			InputHistory.Set(HistoryItemType.ApprovalDateTo, dtTo);

			model.GetUsersWorkSessions(dtFrom, dtTo, managerID, status);

			return this.PartialView("ucApproveUserIndex", model);
		}

		[HttpPost]
		public ActionResult ApproveUserIndex(FormCollection form)
		{
			try
			{
				InputHistory.Set(HistoryItemType.ApprovalDateFrom, form["DateFrom"].ToDate() ?? DateTime.Today.ToFirstDayOfMonth());
				InputHistory.Set(HistoryItemType.ApprovalDateTo, form["DateTo"].ToDate() ?? DateTime.Today.ToLastDayOfMonth());

				int? approvalManager = form["ManagerList"].ToInt();
				int? userWorkDaySessionStatus = form["StatusList"].ToInt();
				var status = UserWorkDaySessionStatus.All;

				if (userWorkDaySessionStatus.HasValue)
				{
					status = (UserWorkDaySessionStatus)userWorkDaySessionStatus.Value;
				}

				InputHistory.Set(HistoryItemType.ApprovalManager, approvalManager.Value);

				int managerID = 0;

				if (approvalManager == 1)
				{
					managerID = this.CurrentUserID;
				}

				this.ViewData["ManagerList"] = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Staff", Selected = 0 == approvalManager },
					new SelectListItem() { Value = "1", Text = "My Staff", Selected = 1 == approvalManager }
				};

				this.ViewData["StatusList"] = new List<SelectListItem>
				{
					new SelectListItem() { Value = "5", Text = "All", Selected = (UserWorkDaySessionStatus)5 == status },
					new SelectListItem() { Value = "1", Text = "Exception", Selected = (UserWorkDaySessionStatus)1 == status },
					new SelectListItem() { Value = "3", Text = "Unapproved", Selected = (UserWorkDaySessionStatus)3 == status },
					new SelectListItem() { Value = "2", Text = "Approved", Selected = (UserWorkDaySessionStatus)2 == status }
				};

				//TimeApproveUserIndexModel model = new TimeApproveUserIndexModel(InputHistory.GetDateTime(HistoryItemType.ApprovalDateFrom, DateTime.Today).Value, InputHistory.GetDateTime(HistoryItemType.ApprovalDateTo, DateTime.Today).Value, managerID);
				//UserWorkDayModel model = new UserWorkDayModel(InputHistory.GetDateTime(HistoryItemType.ApprovalDateFrom, DateTime.Today.ToFirstDayOfMonth()).Value, InputHistory.GetDateTime(HistoryItemType.ApprovalDateTo, DateTime.Today.ToLastDayOfMonth()).Value, managerID, status);
				var model = new UserWorkDayModel();

				return this.View(model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult UnlockTimesheet()
		{
			var user = this.ContextUser;

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
			{
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
			}

			try
			{
				var date = InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value;

				user.UnlockTimesheet(this.CurrentUser, date);

				var model = new TimeIndexModel(user, date);
				this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.IsInCurrentMonth() });

				return this.PartialView("ucIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult LockTimesheet()
		{
			var user = this.ContextUser;

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
			{
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
			}

			try
			{
				var date = InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value;

				user.LockTimesheet(this.CurrentUser, date);

				var model = new TimeIndexModel(user, date);
				this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.IsInCurrentMonth() });

				return this.PartialView("ucIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion TIMESHEET

		#region FLEXIDAY

		[HttpPost]
		public ActionResult BookFlexiDate(string remark)
		{
			var user = this.ContextUser;

			if (this.CurrentUserID != user.UserID && !this.CurrentUser.CanManage(user))
			{
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
			}

			try
			{
				var date = InputHistory.GetDateTime(HistoryItemType.WorkDay, DateTime.Today).Value;

				if (!WorkDay.IsDayWorkable(date))
					throw new EnabillConsumerException(Resources.ERR_WeekendPublicHoliday_Message + " Action cancelled.");

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
				else if (this.ContextUserID == this.CurrentUserID)
				{
					EnabillEmails.NotifyManagerOfBookedFlexiDay(this.ContextUser, flexiday);
				}
				else
				{
					EnabillEmails.NotifyUserOfBookedFlexiDayByManager(this.CurrentUser, this.ContextUser, flexiday);
				}

				return this.PartialView("ucDay", new TimeDayModel(this.ContextUser, date));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex, "Error occurred. Flexiday booking unsuccessful.");
			}
		}

		#endregion FLEXIDAY

		#region LOOK UP AND VIEW DATA

		private void SetViewDataLists(TimeDayModel model)
		{
			this.ViewData["ExtraAllocation"] = model.ExtraAllocations
						.Select(res => new SelectListItem { Text = res.ClientName + " - " + res.ProjectName + " - " + res.ActivityName, Value = res.ActivityID.ToString() });

			foreach (var wa in model.WorkAllocations)
			{
				string ID = wa.WorkAllocation.WorkAllocationID.ToString() + "__" + wa.Activity.ActivityID.ToString();

				if (wa.Client.ClientName == Enabill.Code.Constants.COMPANYNAME && wa.Activity.ActivityName == "Training")
				{
					this.ViewData["TrainingCategoryList" + ID] = WorkAllocationRepo.GetTrainingCategoryExtendedNames()
						.Select(tc => new SelectListItem { Text = tc.Value, Value = tc.Key.ToString(), Selected = tc.Key == wa.WorkAllocation.TrainingCategoryID })
						.ToArray();
				}

				foreach (var ltr in model.WorkAllocations.Select(s => s.AssociatedProjectTickets).ToList())
				{
					var aptItems = new List<SelectListItem>
					{
						new SelectListItem() { Value = "", Text = "Select Ticket" }
					};

					foreach (string tr in ltr)
					{
						aptItems.Add(new SelectListItem() { Value = tr, Text = tr, Selected = tr == wa.WorkAllocation.TicketReference });
					}

					this.ViewData["TicketList" + ID] = aptItems;
				}
			}
		}

		private void SetViewDataLists(TimeWeekModel model)
		{
			foreach (var daily in model.TimeDailyModels)
			{
				foreach (var wa in daily.WorkAllocations)
				{
					string ID = wa.WorkAllocation.WorkAllocationID.ToString() + "__" + wa.Activity.ActivityID.ToString();

					if ((wa.Client.ClientName == "Alacrity" || wa.Client.ClientName == "Saratoga") && wa.Activity.ActivityName == "Training")
					{
						this.ViewData["TrainingCategoryList" + ID] = WorkAllocationRepo.GetTrainingCategoryExtendedNames()
							.Select(tc => new SelectListItem { Text = tc.Value, Value = tc.Key.ToString(), Selected = tc.Key == wa.WorkAllocation.TrainingCategoryID })
							.ToArray();
					}

					foreach (var ltr in daily.WorkAllocations.Select(s => s.AssociatedProjectTickets).ToList())
					{
						var aptItems = new List<SelectListItem>
						{
							new SelectListItem() { Value = "", Text = "Select Ticket" }
						};

						foreach (string tr in ltr)
						{
							aptItems.Add(new SelectListItem() { Value = tr, Text = tr, Selected = tr == wa.WorkAllocation.TicketReference });
						}

						this.ViewData["TicketList" + ID] = aptItems;
					}
				}
			}
		}

		private void SetViewDataTrainingCategories(WorkAllocationExtendedModel model)
		{
			string ID = model.WorkAllocation.WorkAllocationID.ToString() + "__" + model.Activity.ActivityID.ToString();

			if (model.Client.ClientName == Enabill.Code.Constants.COMPANYNAME && model.Activity.ActivityName == "Training")
			{
				this.ViewData["TrainingCategoryList" + ID] = WorkAllocationRepo.GetTrainingCategoryExtendedNames()
					.Select(tc => new SelectListItem { Text = tc.Value, Value = tc.Key.ToString(), Selected = tc.Key == model.WorkAllocation.TrainingCategoryID })
					.ToArray();
			}

			var aptItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "", Text = "Select Ticket" }
			};

			foreach (string tr in model.AssociatedProjectTickets)
			{
				aptItems.Add(new SelectListItem() { Value = tr, Text = tr, Selected = tr == model.WorkAllocation.TicketReference });
			}

			this.ViewData["TicketList"] = aptItems;
		}

		public ActionResult LookupTicket(string term, int clientID, int projectID)
		{
			var list = WorkAllocationRepo.AutoCompleteTicket(term, 20, clientID, projectID);

			return this.Json(list, JsonRequestBehavior.AllowGet);
		}

		#endregion LOOK UP AND VIEW DATA
	}
}