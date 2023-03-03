using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public class UserAllocationRepo : BaseRepo
	{
		#region USER ALLOCATION

		internal static UserAllocation GetByID(int userAllocationID) => DB.UserAllocations
					.SingleOrDefault(ua => ua.UserAllocationID == userAllocationID);

		internal static bool IsActivityHidden(int activityID, int userID) => DB.UserAllocations
			.Where(ua => ua.ActivityID == activityID && ua.UserID == userID)
			.Select(ua => ua.IsHidden)
			.FirstOrDefault();

		internal static IEnumerable<UserAllocation> GetAllForActivityForUser(int activityID, int userID) => DB.UserAllocations
					.Where(ua => ua.ActivityID == activityID && ua.UserID == userID);

		internal static UserAllocation GetForActivityForUserForDate(int activityID, int userID, DateTime refDate) => DB.UserAllocations
					.SingleOrDefault(ua => ua.ActivityID == activityID && ua.UserID == userID && ua.StartDate <= refDate && refDate <= (ua.ConfirmedEndDate ?? refDate));

		internal static void Save(UserAllocation ua)
		{
			if (ua.UserAllocationID == 0)
				DB.UserAllocations.Add(ua);
			else
				DB.Entry(ua).State = EntityState.Modified;

			DB.SaveChanges();
		}

		internal static IEnumerable<ActivityUserDetail> GetUsersAssignedModel(int activityID, DateTime refDate) => from ua in DB.UserAllocations
																												   join u in DB.Users on ua.UserID equals u.UserID
																												   join a in DB.Activities on ua.ActivityID equals a.ActivityID
																												   where ua.ActivityID == activityID && ua.StartDate <= refDate && refDate <= (ua.ConfirmedEndDate ?? refDate)
																												   select new ActivityUserDetail
																												   {
																													   UserAllocationID = ua.UserAllocationID,
																													   ActivityID = activityID,
																													   ActivityName = a.ActivityName,
																													   UserID = ua.UserID,
																													   ChargeRate = ua.ChargeRate,
																													   StartDate = ua.StartDate,
																													   ScheduledEndDate = ua.ScheduledEndDate,
																													   ConfirmedEndDate = ua.ConfirmedEndDate,
																													   UserFullName = u.FullName
																												   }
												   ;

		internal static IEnumerable<ActivityUserDetail> GetUsersAssignedModel(int activityID, DateTime fromDate, DateTime toDate) => (from ua in DB.UserAllocations
																																	  join u in DB.Users on ua.UserID equals u.UserID
																																	  join a in DB.Activities on ua.ActivityID equals a.ActivityID
																																	  where ua.ActivityID == activityID && ua.StartDate <= toDate && ((ua.ConfirmedEndDate.HasValue && ua.ConfirmedEndDate >= fromDate) || (!ua.ConfirmedEndDate.HasValue))
																																	  select new ActivityUserDetail
																																	  {
																																		  UserAllocationID = ua.UserAllocationID,
																																		  ActivityID = activityID,
																																		  ActivityName = a.ActivityName,
																																		  UserID = ua.UserID,
																																		  ChargeRate = ua.ChargeRate,
																																		  StartDate = ua.StartDate,
																																		  ScheduledEndDate = ua.ScheduledEndDate,
																																		  ConfirmedEndDate = ua.ConfirmedEndDate,
																																		  UserFullName = u.FullName
																																	  }
												   )
												   .Distinct();

		internal static IEnumerable<ActivityUserDetail> GetPastUsersAssignedModel(int activityID, DateTime dateTo) => (from ua in DB.UserAllocations
																													   join u in DB.Users on ua.UserID equals u.UserID
																													   join a in DB.Activities on ua.ActivityID equals a.ActivityID
																													   where ua.ActivityID == activityID && ua.StartDate < dateTo && dateTo > (ua.ConfirmedEndDate ?? dateTo)
																													   select new ActivityUserDetail
																													   {
																														   UserAllocationID = ua.UserAllocationID,
																														   ActivityID = activityID,
																														   ActivityName = a.ActivityName,
																														   UserID = ua.UserID,
																														   ChargeRate = ua.ChargeRate,
																														   StartDate = ua.StartDate,
																														   ScheduledEndDate = ua.ScheduledEndDate,
																														   ConfirmedEndDate = ua.ConfirmedEndDate,
																														   UserFullName = u.FullName
																													   }
												   )
												   .Distinct();

		internal static IEnumerable<ActivityUserDetail> GetFutureUsersAssignedModel(int activityID, DateTime dateFrom) => (from ua in DB.UserAllocations
																														   join u in DB.Users on ua.UserID equals u.UserID
																														   join a in DB.Activities on ua.ActivityID equals a.ActivityID
																														   where ua.ActivityID == activityID && ua.StartDate > dateFrom
																														   select new ActivityUserDetail
																														   {
																															   UserAllocationID = ua.UserAllocationID,
																															   ActivityID = activityID,
																															   ActivityName = a.ActivityName,
																															   UserID = ua.UserID,
																															   ChargeRate = ua.ChargeRate,
																															   StartDate = ua.StartDate,
																															   ScheduledEndDate = ua.ScheduledEndDate,
																															   ConfirmedEndDate = ua.ConfirmedEndDate,
																															   UserFullName = u.FullName
																														   }
												   )
												   .Distinct();

		internal static IEnumerable<ActivityUserDetail> GetUsersNotAssignedModel(int activityID, DateTime refDate) => (from ua in DB.UserAllocations
																													   join tU in DB.Users on ua.UserID equals tU.UserID into tempUser
																													   from u in tempUser.DefaultIfEmpty()
																													   join a in DB.Activities on ua.ActivityID equals a.ActivityID
																													   where ua.ActivityID == activityID && ua.StartDate >= refDate && refDate <= (ua.ConfirmedEndDate ?? refDate)
																													   select new ActivityUserDetail
																													   {
																														   UserAllocationID = 0,
																														   ActivityID = activityID,
																														   ActivityName = a.ActivityName,
																														   UserID = ua.UserID,
																														   ChargeRate = ua.ChargeRate,
																														   StartDate = ua.StartDate,
																														   ScheduledEndDate = ua.ScheduledEndDate,
																														   ConfirmedEndDate = ua.ConfirmedEndDate,
																														   UserFullName = u.FullName
																													   }
											)
											.OrderBy(a => a.UserFullName)
											.ToList();

		public static IEnumerable<UserAllocationExtendedModel> GetUserAllocationExtendedModel(int userID) => (from ua in DB.UserAllocations
																											  join u in DB.Users on ua.UserID equals u.UserID
																											  join a in DB.Activities on ua.ActivityID equals a.ActivityID
																											  join p in DB.Projects on a.ProjectID equals p.ProjectID
																											  join c in DB.Clients on p.ClientID equals c.ClientID
																											  join r in DB.Regions on a.RegionID equals r.RegionID
																											  join d in DB.Departments on a.DepartmentID equals d.DepartmentID
																											  where u.UserID == userID
																											  select new UserAllocationExtendedModel()
																											  {
																												  User = u,
																												  Client = c,
																												  Project = p,
																												  Region = r,
																												  Department = d,
																												  Activity = a,
																												  UserAllocation = ua
																											  }
												   )
												   .Distinct()
												   .OrderBy(c => c.Client.ClientName)
												   .ThenBy(r => r.Region.RegionName)
												   .ThenBy(d => d.Department.DepartmentName)
												   .ThenBy(a => a.Activity.ActivityName)
												   .ToList();

		public static List<Activity> GetNonLinkedActivities(int userID, int projectID)
		{
			var activities = from a in DB.Activities
							 where a.ProjectID == projectID
							 select a;

			var list = new List<Activity>();

			foreach (var activity in activities)
			{
				if (DB.UserAllocations.Where(ua => ua.ActivityID == activity.ActivityID && ua.UserID == userID).Count() == 0)
					list.Add(activity);
			}

			return list;
		}

		#endregion USER ALLOCATION

		#region PROJECT

		internal static Project GetProject(int activityID) => (from a in DB.Activities
															   join p in DB.Projects on a.ProjectID equals p.ProjectID
															   where a.ActivityID == activityID
															   select p)
					.SingleOrDefault();

		#endregion PROJECT
	}
}