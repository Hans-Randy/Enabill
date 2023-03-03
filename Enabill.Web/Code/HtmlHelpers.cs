using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Enabill.Models;

namespace Enabill.Web
{
	public static class HtmlHelpers
	{
		/// <summary>
		/// This function to get the version out from the date the file is save.
		/// </summary>
		/// <param name="absolutePath"></param>
		/// <returns>
		/// Return a unique version string
		/// </returns>
		private static string GetVersionNumber(string absolutePath)
		{
			//If using the date instead of the assembly number. See below for using assembly number instead.
			var date = File.GetLastWriteTime(absolutePath);
			return date.Ticks.ToString();

			//If using the assembly number instead of the date. See above for using date instead.
			//string version = typeof(HtmlHelpers).Assembly.GetName().Version.ToString();
			//version = version.Replace(".", "");
		}

		// When using the assembly number
		// string Version = typeof(HtmlHelpers).Assembly.GetName().Version.ToString();

		/// <summary>
		/// This function is only making the a prefix that can be added for generating a unique URL
		/// </summary>
		/// <param name="resUrl"></param>
		/// <returns>
		/// Return a unique string for versioning the URL e.g. "/path/*getPrefix*-main.js"
		/// </returns>
		public static string GetCacheBustedUrl(string resUrl)
		{
			try
			{
				if (HttpRuntime.Cache[resUrl] == null)
				{
					string absolutePath = HttpContext.Current.Server.MapPath(resUrl);
					string version = GetVersionNumber(absolutePath);
					string filePath = HttpContext.Current.Server.MapPath(resUrl);
					string fileName = Path.GetFileName(filePath);
					string relativePath = resUrl.Replace(fileName, "");
					//string tempString = "";

					//tempString = relativePath.Substring(GetNthIndex(relativePath, '/', 1), GetNthIndex(relativePath, '/', 2) + 1);

					//string cacheBustedUrl = $"{tempString}v-{version}/";

					//tempString = relativePath.Replace(tempString, "");

					string cacheBustedUrl = $"{relativePath}v-{version}/";
					//cacheBustedUrl += tempString + fileName;
					cacheBustedUrl += fileName;

					HttpRuntime.Cache.Insert(resUrl, cacheBustedUrl, new CacheDependency(absolutePath));
				}
			}
			catch
			{
				return "";
			}

			return HttpRuntime.Cache[resUrl] as string;
		}

		public static int GetNthIndex(string s, char t, int n)
		{
			int count = 0;
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == t)
				{
					count++;
					if (count == n)
					{
						return i;
					}
				}
			}

