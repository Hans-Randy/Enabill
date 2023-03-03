using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class CommonModelFunctions
	{
		internal static List<ActivitySearchModel> PopulateNoteActivitySearchModelForProjectOwner(User user)
		{
			var model = new List<ActivitySearchModel>();

			foreach (var project in user.GetProjectsForProjectOwner())
			{
				foreach (var activity in project.GetActivities(true))
				{
					model.Add(new ActivitySearchModel(project, activity));
				}
			}

			return model;
		}

		internal static List<ActivitySearchModel> PopulateNoteActivitySearchModelForManager(User user)
		{
			var model = new List<ActivitySearchModel>();

			foreach (var project in user.GetProjectsForProjectOwner())
			{
				foreach (var activity in project.GetActivities(true))
				{
					model.Add(new ActivitySearchModel(project, activity));
				}
			}

			return model;
		}

		internal static List<ActivitySearchModel> PopulateNoteActivitySearchModelForTimeCapturer(User user)
		{
			var model = new List<ActivitySearchModel>();

			foreach (var project in user.GetProjects())
			{
				foreach (var activity in project.GetActivities(true))
				{
					model.Add(new ActivitySearchModel(project, activity));
				}
			}

			return model;
		}

		internal static string SetDayWorkableStatus(User user, DateTime workDay)
		{
			var flexiDay = user.GetFlexiDay(workDay);

			if (flexiDay != null)
				return "This day's been taken as a FlexiDay.";

			var leaveList = user.GetLeave(ApprovalStatusType.Approved, workDay, workDay);

			if (leaveList.Count > 0)
				return "This day's been taken as a leave day.";

			return null;
		}

		internal static bool CheckDayWorkable(string dayUsedForLeaveOrFlexiDay, DateTime workDay)
		{
			if (!string.IsNullOrEmpty(dayUsedForLeaveOrFlexiDay))
				return false;

			if (workDay < EnabillSettings.SiteStartDate)
				return false;

			return true;
		}

		internal static List<UserLeaveModel> LoadLeaveDays(User user, DateTime startDate, DateTime endDate)
		{
			var model = new List<UserLeaveModel>();
			user.GetLeave(startDate, endDate)
					.OrderByDescending(l => l.DateFrom)
					.ThenBy(l => l.DateTo)
					.ToList()
					.ForEach(l => model.Add(new UserLeaveModel(user, l)));

			return model;
		}
	}
}