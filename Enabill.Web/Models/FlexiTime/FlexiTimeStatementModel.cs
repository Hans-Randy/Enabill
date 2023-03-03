using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Web.Models
{
	public class FlexiTimeStatementModel
	{
		#region INITIALIZATION

		public FlexiTimeStatementModel(User user)
		{
			this.RecentWorkSessions = this.LoadRecentWorkSessions(user);
			this.CurrentFlexiBalance = new CurrentFlexiBalanceModel(user);
			this.UserLeave = this.LoadUserLeave(user);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public DateTime LeaveDateFrom { get; private set; }
		public DateTime LeaveDateTo { get; private set; }

		public CurrentFlexiBalanceModel CurrentFlexiBalance { get; private set; }

		public List<TimeCalendarModel> RecentWorkSessions { get; private set; }
		public List<UserLeaveModel> UserLeave { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public List<TimeCalendarModel> LoadRecentWorkSessions(User user)
		{
			//14Days of worksession
			var model = new List<TimeCalendarModel>();

			foreach (var day in Enabill.Helpers.GetDaysInDateSpan(DateTime.Today.AddDays(-20), DateTime.Now))
			{
				model.Add(new TimeCalendarModel(user, day, null));
			}

			return model;
		}

		private List<NoteDetailModel> LoadNotes(User user)
		{
			var model = new List<NoteDetailModel>();

			model.AddRange(user.GetDetailedNotes(DateTime.Today.AddDays(-14), DateTime.Now));

			return model;
		}

		private List<UserLeaveModel> LoadUserLeave(User user)
		{
			this.LeaveDateFrom = DateTime.Today;
			this.LeaveDateTo = this.LeaveDateFrom.AddDays(21);

			var model = new List<UserLeaveModel>();
			user.GetLeave(this.LeaveDateFrom, this.LeaveDateTo).Where(l => l.ApprovalStatus == (int)ApprovalStatusType.Approved || l.ApprovalStatus == (int)ApprovalStatusType.Pending).OrderBy(l => l.DateFrom).ThenBy(l => l.DateTo).ToList().ForEach(l => model.Add(new UserLeaveModel(user, l)));

			return model;
		}

		#endregion FUNCTIONS
	}
}