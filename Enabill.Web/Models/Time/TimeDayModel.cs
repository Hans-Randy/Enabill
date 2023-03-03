using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class TimeDayModel
	{
		#region INITIALIZATION

		public TimeDayModel(User user, DateTime workDay, string callingPage = "")
		{
			this.User = user;
			this.WorkDay = workDay.Date;
			this.DayWorkSessions = user.GetWorkSessions(this.WorkDay);
			this.Clients = this.LoadClients(user);
			this.WorkAllocations = this.LoadUserWorkAllocations(user, this.WorkDay);

			foreach (var t in this.WorkAllocations.Select(wa => wa.AssociatedProjectTickets))
			{
				this.NrOfAssociatedTickets += t.Count;
			}

			this.HasAssociatedTickets = this.NrOfAssociatedTickets > 0;
			this.ExtraAllocations = this.LoadExtraAllocations(user, this.WorkDay);
			this.SetAllocationSettings(user, this.WorkDay);

			this.IsTimesheetLockedForDay = UserRepo.IsWorkSessionApproved(user.UserID, workDay);
			this.IsLeaveDay = user.IsAnyLeaveTakenOnDate(this.WorkDay);
			this.IsFlexiDay = user.IsFlexiDayTakenOnDate(this.WorkDay);
			this.IsFlexiDayPending = user.IsFlexiDayTakenOnDatePending(this.WorkDay);

			this.TypeOfDay = workDay.DayOfWeek == DayOfWeek.Saturday || workDay.DayOfWeek == DayOfWeek.Sunday ? "" : !workDay.IsWorkableDay() ? "Public Holiday" : this.IsLeaveDay ? "Leave Day" : this.IsFlexiDayPending ? "Flexi Day(authorisation pending)" : this.IsFlexiDay ? "Flexi Day" : "";

			this.CallingPage = callingPage;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool HasAssociatedTickets { get; private set; }
		public bool IsLeaveDay { get; private set; }
		public bool IsFlexiDay { get; private set; }
		public bool IsFlexiDayPending { get; private set; }
		public bool IsTimesheetLockedForDay { get; private set; }

		public int NrOfAssociatedTickets { get; private set; }

		public double AllocationDifference { get; private set; }

		public string AllocationDifferenceErrorClass { get; private set; }
		public string AllocatedString { get; private set; }
		public string CallingPage { get; private set; }
		public string DayIsLockedText { get; private set; }
		public string TypeOfDay { get; private set; }

		public DateTime WorkDay { get; private set; }

		public Activity Activity { get; private set; }
		public User User { get; private set; }

		public List<ActivityDetail> ExtraAllocations { get; private set; }
		public List<Client> Clients { get; private set; }
		public List<WorkAllocationExtendedModel> WorkAllocations { get; private set; }
		public List<WorkSession> DayWorkSessions { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<ActivityDetail> LoadExtraAllocations(User user, DateTime workDay)
		{
			var WAs = user.GetWorkAllocations(workDay);

			if (WAs.Count == 0)
				return new List<ActivityDetail>();

			var result = new List<ActivityDetail>();

			foreach (var activity in user.GetActivities())
			{
				if (WAs.Select(wa => wa.ActivityID).Contains(activity.ActivityID))
				{
					var act = ActivityRepo.GetByID(activity.ActivityID);
					var project = ProjectRepo.GetByID(activity.ProjectID);
					result.Add(new ActivityDetail(act, project, project.GetClient(), false));
				}
			}

			return result;
		}

		private List<Client> LoadClients(User user)
		{
			var clientIDs = new List<int>();

			foreach (var ua in user.GetActivities())
			{
				int clientID = ua.GetActivity().GetProject().ClientID;

				if (!clientIDs.Contains(clientID))
					clientIDs.Add(clientID);
			}

			var clients = new List<Client>();

			foreach (int clientID in clientIDs)
			{
				clients.Add(ClientRepo.GetByID(clientID));
			}

			return clients;
		}

		private void SetAllocationSettings(User user, DateTime workDay)
		{
			double difference = user.GetUnallocatedTime(workDay);
			string hrstext = difference < 2 ? "hr" : "hrs";

			this.AllocationDifferenceErrorClass = "";

			if (difference > 0.00)
			{
				this.AllocatedString = hrstext + " Unallocated";
				this.AllocationDifference = difference;
				this.AllocationDifferenceErrorClass = "timeError";
			}
			else if (difference < 0.00)
			{
				this.AllocatedString = hrstext + " Over Allocated";
				this.AllocationDifference = difference * -1;
				this.AllocationDifferenceErrorClass = "OrangeError";

				return;
			}
		}

		public List<WorkAllocationExtendedModel> LoadUserWorkAllocations(User user, DateTime workDay)
		{
			var workAllocations = new List<WorkAllocationExtendedModel>();
			var activities = user.GetActivities(workDay);
			int count = activities.Count;

			foreach (var activity in activities)
			{
				var actWA = user.GetDetailedWorkAllocationsForActivityForDate(activity.GetActivity(), workDay);

				if (actWA.Count == 0)
					actWA.Add(WorkAllocation.CreateNewExtendedModel(this.User, activity.GetActivity(), workDay));

				actWA.ForEach(wa => workAllocations.Add(wa));
			}

			return workAllocations;
		}

		#endregion FUNCTIONS
	}
}