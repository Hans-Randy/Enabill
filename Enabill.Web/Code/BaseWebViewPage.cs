using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Enabill.Models;

namespace Enabill.Web
{
	public abstract class BaseWebViewPage<TModel> : WebViewPage<TModel>
	{
		public User CurrentUser => Settings.Current.CurrentUser;
		public int CurrentUserID => Settings.Current.CurrentUserID;
		public User ContextUser => Settings.Current.ContextUser;
		public int ContextUserID => Settings.Current.ContextUserID;
		public bool IsLoggedIn => Settings.Current.CurrentUserID > 0;

		public UserPreference CurrentUserPreference => Settings.Current.CurrentUserPreferences;

		public override bool IsAjax => this.Request.IsAjaxRequest();
		public int CurrentPageID => (int)this.ViewContext.RouteData.Values["id"];

		#region VERSION

		public Version AppVersion
		{
			get
			{
				var assm = Assembly.GetExecutingAssembly();

				return assm.GetName().Version;
			}
		}

		#endregion VERSION

		#region ROLE

		/// <summary>
		/// Checks if the current logged in user has the required role
		/// </summary>
		/// <param name="role"></param>
		/// <returns></returns>
		public bool UserHasRole(UserRoleType role)
		{
			if (!this.IsLoggedIn)
				return false;

			return Settings.Current.CurrentUser.HasRole(role);
		}

		#endregion ROLE

		#region VERSIONED URL

		public HtmlString IncludeControllerCSS()
		{
			string result = "<link rel=\"stylesheet\" href=\"" + this.Url.Content("~/Content/Views/" + this.ViewContext.RouteData.Values["controller"] + ".css") + "\"/>\n";
			return new HtmlString(result);
		}

		public HtmlString IncludeControllerCSS(bool useCache)
		{
			string resUrl = this.Url.Content("~/Content/Views/" + this.ViewContext.RouteData.Values["controller"] + ".css");
			string result = $"<link rel=\"stylesheet\" href=\"{HtmlHelpers.GetCacheBustedUrl(resUrl)}\"/>\n";
			return new HtmlString(result);
		}

		public HtmlString IncludeControllerScript()
		{
			string result = "<script type=\"text/javascript\" src=\"" + this.Url.Content("~/Scripts/Views/" + this.ViewContext.RouteData.Values["controller"] + ".js") + "\"></script>";
			return new HtmlString(result);
		}

		public HtmlString IncludeControllerScript(bool useCache)
		{
			string resUrl = this.Url.Content("~/Scripts/Views/" + this.ViewContext.RouteData.Values["controller"] + ".js");
			string result = $"<script type=\"text/javascript\" src=\"{HtmlHelpers.GetCacheBustedUrl(resUrl)}\"></script>";
			return new HtmlString(result);
		}

		public HtmlString IncludeWebix()
		{
			var sb = new StringBuilder();
			string cssUrl = this.Url.Content("~/Content/webix.min.css");
			string jsUrl = this.Url.Content("~/Scripts/webix.min.js");
			sb.Append("<link rel=\"stylesheet\" href=\"").Append(cssUrl).Append("\"/>")
				.Append("<script type=\"text/javascript\" src=\"").Append(jsUrl).Append("\"></script>");

			string s = sb.ToString();
			return new HtmlString(s);
		}

		/// <summary>
		/// Code to be able to generate Cache Busting versions of files
		/// </summary>
		/// <param name="fileType">Either css or js.</param>
		/// <param name="useCache">Return either cached URL's or not.</param>
		/// <param name="includeFolders">Send relative folder paths as a string array. css = Root is the Content folder. js = Root is the Scrips folder.</param>
		/// <param name="loadAsync">True or false.</param>
		/// <param name="fList">The file/folder list as a string array parameter.</param>
		/// <returns>URL as a string, either cache encoded or plain</returns>
		public HtmlString IncludeFiles(string fileType, bool useCache, bool includeFolders = false, bool loadAsync = false, params string[] fList)
		{
			string resUrl = "";
			string result = "";

			if (!includeFolders)
			{
				if (fileType == "css")
				{
					foreach (string file in fList)
					{
						resUrl = "/Content/" + file;
						if (useCache)
							result += $"<link rel=\"stylesheet\" href=\"{HtmlHelpers.GetCacheBustedUrl(resUrl)}\"/>\n";
						else
							result += $"<link rel=\"stylesheet\" href=\"{resUrl}\"/>\n";
					}
				}
				else
				{
					foreach (string file in fList)
					{
						resUrl = "/Scripts/" + file;
						if (useCache)
							result += $"<script type=\"text/javascript\" src=\"{HtmlHelpers.GetCacheBustedUrl(resUrl)}\"></script>\n";
						else
							result += $"<script type=\"text/javascript\" src=\"{resUrl}\"></script>\n";
					}
				}
			}
			else
			{
				string filePath;
				string root;
				string[] files = new string[] { };

				fileType = "*." + fileType;

				if (fileType == "*.css")
				{
					root = "/Content";

					foreach (string folder in fList)
					{
						filePath = HttpContext.Current.Server.MapPath(root + folder);
						files = Directory.GetFiles(filePath, fileType);

						foreach (string file in files)
						{
							string filename = Path.GetFileName(file);
							resUrl = root + folder + filename;
							if (useCache)
								result += $"<link rel=\"stylesheet\" href=\"{HtmlHelpers.GetCacheBustedUrl(resUrl)}\"/>\n";
							else
								result += $"<link rel=\"stylesheet\" href=\"{resUrl}\"/>\n";
						}
					}
				}
				else
				{
					root = "/Scripts";

					foreach (string folder in fList)
					{
						filePath = HttpContext.Current.Server.MapPath(root + folder);
						files = Directory.GetFiles(filePath, fileType);

						foreach (string file in files)
						{
							string filename = Path.GetFileName(file);
							resUrl = root + folder + filename;
							if (useCache)
								result += $"<script type=\"text/javascript\" src=\"{HtmlHelpers.GetCacheBustedUrl(resUrl)}\"></script>\n";
							else
								result += $"<script type=\"text/javascript\" src=\"{resUrl}\"></script>\n";
						}
					}
				}
			}

			return new HtmlString(result);
		}

		#endregion VERSIONED URL

		public MvcHtmlString IncludeRichEditor() => MvcHtmlString.Create("<script type=\"text/javascript\" src=\"" + this.Url.Content("~/Content/ckeditor/ckeditor.js") + "\"></script>");

		public MvcHtmlString IncludeDocumentViewer() => MvcHtmlString.Create("<script type=\"text/javascript\" src=\"" + this.Url.Content("~/Scripts/docuvieware-min.js") + "\"></script>");

		public static MvcHtmlString MenuButton(string buttonText, string action, string controller)
		{
			var sb = new StringBuilder();

			sb.Append("<a class='menuAnchor' href='/").Append(controller).Append("/").Append(action).Append("/'>")
				.Append("<div class='menuText'>").Append(buttonText).Append("</div>")
				.Append("</a>")
				.Append("<div class='menuSeparator'><img src=\"/Content/Img/separator.png\" /></div>");

			return MvcHtmlString.Create(sb.ToString());
		}
	}

	public abstract class BaseWebViewPage : BaseWebViewPage<dynamic>
	{
	}
}