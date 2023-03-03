using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Enabill.Repos;

namespace Enabill
{
	public static class Extensions
	{
		#region DATETIME

		public static DateTime ToFirstDayOfMonth(this DateTime date) => new DateTime(date.Year, date.Month, 1);

		public static DateTime ToLastDayOfMonth(this DateTime date) => date.ToFirstDayOfMonth().AddMonths(1).AddDays(-1);

		public static string ToLastDayOfMonthFromPeriod(this int? period) => DateTime.DaysInMonth(period.Value.GetYear(), period.Value.GetMonth()).ToString();

		public static DateTime ToCentralAfricanTime(this DateTime date) => date.ToUniversalTime().AddHours(2);

		public static DateTime ToPreviousDayOfWeek(this DateTime date, DayOfWeek dow)
		{
			while (date.DayOfWeek != dow)
			{
				date = date.AddDays(-1);
			}

			return date;
		}

		public static string ToExceptionDisplayString(this DateTime date) => date.ToString("dd-MMM-yyyy");

		public static bool IsInSameMonth(this DateTime firstDate, DateTime secondDate) => firstDate.Year == secondDate.Year && firstDate.Month == secondDate.Month;

		public static bool IsFirstDayOfMonth(this DateTime date) => date.Day == 1;

		public static bool IsLastDayOfMonth(this DateTime date) => date.AddDays(1).Day == 1;

		public static bool IsInCurrentMonth(this DateTime date) => date.IsInSameMonth(DateTime.Now);

		public static bool IsInTheFuture(this DateTime date) => date > DateTime.Now;

		public static bool IsInFutureMonth(this DateTime date) => !date.IsInCurrentMonth() && date > DateTime.Today;

		public static bool IsInFutureYear(this DateTime date) => date.Year > DateTime.Today.Year;

		public static bool IsInThePast(this DateTime date) => date < DateTime.Now;

		public static bool IsInPastMonth(this DateTime date) => !date.IsInCurrentMonth() && date < DateTime.Today;

		public static bool IsInPastYear(this DateTime date) => date.Year < DateTime.Today.Year;

		public static string ToMonthName(this DateTime date) => date.Month.ToMonthName();

		public static string ToMonthName(this int monthInt)
		{
			switch (monthInt)
			{
				case 1:
					return "January";

				case 2:
					return "February";

				case 3:
					return "March";

				case 4:
					return "April";

				case 5:
					return "May";

				case 6:
					return "June";

				case 7:
					return "July";

				case 8:
					return "August";

				case 9:
					return "September";

				case 10:
					return "October";

				case 11:
					return "November";

				case 12:
					return "December";

				default:
					return "Invalid";
			}
		}

		public static string ToShortMonthName(this int monthInt)
		{
			switch (monthInt)
			{
				case 1:
					return "Jan";

				case 2:
					return "Feb";

				case 3:
					return "Mar";

				case 4:
					return "Apr";

				case 5:
					return "May";

				case 6:
					return "Jun";

				case 7:
					return "Jul";

				case 8:
					return "Aug";

				case 9:
					return "Sep";

				case 10:
					return "Oct";

				case 11:
					return "Nov";

				case 12:
					return "Dec";

				default:
					return "Invalid";
			}
		}

		#endregion DATETIME

		#region DOMAIN RELATED

		//Convert a date to a Integer period eg: 2011/02/15 = 201102.
		public static int ToPeriod(this DateTime date) => (date.Year * 100) + date.Month;

		public static bool IsValidPeriod(this int period)
		{
			if (period < EnabillSettings.SiteStartDate.ToPeriod())
				return false;

			if (period > DateTime.Now.AddYears(1).ToPeriod())
				return false;

			if (period.GetMonth() > 12 || period.GetMonth() < 1)
				return false;

			return true;
		}

		public static int GetYear(this int period)
		{
			double a = period / 100;

			return (int)a;
		}

		public static int GetMonth(this int period)
		{
			double a = period / 100;
			int b = (int)a;
			int c = b * 100;

			return period - c;
		}

		public static DateTime ToDateFromPeriod(this int period)
		{
			if (!period.IsValidPeriod())
				return DateTime.Today.ToFirstDayOfMonth();

			int year = period.GetYear();
			int month = period.GetMonth();

			return new DateTime(year, month, 1);
		}

		public static DateTime FirstDayOfPeriod(this int period)
		{
			if (!period.IsValidPeriod())
				return DateTime.Today.ToFirstDayOfMonth();

			int year = period.GetYear();
			int month = period.GetMonth();

			return new DateTime(year, month, 1);
		}

