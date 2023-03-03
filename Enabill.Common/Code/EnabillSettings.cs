using System;
using Enabill.DB;
using Enabill.Models;

namespace Enabill
{
	public static class EnabillSettings
	{
		#region SITE SETTINGS

		public static int StoryWriterProjectID { get; set; }

		#endregion SITE SETTINGS

		#region CONFIGURATION SETTINGS

		private static Action<object> save_;
		private static Func<object> recall_;
		private static Func<EnabillContext> create_;

		public static void Setup(Action<object> save, Func<object> recall, Func<EnabillContext> create)
		{
			save_ = save;
			recall_ = recall;
			create_ = create;
		}

		internal static EnabillContext DB
		{
			get
			{
				if (!(recall_() is EnabillContext context))
				{
					context = new EnabillContext();
					save_(context);
				}
				return context;
			}
		}

		private static Func<EnabillContextSQL> createSQL_;

		public static void Setup(Action<object> save, Func<object> recall, Func<EnabillContextSQL> createSQL)
		{
			save_ = save;
			recall_ = recall;
			createSQL_ = createSQL;
		}

		internal static EnabillContextSQL DBSQL
		{
			get
			{
				if (!(recall_() is EnabillContextSQL context))
				{
					context = new EnabillContextSQL();
					save_(context);
				}
				return context;
			}
		}

		#endregion CONFIGURATION SETTINGS

		#region DOMAIN SETTINGS

		private static DateTime? siteStartDate { get; set; }

		public static DateTime SiteStartDate
		{
			get
			{
				if (siteStartDate == null)
				{
					var date = new DateTime(2011, 10, 1);
					DateTime.TryParse(Code.Constants.SITESTARTDATE, out date);
					siteStartDate = date;
				}
				return siteStartDate.Value;
			}
		}

		public static string DefaultUserPassword => "enA22";

		public static double AnnualLeaveAvailableToStaff => LeaveTypeHistory.GetForDate(LeaveTypeEnum.Annual, DateTime.Today).DaysGivenAnnually;

		//This default VAT rate is the defaulted value used when creating a client,
		//the client from there onwards will manage the VAT rate for its projects
		public static double DefaultVATRate => VATHistory.GetCurrentVATRate();

		//The max hours a user is can be employed to work on a daily basis,
		//currently, 8 hours is the norm, but 16 is the max allowed
		public static double MaxDailyWorkHours => 16;

		public static int MaxFlexiDaysPerMonth => 2;

		//This value is used to prevent users from booking flexidays for days before
		//a certain date, no one should need to go this far back to book a flexiday
		public static DateTime EarliestPossibleFlexiDate
		{
			get
			{
				var cutOffDate = DateTime.Today.ToFirstDayOfMonth().AddMonths(-1);

				if (cutOffDate < SiteStartDate)
					return SiteStartDate;
				return cutOffDate;
			}
		}

		public static DateTime LatestPossibleFlexiDate =>
				// currently set for next week Friday.
				DateTime.Today.ToPreviousDayOfWeek(DayOfWeek.Saturday).AddDays(14);

		#endregion DOMAIN SETTINGS

		#region SITE PRELOADS

		public static User GetSystemUser() => User.GetSystemUser();

		#endregion SITE PRELOADS
	}
}