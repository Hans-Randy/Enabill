using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Enabill.Models;

namespace Enabill.Web
{
	public class Helpers
	{
		private static List<DateTime> workTimes;
		private static DateTime workDate;

		public static SelectList WorkTimeList(DateTime date, List<WorkSession> dayWorkSessions, DateTime? selected, bool isStartTime)
		{
			if (!selected.HasValue)
			{
				var userPreference = Settings.Current.CurrentUserPreferences;

				selected = Settings.Current.CurrentUserPreferences.DefaultWorkSessionStartTime;

				if (!isStartTime)
					selected = Settings.Current.CurrentUserPreferences.DefaultWorkSessionEndTime;
			}

			return WorkTimeList(date, dayWorkSessions, selected);
		}

		public static SelectList WorkTimeList(DateTime date, List<WorkSession> dayWorkSessions, DateTime? selected)
		{
			if (dayWorkSessions == null)
				dayWorkSessions = new List<WorkSession>();

			if (workTimes == null || workDate != date)
			{
				workDate = date;
				workTimes = new List<DateTime>();

				for (int m = 0; m < 1440; m += 15)
				{
					workTimes.Add(date.Date.AddMinutes(m));
				}
			}

			if (selected == null)
			{
				selected = date.AddHours(9);
			}

			if (dayWorkSessions.Count > 0)
			{
				selected = dayWorkSessions.Max(ws => ws.EndTime).AddMinutes(15);
			}

			var tl = from wt in workTimes
					 select new
					 {
						 value = wt,
						 text = wt.ToString("HH:mm")
					 };

			return new SelectList(tl, "value", "text", selected);
		}

		public static Dictionary<int, int> GetDropDownOfNumbers(int startingValue, int endingValue)
		{
			var model = new Dictionary<int, int>();

			for (int k = startingValue; k <= endingValue; k++)
			{
				model.Add(k, k);
			}

			return model;
		}

		internal static Dictionary<DateTime, string> GetDropDownOfMonths(DateTime dateFrom, DateTime dateTo)
		{
			var model = new Dictionary<DateTime, string>();

			for (var k = dateFrom.ToFirstDayOfMonth(); k <= dateTo.ToLastDayOfMonth(); k = k.AddMonths(1))
			{
				model.Add(k, k.Year + " " + k.ToMonthName());
			}

			return model;
		}

		public static string HashedString(string s) => Enabill.Helpers.HashSha512(s);

		#region PASSPHRASE FUNCTIONS

		internal static bool ConfirmPassphraseIsValid(string passphrase) => Enabill.Helpers.ConfirmPassphraseIsValid(passphrase);

		#endregion PASSPHRASE FUNCTIONS
	}
}