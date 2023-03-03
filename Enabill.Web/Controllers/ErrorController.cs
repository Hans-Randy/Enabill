using System;
using System.Web.Mvc;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	public class ErrorController : BaseController
	{
		public ActionResult HttpError()
		{
			Exception ex = null;

			try
			{ ex = Settings.AppError; }
			catch { }

			return this.ErrorView(new ErrorModel(ex));
		}

		public ActionResult Http404(string errorMessage)
		{
			if (string.IsNullOrEmpty(errorMessage))
			{
				errorMessage = "We have logged the problem and will analyse it soon. If the problem is interfering with urgent work, please use the feedback tool to get a more immediate response.";

				if (this.CurrentUserID == 0)
					errorMessage = "We have logged the problem and will analyse it soon. Please continue to the home page to attempt to log in.";
			}

			return this.ErrorView(new ErrorModel("The page you requested was not found", errorMessage));
		}

		// (optional) Redirect to home when /Error is navigated to directly
		public ActionResult Index() => this.RedirectToAction("Index", "Home");
	}
}