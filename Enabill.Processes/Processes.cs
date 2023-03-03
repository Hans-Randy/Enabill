using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Alacrity.DataAccess.SqlServer;
using ClosedXML.Excel;
using Enabill.Code;
using Enabill.Emailing;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;
using Enabill.Repository.SqlServer;
using NLog;
using ServiceStack.Text;

namespace Enabill.Processes
{
	public class ProcessEventArgs : EventArgs
	{
		private readonly int progress;

		public ProcessEventArgs(int progress)
		{
			this.progress = progress;
		}

		public int Progress => this.progress;
	}

	public static class Processes
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		#region EVENTS

		public static event EventHandler<ProcessEventArgs> OnProgress;

		#endregion EVENTS

		#region PROCESS

		public static void LogEnteredMethod(string methodName) => logger.Debug($"Entered method: {methodName}");

		private static void LogExitMethod(string methodName) => logger.Debug($"Exit method: {methodName}");

		public static void RunLeaveCycleBalanceProcess(User userExecuting)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			try
			{
				//This should be a once off process to be run after the initial data import.
				//Subsequent leave approvals etc will auto update the relevant balance.
				if (!userExecuting.HasRole(UserRoleType.SystemAdministrator))
					throw new UserRoleException("You do not have the required permissions to execute this process. Action cancelled.");

				foreach (var lcb in LeaveCycleBalanceRepo.GetAll().ToList())
				{
					//The spreadsheet does not have the enddate set. Update Enddate based on the cycle period for the leavetype
					//Also check whether the current cycle has expired(<= today) and insert a new one as it won't exist as at the time of the initial load

					switch ((LeaveTypeEnum)lcb.LeaveTypeID)
					{
						case LeaveTypeEnum.Sick:
							if (lcb.StartDate.AddYears(3).AddDays(-1) != lcb.EndDate)
							{
								lcb.EndDate = lcb.StartDate.AddYears(3).AddDays(-1);
								lcb.Active = lcb.EndDate <= DateTime.Today ? 0 : 1;
								LeaveCycleBalanceRepo.Save(lcb);
							}

							break;

						case LeaveTypeEnum.Compassionate:
							if (lcb.StartDate.AddYears(1).AddDays(-1) != lcb.EndDate)
							{
								lcb.EndDate = lcb.StartDate.AddYears(1).AddDays(-1);
								lcb.Active = lcb.EndDate <= DateTime.Today ? 0 : 1;
								LeaveCycleBalanceRepo.Save(lcb);
							}

							break;
					}

					if (lcb.EndDate <= DateTime.Today)
						InsertNewCycle(lcb);
				}

				//Update the sick leave balance
				foreach (var leave in LeaveRepo.GetApprovedLeaveByLeaveType(LeaveTypeEnum.Sick).ToList())
				{
					double taken = leave.NumberOfDays < 1 ? leave.NumberOfDays : 1;
					var user = UserRepo.GetByID(leave.UserID);

					user.RecalculateLeaveCycleBalances(user.UserID, LeaveTypeEnum.Sick, leave.DateFrom, leave.DateTo, taken, userExecuting);
				}

				//Update the compassionate leave balance
				foreach (var leave in LeaveRepo.GetApprovedLeaveByLeaveType(LeaveTypeEnum.Compassionate).ToList())
				{
					double taken = leave.NumberOfDays < 1 ? leave.NumberOfDays : 1;
					var user = UserRepo.GetByID(leave.UserID);

					user.RecalculateLeaveCycleBalances(user.UserID, LeaveTypeEnum.Compassionate, leave.DateFrom, leave.DateTo, taken, userExecuting);
				}
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		public static void RunRenewLeaveCycleProcess()
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			try
			{
				// Deactivate obsolete leave cycle balances records
				var obsoleteList = UserRepo.GetObsoleteLeaveCycleBalanceRecords();
				int lcbCount = 0;
				string userName = "";

				foreach (var User in obsoleteList)
				{
					LeaveCycleBalanceRepo.DeactivateLeaveCycleBalanceRecords(User.UserID, out lcbCount, out userName);

					logger.Debug($"Deactivated Leave Cycles: {userName} x {lcbCount}");
				}

				// Correct Leave Cycle Balance Discrepancies
				foreach (var user in UserRepo.GetLeaveBalanceDiscrepancies())
				{
					LeaveCycleBalanceRepo.CorrectLeaveCycleBalanceRecords(user.UserID, user.LeaveType);

					logger.Debug($"Corrected Leave Cycles. Name: {user.UserName} - Leave Type: {(LeaveTypeEnum)user.LeaveType}");
				}

				// Get a list of records with an invalid endate and update accordingly.
				var leaveCycleList = LeaveCycleBalanceRepo.GetLeaveCyclesThatExpireToday().ToList();

				logger.Debug($"Number of leave cycles that expire today: {leaveCycleList.Count}");

				var userList = UserRepo.GetAllActive();

				foreach (var lcb in leaveCycleList)
				{
					logger.Debug("Dumping leave cycle balance that requires renewing");
					logger.Debug($"{lcb}");

					var user = userList.Single(u => u.UserID == lcb.UserID);

					if (user.EmployEndDate == null || user.EmployEndDate > DateTime.Today)
					{
						InsertNewCycle(lcb);
					}
					else
					{
						logger.Debug("Excluding user with a populated end date");
						logger.Debug($"User name: {user.FullName} - End date: {user.EmployEndDate}");
						logger.Debug($"UserID: {user.UserID}");
					}
				}
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		public static void InsertNewCycle(LeaveCycleBalance lcb)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			try
			{
				var renewedLeaveCycleBalance = new LeaveCycleBalance
				{
					UserID = lcb.UserID,
					LeaveTypeID = lcb.LeaveTypeID
				};

				renewedLeaveCycleBalance.StartDate = renewedLeaveCycleBalance.LeaveTypeID == (int)LeaveTypeEnum.Sick ? lcb.EndDate.AddDays(1) : new DateTime(DateTime.Now.Year, 1, 1);
				renewedLeaveCycleBalance.EndDate = renewedLeaveCycleBalance.LeaveTypeID == (int)LeaveTypeEnum.Sick ? renewedLeaveCycleBalance.StartDate.AddYears(3).AddDays(-1) : new DateTime(DateTime.Now.Year, 12, 31);
				renewedLeaveCycleBalance.Taken = 0;
				renewedLeaveCycleBalance.OpeningBalance = renewedLeaveCycleBalance.LeaveTypeID == (int)LeaveTypeEnum.Sick ? 30 : 3;
				renewedLeaveCycleBalance.ClosingBalance = renewedLeaveCycleBalance.OpeningBalance;
				renewedLeaveCycleBalance.LastUpdatedDate = DateTime.Now;
				renewedLeaveCycleBalance.Active = 1;

				var existingLeaveCycleBalance = LeaveCycleBalanceRepo.GetLeaveCycleBalanceForUserDate(renewedLeaveCycleBalance.UserID, (LeaveTypeEnum)renewedLeaveCycleBalance.LeaveTypeID, renewedLeaveCycleBalance.StartDate);

				if (existingLeaveCycleBalance == null)
				{
					//De-activate the current cycle
					lcb.Active = 0;

					LeaveCycleBalanceRepo.Save(lcb);

					//Insert the new cycle
					LeaveCycleBalanceRepo.Save(renewedLeaveCycleBalance);
				}
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		public static void RunMonthEndLeaveFlexiBalanceProcess(User userExecuting, int numberOfMonths, string userIDs)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			try
			{
				if (userIDs?.Length == 0 || userIDs == null)
				{
					int userProcessCount = 0;

					foreach (var user in UserRepo.GetAllActive().ToList())
					{
						logger.Trace($"User: {user.UserName}");

						userProcessCount++;

						user.RecalculateLeaveAndFlexiBalances(userExecuting, user, numberOfMonths);
					}
				}
				else
				{
					foreach (int id in userIDs.ToIntArray())
					{
						var user = UserRepo.GetByID(id);

						logger.Trace($"User: {user.UserName}");

						user.RecalculateLeaveAndFlexiBalances(userExecuting, user, numberOfMonths);
					}
				}

				//Notification now send after UCTC complete Only send emails notification of the auto job ran and not when a user runs it from the app
				//if (userExecuting == null)
				//   EnabillEmails.NotifyLeaveFlexiUpdateJobComplete();
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		public static void RunMonthEndUserCostToCompanyProcess(User userExecuting, int numberOfMonths, string passphrase)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			try
			{
				if (userExecuting == null)
					userExecuting = EnabillSettings.GetSystemUser();

				//if initiated from automated job need to get password from DB
				if (passphrase == "RetrieveFromDB")
					passphrase = PassPhraseRepo.GetPassPhrase().PassPhraseName;

				var currentMonthDate = DateTime.Today.ToFirstDayOfMonth();
				var startDate = currentMonthDate.AddMonths(Math.Abs(numberOfMonths) * -1);
				startDate = startDate.ConfigureDate();

				var monthToRunFor = startDate;

				while (monthToRunFor < currentMonthDate)
				{
					Month.CreateForMonth(monthToRunFor);
					UserCostToCompany.ConfigureUserCostToCompanyForMonth(passphrase, monthToRunFor);

					monthToRunFor = monthToRunFor.AddMonths(1);
				}

				//Only send emails notification of the auto job ran and not when a user runs it from the app
				if (userExecuting == null)
					EnabillEmails.NotifyMonthEndJobComplete();
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		public static List<string> DailyProcess(User callingUser)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			var processExceptions = new List<string>();

			try
			{
				var currentPeriod = FinPeriod.GetCurrentFinPeriod();

				// Get all the active rules
				var invoiceRules = InvoiceRule.GetActive();

				var specificRule = invoiceRules.SingleOrDefault(ir => ir.InvoiceRuleID == 91);

				double total = invoiceRules.Count;
				double current = 0.0D;

				foreach (var rule in invoiceRules)
				{
					// Does it have an invoice for the period?
					var inv = rule.GetLatestInvoiceForFinPeriod(currentPeriod);

					if (inv != null)
					{
						// Do we skip?
						if (inv.BillingMethodID == (int)BillingMethodType.AdHoc)
							continue;

						if (inv.Period != currentPeriod.FinPeriodID)
							continue;

						// Can we update the existing invoice?
						try
						{
							if (inv.IsOpen)
								inv.ProcessInvoice();
						}
						catch (Exception ex)
						{
							logger.Debug($"Exception thrown. Dumping stacktrace: \n{Environment.StackTrace}");
							logger.Fatal($"Exception: \n{ex.Dump()}");

							while (ex.InnerException != null)
								ex = ex.InnerException;

							string projectName = string.Empty;

							foreach (string s in rule.ProjectNames)
							{
								projectName += s + ", ";
							}

							processExceptions.Add(string.Format($"UPDATE Rule for {rule.Client.ClientName}, project {projectName} Inv {inv.InvoiceID} :") + ex.Message);
						}
					}
					else
					{
						try
						{
							// We need to make a new invoice
							var invoice = rule.CreateInvoice(callingUser, DateTime.Today);

							invoice.Save(callingUser);
							invoice.ProcessInvoice();
						}
						catch (Exception ex)
						{
							logger.Debug($"Exception thrown. Dumping stacktrace: \n{Environment.StackTrace}");
							logger.Fatal($"Exception: \n{ex.Dump()}");

							while (ex.InnerException != null)
								ex = ex.InnerException;

							string projectName = string.Empty;

							foreach (string s in rule.ProjectNames)
							{
								projectName += s + ", ";
							}

							processExceptions.Add(string.Format($"CREATE Rule for {rule.Client.ClientName}, project {projectName} :") + ex.Message);
						}
					}

					current++;

					OnProgress?.Invoke(null, new ProcessEventArgs((int)Math.Round(current / total)));
				}

				return processExceptions;
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");

				return null;
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		public static List<WorkAllocationExceptionModel> GetTimeCaptureExceptions(DateTime dateFrom, DateTime dateTo, User callingUser)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			try
			{
				var users = new List<User>();

				if (callingUser == null)
					users = UserRepo.GetAll().ToList();
				else
					users = callingUser.GetStaffOfManager();

				var userExceptionDetails = new List<WorkAllocationExceptionModel>();
				var allExceptions = new List<WorkAllocationExceptionModel>();

				foreach (var user in users)
				{
					if (user.HasRole(UserRoleType.TimeCapturing) && user.IsActive)
					{
						user.IsTimeCaptured(dateFrom, dateTo, callingUser, out userExceptionDetails);
						allExceptions.AddRange(userExceptionDetails);
					}
				}

				return allExceptions;
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");

				return null;
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		public static void RunAndEmailReports(string frequency, string accountant)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			try
			{
				int frequencyNo = 0;
				var dateFrom = DateTime.Today;
				var dateTo = DateTime.Today;

				switch (frequency)
				{
					case "MONTHLY":
						frequencyNo = (int)FrequencyType.Monthly;
						dateFrom = DateTime.Today.ToFirstDayOfMonth().AddMonths(-1);
						dateTo = DateTime.Today.ToFirstDayOfMonth().AddDays(-1);

						break;

					case "WEEKLY":
						frequencyNo = (int)FrequencyType.Weekly;
						dateFrom = DateTime.Today.ToFirstDayOfMonth();
						dateTo = DateTime.Today;

						break;
				}

				foreach (var report in ReportEmailRepo.GetReportList(frequencyNo).ToList())
				{
					string reportName = (FrequencyType)frequencyNo + " " + report.ReportName;
					string reportPath;

					if (report.ReportName == "Activity Report" || report.ReportName == "Activity Analysis Report")
					{
						var userList = ReportEmailRepo.GetReportUserEmailList(report.ReportID, frequencyNo).ToList();
						var recipients = new List<string>();

						foreach (var user in userList)
						{
							reportPath = RunReportAndSaveToDisk(report.ReportID, report.ReportName, frequencyNo, accountant, user.UserID);
							recipients.Add(user.Email);

							if (!string.IsNullOrEmpty(reportPath))
								EnabillEmails.EmailReport(dateFrom, dateTo, recipients, reportName, reportPath);

							recipients.Remove(user.Email);
						}
					}
					//Email for ProjectManager Activity Report done in RunReportAndSaveToDisk as individual reports need to be send
					else if (report.ReportName != "ProjectManager Activity Report" || report.ReportName != "Encentivize Report")
					{
						reportPath = RunReportAndSaveToDisk(report.ReportID, report.ReportName, frequencyNo, accountant);

						var userList = ReportEmailRepo.GetReportUserEmailList(report.ReportID, frequencyNo).ToList();
						var recipients = new List<string>();

						foreach (var user in userList)
						{
							recipients.Add(user.Email);
						}

						if (!string.IsNullOrEmpty(reportPath))
							EnabillEmails.EmailReport(dateFrom, dateTo, recipients, reportName, reportPath);
					}
				}
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		public static void DeactivateEmployees()
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			try
			{
				var processUser = User.GetSystemUser();
				var dtFrom = new DateTime();
				var dtTo = new DateTime();

				TimesheetSchedule? timesheetSchedule = null;

				var endingUsers = UserRepo.GetAllActive().ToList().Where(u => u.EmployEndDate != null && u.EmployEndDate < DateTime.Today.Date).ToList();

				logger.Debug($"Number of users that should be deactivated today: {endingUsers.Count}");

				// This section is for deactivating users that have an end date prior to today's date
				if (endingUsers?.Count() > 0)
				{
					foreach (var user in endingUsers)
					{
						var ua = UserRepo.GetUserAllocationByUserID(user.UserID).ToList();
						var allocationsToBeEnded = ua.Where(a => a.ConfirmedEndDate == null || a.ConfirmedEndDate > user.EmployEndDate).ToList();

						if (allocationsToBeEnded != null)
						{
							logger.Debug($"Allocations that should be ended by the job: \n{allocationsToBeEnded.Dump()}");

							foreach (var userAllocation in allocationsToBeEnded)
							{
								userAllocation.SetConfirmedEndDateOnUserAllocationForActivity(processUser, (DateTime)user.EmployEndDate);
							}
						}

						logger.Debug($"Deactivating user {user.FullName}");

						try
						{
							var body = new StringBuilder();
							string addressee = "HR";

							var recipients = new List<string>
							{
								Constants.EMAILADDRESSSUPPORT,
								Constants.EMAILADDRESSHR
							};

							if (user.Manager != null)
							{
								recipients.Add(user.Manager.Email);
								addressee = string.Concat(user.Manager.FullName, " / ", addressee);
							}

							logger.Debug("Setting up mails to be sent");

							string subject = "User Deactivated";

							body.AppendFormat($"<h1>Good day {addressee},</h1>").AppendLine()
								.AppendLine("<br />");

							if (user.Deactivate(processUser))
							{
								//var leaveDays = user.GetLeaveBalance(LeaveTypeEnum.Annual);

								string sharedline = string.Format($"<p>Employee {user.FullName} has successfully been deactivated on Enabill.");

								body.AppendLine(sharedline)
									.AppendLine("<br />")
									.AppendLine("<br />")
									.AppendLine("Kindly proceed with de-activation on any other systems.")
									.AppendLine("<br />")
									//.AppendFormat($"<p>Annual leave balance: {leaveDays.Balance}<p>").AppendLine()
									//.AppendLine("<br />")
									.AppendLine("<h4>From Enabill Automated Services</h4>");

								logger.Debug("Sending mail to HR and manager");

								SendEmail(subject, recipients, body);

								recipients.Clear();

								body.Clear();

								logger.Debug("Setting up mail to ITS");

								recipients.Add("support@resolveits.co.za");

								body.AppendLine("<h1>Good day ITS Service,</h1>")
									.AppendLine("<br />")
									.AppendLine("<br />")
									.AppendLine(sharedline)
									.AppendLine("<br />")
									.AppendLine("<h4>From Enabill Automated Services</h4>");

								logger.Debug("Sending mail to ICT");

								SendEmail(subject, recipients, body);
							}
							else
							{
								body.AppendLine(string.Format($"<p>Employee {user.FullName} has not been deactivated on Enabill."))
									.AppendLine("<br />")
									.AppendLine("Kindly check if user is not assigned to projects or as a manger of other users.")
									.AppendLine("<br />")
									.AppendLine("<h4>From Enabill Automated Services</h4>");

								logger.Debug("Sending mail to HR and manager");

								SendEmail(subject, recipients, body);
							}

							subject = null;
							recipients = null;
							body = null;
						}
						catch (Exception ex)
						{
							logger.Debug($"User {user.FullName} could not be deactivated");
							logger.Error($"Exception: \n{ex.Dump()}");
						}
					}
				}

				int currentDay = DateTime.Today.Day;
				int nextMonthFirstDay = DateTime.Today.ToFirstDayOfMonth().Day;
				dtFrom = DateTime.Now.ToFirstDayOfMonth();
				dtTo = DateTime.Today.AddDays(-1);

				logger.Debug($"Current Day: {currentDay}");
				logger.Debug($"Current Day: {nextMonthFirstDay}");
				logger.Debug($"Date To: {dtTo}");
				logger.Debug($"Date From initial value: {dtFrom}");

				if (currentDay == nextMonthFirstDay)
				{
					timesheetSchedule = TimesheetSchedule.FirstDayOfNextTheMonth;
					dtFrom = dtTo.ToFirstDayOfMonth();

					logger.Debug($"Date From updated value: {dtFrom}");
				}
				else
				{
					var workableDays = WorkDay.GetWorkableDays(true, DateTime.Today.ToFirstDayOfMonth(), DateTime.Today.ToLastDayOfMonth()).OrderBy(u => u.WorkDate);
					int thisMonthLastDayMinusSeven = DateTime.Today.ToLastDayOfMonth().Day - 7;
					int thisMonthLastWorkingDay = workableDays.Last().WorkDate.Day;

					logger.Debug($"Last day of the month minus 7: {thisMonthLastDayMinusSeven}");
					logger.Debug($"Last working day of this month: {thisMonthLastWorkingDay}");

					if (currentDay == thisMonthLastDayMinusSeven)
					{
						timesheetSchedule = TimesheetSchedule.LastDayOfTheMonthMinusSevenDays;
					}
					else if (currentDay == thisMonthLastWorkingDay)
					{
						timesheetSchedule = TimesheetSchedule.LastWorkingDayOfTheMonth;
					}
					else
					{
						logger.Debug("Today does not fall into one of the 3 predefined time schedules where all user timesheets need to be validated");
					}
				}

				// We do this check to prevent duplicates from being sent if the current day falls within the 3 intervals
				if (timesheetSchedule == null)
				{
					logger.Debug("TimesheetSchedule is null. We now check if there are any users with end dates to validate their timesheets");

					var usersWithEndDate = UserRepo.GetAllActive().Where(u => u.EmployEndDate != null && u.EmployEndDate < DateTime.Today.Date).ToList();

					if (usersWithEndDate?.Count() > 0)
					{
						logger.Debug($"{usersWithEndDate.Count} user(s) with an end date found");

						ValidateTimesheets(dtFrom, dtTo, processUser, usersWithEndDate);
					}
					else
					{
						logger.Debug("No users with an end date found");
					}
				}
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		private static void ValidateTimesheets(DateTime dtFrom, DateTime dtTo, User processUser, IList<User> users = null)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			try
			{
				if (users != null)
					dtFrom = dtFrom.AddMonths(-3);

				GetUsersWorkSessions(dtFrom, dtTo, 0, UserWorkDayStatus.All, out var userRepository);

				if (users == null)
				{
					logger.Debug("No users were passed in. Assign all the users from the repository to the list that will be evaluated");

					users = userRepository.Users;
				}

				bool mailManager = false;
				const bool mailHasBeenSent = false;

				foreach (var user in users)
				{
					var userworkdays = userRepository.UsersWorkDayModel.Where(wd => wd.User.UserID == user.UserID);
					var invalidWorkDays = userworkdays.Where(uw => uw.HasException && !(uw.IsLeaveDay || uw.IsPendingLeaveDay) && !(uw.IsFlexiDay || uw.IsPendingFlexiDay));

					if (user.EmployEndDate != null)
					{
						logger.Debug($"Employee {user.FullName} has an end date populated. Manager should be mailed");

						invalidWorkDays = userworkdays.Where(uw => uw.HasException && !(uw.IsLeaveDay || uw.IsPendingLeaveDay) && !(uw.IsFlexiDay || uw.IsPendingFlexiDay) && uw.WorkDay.WorkDate < user.EmployEndDate);
						mailManager = true;
					}

					//TODO: remove this mailhasbeensent variable
					if (!mailHasBeenSent && (invalidWorkDays.Any() || mailManager))
					{
						var body = new StringBuilder();
						var recipients = new List<string>();

						string subject = "Incomplete timesheets";

						if (mailManager)
						{
							recipients.Add(user.Manager.Email);
							subject = "Employee with an end date";
							body.AppendFormat($"<h1>Good day {user.Manager.FullName},</h1>").AppendLine();

							var nonNullEndDate = DateTime.MinValue;

							if (user.EmployEndDate != null)
							{
								nonNullEndDate = (DateTime)user.EmployEndDate;
							}

							if (!invalidWorkDays.Any())
							{
								body.AppendFormat($"<p>Employee {user.FullName} has an end date of {nonNullEndDate.ToShortDateString()}. Time sheets are all valid.<p>").AppendLine();
							}
							else
							{
								body.AppendFormat($"<p>Employee {user.FullName} has an end date of {nonNullEndDate.ToShortDateString()} and their time sheet is outstanding<p>").AppendLine()
									.AppendFormat("<p>Please consult the list below for any incomplete days:<p>").AppendLine();
							}
						}
						else
						{
							recipients.Add(user.Email);
							body.AppendFormat($"<h1>Good day {user.FullName},</h1>").AppendLine()
								.AppendFormat("<p>Your Enabill timesheet is incomplete. Please see the below list for details<p>").AppendLine()
								.AppendFormat("<p>This also serves as a reminder to please keep your timesheets up-to-date on Enabill<p>").AppendLine()
								.AppendFormat("<p>These need to be completed by COB on <i>last working day of month</i> at the latest<p>").AppendLine()
								.AppendFormat("<p>Your assistance and co-operation with the above is greatly appreciated<p>").AppendLine();
						}

						body.AppendLine("<br />");

						if (invalidWorkDays.Any())
						{
							body.AppendFormat("<p>Incomplete days:<p>").AppendLine();

							foreach (var userWorkDayModel in invalidWorkDays)
							{
								body.AppendLine("<ul>")
									.AppendFormat($"<li>Date: {userWorkDayModel.WorkDay.WorkDate.ToShortDateString()}</li>").AppendLine()
									.AppendLine("<ul>")
									.AppendFormat($"<li>Incomplete reason: {userWorkDayModel.Reason}</li>").AppendLine()
									.AppendLine("</ul>")
									.AppendLine("</ul>");
							}
						}

						body.AppendLine("<br />");

						if (mailManager)
						{
							var projects = user.GetProjectsForProjectOwner();
							var usersUnderThisManager = user.GetStaffOfManager();

							if (projects?.Count > 0)
							{
								body.AppendFormat("<p>User is still the owner of at least one project. Please consult the below list of projects that ownership needs to be re-assigned<p>").AppendLine();

								foreach (var project in projects)
								{
									body.AppendLine("<ul>")
										.AppendFormat($"<li>Project ID: {project.ProjectID}</li>").AppendLine()
										.AppendLine("<ul>")
										.AppendFormat($"<li>Project Name: {project.ProjectName}</li>").AppendLine()
										.AppendFormat($"<li>Project Description: {project.ProjectDesc}</li>").AppendLine()
										.AppendLine("</ul>")
										.AppendLine("</ul>");
								}
							}

							if (usersUnderThisManager?.Count > 0)
							{
								body.AppendFormat("<p>User is still the manager of at least one other user. Please consult the below list of users that should be re-assigned to another manager<p>").AppendLine();

								foreach (var u in usersUnderThisManager)
								{
									body.AppendLine("<ul>")
										.AppendFormat($"<li>User ID: {u.UserID}</li>").AppendLine()
										.AppendLine("<ul>")
										.AppendFormat($"<li>User Name: {u.FullName}</li>").AppendLine()
										.AppendLine("</ul>")
										.AppendLine("</ul>");
								}
							}
						}
						body.AppendLine("<br />")
							.AppendLine("<h4>From Enabill Automated Services</h4>");

						logger.Debug($"Mail will now be sent to {(mailManager ? "the user's manager." : "the user.")}");

						SendEmail(subject, recipients, body);
					}
					else
					{
						user.Deactivate(processUser);
					}
				}
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		private static bool SendEmail(string subject, List<string> recipients, StringBuilder body)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			try
			{
				if (body == null)
				{
					logger.Warn("Error in method SendEmail. Body that was supplied is null");

					return false;
				}

				if (recipients == null || recipients.Count < 0)
				{
					logger.Warn("Error in method SendEmail. recipient list that was supplied is null or empty");

					return false;
				}

				var email = new EnabillEmailModel(subject, recipients, body.ToString());

				EnabillEmails.SendEmail(email);

				return true;
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");

				return false;
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		private static void GetUsersWorkSessions(DateTime? dateFrom, DateTime? dateTo, int managerID, UserWorkDayStatus status, out UserRepository userRepository)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			userRepository = null;

			try
			{
				userRepository = new UserRepository(new DbManager("EnabillContext"));
				userRepository.GetTimesheets((DateTime)dateFrom, (DateTime)dateTo, managerID, status);

				foreach (var u in userRepository.UsersWorkDayModel)
				{
					if (u.IsBeforeStartDay)
					{
						u.Reason = WorkDayReason.BeforeStartDate;
					}
					else if (u.IsAfterEndDate)
					{
						u.Reason = WorkDayReason.AfterEndDate;
					}
					else if (u.WorkDay.IsWorkable)
					{
						if (u.IsLeaveDay)
						{
							u.Reason = WorkDayReason.LeaveDay;
						}
						else if (u.IsPendingLeaveDay)
						{
							u.Reason = WorkDayReason.LeaveDay;
						}
						else if (u.IsFlexiDay)
						{
							u.Reason = WorkDayReason.FlexiDay;
						}
						else if (u.TotalTime > 0 && u.AllocatedTime > 0 && u.TotalTime != u.AllocatedTime)
						{
							u.Reason = WorkDayReason.IncorrectTimeAllocation;
						}
						else if (u.TotalTime == 0)
						{
							u.Reason = WorkDayReason.NoTimeAllocation;
						}
						else
						{
							u.Reason = WorkDayReason.CorrectTimeAllocation;
						}
					}
					else if (u.TotalTime > 0 && u.TotalTime != u.AllocatedTime)
					{
						u.Reason = WorkDayReason.IncorrectTimeAllocation;
					}
					else if (u.TotalTime > 0 && u.AllocatedTime > 0 && u.TotalTime == u.AllocatedTime)
					{
						u.Reason = WorkDayReason.CorrectTimeAllocation;
					}
					else
					{
						u.Reason = WorkDayReason.NotWorkable;
					}
				}
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		public static string RunReportAndSaveToDisk(int reportID, string reportName, int frequency, string accountant, int userID = 0)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			logger.Debug($"Report: {reportName}");

			try
			{
				int clientID = 0;
				int departmentID = 0;
				int divisionID = 0;
				string activityName = "0";
				string fileNameSuffix = "";
				string projectName = "0";
				string reportPath = "";
				string templatePath = "";
				var dateFrom = new DateTime();
				var dateTo = new DateTime();
				var fifthDayOfMonth = DateTime.MinValue;

				ActivityReportPrintModel activityModel = null;
				List<UserTimeSplitReportModel> activityRes = null;

				TrainingReportPrintModel trainingModel = null;
				List<UserTimeSplitReportModel> trainingRes = null;

				DSActivityReportModel dsModel = null;
				List<UserTimeSplitReportModel> dsRes = null;

				MISAllocatedTimePrintModel misModel = null;
				List<MISAllocatedTimeModel> misRes = null;

				WorkAllocationExceptionPrintModel exceptionModel = null;
				List<WorkAllocationExceptionModel> exceptionRes = null;

				List<UserWorkDayExcelModel> resEnc;
				EncentivizeReportModel encModel = null;

				ProjectManagerActivityReportModel paModel = null;
				List<ProjectActivityTimeReportModel> resPa = null;

				DataTable dataTable = null;

				switch ((FrequencyType)frequency)
				{
					case FrequencyType.Weekly:
						dateFrom = DateTime.Today.ToFirstDayOfMonth();
						dateTo = DateTime.Today;

						break;

					case FrequencyType.Monthly:
						dateFrom = DateTime.Today.ToFirstDayOfMonth().AddMonths(-1);
						dateTo = DateTime.Today.ToFirstDayOfMonth().AddDays(-1);

						break;
				}

				switch (reportName)
				{
					case "Activity Report":
						fileNameSuffix = "ActivityReport";
						templatePath = ReportHelpers.TemplatePath(fileNameSuffix, templatePath);
						//Call Tracer Excel Export functions
						activityModel = new ActivityReportPrintModel(dateFrom, dateTo, userID);
						activityRes = activityModel.ActivityReport.ToList();
						dataTable = ToADOTable<UserTimeSplitReportModel>(activityRes);
						reportPath = FillClosedXml(templatePath, dataTable);

						break;

					case "Activity Analysis Report":
						fileNameSuffix = "ActivityAnalysisReport";
						templatePath = ReportHelpers.TemplatePath(fileNameSuffix, templatePath);
						//Call Tracer Excel Export functions
						activityModel = new ActivityReportPrintModel(dateFrom, dateTo, userID);
						activityRes = activityModel.ActivityReport.ToList();
						dataTable = ToADOTable<UserTimeSplitReportModel>(activityRes);
						reportPath = FillClosedXml(templatePath, dataTable);

						break;

					case "Development Services Activity Report":
						fileNameSuffix = "DSActivityReport";
						templatePath = ReportHelpers.TemplatePath(fileNameSuffix, templatePath);
						dsModel = new DSActivityReportModel(dateFrom, dateTo);
						dsRes = dsModel.DSActivityReport.ToList();
						//Call Tracer Excel Export functions
						dataTable = ToADOTable<UserTimeSplitReportModel>(dsRes);
						reportPath = FillClosedXml(templatePath, dataTable);

						break;

					case "MIS Allocated Time Report":
						divisionID = 0;
						departmentID = 0;
						fileNameSuffix = "MISAllocatedTimeReport";
						templatePath = ReportHelpers.TemplatePath(fileNameSuffix, templatePath);
						//Call Tracer Excel Export functions
						misModel = new MISAllocatedTimePrintModel(dateFrom, dateTo, divisionID, departmentID);
						misRes = misModel.MISAllocatedTimeReport.ToList();
						dataTable = ToADOTable<MISAllocatedTimeModel>(misRes);
						reportPath = FillClosedXml(templatePath, dataTable);

						break;

					case "Percentage Allocation Report":
						{
							var Managers = ReportEmailRepo.GetManagerUserEmailList(reportID, frequency).ToList();

							if ((FrequencyType)frequency == FrequencyType.Monthly)
							{
								fileNameSuffix = "PercentageAllocationReport";
								templatePath = ReportHelpers.TemplatePath(fileNameSuffix, templatePath);
								List<PercentageAllocationModel> query = null;
								int yr = DateTime.Today.Year;
								int mth = DateTime.Today.Month;
								int day = DateTime.Today.Day;
								fifthDayOfMonth = new DateTime(yr, mth, day).AddMonths(-1);
								int finPeriod = Convert.ToInt32(yr + "0" + fifthDayOfMonth.Month);
								query = PercentageAllocationReportRepo.ExecutePercentageAllocationReport(yr, fifthDayOfMonth.Month, finPeriod, accountant);
								dataTable = ToADOTable<PercentageAllocationModel>(query);
								reportPath = FillClosedXml(templatePath, dataTable);
							}
							else if ((FrequencyType)frequency == FrequencyType.Weekly)
							{
								foreach (var Manager in Managers)
								{
									int year = dateFrom.Year;
									int month = dateFrom.Month;
									int finPeriod = Convert.ToInt32(dateFrom.Year + "0" + dateFrom.Month);
									List<PercentageAllocationModel> query = null;
									fileNameSuffix = "PercentageAllocationReport";
									templatePath = ReportHelpers.TemplatePath(fileNameSuffix, templatePath);
									query = PercentageAllocationReportRepo.ExecutePercentageAllocationReport(year, month, finPeriod, Manager.ManagerID.ToString());
									dataTable = ToADOTable<PercentageAllocationModel>(query);
									reportPath = FillClosedXml(templatePath, dataTable);

									var recipients = new List<string>
									{
										Manager.Email
									};

									EnabillEmails.EmailReport(dateFrom, dateTo, recipients, reportName, reportPath);

									reportPath = "";
								}
							}
						}

						break;

					case "ProjectManager Activity Report":
						foreach (var projectManager in ReportEmailRepo.GetReportUserEmailList(reportID, frequency).ToList())
						{
							//reportPath = Path.GetTempFileName().Replace("tmp", "xls");
							int projectManagerID = projectManager.UserID;
							fileNameSuffix = "PMActivityReport";
							templatePath = ReportHelpers.TemplatePath(fileNameSuffix, templatePath);
							paModel = new ProjectManagerActivityReportModel(dateFrom, dateTo, projectManagerID);
							resPa = paModel.ProjectActivityReport.ToList();
							//Call Tracer Excel Export functions
							dataTable = ToADOTable<ProjectActivityTimeReportModel>(resPa);
							reportPath = FillClosedXml(templatePath, dataTable);
							//Because we need to email a seperate report for each manager we doing it here

							var recipients = new List<string>
							{
								projectManager.Email
							};

							EnabillEmails.EmailReport(dateFrom, dateTo, recipients, reportName, reportPath);
						}

						break;

					case "Training Report":
						clientID = 159; //Client ID of Saratoga Software
						projectName = "ZZ Internal";
						activityName = "Training";
						fileNameSuffix = "TrainingReport";
						templatePath = ReportHelpers.TemplatePath(fileNameSuffix, templatePath);
						//Call Tracer Excel Export functions
						trainingModel = new TrainingReportPrintModel(dateFrom, dateTo, clientID, projectName, activityName, "0", "0");
						trainingRes = trainingModel.TrainingReport.ToList();
						dataTable = ToADOTable<UserTimeSplitReportModel>(trainingRes);
						reportPath = FillClosedXml(templatePath, dataTable);
						break;

					case "Work Allocation Exception Report":
						divisionID = 0;
						fileNameSuffix = "WorkAllocationExceptionReport";
						templatePath = ReportHelpers.TemplatePath(fileNameSuffix, templatePath);
						//Call Tracer Excel Export functions
						exceptionModel = new WorkAllocationExceptionPrintModel(dateFrom, dateTo, divisionID);
						exceptionRes = exceptionModel.WorkAllocationExceptionReport.ToList();
						dataTable = ToADOTable<WorkAllocationExceptionModel>(exceptionRes);
						reportPath = FillClosedXml(templatePath, dataTable);

						break;
				}

				return reportPath;
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");

				return null;
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		public static void RunEmail(DateTime dateFrom, int? managerID, EmailType emailType)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			try
			{
				switch (emailType)
				{
					case EmailType.LeaveRequestsPending:
						LeaveRequestsPendingModel leaveRequestsPendingModel = null;

						if (managerID.HasValue)
							leaveRequestsPendingModel = new LeaveRequestsPendingModel(dateFrom, managerID.Value);
						else
							leaveRequestsPendingModel = new LeaveRequestsPendingModel(dateFrom);

						EnabillEmails.RunLeaveRequestsPendingEmail(dateFrom, leaveRequestsPendingModel);

						break;
				}
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		public static void RecalculateLeaveBalances(LeaveTypeEnum leaveType)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);
			try
			{
				foreach (var user in UserRepo.GetAll().ToList())
				{
					var leaveCycleBalance = LeaveCycleBalanceRepo.GetLastLeaveCycleBalanceForUser(user.UserID, leaveType);

					if (leaveCycleBalance != null)
					{
						var leaves = user.GetLeave(leaveCycleBalance.StartDate, leaveCycleBalance.EndDate);
						double totalDaysInCycle = leaves.Where(x => x.LeaveType == 2 && x.ApprovalStatus == 4).Select(t => t.NumberOfDays).Sum();
					}
				}

				Console.WriteLine("End");
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		public static void TimeReminder()
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			try
			{
				// Checks to see if Email should be sent
				// If no valid dates, then do not send any Emails
				if (!TimeReminderCheckDate())
					return;

				string monthName = DateTime.Now.ToMonthName();
				var dtFrom = DateTime.Now.ToFirstDayOfMonth();
				var dtTo = DateTime.Today.AddDays(-1);
				var lastWorkdayOfMonth = WorkDayRepo.GetLastWorkdateByDate(DateTime.Today.ToLastDayOfMonth()).WorkDate.Date;

				logger.Debug($"Date From: {dtFrom}");
				logger.Debug($"Date To: {dtTo}");

				//Permanent and Montly Contractors
				TimeReminderSendMail(dtFrom, dtTo, monthName, lastWorkdayOfMonth, "Permanent");
				//Hourly Contractors
				TimeReminderSendMail(dtFrom, dtTo, monthName, lastWorkdayOfMonth, "Hourly");
				//Secondary Distribution List
				TimeReminderSendMail(dtFrom, dtTo, monthName, lastWorkdayOfMonth, "SecondaryList");
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		private static bool TimeReminderCheckDate()
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			string day;
			string dayValue;
			string timeReminderFile;

			try
			{
				//Get Configuration File Settings
				timeReminderFile = Constants.PATHTIMEREMINDERFILE;

				//Get Time Check Dates File Settings
				var xelement = XElement.Load(timeReminderFile);

				foreach (var date in xelement.Elements())
				{
					day = (string)date.Attribute("day");
					dayValue = date.Value;

					if (TimeReminderCheckValidDay(day, dayValue))
						return true;
				}
			}
			catch (Exception ex)
			{
				logger.Error($"Exception: \n{ex.Dump()}");
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}

			return false;
		}

		public static bool TimeReminderCheckValidDay(string day, string dayValue)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			/*
			Key:
				12 = Actual day.
				FirstDayOfMonth = First day of the month
				LastDayOfMonth = Last day of the month
			Value (Of current month):
				-7 = Plus or minus the number of days from value in Key
				WorkingDayBefore = Last Working Day Before value in Key
				WorkingDayAfter = Last Working Day After value in Key
			*/

			var dayToCheck = DateTime.Today.AddDays(1); //Added a day so that the default check won't return true.

			try
			{
				// day
				if (int.TryParse(day, out int num))
				{
					dayToCheck = DateTime.Today.ToFirstDayOfMonth().AddDays(num - 1);
				}
				else
				{
					switch (day)
					{
						case "FirstDayOfMonth":
							dayToCheck = DateTime.Today.ToFirstDayOfMonth();
							break;

						case "LastDayOfMonth":
							dayToCheck = DateTime.Today.ToLastDayOfMonth();
							break;
					}
				}

				// dayValue
				if (int.TryParse(dayValue, out num))
				{
					if (DateTime.Today.Date == dayToCheck.AddDays(num).Date)
						return true;
				}
				else
				{
					switch (dayValue)
					{
						case "WorkingDayBefore":
							dayToCheck = WorkDayRepo.GetLastWorkdateByDate(dayToCheck.AddDays(-1)).WorkDate;

							break;

						case "WorkingDayAfter":
							dayToCheck = WorkDayRepo.GetLastWorkdateByDate(dayToCheck.AddDays(1)).WorkDate;

							break;
					}

					if (DateTime.Today.Date == dayToCheck.Date)
						return true;
				}
			}
			catch (Exception ex)
			{
				logger.Error($"Exception: \n{ex.Dump()}");
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}

			return false;
		}

		public static void TimeReminderSendMail(DateTime dtFrom, DateTime dtTo, string monthName, DateTime lastWorkdayOfMonth, string recipient)
		{
#if DEBUG

			dtFrom = new DateTime(2017, 09, 21);
			dtTo = new DateTime(2017, 09, 23);

#endif

			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			string subject = "Time Reminder";
			var recipients = new List<string>();
			var body = new StringBuilder();

			try
			{
				if (recipient == "SecondaryList") //Secondary List
				{
					body.AppendLine("<p><h3>Good day,</h3></p>")
						.AppendLine("<p><br /></p>")
						.Append("<p>Try the new <a href=https://drive.google.com/file/d/1Ad0BHVYDwHKFqLQ1xclNMR0Wj8DT-a5j/view?usp=sharing> Week View!</a></p>")
						.AppendLine("<p><br /></p>")
						.Append("<p>Please start updating your ").Append(monthName).Append(" Enabill times.</p>")
						.AppendLine("<br />")
						.AppendLine("<p>Your assistance and co-operation with the above is greatly appreciated.</p>")
						.Append("<p>Click here to go to Enabill: http://").Append(Constants.SITEADDRESS).AppendLine("</p>")
						.AppendLine("<p><h4>From Enabill Automated Services</h4></p>");

					try
					{
						// Send Email
						logger.Debug($"Sending Mail to Distribution List {recipient}");

						recipients.Add(Constants.EMAILADDRESSACCOUNTS);

						SendEmail(subject, recipients, body);
					}
					catch (Exception ex)
					{
						logger.Debug($"Distribution List {recipient} could not be Emailed");
						logger.Error($"Exception: \n{ex.Dump()}");
					}
					finally
					{
						recipients.Clear();
						body.Clear();
					}
				}
				else //Time Capturers
				{
					bool hourlyContractor = recipient == "Hourly";

					var timeCaptureUsers = UserRepo.GetAllTimeCaptureUsers(hourlyContractor).ToList();

					logger.Debug($"Total number of{(hourlyContractor ? " Hourly" : "")} Time Capture users: {timeCaptureUsers.Count}");

					//This section is for finding which days have time recorded outstanding
					if (timeCaptureUsers?.Count() > 0)
					{
						foreach (var user in timeCaptureUsers)
						{
							logger.Debug($"User {user.FullName}");

							body.Append("<p><h3>Good day ").Append(user.FirstName).AppendLine(",</h3></p>")
								.AppendLine("<p><br /></p>")
								.Append("<p>Try the new <a href=https://drive.google.com/file/d/1Ad0BHVYDwHKFqLQ1xclNMR0Wj8DT-a5j/view?usp=sharing> Week View!</a></p>")
								.AppendLine("<p><br /></p>")
								.Append("<p>Please start updating your ").Append(monthName).Append(" Enabill times.</p>");

							// Find Workdays
							var workableDays = WorkDay.GetWorkableDays(true, dtFrom, dtTo).OrderBy(u => u.WorkDate);

							if (workableDays != null || workableDays.Any())
							{
								// Check if any outstanding time
								var workAllocationExceptionList = UserRepo.GetTimeCaptureExceptions(user.UserID, dtFrom, dtTo).ToList();

								if (workAllocationExceptionList.Count > 0)
								{
									bool outstandingDays = false;
									var outStandingDay = DateTime.Today.AddYears(-100);

									foreach (var outstandingDay in workAllocationExceptionList.OrderByDescending(o => o.WorkDate.Date))
									{
										if (outStandingDay != outstandingDay.WorkDate && !UserLeaveGeneralRepo.GetValidLeaveDayByUserID(user.UserID, outstandingDay.WorkDate) && !UserRepo.IsFlexiDay(user.UserID, outstandingDay.WorkDate))
										{
											if (!outstandingDays)
											{
												body.AppendLine("<p><br /></p>")
													.AppendLine("<p>See outstanding days below:</p>");

												outstandingDays = true;
											}

											outStandingDay = outstandingDay.WorkDate;

											body.Append("<p>").Append(outstandingDay.WorkDate.ToShortDateString()).AppendLine("</p>");
										}
									}

									if (outstandingDays)
										body.AppendLine("<br />");
								}

								body.AppendLine("<p>Your assistance and co-operation with the above is greatly appreciated.</p>")
									.Append("<p>Click here to go to Enabill: http://").Append(Constants.SITEADDRESS).AppendLine("</p>")
									.AppendLine("<p><h4>From Enabill Automated Services</h4></p>");

								try
								{
									// Send Email
									logger.Debug($"Sending Mail to Employee {recipient}");

									recipients.Add(user.Email);

									SendEmail(subject, recipients, body);
								}
								catch (Exception ex)
								{
									logger.Debug($"User {user.FullName} could not be Emailed");
									logger.Error($"Exception: \n{ex.Dump()}");
								}
								finally
								{
									recipients.Clear();
									body.Clear();
								}
							}
						}
					}
				}

				subject = null;
				recipients = null;
				body = null;
			}
			catch (Exception ex)
			{
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		#endregion PROCESS

		#region TRACER EXCEL EXPORT FUNCTIONS

		private static string CleanFileName(string src)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			try
			{
				var enc = new ASCIIEncoding();
				byte[] conv = enc.GetBytes(src);
				// System.Text.ASCIIEncoding.ASCII.GetString(conv);

				return Encoding.GetEncoding("iso-8859-8").GetString(conv);
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");

				return null;
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		private static string FillClosedXml(string templatePath, DataTable dataTable)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);

			try
			{
				// Do temp dir cleanup....
				DoTempDirCleanup();

				// construct the path and name of the new temp xlsx file
				string destFilename = string.Format($"{TempPath}/{Guid.NewGuid().ToString()}.xlsx");
				destFilename = destFilename.Replace("//", "/");

				//try
				//{
				// this collection contains all the column names (as per source dataTable) and the excel column index it will be mapped to
				var colindex = new Dictionary<string, int>();

				//System.IO.File.Copy(System.Web.Hosting.HostingEnvironment.MapPath(templatePath), destFilename);
				File.Copy(templatePath, destFilename);

				// find the template package
				var package = new XLWorkbook(destFilename);

				//TODO MEL change to correct exception
				if (package == null)
					throw new FeedbackException();//NotFoundException(String.Format($"File cannot be found: {templatePath}"));

				// find the 'Data' worksheet
				var ws = package.Worksheet("Data");

				if (ws == null)
					throw new Exception(string.Format($"Template file does not have a 'Data' worksheet : {templatePath}"));

				// find the 'datatable' table
				var table = ws.Tables.SingleOrDefault()?.Name == "Table2" ? ws.Table("Table2") : ws.Table("datatable");

				if (table == null)
					throw new Exception(string.Format($"Template file does not contain a table with reference named 'datatable' : {templatePath}"));

				// find the 'fields' range
				var fields = package.Range("fields");

				if (fields == null)
					throw new Exception(string.Format($"Template file does not contain a range/table with reference named 'fields' : {templatePath}"));

				// now prep a collection of columnname vs indexes
				for (int i = fields.FirstColumn().ColumnNumber(); i <= fields.LastColumn().ColumnNumber(); i++)
				{
					colindex.Add(ws.Cell(fields.FirstRow().RowNumber(), i).Value.ToString(), i);
				}

				// removes the fields row
				fields.FirstRow().Delete();

				// this loop will add the fields that are found in the source table (but not defined in the template) to the output
				foreach (DataColumn col in dataTable.Columns)
				{
					if (!colindex.ContainsKey(col.Caption))
					{
						table.InsertColumnsAfter(1);
						table.HeadersRow().Cell(table.HeadersRow().RangeAddress.LastAddress.ColumnNumber).Value = col.Caption;
						colindex.Add(col.Caption, table.HeadersRow().RangeAddress.LastAddress.ColumnNumber);
						//table.Cell(2, colindex[col.Caption]).DataType = ClosedXML.Excel.XLCellValues.Text;
					}
				}

				// ... ok... the meat of this routine... this is where we do the actual cell-by-cell construction of the new output
				object cellval = null;
				IXLCell cell = null;

				if (dataTable.Rows.Count > 0)
				{
					table.InsertRowsBelow(dataTable.Rows.Count - 1, true);

					for (int i = 0; i < dataTable.Rows.Count; i++)
					{
						foreach (var col in colindex)
						{
							cell = table.Cell(i + 2, col.Value);
							cellval = dataTable.Rows[i][col.Key];
							cell.Style = table.Cell(2, col.Value).Style;
							cell.DataType = table.Cell(2, col.Value).DataType;

							if (dataTable.Columns.Contains(col.Key) && cellval != null)
							{
								cell.DataType = table.Cell(2, col.Value).DataType;

								try
								{
									cell.Value = GetCellValue(cellval, dataTable.Columns[col.Key].DataType);
								}
								catch { }
							}
						}
					}
				}

				try
				{
					// Autowidth all fields
					ws.Columns().AdjustToContents();
				}
				catch
				{
				}

				// Save file to new path
				package.Save();

				package = null;

				//FilePathResult res = File(destFilename, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", CleanFileName(String.Format($"{CurrentUser.UserName}_{DateTime.Now.ToString("yyyyMMddHHmmss")}_{filenamesuffix}.xlsx")));
				//return res;
				//Only want to return the reportPath and not open as it will be emailed
				return destFilename;
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");

				return null;
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		public static DataTable ToADOTable<T>(IEnumerable<T> varlist)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);
			try
			{
				var dtReturn = new DataTable();
				// Use reflection to get property names, to create table
				// column names
				var oProps = typeof(T).GetProperties();

				foreach (var pi in oProps)
				{
					var colType = pi.PropertyType;

					if (colType.IsGenericType && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
						colType = colType.GetGenericArguments()[0];

					dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
				}
				foreach (var rec in varlist)
				{
					var dr = dtReturn.NewRow();

					foreach (var pi in oProps)
						dr[pi.Name] = pi.GetValue(rec, null) ?? DBNull.Value;

					dtReturn.Rows.Add(dr);
				}

				return dtReturn;
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");

				return null;
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		private static object GetCellValue(object cellval, Type type)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);
			try
			{
				if (cellval == null)
					return null;
				if (type == typeof(string))
				{
					string val = cellval.ToString();
					//if (cellval.ToString().Length > 255)
					//    val = cellval.ToString().Substring(0, 254);

					return RemoveInvalidXMLChars(val);
				}

				if (type == typeof(decimal))
				{
					string val = cellval.ToString();

					if (string.IsNullOrEmpty(val))
						return 0;

					return Convert.ToDecimal(val);
				}

				return cellval;
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");

				return null;
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		public static string RemoveInvalidXMLChars(string text)
		{
			LogEnteredMethod(MethodBase.GetCurrentMethod().Name);
			try
			{
				if (string.IsNullOrEmpty(text))
					return "";

				if (_invalidXMLChars.IsMatch(text))
					return _invalidXMLChars.Replace(text, "");

				return text;
			}
			catch (Exception ex)
			{
				logger.Debug($"Exception thrown. Stacktrace: \n{Environment.StackTrace}");
				logger.Fatal($"Exception: \n{ex.Dump()}");

				return null;
			}
			finally
			{
				LogExitMethod(MethodBase.GetCurrentMethod().Name);
			}
		}

		// filters control characters but allows only properly-formed surrogate sequences
		private static readonly Regex _invalidXMLChars = new Regex(
			@"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
			RegexOptions.Compiled);

		private static void DoTempDirCleanup()
		{
			foreach (string filename in Directory.GetFiles(TempPath))
			{
				try
				{
					File.Delete(filename);
				}
				catch { } //explicit dummy catch
			}
		}

		private static string TempPath => Constants.PATHTEMP.Replace("\\", "/");

		#endregion TRACER EXCEL EXPORT FUNCTIONS
	}
}