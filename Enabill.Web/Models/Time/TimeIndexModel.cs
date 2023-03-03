using System;
using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class TimeIndexModel
	{
		#region INITIALIZATION

		public TimeIndexModel(User user, DateTime workDay)
		{
			this.StandardAmtColumns = 9; // if column are added are removed from the view, change this value
			this.User = user;
			this.WorkDay = workDay;
			this.LoadCalendar(user, workDay);
			this.CanTimesheetLockStatusBeManaged = user.CanTimesheetLockStatusBeManaged(workDay);
			this.IsTimesheetLocked = true;

			if (this.CanTimesheetLockStatusBeManaged)
				this.IsTimesheetLocked = user.IsTimesheetLockedForDate(workDay);

			this.TotlAmtColumns = this.StandardAmtColumns + this.ActivityList.Count;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool CanTimesheetLockStatusBeManaged { get; private set; }
		public bool IsTimesheetLocked { get; private set; }

		public int StandardAmtColumns { get; private set; }
		public int TotlAmtColumns { get; private set; }

		public DateTime WorkDay { get; private set; }

		public Dictionary<int, string> ActivityList { get; private set; }
		public User User { get; private set; }

		public List<TimeCalendarModel> Calendar { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private void LoadCalendar(User user, DateTime workDay)
		{
			this.ActivityList = new Dictionary<int, string>();
			user.GetWorkedActivitiesForDateSpan(workDay.ToFirstDayOfMonth(), workDay.ToLastDayOfMonth())
					.ForEach(a => this.ActivityList.Add(a.ActivityID, string.Format($"{a.ClientName}<br />{a.ProjectName}<br />{a.ActivityName}")));

			this.Calendar = new List<TimeCalendarModel>();
			Enabill.Helpers.GetDaysInMonth(workDay).ForEach(w => this.Calendar.Add(new TimeCalendarModel(user, w, this.ActivityList)));
		}

		#endregion FUNCTIONS
	}
}