		public static DateTime LastDayOfPeriod(this int period)
		{
			if (!period.IsValidPeriod())
				return DateTime.Today.ToLastDayOfMonth();

			int year = period.GetYear();
			int month = period.GetMonth();

			return new DateTime(year, month, 1).ToLastDayOfMonth();
		}

		public static bool IsWorkableDay(this DateTime date) => WorkDayRepo.IsDayWorkable(date);

		#endregion DOMAIN RELATED

		#region ARRAYS

		public static List<string> ToStringArray(this string ids)
		{
			var intArray = new List<string>();

			if (!string.IsNullOrEmpty(ids))
			{
				foreach (string id in ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
					intArray.Add(id);
			}

			return intArray;
		}

		public static List<int> ToIntArray(this string ids)
		{
			var intArray = new List<int>();

			if (!string.IsNullOrEmpty(ids))
			{
				foreach (string id in ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					if (int.TryParse(id, out int ConfirmedInt))
						intArray.Add(ConfirmedInt);
				}
			}

			return intArray;
		}

		public static List<Guid> ToGuidArray(this string ids)
		{
			var guidArray = new List<Guid>();

			if (!string.IsNullOrEmpty(ids))
			{
				foreach (string id in ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					if (Guid.TryParse(id, out var ConfirmedGuid))
						guidArray.Add(ConfirmedGuid);
				}
			}

			return guidArray;
		}

		#endregion ARRAYS

		#region ENUM

		public static List<SelectListItem> GetEnumSelectList<T>()
		{
			var enums = new List<SelectListItem>();
			var type = typeof(T);

			foreach (int value in Enum.GetValues(type))
			{
				var e = (T)Enum.Parse(typeof(T), value.ToString(), true);

				enums.Add
					(
						new SelectListItem()
						{
							Value = value.ToString(),
							Text = GetEnumDescription(e)
						}
					);
			}

			return enums;
		}

		public static string GetEnumDescription<T>(this T element)
		{
			var type = element.GetType();

			var memberInfo = type.GetMember(element.ToString());

			if (memberInfo?.Length > 0)
			{
				object[] attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

				if (attributes?.Length > 0)
				{
					return ((DescriptionAttribute)attributes[0]).DescriptionValue;
				}
			}

			return element.ToString();
		}

		public static string GetEnumDescriptionById(this Enum value)
		{
			var fi = value.GetType().GetField(value.ToString());

			var attributes =
				(
					DescriptionAttribute[])fi.GetCustomAttributes(
					typeof(DescriptionAttribute),
					false
				);

			if (attributes?.Length > 0)
				return attributes[0].DescriptionValue;
			else
				return value.ToString();
		}

		public static T GetEnumIdByDescription<T>(string description)
		{
			var type = typeof(T);

			if (!type.IsEnum)
				throw new InvalidOperationException();

			foreach (var field in type.GetFields())
			{
				if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
				{
					if (attribute.DescriptionValue == description)
						return (T)field.GetValue(null);
				}
				else
				{
					if (field.Name == description)
						return (T)field.GetValue(null);
				}
			}
			throw new ArgumentException("Not found.", nameof(description));
			// or return default(T);
		}

		#endregion ENUM
	}

	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	/// <summary>
	/// This attribute is used to represent a string value
	/// for a value in an enum.
	/// </summary>
	public class DescriptionAttribute : Attribute
	{
		#region Properties

		/// <summary>
		/// Holds the stringvalue for a value in an enum.
		/// </summary>
		public string DescriptionValue { get; protected set; }

		#endregion Properties

		#region Constructor

		/// <summary>
		/// Constructor used to init a StringValue Attribute
		/// </summary>
		/// <param name="value"></param>
		public DescriptionAttribute(string value)
		{
			this.DescriptionValue = value;
		}

		#endregion Constructor
	}

	public static class DescriptionExtention
	{
		/// <summary>
		/// Will get the string value for a given enums value, this will
		/// only work if you assign the StringValue attribute to
		/// the items in your enum.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>

		public static string GetDescription(this Enum value)
		{
			// Get the type
			var type = value.GetType();

			// Get fieldinfo for this type
			var fieldInfo = type.GetField(value.ToString());

			// Get the stringvalue attributes
			var attribs = fieldInfo.GetCustomAttributes(
				typeof(DescriptionAttribute), false) as DescriptionAttribute[];

			// Return the first if there was a match.
			return attribs.Length > 0 ? attribs[0].DescriptionValue : null;
		}
	}
}