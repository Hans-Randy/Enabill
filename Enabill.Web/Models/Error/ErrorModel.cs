using System;

namespace Enabill.Web.Models
{
	public class ErrorModel
	{
		#region PROPERTIES

		public bool IsAccessDenied { get; set; }

		public string ErrorMessage { get; set; }
		public string Title { get; set; }

		public Exception Exception { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public ErrorModel(Exception ex)
		{
			this.Title = "Sorry, an error occurred while processing your request.";

			this.ErrorMessage = "An error was detected but we are unable to analyse the issue at this time. If the problem persists, please be sure to report it and we'll do the rest.";

			if (ex is EnabillException)
				this.ErrorMessage = ex.Message;

			this.Exception = ex;

			this.IsAccessDenied = false;
		}

		public ErrorModel(string errorMessage, bool isAccessDenied = false)
		{
			this.Title = "Sorry, an error occurred while processing your request.";

			if (this.IsAccessDenied)
				this.Title = "Access Denied";

			this.ErrorMessage = "An error was detected but we are unable to analyse the issue at this time. If the problem persists, please be sure to report it and we'll do the rest.";

			if (isAccessDenied)
				this.ErrorMessage = "You do not have the required permissions to enter this part of the site.";

			if (!string.IsNullOrEmpty(errorMessage))
				this.ErrorMessage = errorMessage;

			this.Exception = null;

			this.IsAccessDenied = isAccessDenied;
		}

		public ErrorModel(string title, string errorMessage, bool isAccessDenied = false)
		{
			this.Title = "Sorry, an error occurred while processing your request.";

			if (!string.IsNullOrEmpty(title))
				this.Title = title;

			this.ErrorMessage = "An error was detected but we are unable to analyse the issue at this time. If the problem persists, please be sure to report it and we'll do the rest.";

			if (!string.IsNullOrEmpty(errorMessage))
				this.ErrorMessage = errorMessage;

			this.Exception = null;

			this.IsAccessDenied = isAccessDenied;
		}

		public ErrorModel(string title, string errorMessage, Exception ex, bool isAccessDenied = false)
		{
			this.Title = "Sorry, an error occurred while processing your request.";

			if (!string.IsNullOrEmpty(title))
				this.Title = title;

			this.ErrorMessage = "An error was detected but we are unable to analyse the issue at this time. If the problem persists, please be sure to report it and we'll do the rest.";

			if (!string.IsNullOrEmpty(errorMessage))
				this.ErrorMessage = errorMessage;

			this.Exception = ex;

			this.IsAccessDenied = isAccessDenied;
		}

		#endregion FUNCTIONS
	}
}