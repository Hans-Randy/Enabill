using System;
using System.Linq;
using System.Web.Mvc;
using Enabill.Models;
using Enabill.Repos;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class UserCostToCompanyController : BaseController
	{
		[HttpGet]
		public ActionResult Index()
		{
			if (!Settings.Current.PassphraseIsValid)
				return this.RedirectToAction("Passphrase", "Home", new { cont = "UserCostToCompany" });

			var monthDate = InputHistory.GetDateTime(HistoryItemType.UserCostToCompanyPeriod, DateTime.Now).Value;
			monthDate = monthDate.ConfigureDate();

			if (monthDate >= DateTime.Today.ToFirstDayOfMonth())
				monthDate = DateTime.Today.ToFirstDayOfMonth().AddMonths(-1);

			var model = new UserCostToCompanyIndexModel(Settings.Current.Passphrase, monthDate.ToPeriod());

			return this.View(model);
		}

		[HttpPost]
		public ActionResult Month(string date)
		{
			try
			{
				if (!Settings.Current.PassphraseIsValid)
					return this.Content("-1");

				var monthDate = DateTime.Today;

				if (!DateTime.TryParse(date, out monthDate))
					monthDate = InputHistory.GetDateTime(HistoryItemType.UserCostToCompanyPeriod, DateTime.Now).Value;

				monthDate.ConfigureDate();

				if (monthDate >= DateTime.Today.ToFirstDayOfMonth())
					monthDate = DateTime.Today.ToFirstDayOfMonth().AddMonths(-1);

				InputHistory.Set(HistoryItemType.UserCostToCompanyPeriod, monthDate);

				var model = new UserCostToCompanyIndexModel(Settings.Current.Passphrase, monthDate.ToPeriod());

				return this.PartialView("ucIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult Save(FormCollection form)
		{
			try
			{
				var monthDate = InputHistory.GetDateTime(HistoryItemType.UserCostToCompanyPeriod, DateTime.Today).Value;
				DateTime.TryParse(form["MonthDate"], out monthDate);

				if (monthDate < Settings.SiteStartDate || monthDate >= DateTime.Today.ToFirstDayOfMonth())
					throw new Exception("TODO");

				InputHistory.Set(HistoryItemType.UserCostToCompanyPeriod, monthDate);

				var month = Enabill.Models.Month.GetByPeriod(monthDate.ToPeriod());
				month.SetOverheadPercentage(Settings.Current.CurrentUser, double.Parse(form["OverheadPercentage"]) / 100);

				foreach (int userID in UserCostToCompany.GetAllForMonth(monthDate).Select(uc => uc.UserID).ToList())
				{
					var u = UserRepo.GetByID(userID);
					u.SetUserCostToCompanyForMonth(Settings.Current.Passphrase, monthDate.ToPeriod(), double.Parse(form["dCost_" + userID].Replace(" ", "")));
				}
			}
			catch (EnabillPassphraseException ex)
			{
				return this.RedirectToAction("Passphrase", "Home", new { cont = "UserCostToCompany" });
			}

			return this.RedirectToAction("Index");
		}
	}
}