			return -1;
		}

		public static MvcHtmlString CollapsableTable(this HtmlHelper helper, string id, string className, string title, string partialViewName, object model, bool collapsed = false) => CollapsableTable(helper, id, className, title, partialViewName, model, null, collapsed);

		public static MvcHtmlString CollapsableTable(this HtmlHelper helper, string id, string className, string title, string partialViewName, object model, string imageLocation, bool collapsed = false)
		{
			var sb = new StringBuilder();

			sb.AppendFormat($"<table id=\"{id}\" class=\"{className} collapseTable\">").AppendLine()
				.AppendLine("<tr class=\"titleRow\">");

			if (string.IsNullOrEmpty(title))
				title = string.Empty;

			if (!string.IsNullOrEmpty(imageLocation))
				sb.AppendFormat($"<th class=\"iconCell vCenter\"><img class=\"headerIcon\" src=\"{imageLocation}\" /></th>").AppendLine();
			else
				sb.AppendLine("<th class=\"noCell\"></th>");

			sb.AppendFormat($"<th class=\"titleCell\">{title}</th>").AppendLine()
				.AppendFormat($"<th class=\"vCenter toggleCell\"><img class=\"toggleImage\" src=\"../../Content/Img/{(collapsed ? "maximize" : "minimize")}.png\" onclick=\"CollapsableTable.toggle($('#{id}')); return false;\" /></th>").AppendLine()
				.AppendFormat($"</tr><tr class=\"expanded\" style=\"{(collapsed ? "display: none;" : string.Empty)}\">").AppendLine()
				.AppendFormat($"<td colspan=\"3\">{helper.Partial(partialViewName, model)}</td>").AppendLine()
				.AppendLine("</tr>")
				.Append("<tr class=\"collapsed\" style=\"").Append(!collapsed ? "display: none;" : string.Empty).AppendLine("\" ><td colspan=\"3\"></td></tr>")
				.AppendLine("</table>");

			return MvcHtmlString.Create(sb.ToString());
		}

		public static MvcHtmlString CollapsableHomeTable(this HtmlHelper helper, string id, string className, string title, string partialViewName, object model, string imageLocation, bool collapsed = false, CollapseColumnType collapseTableType = CollapseColumnType.CollapseMyTimesheet)
		{
			var sb = new StringBuilder();

			sb.AppendFormat($"<table id=\"{id}\" class=\"{className} collapseTable\">").AppendLine()
				.AppendLine("<tr class=\"titleRow\">");

			if (string.IsNullOrEmpty(title))
				title = string.Empty;

			if (!string.IsNullOrEmpty(imageLocation))
				sb.AppendFormat($"<th class=\"iconCell vCenter\"><img class=\"headerIcon\" src=\"{imageLocation}\" /></th>").AppendLine();
			else
				sb.AppendLine("<th class=\"noCell\"></th>");

			sb.AppendFormat($"<th class=\"titleCell\">{title}</th>").AppendLine()
				.AppendFormat($"<th class=\"vCenter toggleCell\"><img class=\"toggleImage\" src=\"../../Content/Img/{(collapsed ? "maximize" : "minimize")}.png\" onclick=\"IndexTables.toggle($('#{id}'), {(int)collapseTableType}); return false;\" /></th>").AppendLine()
				.AppendFormat($"</tr><tr class=\"expanded\" style=\"{(collapsed ? "display: none;" : string.Empty)}\">").AppendLine()
				.AppendFormat($"<td colspan=\"3\">{helper.Partial(partialViewName, model)}</td>").AppendLine()
				.AppendLine("</tr>")
				.Append("<tr class=\"collapsed\" style=\"").Append(!collapsed ? "display: none;" : string.Empty).AppendLine("\" ><td colspan=\"3\"></td></tr>")
				.AppendLine("</table>");

			return MvcHtmlString.Create(sb.ToString());
		}

		public static MvcHtmlString TitleTable(this HtmlHelper helper, string id, string className, string title) => TitleTable(helper, id, className, title, null);

		public static MvcHtmlString TitleTable(this HtmlHelper helper, string id, string className, string title, string imageLocation) => TitleTable(helper, id, className, title, imageLocation, null, null, null, null, null, null);

		public static MvcHtmlString TitleTable(this HtmlHelper helper, string id, string className, string title, string imageLocation, string col3PartialView, object col3Model, string col4PartialView, object col4Model, string col5PartialView, object col5Model)
		{
			var s = new StringBuilder();

			s.AppendFormat($"<table id=\"{id}\" class=\"{className} headingTable\">").AppendLine()
				.AppendLine("<tr>")
				.AppendFormat($"<td class=\"{className} headingTableTitle\">").AppendLine();

			//Col1

			if (!string.IsNullOrEmpty(imageLocation))
			{
				s.AppendLine("<span class=\"imageWithText\">")
					.AppendFormat($"<img src=\"{imageLocation}\">").AppendLine()
					.AppendLine("</span>");
			}

			s.AppendLine(title)
				.AppendLine("</td>")
				.AppendFormat($"<td class=\"{className} headingTableRoundingImage\">").AppendLine()
				.AppendLine("&nbsp;</td>");

			//Col 2 - just the image

			//Col 3
			if (!string.IsNullOrEmpty(col3PartialView) && col3Model != null)
				s.AppendFormat($"<td class=\"headingTableCol3\">{helper.Partial(col3PartialView, col3Model)}</td>").AppendLine();
			else
				s.AppendLine("<td class=\"headingTableCol3\"></td>");

			//Col 4
			if (!string.IsNullOrEmpty(col4PartialView) && col4Model != null)
				s.AppendFormat($"<td class=\"headingTableCol4\">{helper.Partial(col4PartialView, col4Model)}</td>").AppendLine();
			else
				s.AppendLine("<td class=\"headingTableCol4\"></td>");

			//Col 5
			if (!string.IsNullOrEmpty(col5PartialView) && col5Model != null)
				s.AppendFormat($"<td class=\"headingTableCol5\">{helper.Partial(col5PartialView, col5Model)}</td>").AppendLine();
			else
				s.AppendLine("<td class=\"headingTableCol5\"></td>");

			s.AppendLine("<td>")
				.AppendLine(helper.Partial("ucSearch").ToString())
				.AppendLine("<td>")
				.AppendLine("</tr>")
				.AppendLine("</table>");

			return new MvcHtmlString(s.ToString());
		}
	}
}