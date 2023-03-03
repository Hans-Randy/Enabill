using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public abstract class ProjectRepo : BaseRepo
	{
		#region PROJECT SPECFIC

		public static List<string> GetDistinctProjectNames() => (from p in DB.Projects
																 select p.ProjectName)
					 .Distinct()
					 .ToList();

		public static List<string> GetDistinctProjectNamesForClientID(int clientID) => (from p in DB.Projects
																						where p.ClientID == clientID
																						select p.ProjectName)
					 .Distinct()
					 .ToList();

		public static Project GetByID(int projectID) => DB.Projects
					.SingleOrDefault(p => p.ProjectID == projectID);

		public static Project GetByName(string projectName) => DB.Projects
					.SingleOrDefault(r => r.ProjectName == projectName);

		public static Project GetByClientIDProjectName(int clientID, string projectName) => DB.Projects
					.SingleOrDefault(p => p.ClientID == clientID && p.ProjectName == projectName);

		public static IEnumerable<Project> GetAll() => DB.Projects;

		public static List<Project> GetAllActiveProjects() => DB.Projects.Where(p => p.DeactivatedDate == null).OrderBy(p => p.ProjectName).ToList();

		public static List<ProjectViewData> GetViewDataActiveProject() => DB.Projects.Where(p => p.DeactivatedDate == null).Select(p => new ProjectViewData { ProjectID = p.ProjectID, ProjectName = p.ProjectName }).OrderBy(p => p.ProjectName).ToList();

		public static List<ProjectViewData> GetViewDataActiveProjectsForUser(int userID) => (from u in DB.Users
																							 join ua in DB.UserAllocations on u.UserID equals ua.UserID
																							 join a in DB.Activities on ua.ActivityID equals a.ActivityID
																							 join p in DB.Projects on a.ProjectID equals p.ProjectID
																							 where
																								 (p.ScheduledEndDate == null || p.ScheduledEndDate > DateTime.Now)
																								 && u.UserID == userID
																							 select new ProjectViewData
																							 {
																								 ProjectName = p.ProjectName,
																								 ProjectID = p.ProjectID
																							 }).Distinct().ToList();

		public static BillingMethodViewData GetBillingMethodByProjectID(int projectID) => (from p in DB.Projects
																						   join bm in DB.BillingMethods on p.BillingMethodID equals bm.BillingMethodID
																						   where
																							   p.ProjectID == projectID
																						   select new BillingMethodViewData
																						   {
																							   BillingMethodName = bm.BillingMethodName,
																							   BillingMethodID = bm.BillingMethodID
																						   }).SingleOrDefault();

		public static List<int> GetAllProjectIDs() => DB.Projects.Select(p => p.ProjectID).ToList();

		public static IQueryable<SupportEmail> GetSupportEmails() => DB.SupportEmails.Distinct();

		public static SupportEmail GetBySupportEmailAddress(string supportEmailAddress) => DB.SupportEmails
					.SingleOrDefault(p => p.SupportEmailAddress == supportEmailAddress);

		public static IQueryable<Project> GetSupportProjectsForClient(int clientID) => DB.Projects.Where(p => p.ClientID == clientID).Distinct();

		internal static List<ProjectSearchResult> Search(string q, int projectOwnerID, bool isActive)
		{
			if (q == null)
				q = string.Empty;
			var currentDate = DateTime.Today;

			var list = from p in DB.Projects
					   join c in DB.Clients on p.ClientID equals c.ClientID
					   join r in DB.Regions on p.RegionID equals r.RegionID
					   join b in DB.BillingMethods on p.BillingMethodID equals b.BillingMethodID
					   join d in DB.Departments on p.DepartmentID equals d.DepartmentID
					   where (p.ProjectName.Contains(q)
					   || p.ProjectCode.Contains(q)
					   || c.ClientName.Contains(q))
					   && (p.ProjectOwnerID == (projectOwnerID > 0 ? projectOwnerID : p.ProjectOwnerID)
					   )
					   select new ProjectSearchResult()
					   {
						   ProjectID = p.ProjectID,
						   ProjectName = p.ProjectName,
						   ClientName = c.ClientName,
						   BillingMethodTypeName = b.BillingMethodName,
						   Region = r.RegionName,
						   Department = d.DepartmentName,
						   Project = p,
						   IsActive = isActive,
						   ScheduledEndDate = p.ScheduledEndDate.Value,
						   ProjectCode = p.ProjectCode,
						   ProjectValue = p.ProjectValue,
						   IsFixedCost = b.BillingMethodName.Contains("Fixed Cost")
					   }
					;

			var resultList = list.OrderBy(p => p.ClientName).ThenBy(p => p.ProjectName).ToList();
			foreach (var item in resultList)
			{
				item.Activities = GetActivities(item.ProjectID, isActive).Select(a => a.ActivityName).ToList();
			}

			return resultList;
		}

		public static int GetUsersAssignedToProjectModel(int projectID, DateTime refDate) => (from ua in DB.UserAllocations
																							  join u in DB.Users on ua.UserID equals u.UserID
																							  join a in DB.Activities on ua.ActivityID equals a.ActivityID
																							  join p in DB.Projects on a.ProjectID equals p.ProjectID
																							  where p.ProjectID == projectID && ua.StartDate <= refDate && refDate <= (ua.ConfirmedEndDate ?? refDate)
																							  select ua.ActivityID
					 ).Count();

		public static int GetInvoiceRulesByProjectID(int projectID)
		{
			var currentDate = DateTime.Today;

			return (from ir in DB.InvoiceRules
					where ir.ProjectID == projectID
					&& (ir.DateTo == null || ir.DateTo > currentDate)
					select ir.InvoiceRuleID).Count();
		}

		internal static void Save(Project project)
		{
			if (project.ProjectID == 0)
				DB.Projects.Add(project);

			DB.SaveChanges();
		}

		internal static void Delete(Project project)
		{
			try
			{
				DB.Projects.Remove(project);
				DB.SaveChanges();
			}
			catch
			{
				throw new NullReferenceException("The project could not be found in the records");
			}
		}

		public static List<User> GetAllProjectManagers() => (from p in DB.Projects
															 join u in DB.Users on p.ProjectOwnerID equals u.UserID
															 select u)
					 .Distinct()
					 .OrderBy(u => u.FullName)
					 .ToList();

		#endregion PROJECT SPECFIC

		#region ACTIVITY

		internal static Activity GetActivity(int projectID, int activityID)
		{
			var activity = DB.Activities
									.SingleOrDefault(a => a.ProjectID == projectID && a.ActivityID == activityID);

			if (activity != null)
				return activity;

			if (activity.IsActive)
				return activity;

			return new Activity();
		}

		internal static IEnumerable<Activity> GetActivities(int projectID) => DB.Activities
						.Where(a => a.ProjectID == projectID);

		internal static IEnumerable<Activity> GetActivities(int projectID, bool isActive) => DB.Activities
						.Where(a => a.ProjectID == projectID && a.IsActive == isActive);

		//internal static IEnumerable<Activity> GetActivities(int projectID, bool activeState)
		//{
		//    return DB.Activities
		//                .Where(a => a.ProjectID == projectID && a.IsActive == activeState);
		//}

		internal static void SaveActivity(Activity activity)
		{
			if (activity.ActivityID == 0)
				DB.Activities.Add(activity);

			DB.SaveChanges();
		}

		internal static void DeleteActivity(Activity activity)
		{
			DB.Activities.Remove(activity);
			DB.SaveChanges();
		}

		public static void DeleteUserAllocation(UserAllocation ua)
		{
			DB.UserAllocations.Remove(ua);
			DB.SaveChanges();
		}

		#endregion ACTIVITY

		#region JSON LOOKUPS

		public static List<JsonLookup> AutoComplete(string client, string partialProject, int topCount)
		{
			if (string.IsNullOrEmpty(client))
			{
				return (from p in DB.Projects
						where p.ProjectName.StartsWith(partialProject)
						select new JsonLookup
						{
							id = p.ProjectID,
							label = p.ProjectName,
							value = p.ProjectName
						}).Distinct()
						.OrderBy(p => p.label)
						.Take(topCount)
						.ToList();
			}
			else
			{
				return (from p in DB.Projects
						join c in DB.Clients on p.ClientID equals c.ClientID
						where c.ClientName == client
						&& p.ProjectName.StartsWith(partialProject)
						select new JsonLookup
						{
							id = p.ProjectID,
							label = p.ProjectName,
							value = p.ProjectName
						}).Distinct()
						.OrderBy(p => p.label)
						.Take(topCount)
						.ToList();
			}
		}

		#endregion JSON LOOKUPS
	}
}