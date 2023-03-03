using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	public abstract class BaseController : Controller
	{
		public User CurrentUser => Settings.Current.CurrentUser;
		public int CurrentUserID => Settings.Current.CurrentUserID;

		public User ContextUser => Settings.Current.ContextUser;
		public int ContextUserID => Settings.Current.ContextUserID;

		public UserPreference CurrentUserPreferences => Settings.Current.CurrentUserPreferences;

		protected void AddAsDropdownSource<T>(ICollection<T> list, object selectedValue = null)
		{
			string name = typeof(T).Name + "ID";
			this.AddAsDropdownSource(list, name, selectedValue);
		}

		protected void AddAsDropdownSource<T>(ICollection<T> list, string name, object selectedValue = null)
		{
			string key, value;

			if (name.EndsWith("ID"))
			{
				key = name;
				value = name.Substring(0, name.Length - 2) + "Name";
			}
			else
			{
				key = $"{name}ID";
				value = $"{name}Name";		
			}
			this.AddAsDropdownSource(list, name, key, value, selectedValue);
		}

		protected void AddAsDropdownSource<T>(ICollection<T> list, string name, string key, string value, object selectedValue = null) => this.ViewData[name] = new SelectList(list, key, value, selectedValue);

		protected static bool IsCheckBoxChecked(string value)
		{
			if (string.IsNullOrEmpty(value))
				return false;

			if (value == "on")
				return true;

			return value.Split(',').Length > 1;
		}

		public bool IsAJAX() => this.Request.IsAjaxRequest();

		protected override void OnAuthorization(AuthorizationContext filterContext)
		{
			if (this.CurrentUser?.MustResetPwd == true)
				filterContext.Result = this.RedirectToAction("ChangePassword", "Account");

			if (!this.Request.IsAuthenticated && this.IsAJAX())
				filterContext.Result = this.Json(new { IsError = true, Description = "Your session has come to an end. Please log in again." }, JsonRequestBehavior.AllowGet);

			base.OnAuthorization(filterContext);
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!this.IsAJAX())
			{
				int? userID = null;

				if (this.CurrentUserID > 0)
					userID = this.CurrentUserID;

				string requestedURL = filterContext.RequestContext.HttpContext.Request.Url.ToString();
				string userAgent = filterContext.RequestContext.HttpContext.Request.UserAgent;
				string ipAddress = filterContext.RequestContext.HttpContext.Request.UserHostAddress;

				WebHistory.CaptureWebHistoryRequest(userID, requestedURL, userAgent, ipAddress);
			}
		}

		protected ActionResult ReturnJsonException(Exception ex, string defaultErrorMessage = "An error was detected. Action cancelled.")
		{
			if (ex is EnabillException)
			{
				return this.Json(new
				{
					IsError = true,
					Description = ex.Message
				});
			}

			return this.Json(new
			{
				IsError = true,
				Description = defaultErrorMessage,
				Detail = ex.Message
			});
		}

		protected ActionResult ReturnJsonResult(bool isError, string description) => this.Json(new
		{
			isError,
			description
		});

		protected ActionResult ErrorView(ErrorModel model) => this.View("CustomError", model);

		public void InitialiseLogging() => this.CurrentUser.LogItem = new LogItem
		{
			LogType = LogType.LeaveCycleBalance,
			LogSource = this.ControllerContext.RouteData.Values["Controller"] + "/" + this.ControllerContext.RouteData.Values["Action"],
			LogMessages = new List<LogMessage>(),
			LogDate = DateTime.Now
		};

		public void SaveLogs() => Enabill.Helpers.Log(this.CurrentUser.LogItem);
	}

	public abstract class SearchableController : BaseController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			this.ViewBag.CanSearch = true;
			this.ViewBag.SearchLabelText = this.GetSearchLabelText();
			base.OnActionExecuting(filterContext);
		}

		protected virtual string GetSearchLabelText() => "criteria";

		public abstract ActionResult Index(string q, bool? isActive);

		protected void SaveSearchInput(HistoryItemType item, string value)
		{
			this.ViewBag.SearchInput = value;
			InputHistory.Set(item, value);
		}

		protected void SaveAllInput(HistoryItemType item, string value, HistoryItemType filter, bool isActive)
		{
			this.ViewBag.SearchInput = value;
			InputHistory.Set(item, value);

			if (item == HistoryItemType.ClientSearchCriteria)
				this.ViewBag.isClientActive = isActive;
			else if (item == HistoryItemType.UserSearchCriteria)
				this.ViewBag.isUserActive = isActive;
			else if (item == HistoryItemType.ProjectSearchCriteria)
				this.ViewBag.isProjectActive = isActive;
			else if (item == HistoryItemType.RegionSearchCriteria)
				this.ViewBag.isRegionActive = isActive;

			InputHistory.Set(filter, isActive);
		}
	}
}