using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Enabill.Web
{
	public static class Extensions
	{
		#region DATETIME EXTENTIONS

		public static DateTime ToFirstDayOfMonth(this DateTime date) => Enabill.Extensions.ToFirstDayOfMonth(date);

		public static DateTime ToLastDayOfMonth(this DateTime date) => Enabill.Extensions.ToLastDayOfMonth(date);

		public static int WeekNo(this DateTime day) => (day.DayOfYear / 7) + 1;

		public static DateTime? ToDate(this string date)
		{
			if (DateTime.TryParseExact(date, "yyyy-MM-dd", null, DateTimeStyles.None, out var res))
				return res;
			else
				if (DateTime.TryParse(date, out res))
				return res;

			return null;
		}

		public static DateTime ToDateFromPeriod(this int period) => Enabill.Extensions.ToDateFromPeriod(period);

		public static DateTime FirstDayOfPeriod(this int period) => Enabill.Extensions.FirstDayOfPeriod(period);

		public static DateTime LastDayOfPeriod(this int period) => Enabill.Extensions.LastDayOfPeriod(period);

		public static string ToDisplayString(this DateTime? date)
		{
			if (date == null)
				return string.Empty;

			return date.Value.ToString(Settings.DateDisplayFormat);
		}

		public static string ToDisplayString(this DateTime date) => date.ToString(Settings.DateDisplayFormat);
		public static string ToShortDisplayString(this DateTime date) => date.ToString(Settings.ShortDateDisplayFormat);
		public static string ToHolidayDateString(this DateTime date) => date.ToString("MMM dd ddd");

		public static string ToLongDisplayString(this DateTime? date)
		{
			if (date == null)
				return string.Empty;

			return date.Value.ToString(Settings.LongDateDisplayFormat);
		}

		public static string ToLongDisplayString(this DateTime date) => date.ToString(Settings.LongDateDisplayFormat);

		public static string ToFullDateDisplayString(this DateTime date) => date.ToString(Settings.FullDateDisplayFormat);

		public static string ToExceptionDisplayString(this DateTime date) => Enabill.Extensions.ToExceptionDisplayString(date);

		public static bool IsInSameMonth(this DateTime firstDate, DateTime secondDate) => Enabill.Extensions.IsInSameMonth(firstDate, secondDate);

		#endregion DATETIME EXTENTIONS

		#region INT EXTENTIONS

		public static int ToPeriod(this DateTime date) => (date.Year * 100) + date.Month;

		public static int ToPeriod(this string dateString)
		{
			var date = DateTime.Today;
			DateTime.TryParse(dateString, out date);

			return date.ToPeriod();
		}

		public static int? ToInt(this string intString)
		{
			if (int.TryParse(intString, out int i))
				return i;
			else
				return null;
		}

		public static bool? ToBool(this string intString)
		{
			if (intString == "true,false")
				intString = intString.Substring(0, 4);

			if (bool.TryParse(intString, out bool i))
				return i;
			else
				return null;
		}

		public static List<int> ToIntArray(this string s) => Enabill.Extensions.ToIntArray(s);

		#endregion INT EXTENTIONS

		#region DOUBLE EXTENTIONS

		public static double? ToDouble(this string dbl)
		{
			if (double.TryParse(dbl, out double res))
				return res;
			else
				return null;
		}

		#endregion DOUBLE EXTENTIONS

		#region STRING EXTENTIONS

		public static string ToMonthName(this DateTime date) => ToMonthName(date.Month);

		public static string ToMonthName(this int monthInt) => Enabill.Extensions.ToMonthName(monthInt);

		public static string ToShortMonthName(this int monthInt) => Enabill.Extensions.ToShortMonthName(monthInt);

		public static string ToDoubleString(this double? value, bool forceDecimalValues = true)
		{
			if (!value.HasValue)
				return "0.00";

			return ToDoubleString(value.Value, forceDecimalValues);
		}

		public static string ToDoubleString(this double value, bool forceDecimalValues = true)
		{
			if (!forceDecimalValues && value == (int)value)
				return ((int)value).ToString();

			return value.ToString("#,###,##0.00");
		}

		#endregion STRING EXTENTIONS

		#region MVC HTML STRING EXTENTIONS

		public static MvcHtmlString CleanLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
		{
			string lbl = ExpressionHelper.GetExpressionText(expression);

			if (lbl.EndsWith("ID"))
				lbl = lbl.Remove(lbl.Length - 2, 2);

			return lbl.SplitOnCaps();
		}

		private const char SPACE = ' ';

		public static MvcHtmlString SplitOnCaps(this MvcHtmlString value) => value.ToString().SplitOnCaps();

		public static MvcHtmlString SplitOnCaps(this string value)
		{
			var words = Regex.Matches(value, @"[A-Z][a-z\d]*");
			string s = string.Empty;

			for (int i = 0; i < words.Count; i++)
				s += words[i].Value + SPACE;

			return MvcHtmlString.Create(s.Trim());
		}

		public static MvcHtmlString Tabs(this HtmlHelper html, string id, params TabPage[] tabs)
		{
			var sb = new StringBuilder();

			sb.Append(@"<div id=""").Append(id).Append(@"""><ul>");

			for (int i = 0; i < tabs.Length; i++)
			{
				if (tabs[i] == null)
					continue;
				sb.Append(@"<li><a href=""#tabs-").Append(i).Append(@""" onclick='Tabs.Tab").Append(i).Append("(); return false;'>").Append(tabs[i].Title).Append("</a></li>");
			}

			sb.Append("</ul>");

			bool includeWrapper;

			for (int i = 0; i < tabs.Length; i++)
			{
				if (tabs[i] == null)
					continue;
				sb.Append(@"<div id=""tabs-").Append(i).Append(@""">");

				if (!string.IsNullOrWhiteSpace(tabs[i].PartialViewName))
				{
					includeWrapper = !string.IsNullOrWhiteSpace(tabs[i].WrapperID);

					if (includeWrapper)
						sb.Append("<div id=\"").Append(tabs[i].WrapperID).Append("\">");

					sb.Append(html.Partial(tabs[i].PartialViewName, tabs[i].Model).ToString());

					if (includeWrapper)
						sb.Append("</div>");
				}
				else
				{
					sb.Append(tabs[i].Body);
				}

				sb.Append("</div>");
			}

			sb.Append("</div>");

			return MvcHtmlString.Create(sb.ToString());
		}

		/// <summary>
		/// Creates an ajaxable adding and listing section
		/// </summary>
		/// <param name="html"></param>
		/// <param name="controller">The controller in which the actions are located</param>
		/// <param name="createAction">The action to render the create content</param>
		/// <param name="editAction"></param>
		/// <param name="saveEditAction"></param>
		/// <param name="listAction">The action to list the data</param>
		/// <param name="deleteAction"></param>
		/// <param name="extraData">Any extra data sent up with the list and create actions</param>
		/// <returns></returns>
		public static MvcHtmlString AjaxCreateList(this HtmlHelper html, string controller,
			string createAction = "Create",
			string editAction = "Edit",
			string saveEditAction = "SaveEdit",
			string listAction = "Index",
			string deleteAction = "Delete",
			object extraData = null)
		{
			var sb = new StringBuilder(512);

			sb.Append(@"<a id=""ajax-create-anchor"" href=""#"" onclick=""Crud.create();return false;"">Create new</a>");

			if (extraData != null)
			{
				sb.Append(@"<form id=""ajax-data"">");
				var t = extraData.GetType();

				foreach (var prop in t.GetProperties())
				{
					sb.Append(@"<input type=""hidden"" name=""").Append(prop.Name).Append(@""" value=""").Append(prop.GetValue(extraData, null)).Append(@""" />");
				}

				sb.Append("</form>");
			}

			sb.Append(@"<input type=""hidden"" id=""create-url"" value=""/").Append(controller).Append("/").Append(createAction).Append(@""" />")
				.Append(@"<input type=""hidden"" id=""edit-url"" value=""/").Append(controller).Append("/").Append(editAction).Append(@""" />")
				.Append(@"<input type=""hidden"" id=""saveEdit-url"" value=""/").Append(controller).Append("/").Append(saveEditAction).Append(@""" />")
				.Append(@"<input type=""hidden"" id=""list-url"" value=""/").Append(controller).Append("/").Append(listAction).Append(@""" />")
				.Append(@"<input type=""hidden"" id=""delete-url"" value=""/").Append(controller).Append("/").Append(deleteAction).Append(@""" />")
				.Append(@"<div id=""ajax-create""></div><div id=""ajax-list""></div>");

			return MvcHtmlString.Create(sb.ToString());
		}

		#endregion MVC HTML STRING EXTENTIONS

		#region DISK FILE STORE

		public static DataTable ToADOTable<T>(this IEnumerable<T> varlist)
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

		#endregion DISK FILE STORE
	}
}