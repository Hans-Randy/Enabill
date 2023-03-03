using System;
using System.Linq;
using System.Web.Mvc;
using Enabill.Emailing;
using Enabill.Models;
using Enabill.Repos;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class ConfigurationController : BaseController
	{
		public ActionResult Index() => this.View();

		[HttpPost]
		public ActionResult GetRunMonthEndLeaveFlexiBalanceProcessPartial()
		{
			this.ViewData["UserList"] = UserRepo.GetAll().OrderBy(u => u.UserName).Select(u => new SelectListItem { Value = u.UserID.ToString(), Text = u.UserName });

			return this.PartialView("ucRunMonthEndLeaveFlexiBalanceProcess");
		}

		[HttpPost]
		public ActionResult RunMonthEndLeaveFlexiBalanceProcess(int numberOfMonths, string userIDs)
		{
			try
			{
				Processes.Processes.RunMonthEndLeaveFlexiBalanceProcess(this.CurrentUser, numberOfMonths, userIDs);

				return this.ReturnJsonResult(false, "Process ran successfully.");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult RunLeaveCycleBalanceProcess()
		{
			try
			{
				Processes.Processes.RunLeaveCycleBalanceProcess(this.CurrentUser);

				return this.ReturnJsonResult(false, "Process ran successfully.");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult RunRenewLeaveCycleProcess()
		{
			try
			{
				Processes.Processes.RunRenewLeaveCycleProcess();

				return this.ReturnJsonResult(false, "Process ran successfully.");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult IsPassphraseCorrect() => this.Content(Settings.Current.PassphraseIsValid.ToString());

		[HttpPost]
		public ActionResult GetRunUserCostToCompanyProcessPartial() => this.PartialView("ucRunMonthEndUserCostToCompanyProcess");

		[HttpPost]
		public ActionResult RunUserCostToCompanyProcess(int numberOfMonths)
		{
			try
			{
				Processes.Processes.RunMonthEndUserCostToCompanyProcess(this.CurrentUser, numberOfMonths, Settings.Current.Passphrase);

				return this.ReturnJsonResult(false, "Process ran successfully.");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult RunDailyInvoiceProcess()
		{
			try
			{
				var exceptions = Processes.Processes.DailyProcess(EnabillSettings.GetSystemUser());
				if (exceptions.Count == 0)
					return this.ReturnJsonResult(true, "Process ran successfully");
				else
				{
					EnabillEmails.NotifyUsersOfInvoiceExceptions(exceptions);

					return this.PartialView("ucDailyProcessExceptions", new DailyProcessExceptionModel(exceptions));
				}
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult RunTimesheetApprovalProcess()
		{
			try
			{
				var fromDate = DateTime.Now.ToFirstDayOfMonth().AddMonths(-1);
				var toDate = DateTime.Now.ToFirstDayOfMonth().AddDays(-1);

				var user = Settings.Current.CurrentUser;

				if (user.HasRole(UserRoleType.SystemAdministrator))
					user = null;

				var exceptions = Processes.Processes.GetTimeCaptureExceptions(fromDate, toDate, user);

				if (exceptions.Count == 0)
					return this.ReturnJsonResult(true, "Process ran without any exceptions.");
				else
					return this.PartialView("ucTimesheetExceptions", new TimesheetExceptionModel(exceptions));
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#region USER PREFERENCES

		[HttpPost]
		public ActionResult GetPreferenceView()
		{
			var userPref = this.CurrentUser.GetUserPreference();
			return this.PartialView("ucPreferences", userPref);
		}

		[HttpPost]
		public ActionResult SetupPreferences(DateTime startTime, DateTime endTime, double? lunch, bool dayView)
		{
			try
			{
				var user = this.CurrentUser;
				var userPref = user.GetUserPreference();

				userPref.DefaultWorkSessionStartTime = startTime;
				userPref.DefaultWorkSessionEndTime = endTime;
				userPref.DefaultLunchDuration = lunch ?? 0;
				userPref.DayView = dayView;

				userPref.Save(this.CurrentUser);

				return this.ReturnJsonResult(false, "Preferences saved successfully.");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult WeekThreeStateUpdate(string threeState)
		{
			string tState = threeState;

			try
			{
				var user = this.CurrentUser;
				var userPref = user.GetUserPreference();

				userPref.ThreeState = tState;

				userPref.Save(this.CurrentUser);

				return this.Json("OK");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion USER PREFERENCES

		#region PASSPHRASES

		[HttpPost]
		public ActionResult GetPassPhraseView() => this.PartialView("ucPassPhrase", "");

		[HttpPost]
		public ActionResult SetupPassPhrase(string oldPassPhrase, string newPassPhrase, string confirmPassPhrase)
		{
			try
			{
				var user = this.CurrentUser;
				bool isValid = false;

				if (Helpers.ConfirmPassphraseIsValid(oldPassPhrase))
				{
					if (newPassPhrase != confirmPassPhrase)
					{
						return this.ReturnJsonResult(false, "New password does not match confirm password value.");
					}
					else
					{
						Settings.Current.Passphrase = newPassPhrase;

						//update the password on the passphrase table as there are views which management are
						// using for reporting in excel
						var passPhrase = PassPhraseRepo.GetPassPhrase();
						passPhrase.PassPhraseName = newPassPhrase;
						passPhrase.ModifiedBy = this.CurrentUserID;
						passPhrase.ModifiedDate = DateTime.Now;
						passPhrase.Save();

						//reencrypt existing uctc with the new password
						foreach (var userCostToCompany in UserCostToCompany.GetAll().ToList())
						{
							userCostToCompany._decCostToCompany = userCostToCompany.GetCostToCompanyAmount(oldPassPhrase, out isValid);
							userCostToCompany.ModifiedByID = this.CurrentUserID;
							userCostToCompany.ModifiedDate = DateTime.Now;
							userCostToCompany.ModifyReason = "Reset PassPhrase";
							userCostToCompany.Save(newPassPhrase);
						}

						return this.ReturnJsonResult(false, "PassPhrase successfully updated.");
					}
				}
				else
				{
					return this.ReturnJsonResult(false, "Old PassPhrase invalid.");
				}
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion PASSPHRASES

		[HttpPost]
		public ActionResult ManageFinPeriods(string getAll)
		{
			try
			{
				var model = new FinPeriodEditModel(getAll);

				return this.PartialView("ucManageFinPeriods", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult NewFinPeriod() => this.PartialView("ucAddPeriod");

		[HttpPost]
		public ActionResult UpdateFinPeriod(int finPeriod)
		{
			try
			{
				FinPeriod.UpdateFinPeriod(finPeriod);
				var model = new FinPeriodEditModel("No");

				return this.PartialView("ucManageFinPeriods", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult AddFinPeriod(DateTime dateFrom, DateTime dateTo, bool isCurrent)
		{
			try
			{
				string message = FinPeriod.AddNewFinPeriod(dateFrom, dateTo, isCurrent);

				return this.ReturnJsonResult(false, message);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult RunAndEmailReports(string frequency)
		{
			try
			{
				Processes.Processes.RunAndEmailReports(frequency, "%");

				return this.ReturnJsonResult(false, "Email Process ran successfully.");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}
	}
}