using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public abstract class UserRepo : BaseRepo
	{
		#region USER SPECIFIC

		public static User GetByID(int userID) => DB.Users
					.SingleOrDefault(u => u.UserID == userID && !u.IsSystemUser);

		public static User GetForLogin(string loginName) => DB.Users.SingleOrDefault(u => (u.UserName == loginName || u.Email == loginName) && u.CanLogin && !u.IsSystemUser && u.IsActive);

		public static IEnumerable<User> GetAll(bool isActive = true) => DB.Users.Where(u => !u.IsSystemUser && u.IsActive == isActive).OrderBy(u => u.UserName);

		public static IEnumerable<User> GetAllActiveOrRecentlyInactive()
		{
			var lastMonthForInactiveEmployees = DateTime.Today.AddMonths(-1);

			return DB.Users.Where(u => !u.IsSystemUser && (u.IsActive || (u.EmployEndDate != null && u.EmployEndDate >= lastMonthForInactiveEmployees))).OrderBy(u => u.UserName);
		}

		public static IEnumerable<User> GetAllActive() => DB.Users.Where(u => !u.IsSystemUser && u.IsActive).OrderBy(u => u.UserName);

		public static List<User> GetAllActiveUsers() => DB.Users.Where(u => !u.IsSystemUser && u.IsActive).OrderBy(u => u.UserName).ToList();

		public static List<User> GetAllTimeCaptureUsers(bool hourlyContractor)
		{
			if (hourlyContractor)
				return DB.Users.AsEnumerable().Where(u => !u.IsSystemUser && u.IsActive && u.EmploymentTypeID == (int)EmploymentTypeEnum.HourlyContractor && u.HasRole(UserRoleType.TimeCapturing)).OrderBy(u => u.UserName).ToList();
			else // Permanent or Monthly Contractor
				return DB.Users.AsEnumerable().Where(u => !u.IsSystemUser && u.IsActive && (u.EmploymentTypeID == (int)EmploymentTypeEnum.Permanent || u.EmploymentTypeID == (int)EmploymentTypeEnum.MonthlyContractor) && u.HasRole(UserRoleType.TimeCapturing)).OrderBy(u => u.UserName).ToList();
		}

		public static List<User> GetObsoleteLeaveCycleBalanceRecords() => DB.Users.AsQueryable()
					.Join(DB.LeaveCycleBalances,
					u => u.UserID,
					lcb => lcb.UserID,
					(u, lcb) => new { u, lcb })
					.Where(ulcb => !ulcb.u.IsSystemUser && !ulcb.u.IsActive && ulcb.lcb.Active == 1)
					.OrderBy(ulcb => ulcb.u.UserName)
					.Select(ulcb => ulcb.u)
					.Distinct()
					.ToList();

		public static List<UserLeaveDiscrepancy> GetLeaveBalanceDiscrepancies()
		{
			var removeList = new List<UserLeaveDiscrepancy>();

			// Get list of users where there is a discrepancy between the number of leave days and leave cycle balance total
			var results = (
								from l in DB.Leaves
								join lcb in DB.LeaveCycleBalances on new
								{
									join1 = l.UserID,
									join2 = l.LeaveType
								}
								equals new
								{
									join1 = lcb.UserID,
									join2 = lcb.LeaveTypeID
								}
								join u in DB.Users
									on lcb.UserID equals u.UserID
								where l.DateFrom >= lcb.StartDate && l.DateTo <= lcb.EndDate
								where l.ApprovalStatus == (int)ApprovalStatusType.Approved
								where lcb.Active == 1
								where u.IsActive
								group l by new
								{
									u.UserID,
									lcb.LeaveTypeID,
									u.UserName,
									lcb.Taken
								} into g
								orderby g.Key.UserName
								where Math.Round(g.Sum(x => x.NumberOfDays)) != Math.Round(g.Key.Taken)
								select new UserLeaveDiscrepancy()
								{
									UserID = g.Key.UserID,
									UserName = g.Key.UserName,
									LeaveType = g.Key.LeaveTypeID,
									Taken = (float)g.Key.Taken,
									Number = (float)g.Sum(x => x.NumberOfDays)
								}
							).ToList();

			foreach (var result in results)
			{
				// Find any records where the leave extends beyond the active leave cycle
				var results2 = (
									from l in DB.Leaves
									join lcb in DB.LeaveCycleBalances on new
									{
										join1 = l.UserID,
										join2 = l.LeaveType
									}
									equals new
									{
										join1 = lcb.UserID,
										join2 = lcb.LeaveTypeID
									}
									join u in DB.Users
										on lcb.UserID equals u.UserID
									where u.UserID == result.UserID
									where l.LeaveType == result.LeaveType
									where l.DateFrom >= lcb.StartDate && l.DateTo > lcb.EndDate
									where l.ApprovalStatus == (int)ApprovalStatusType.Approved
									where lcb.Active == 1
									where u.IsActive
									group l by new
									{
										u.UserID,
										lcb.LeaveTypeID,
										lcb.Taken,
										l.DateFrom,
										l.DateTo,
										lcb.EndDate
									} into g
									where Math.Round(g.Sum(x => x.NumberOfDays)) != Math.Round(g.Key.Taken)
									select new UserLeaveDiscrepancy()
									{
										UserID = g.Key.UserID,
										LeaveType = g.Key.LeaveTypeID,
										DateFrom = g.Key.DateFrom,
										DateTo = g.Key.DateTo,
										EndDate = g.Key.EndDate
									}
								).ToList();

				// Find days before and after the end of the active leave cycle
				foreach (var result2 in results2)
				{
					int noOfDaysBefore = DB.Database.SqlQuery<int>("EXEC GetNoOfWorkDays {0}, {1}, 1, 0", result2.DateFrom, result2.EndDate).FirstOrDefault();

					if (result.Number + noOfDaysBefore == result.Taken)
					{
						removeList.Add(result);
					}
				}
			}

			// Remove any relevant records from the results list
			foreach (var item in removeList)
			{
				results.Remove(item);
			}

			return results;
		}

		public static User GetByUserName(string userName) =>
			DB.Users.SingleOrDefault(u => u.UserName == userName && !u.IsSystemUser && u.IsActive);

		public static User GetByEmail(string email) => DB.Users.SingleOrDefault(u => u.Email == email && !u.IsSystemUser);

		public static User GetActiveByEmail(string email) => DB.Users.SingleOrDefault(u => u.Email == email && !u.IsSystemUser && u.IsActive);

		internal static IEnumerable<User> FilterAllByName(string nameFilter, bool isActive) => GetAll(isActive)
					.Where(u => u.UserName.Contains(nameFilter.ToLower()));

		internal static IEnumerable<User> FilterByNameForManager(int managerID, string nameFilter) => GetStaffForManager(managerID)
					.Where(u => u.UserName.Contains(nameFilter));

		internal static IEnumerable<User> FilterByNameForPM(int projectOwnerID, string nameFilter) => GetStaffForProjectOwner(projectOwnerID)
					.Where(u => u.UserName.Contains(nameFilter));

		internal static IEnumerable<User> GetUsersAssignedToActivity(int activityID, DateTime refDate) => from ua in DB.UserAllocations
																										  join u in DB.Users on ua.UserID equals u.UserID
																										  where ua.ActivityID == activityID && ua.StartDate <= refDate && refDate <= (ua.ConfirmedEndDate ?? refDate)
																										  select u
									;

		public static List<User> GetUsersAssignedToProject(int projectID)
		{
			var refDate = DateTime.Today;

			return (from ua in DB.UserAllocations
					join u in DB.Users on ua.UserID equals u.UserID
					join a in DB.Activities on ua.ActivityID equals a.ActivityID
					where a.ProjectID == projectID
					&& ua.StartDate <= refDate && refDate <= (ua.ConfirmedEndDate ?? refDate)
					select u
									 ).Distinct()
									  .ToList();
		}

		internal static IEnumerable<WorkAllocation> GetWorkAllocationsAssignedToActivity(int activityID, DateTime refDate) =>
									  from wa in DB.WorkAllocations
									  join u in DB.Users on wa.UserID equals u.UserID
									  join ua in DB.UserAllocations on u.UserID equals ua.UserID
									  where wa.ActivityID == activityID && wa.DayWorked >= ua.StartDate && wa.DayWorked <= (ua.ConfirmedEndDate ?? refDate)
									  select wa
									;

		internal static IEnumerable<WorkAllocation> GetWorkAllocationsAssignedToActivity(int projectID, int activityID, int userID, DateTime refDate) =>
									  from wa in DB.WorkAllocations
									  join a in DB.Activities on wa.ActivityID equals a.ActivityID
									  join u in DB.Users on wa.UserID equals u.UserID
									  join ua in DB.UserAllocations on u.UserID equals ua.UserID
									  where wa.ActivityID == activityID && a.ProjectID == projectID && wa.UserID == userID && wa.DayWorked >= ua.StartDate && wa.DayWorked <= (ua.ConfirmedEndDate ?? refDate)
									  select wa
									;

		internal static IEnumerable<User> GetUsersNotAssignedToActivity(int activityID, DateTime refDate) => from u in DB.Users
																											 join ua in DB.UserAllocations.Where(us => us.ActivityID == activityID) on u.UserID equals ua.UserID into t_ua
																											 from ua2 in t_ua.DefaultIfEmpty()
																											 where ua2.UserID == null && ua2.StartDate <= refDate && refDate <= (ua2.ConfirmedEndDate ?? refDate)
																											 select u
				   ;

		internal static void Save(User user)
		{
			if (user.UserID == 0)
				DB.Users.Add(user);

			DB.SaveChanges();
		}

		public static EmploymentType GetEmploymentType(int emplymentTypeID) => DB.EmploymentTypes.SingleOrDefault(e => e.EmploymentTypeID == emplymentTypeID);

		public static IEnumerable<EmploymentType> GetAllEmploymentType() => DB.EmploymentTypes;

		public static List<EmploymentType> GetAllEmploymentTypesList() => DB.EmploymentTypes.ToList();

		#endregion USER SPECIFIC

		#region USER PREFERENCE

		internal static UserPreference GetUserPreference(int userID) => DB.UserPreferences
					.SingleOrDefault(up => up.UserID == userID);

		#endregion USER PREFERENCE

		#region ROLE

		public static IEnumerable<Role> GetRoles(int userID) => from ur in DB.UserRoles
																join r in DB.Roles on ur.RoleID equals r.RoleID
																where ur.UserID == userID
																select r
					;

		public static IEnumerable<Role> GetOtherRoles(int userID)
		{
			var current = GetRoles(userID).ToList();
			var all = DB.Roles.ToList();

			return all.Except(current);
		}

		internal static void AssignRoleToUser(UserRole userRole)
		{
			var ur = DB.UserRoles.SingleOrDefault(r => r.UserID == userRole.UserID && r.RoleID == userRole.RoleID);

			if (ur != null)
				return;

			if (userRole.UserRoleID == 0)
				DB.UserRoles.Add(userRole);

			DB.SaveChanges();
		}

		internal static void RevokeRoleFromUser(int userID, int roleID)
		{
			var ur = DB.UserRoles.SingleOrDefault(r => r.UserID == userID && r.RoleID == roleID);

			if (ur != null)
			{
				DB.UserRoles.Remove(ur);
				DB.SaveChanges();
			}
		}

		internal static IEnumerable<User> GetByRoleBW(int roleBW) => (from u in DB.Users
																	  join ur in DB.UserRoles on u.UserID equals ur.UserID
																	  where u.IsActive && (ur.RoleID & roleBW) > 0
																	  select u
				   )
				   .Distinct();

		public static IEnumerable<User> GetUsersByRole(int roleID) => (from u in DB.Users
																	   join ur in DB.UserRoles on u.UserID equals ur.UserID
																	   where !u.IsSystemUser && ur.RoleID == roleID && u.IsActive
																	   select u)
					 .Distinct();

		#endregion ROLE

		#region USER HISTORIES

		internal static UserHistory GetUserHistory(int userID, int period) => DB.UserHistories
					.SingleOrDefault(u => u.UserID == userID && u.Period == period);

		internal static void SaveUserHistory(UserHistory uH)
		{
			if (uH.UserHistoryID <= 0)
				DB.UserHistories.Add(uH);

			DB.SaveChanges();
		}

		#endregion USER HISTORIES

		#region MANAGER

		public static IEnumerable<User> GetManagerList() => DB.Users.Join(DB.Users.AsEnumerable(), u => u.ManagerID, m => m.UserID, (u, m) => u).Where(u => u.IsActive).Distinct();

		public static IEnumerable<User> GetStaffForManager(int managerID) => DB.Users
				.Where(u => u.ManagerID == managerID && u.IsActive);

		internal static IEnumerable<User> GetStaffForManager(int managerID, int roleBW) => from u in DB.Users
																						   join ua in DB.UserRoles on u.UserID equals ua.UserID
																						   where u.ManagerID == managerID && ((ua.RoleID & roleBW) > 0) && u.IsActive
																						   select u;

		internal static IEnumerable<User> GetStaffForManager(int managerID, UserRoleType userRoleType) => from u in DB.Users
																										  join ur in DB.UserRoles on u.UserID equals ur.UserID
																										  where (ur.RoleID == (int)userRoleType & u.ManagerID == managerID) && u.IsActive
																										  select u;

		#endregion MANAGER

		#region SECONDARY MANAGERS

		internal static IEnumerable<User> GetSecondaryManagers(int userID) => from s in DB.SecondaryManagerAllocations
																			  join u in DB.Users on s.ManagerID equals u.UserID
																			  where s.UserID == userID
																			  select u;

		internal static void AssignSecondaryManagerToUser(SecondaryManagerAllocation model)
		{
			var temp = DB.SecondaryManagerAllocations.SingleOrDefault(x => x.ManagerID == model.ManagerID && x.UserID == model.UserID);
			if (temp != null)
				return;

			if (model.StaffManagerAllocationID == 0)
				DB.SecondaryManagerAllocations.Add(model);

			DB.SaveChanges();
		}

		#endregion SECONDARY MANAGERS

		#region PROJECT OWNER

		internal static IEnumerable<Project> GetProjectsForProjectOwner(int userID) => DB.Projects
					.Where(p => p.ProjectOwnerID == userID);

		internal static IEnumerable<User> GetStaffForProjectOwner(int projectOwnerID) => (from u in DB.Users
																						  join l in DB.UserAllocations on u.UserID equals l.UserID
																						  join a in DB.Activities on l.ActivityID equals a.ActivityID
																						  join p in DB.Projects on a.ProjectID equals p.ProjectID
																						  where p.ProjectOwnerID == projectOwnerID
																						  select u).Distinct();

		#endregion PROJECT OWNER

		#region PROJECT

		internal static IEnumerable<Project> GetProjects(int userID) => (from ua in DB.UserAllocations
																		 join a in DB.Activities on ua.ActivityID equals a.ActivityID
																		 join p in DB.Projects on a.ProjectID equals p.ProjectID
																		 where ua.UserID == userID
																		 select p
					)
					.Distinct();

		internal static List<int> GetProjectIDsLinkedToUser(int userID) => (from ua in DB.UserAllocations
																			join a in DB.Activities on ua.ActivityID equals a.ActivityID
																			join p in DB.Projects on a.ProjectID equals p.ProjectID
																			where ua.UserID == userID
																			select p.ProjectID
					).ToList();

		internal static List<int> GetProjectIDsLinkedToProjectOwner(int projectOwnerID) => (from p in DB.Projects
																							where p.ProjectOwnerID == projectOwnerID
																							select p.ProjectID
					).ToList();

		internal static List<int> GetProjectIDsManagedByUser(int userID) => (from p in DB.Projects
																			 where p.ProjectOwnerID == userID
																			 select p.ProjectID
					).ToList();

		#endregion PROJECT

		#region ACTIVITY

		public static Activity GetActivityByID(int activityID) => DB.Activities
					.SingleOrDefault(u => u.ActivityID == activityID);

		internal static IEnumerable<UserActivity> GetActivities(int userID, DateTime refDate)
		{
			var list = from a in DB.Activities
					   join p in DB.Projects on a.ProjectID equals p.ProjectID
					   join c in DB.Clients on p.ClientID equals c.ClientID
					   join ua in DB.UserAllocations on a.ActivityID equals ua.ActivityID
					   join wa in DB.WorkAllocations on a.ActivityID equals wa.ActivityID into HasWA
					   from waGroup in HasWA.Where(i => i.UserID == userID && i.DayWorked == refDate).DefaultIfEmpty()
					   where ua.UserID == userID
						   && ua.StartDate <= refDate
						   && refDate <= (ua.ConfirmedEndDate ?? refDate)
						   && (a.IsActive || waGroup != null)
					   select new UserActivity
					   {
						   ClientID = c.ClientID,
						   ClientName = c.ClientName,
						   ProjectID = p.ProjectID,
						   ProjectName = p.ProjectName,
						   ActivityID = a.ActivityID,
						   ActivityName = a.ActivityName,
						   UserAllocationID = ua.UserAllocationID,
						   StartDate = ua.StartDate,
						   ScheduledEndDate = ua.ScheduledEndDate,
						   ConfirmedEndDate = ua.ConfirmedEndDate,
						   IsConfirmed = ua.ConfirmedEndDate.HasValue,
						   ChargeRate = ua.ChargeRate
					   };

			return list.Distinct();
		}

		internal static IEnumerable<int> GetDefaultActivities()
		{
			var list = (from a in DB.Activities
						where a.IsDefault
						select a.ActivityID)
						.Distinct()
						.ToList();

			return list;
		}

		internal static IEnumerable<UserActivity> GetPastActivities(int userID, DateTime refDate) => from ua in DB.UserAllocations
																									 join u in DB.Users on ua.UserID equals u.UserID
																									 join a in DB.Activities on ua.ActivityID equals a.ActivityID
																									 join p in DB.Projects on a.ProjectID equals p.ProjectID
																									 join c in DB.Clients on p.ClientID equals c.ClientID
																									 where ua.UserID == userID && refDate > ua.ConfirmedEndDate
																									 select new UserActivity
																									 {
																										 ClientID = c.ClientID,
																										 ClientName = c.ClientName,
																										 ProjectID = p.ProjectID,
																										 ProjectName = p.ProjectName,
																										 UserAllocationID = ua.UserAllocationID,
																										 ActivityID = a.ActivityID,
																										 ActivityName = a.ActivityName,
																										 ChargeRate = ua.ChargeRate,
																										 StartDate = ua.StartDate,
																										 ScheduledEndDate = ua.ScheduledEndDate,
																										 ConfirmedEndDate = ua.ConfirmedEndDate,
																										 IsConfirmed = ua.ConfirmedEndDate.HasValue
																									 }
											 ;

		internal static IEnumerable<UserActivity> GetFutureActivities(int userID, DateTime refDate) => from ua in DB.UserAllocations
																									   join u in DB.Users on ua.UserID equals u.UserID
																									   join a in DB.Activities on ua.ActivityID equals a.ActivityID
																									   join p in DB.Projects on a.ProjectID equals p.ProjectID
																									   join c in DB.Clients on p.ClientID equals c.ClientID
																									   where ua.UserID == userID && ua.StartDate > refDate
																									   select new UserActivity
																									   {
																										   ClientID = c.ClientID,
																										   ClientName = c.ClientName,
																										   ProjectID = p.ProjectID,
																										   ProjectName = p.ProjectName,
																										   UserAllocationID = ua.UserAllocationID,
																										   ActivityID = a.ActivityID,
																										   ActivityName = a.ActivityName,
																										   ChargeRate = ua.ChargeRate,
																										   StartDate = ua.StartDate,
																										   ScheduledEndDate = ua.ScheduledEndDate,
																										   ConfirmedEndDate = ua.ConfirmedEndDate,
																										   IsConfirmed = ua.ConfirmedEndDate.HasValue
																									   }
											 ;

		internal static IEnumerable<Activity> GetActivitiesForDateSpan(int userID, DateTime startDate, DateTime endDate)
		{
			var IDs = (from ua in DB.UserAllocations
					   join a in DB.Activities on ua.ActivityID equals a.ActivityID
					   where ua.UserID == userID
					   && (
							 (ua.StartDate >= startDate && (ua.ConfirmedEndDate ?? endDate) >= endDate)
						  || (ua.StartDate >= startDate && ua.StartDate <= endDate)
						  || ((ua.ConfirmedEndDate ?? endDate) >= startDate && (ua.ConfirmedEndDate ?? endDate) <= endDate)
					   )
					   select a.ActivityID)
							 .Distinct()
							 .ToList();

			var model = new List<Activity>();

			foreach (int id in IDs)
			{
				model.AddRange(
					from a in DB.Activities
					join p in DB.Projects on a.ProjectID equals p.ProjectID
					join c in DB.Clients on p.ClientID equals c.ClientID
					where a.ActivityID == id
					orderby c.ClientName, p.ProjectName, a.ActivityName
					select a
				);
			}

			return model;
		}

		internal static List<ActivityDetail> GetWorkedActivitiesForDateSpan(int userID, DateTime startDate, DateTime endDate)
		{
			var list = (from wa in DB.WorkAllocations
						join a in DB.Activities on wa.ActivityID equals a.ActivityID
						join p in DB.Projects on a.ProjectID equals p.ProjectID
						join c in DB.Clients on p.ClientID equals c.ClientID
						where wa.UserID == userID && wa.DayWorked >= startDate && wa.DayWorked <= endDate
						select new { activity = a, project = p, client = c })
						.ToList();

			return list.Select(m => new ActivityDetail(m.activity, m.project, m.client, false))
					.Distinct(new ActivityDetailModelComparer())
					.OrderBy(m => m.ClientName)
					.ThenBy(m => m.ProjectName)
					.ThenBy(m => m.ActivityName)
					.ToList();
		}

		internal static List<ActivityDetail> GetWorkedActivitiesForDateSpanForWorkSessionStatus(int userID, DateTime startDate, DateTime endDate, int workSessionStatus)
		{
			var list = (from wa in DB.WorkAllocations
						join a in DB.Activities on wa.ActivityID equals a.ActivityID
						join p in DB.Projects on a.ProjectID equals p.ProjectID
						join c in DB.Clients on p.ClientID equals c.ClientID
						join ws in DB.WorkSessions on wa.UserID equals ws.UserID
						where wa.UserID == userID && wa.DayWorked >= startDate && wa.DayWorked < endDate
						&& ws.StartTime >= startDate && ws.EndTime < endDate
						&& ws.WorkSessionStatusID == workSessionStatus
						select new { activity = a, project = p, client = c })
						.ToList();

			return list.Select(m => new ActivityDetail(m.activity, m.project, m.client, false))
					.Distinct(new ActivityDetailModelComparer())
					.OrderBy(m => m.ClientName)
					.ThenBy(m => m.ProjectName)
					.ThenBy(m => m.ActivityName)
					.ToList();
		}

		#endregion ACTIVITY

		#region EXPENSES

		internal static List<ExpenseDetail> GetExpensesForDateSpan(int userID, DateTime startDate, DateTime endDate)
		{
			var list = (from e in DB.Expenses
						join p in DB.Projects on e.ProjectID equals p.ProjectID
						join c in DB.Clients on e.ClientID equals c.ClientID
						join ec in DB.ExpenseCategoryTypes on e.ExpenseCategoryTypeID equals ec.ExpenseCategoryTypeID
						where e.UserID == userID && e.Amount > 0 && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate
						select new { expense = e, project = p, client = c, category = ec })
						.ToList();

			return list.Select(m => new ExpenseDetail(m.expense, m.project, m.client, m.category))
					.Distinct(new ExpenseDetailModelComparer())
					.OrderBy(m => m.ExpenseDate)
					.ThenBy(m => m.ClientName)
					.ThenBy(m => m.ProjectName)
					.ToList();
		}

		public static int GetExpenseStatus(int userID)
		{
			int count = (from e in DB.Expenses
						 where e.UserID == userID
						 select e.ExpenseID).Count();

			// Expenses exist for user
			if (count > 0)
			{
				count = (from e in DB.Expenses
						 where e.UserID == userID
						 && !e.Locked
						 select e.ExpenseID).Count();

				if (count > 0)
				{
					// Unapproved Expenses pending for user
					return 4;
				}
				else
				{
					// All expenses approved for user
					return 2;
				}
			}
			else
			{
				// No expenses exist for user
				return 1;
			}
		}

		#endregion EXPENSES

		#region WORKSESSION

		internal static WorkSession GetWorkSessionByID(int userID, int workSessionID) => (from ws in DB.WorkSessions
																						  where ws.UserID == userID && ws.WorkSessionID == workSessionID
																						  select ws).SingleOrDefault();

		public static IEnumerable<WorkSession> GetWorkSessionsForDate(int userID, DateTime dateTime) => from ws in DB.WorkSessions
																										where ws.UserID == userID
																										&& ws.StartTime.Year == dateTime.Year
																										&& ws.StartTime.Month == dateTime.Month
																										&& ws.StartTime.Day == dateTime.Day
																										select ws
				   ;

		public static bool IsWorkSessionApproved(int userID, DateTime date)
		{
			var ws = DB.WorkSessions.FirstOrDefault(w => w.StartTime.Year == date.Year
															&& w.StartTime.Month == date.Month
															&& w.StartTime.Day == date.Day
															&& w.UserID == userID);

			var nws = DB.NonWorkSessions.FirstOrDefault(n => n.Date == date
													   && n.UserID == userID);

			if (ws == null)
			{
				return nws != null;
			}
			else if (ws.WorkSessionStatusID == 2)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static int GetWorkSessionStatus(int userID, DateTime date)
		{
			var ws = DB.WorkSessions.FirstOrDefault(w => w.StartTime.Year == date.Year
															&& w.StartTime.Month == date.Month
															&& w.StartTime.Day == date.Day
															&& w.UserID == userID);
			if (ws == null)
				return 3;

			return ws.WorkSessionStatusID;
		}

		public static IEnumerable<WorkSession> GetWorkSessionsForDateSpan(int userID, DateTime startDate, DateTime endDate) => DB.WorkSessions
					.Where(ws => ws.UserID == userID && ws.StartTime >= startDate && ws.EndTime <= endDate);

		public static IEnumerable<NonWorkSession> GetNonWorkSessionsForDateSpan(int userID, DateTime startDate, DateTime endDate) => DB.NonWorkSessions
					.Where(nws => nws.UserID == userID && nws.Date >= startDate.Date && nws.Date < endDate.Date);

		internal static void SaveWorkSession(WorkSession ws)
		{
			//Only do this if changes are applied to a month other than the current
			var user = GetByID(ws.UserID);
			//int monthsToRecalc = DateTime.Today.Year != ws.StartTime.Year ? 12 - ws.StartTime.Month + DateTime.Today.Month : DateTime.Today.Month - 1;
			//if (ws.StartTime.IsInPastMonth())
			//    user.ExecuteFlexiBalanceLeaveBalanceProcess(user, monthsToRecalc);

			if (ws.WorkSessionID == 0)
				DB.WorkSessions.Add(ws);

			DB.SaveChanges();
		}

		internal static void DeleteWorkSession(WorkSession ws)
		{
			//Only do this is changes is applied to a month other than the current
			var user = GetByID(ws.UserID);
			int monthsToRecalc = DateTime.Today.Year != ws.StartTime.Year ? 12 - ws.StartTime.Month + DateTime.Today.Month : DateTime.Today.Month - 1;

			if (ws.StartTime.IsInPastMonth())
				user.ExecuteFlexiBalanceLeaveBalanceProcess(user, monthsToRecalc);

			DB.WorkSessions.Remove(ws);
			DB.SaveChanges();
		}

		internal static void SaveNonWorkSession(NonWorkSession nws)
		{
			var user = GetByID(nws.UserID);

			if (nws.NonWorkSessionID == 0)
				DB.NonWorkSessions.Add(nws);

			DB.SaveChanges();
		}

		public static void DeleteNonWorkSession(NonWorkSession nws)
		{
			DB.NonWorkSessions.Remove(nws);
			DB.SaveChanges();
		}

		public static void SaveNextWorkDay(int userID, DateTime dayToCopy, bool isPrevious) => DB.Database.ExecuteSqlCommand("EXEC DuplicateWorkDay {0}, {1}, {2}", userID, dayToCopy.Date, isPrevious);

		public static void SaveNextWeek(int userID, string modifiedBy, DateTime mondayOfCurrentWeek, bool isPrevious) => DB.Database.ExecuteSqlCommand("EXEC DuplicateWeek {0}, {1}, {2}, {3}", userID, modifiedBy, mondayOfCurrentWeek.Date, isPrevious);

		#endregion WORKSESSION

		#region WORKALLOCATION

		internal static WorkAllocation GetWorkAllocation(int userID, int workAllocationID) => DB.WorkAllocations
					.SingleOrDefault(wa => wa.UserID == userID && wa.WorkAllocationID == workAllocationID);

		public static IEnumerable<WorkAllocation> GetWorkAllocationsForDate(int userID, DateTime workday) => DB.WorkAllocations
					.Where(wa => wa.UserID == userID && wa.DayWorked == workday);

		internal static IEnumerable<WorkAllocation> GetWorkAllocationsForSpan(int userID, DateTime fromDate, DateTime toDate) => DB.WorkAllocations
					.Where(wa => wa.UserID == userID && wa.DayWorked >= fromDate && wa.DayWorked <= toDate);

		internal static IEnumerable<WorkAllocation> GetWorkAllocationsForActivityForDate(int userID, int activityID, DateTime workDay) => from wa in DB.WorkAllocations
																																		  where wa.UserID == userID && wa.ActivityID == activityID && wa.DayWorked == workDay
																																		  select wa;

		internal static IEnumerable<WorkAllocation> GetLastWorkAllocationDateForUserForActivity(int userID, int activityID) => from wa in DB.WorkAllocations
																															   where wa.UserID == userID && wa.ActivityID == activityID
																															   select wa;

		internal static void SaveWorkAllocation(WorkAllocation wa)
		{
			if (wa.WorkAllocationID == 0)
				DB.WorkAllocations.Add(wa);

			//Only do this if change is applied to a month other than the current
			var user = GetByID(wa.UserID);
			//int monthsToRecalc = DateTime.Today.Year != wa.DayWorked.Year ? 12 - wa.DayWorked.Month + DateTime.Today.Month : DateTime.Today.Month - 1;
			//if (wa.DayWorked.IsInPastMonth())
			//    user.ExecuteFlexiBalanceLeaveBalanceProcess(user, monthsToRecalc);

			DB.SaveChanges();
		}

		internal static void SaveAllWorkAllocation(IEnumerable<WorkAllocation> workAllocations)
		{
			foreach (var wa in workAllocations)
			{
				if (wa.WorkAllocationID == 0)
					DB.WorkAllocations.Add(wa);
			}

			DB.SaveChanges();
		}

		internal static void DeleteWorkAllocation(WorkAllocation wa)
		{
			DB.WorkAllocations.Remove(wa);
			DB.SaveChanges();
		}

		internal static void DeleteAllWorkAllocation(IEnumerable<WorkAllocation> workAllocations)
		{
			//Only do this is changes is applied to a month other than the current
			if (workAllocations.ToList().Count > 0)
			{
				var waFirst = workAllocations.First();
				var user = GetByID(waFirst.UserID);
				//int monthsToRecalc = DateTime.Today.Year != waFirst.DayWorked.Year ? 12 - waFirst.DayWorked.Month + DateTime.Today.Month : DateTime.Today.Month - 1;
				//if (waFirst.DayWorked.IsInPastMonth())
				//    user.ExecuteFlexiBalanceLeaveBalanceProcess(user, monthsToRecalc);
			}

			foreach (var wa in workAllocations)
			{
				var note = WorkAllocationRepo.GetNote(wa.WorkAllocationID);

				if (note != null)
				{
					DeleteNote(note);
				}

				DB.WorkAllocations.Remove(wa);
			}

			DB.SaveChanges();
		}

		#endregion WORKALLOCATION

		#region NOTES

		internal static void DeleteNote(Note note)
		{
			DB.Notes.Remove(note);
			DB.SaveChanges();
		}

		internal static Note GetNoteForWorkAllocation(int userID, int workallocationID) => (from n in DB.Notes
																							join wa in DB.WorkAllocations on n.WorkAllocationID equals wa.WorkAllocationID
																							where wa.UserID == userID && wa.WorkAllocationID == workallocationID
																							select n
						)
						.SingleOrDefault();

		internal static IEnumerable<Note> GetNotes(int userID, DateTime startDate, DateTime endDate) => from n in DB.Notes
																										join wa in DB.WorkAllocations on n.WorkAllocationID equals wa.WorkAllocationID
																										where wa.UserID == userID && wa.DayWorked >= startDate && wa.DayWorked <= endDate
																										select n
				   ;

		internal static IEnumerable<Note> GetNotesForActivity(int userID, int activityID) => from n in DB.Notes
																							 join wa in DB.WorkAllocations on n.WorkAllocationID equals wa.WorkAllocationID
																							 where wa.ActivityID == activityID && wa.UserID == userID
																							 select n
					   ;

		internal static IEnumerable<Note> GetNotesForProject(int userID, int projectID) => from n in DB.Notes
																						   join wa in DB.WorkAllocations on n.WorkAllocationID equals wa.WorkAllocationID
																						   join a in DB.Activities on wa.ActivityID equals a.ActivityID
																						   where wa.UserID == userID && a.ProjectID == projectID
																						   select n
					   ;

		internal static Note GetNoteForActivityForDate(int userID, int activityID, DateTime workDay) => (from n in DB.Notes
																										 join wa in DB.WorkAllocations on n.WorkAllocationID equals wa.WorkAllocationID
																										 where wa.UserID == userID && wa.ActivityID == activityID && wa.DayWorked == workDay
																										 select n
						)
						.SingleOrDefault();

		internal static void SaveNote(Note note)
		{
			if (note.NoteID == 0)
				DB.Notes.Add(note);

			DB.SaveChanges();
		}

		internal static IEnumerable<NoteDetailModel> GetDetailedNotes(int userID, DateTime dateFrom, DateTime dateTo) => from n in DB.Notes
																														 join wa in DB.WorkAllocations on n.WorkAllocationID equals wa.WorkAllocationID
																														 join u in DB.Users on wa.UserID equals u.UserID
																														 join a in DB.Activities on wa.ActivityID equals a.ActivityID
																														 join p in DB.Projects on a.ProjectID equals p.ProjectID
																														 where wa.UserID == userID && wa.DayWorked >= dateFrom && wa.DayWorked <= dateTo
																														 select new NoteDetailModel
																														 {
																															 Note = n,
																															 WorkAllocation = wa,
																															 User = u,
																															 Activity = a,
																															 Project = p
																														 };

		#endregion NOTES

		#region FLEXIBALANCE

		internal static FlexiBalance GetMostRecentFlexiBalance(int userID, DateTime date)
		{
			var maxDate = DB.FlexiBalances
						.Where(f => f.UserID == userID && f.BalanceDate <= date).DefaultIfEmpty().Max(f => (DateTime?)f.BalanceDate);
			if (maxDate == null)
				return null;

			return DB.FlexiBalances
					.SingleOrDefault(fb => fb.UserID == userID && fb.BalanceDate == maxDate);
		}

		internal static FlexiBalance GetFlexiBalance(int userID, DateTime date) => DB.FlexiBalances
					.SingleOrDefault(fb => fb.UserID == userID && fb.BalanceDate == date);

		internal static IEnumerable<FlexiBalance> GetFlexiBalancesForDateSpan(int userID, DateTime startDate, DateTime endDate) => DB.FlexiBalances
					.Where(fb => fb.UserID == userID && fb.BalanceDate >= startDate && fb.BalanceDate <= endDate);

		internal static void SaveFlexiBalance(FlexiBalance flexiBalance)
		{
			if (flexiBalance.FlexiBalanceID == 0)
				DB.FlexiBalances.Add(flexiBalance);

			DB.SaveChanges();
		}

		internal static FlexiBalance GetFlexiBalancePriorToDate(int userID, DateTime date)
		{
			var list = (from fb in DB.FlexiBalances
						where fb.UserID == userID && fb.BalanceDate < date && fb.BalanceDate > EnabillSettings.SiteStartDate
						select fb)
									   .ToList();

			//the above list only contains FlexiBalances that have a Balance date = first day of the month. If this list is empty, then we
			//need to check for a flexi balance inserted into the records for the date of their EmployStartDate

			if (list.Count == 0)
			{
				return (from fb in DB.FlexiBalances
						join u in DB.Users on fb.UserID equals u.UserID
						where fb.UserID == userID && fb.BalanceDate == u.EmployStartDate
						select fb
						).SingleOrDefault();
			}

			var maxDate = list.Max(fb => fb.BalanceDate);

			return list.SingleOrDefault(fb => fb.BalanceDate == maxDate);
		}

		internal static void DeleteFlexiBalances(int userID)
		{
			foreach (var item in DB.FlexiBalances.Where(fb => fb.UserID == userID).ToList())
			{
				DB.FlexiBalances.Remove(item);
				DB.SaveChanges();
			}
		}

		#endregion FLEXIBALANCE

		#region FLEXIBALANCEADJUSTMENT

		internal static FlexiBalanceAdjustment GetFlexiBalanceAdjustmentForDate(int userID, DateTime date)
		{
			var startDate = date.Date;
			var endDate = date.AddDays(1).Date;

			if (DB.FlexiBalanceAdjustments.Where(fba => fba.UserID == userID && fba.ImplementationDate >= startDate.Date && fba.ImplementationDate < endDate).Count() == 0)
				return null;

			var maxDate = DB.FlexiBalanceAdjustments.Max(fba => fba.DateAdjusted);

			return DB.FlexiBalanceAdjustments
					.SingleOrDefault(fba => fba.UserID == userID
										&& fba.ImplementationDate >= startDate.Date
										&& fba.ImplementationDate < endDate
										&& fba.DateAdjusted == maxDate);
		}

		internal static FlexiDay GetFlexiDayForUserForDate(int userID, DateTime dateTime) => DB.FlexiDays
					.SingleOrDefault(fd => fd.UserID == userID && fd.FlexiDate == dateTime && fd.ApprovalStatusID == (int)ApprovalStatusType.Approved);

		internal static FlexiDay GetFlexiDayPending(int userID, DateTime dateTime) => DB.FlexiDays
					.SingleOrDefault(fd => fd.UserID == userID && fd.FlexiDate == dateTime && fd.ApprovalStatusID == (int)ApprovalStatusType.Pending);

		public static void SaveFlexiBalanceAdjustment(FlexiBalanceAdjustment fba)
		{
			if (fba.FlexiBalanceAdjustmentID == 0)
				DB.FlexiBalanceAdjustments.Add(fba);

			DB.SaveChanges();
		}

		public static IEnumerable<FlexiBalanceAdjustment> GetFlexiBalanceAdjustments(int userID, DateTime dateFrom, DateTime dateTo) => DB.FlexiBalanceAdjustments
					.Where(fba => fba.UserID == userID && fba.ImplementationDate >= dateFrom && fba.ImplementationDate <= dateTo);

		public static FlexiBalanceAdjustment GetFlexiBalanceManualAdjustmentByID(int id) => DB.FlexiBalanceAdjustments
											.Where(fma => fma.FlexiBalanceAdjustmentID == id)
											.SingleOrDefault();

		public static void DeleteFlexiBalanceManualAdjustment(FlexiBalanceAdjustment fma)
		{
			DB.FlexiBalanceAdjustments.Remove(fma);
			DB.SaveChanges();
		}

		#endregion FLEXIBALANCEADJUSTMENT

		#region FLEXIDAY

		internal static void SaveFlexiDay(FlexiDay fd)
		{
			if (fd.FlexiDayID == 0)
				DB.FlexiDays.Add(fd);

			DB.SaveChanges();
		}

		public static IEnumerable<FlexiDay> GetFlexiDayForUserForDateSpan(int userID, DateTime startDate, DateTime endDate) => DB.FlexiDays
					.Where(fd => fd.UserID == userID && fd.FlexiDate >= startDate && fd.FlexiDate <= endDate);

		public static IEnumerable<FlexiDay> GetApprovedFlexiDayForUserForDateSpan(int userID, DateTime startDate, DateTime endDate) => DB.FlexiDays
					.Where(fd => fd.UserID == userID && fd.FlexiDate >= startDate && fd.FlexiDate <= endDate && fd.ApprovalStatusID == (int)ApprovalStatusType.Approved);

		public static bool IsFlexiDay(int userID, DateTime dateToCheck) => DB.FlexiDays
			.Where(fd => fd.UserID == userID && fd.FlexiDate == dateToCheck).Count() > 0;

		internal static FlexiDay GetFlexiDayByID(int userID, int flexiDayID) => DB.FlexiDays
					.SingleOrDefault(fd => fd.UserID == userID && fd.FlexiDayID == flexiDayID);

		#endregion FLEXIDAY

		#region LEAVE

		internal static Leave GetLeaveForDate(int userID, DateTime date) => DB.Leaves
					.SingleOrDefault(l => l.UserID == userID && l.DateFrom <= date && l.DateTo >= date);

		internal static Leave GetLeaveForDate(int userID, ApprovalStatusType approvalStatus, DateTime date) =>
			// Commented out code below as partial leave can return multiple results, therefore changed to FirstOrDefault
			//return DB.Leaves
			//        .SingleOrDefault(l => l.UserID == userID && l.ApprovalStatus == (int)approvalStatus && l.DateFrom <= date && l.DateTo >= date);

			DB.Leaves.FirstOrDefault(l => l.UserID == userID && l.ApprovalStatus == (int)approvalStatus && l.DateFrom <= date && l.DateTo >= date);

		internal static Leave GetLeaveForDate(int userID, LeaveTypeEnum leaveType, ApprovalStatusType approvalStatus, DateTime date) => DB.Leaves
					.SingleOrDefault(l => l.UserID == userID && l.LeaveType == (int)leaveType && l.ApprovalStatus == (int)approvalStatus && l.DateFrom <= date && l.DateTo >= date);

		internal static Leave GetLeaveByIDForUser(int userID, int leaveID) => DB.Leaves
					.SingleOrDefault(l => l.UserID == userID && l.LeaveID == leaveID);

		internal static IEnumerable<Leave> GetLeaveForUserForDates(int userID, DateTime dateFrom, DateTime dateTo) => DB.Leaves
						.Where(l => l.UserID == userID && (
															(l.DateFrom >= dateFrom && l.DateFrom <= dateTo)
													   || (l.DateTo >= dateFrom && l.DateTo <= dateTo)
													   || (l.DateFrom <= dateFrom && l.DateTo >= dateTo)
							   ));

		internal static IEnumerable<Leave> GetLeaveForUserForDates(int userID, LeaveTypeEnum leaveType, DateTime dateFrom, DateTime dateTo) => DB.Leaves
						.Where(l => l.UserID == userID && l.LeaveType == (int)leaveType && (
															(l.DateFrom >= dateFrom && l.DateFrom <= dateTo)
													   || (l.DateTo >= dateFrom && l.DateTo <= dateTo)
													   || (l.DateFrom <= dateFrom && l.DateTo >= dateTo)
							   ));

		internal static IEnumerable<Leave> GetLeaveForUserForDates(int userID, ApprovalStatusType approvalStatus, DateTime dateFrom, DateTime dateTo) => DB.Leaves
						.Where(l => l.UserID == userID && l.ApprovalStatus == (int)approvalStatus && (
															(l.DateFrom >= dateFrom && l.DateFrom <= dateTo)
													   || (l.DateTo >= dateFrom && l.DateTo <= dateTo)
													   || (l.DateFrom <= dateFrom && l.DateTo >= dateTo)
							   ));

		public static double GetLeaveDaysForUserForDates(int userID, ApprovalStatusType approvalStatus, DateTime dateFrom, DateTime dateTo) => DB.Leaves
						.Where(l => l.UserID == userID && l.ApprovalStatus == (int)approvalStatus && (
															(l.DateFrom >= dateFrom && l.DateFrom <= dateTo)
													   || (l.DateTo >= dateFrom && l.DateTo <= dateTo)
													   || (l.DateFrom <= dateFrom && l.DateTo >= dateTo)
							   )).Count();

		public static double GetTotalLeaveDaysForUserForDates(int userID, ApprovalStatusType approvalStatus, DateTime dateFrom, DateTime dateTo)
		{
			var balanceDate = dateFrom.AddMonths(-1).ToFirstDayOfMonth();

			var data = DB.IndividualLeaveDays
						.Where(l => l.UserID == userID && l.ApprovalStatus == (int)approvalStatus
							   && l.WorkDate >= dateFrom && l.WorkDate <= dateTo && l.BalanceDate == balanceDate);

			if (data.Count() > 0)
				return data.Sum(d => d.NumberOfDays);
			else
				return 0;
		}

		public static double GetTotalLeaveHoursForUserForDates(int userID, ApprovalStatusType approvalStatus, DateTime dateFrom, DateTime dateTo)
		{
			var balanceDate = dateTo.AddMonths(-1).ToFirstDayOfMonth();
			var data = DB.IndividualLeaveDays
						.Where(l => l.UserID == userID && l.ApprovalStatus == (int)approvalStatus
							   && l.WorkDate >= dateFrom && l.WorkDate <= dateTo && l.BalanceDate == balanceDate);

			if (data.Count() > 0)
				return data.Sum(d => d.HoursTaken);
			else
				return 0;
		}

		internal static IEnumerable<Leave> GetLeaveForUserForDates(int userID, LeaveTypeEnum leaveType, ApprovalStatusType approvalStatus, DateTime dateFrom, DateTime dateTo) => DB.Leaves
						.Where(l => l.UserID == userID && l.LeaveType == (int)leaveType && l.ApprovalStatus == (int)approvalStatus && (
															(l.DateFrom >= dateFrom && l.DateFrom <= dateTo)
													   || (l.DateTo >= dateFrom && l.DateTo <= dateTo)
													   || (l.DateFrom <= dateFrom && l.DateTo >= dateTo)
							   ));

		internal static Leave GetLeaveForUserForDate(int userID, LeaveTypeEnum leaveType, ApprovalStatusType approvalStatus, DateTime date) => DB.Leaves
					.SingleOrDefault(l => l.UserID == userID && l.LeaveType == (int)leaveType && l.ApprovalStatus == (int)approvalStatus && l.DateFrom <= date && l.DateTo >= date);

		public static IEnumerable<LeaveManualAdjustment> GetLeaveManualAdjustments(int userID, LeaveTypeEnum leaveType) => DB.LeaveManualAdjustments
					.Where(lma => lma.UserID == userID && lma.LeaveTypeID == (int)leaveType);

		public static IEnumerable<LeaveManualAdjustment> GetLeaveManualAdjustments(int userID) => DB.LeaveManualAdjustments
					.Where(lma => lma.UserID == userID);

		#endregion LEAVE

		#region LEAVEPARTIAL

		public static bool GetValidWorkDay(DateTime date)
		{
			int workDayCount = DB.WorkDays.Where(l => l.WorkDate == date && l.IsWorkable).Count();

			return workDayCount > 0;
		}

		public static bool IsDuplicateLeaveType(int userID, DateTime date, int leaveType)
		{
			int duplicateCount = DB.Leaves.Where(l => l.UserID == userID && l.DateFrom == date && l.LeaveType == leaveType && (l.ApprovalStatus == (int)ApprovalStatusType.Approved || l.ApprovalStatus == (int)ApprovalStatusType.Pending)).Count();

			return duplicateCount > 0;
		}

		public static bool HoursExceeded(int userID, DateTime date, double workHours, int? leaveHours = 0)
		{
			double existingHours = DB.Leaves.Where(l => l.UserID == userID && l.DateFrom == date && (l.ApprovalStatus == (int)ApprovalStatusType.Approved || l.ApprovalStatus == (int)ApprovalStatusType.Pending)).Select(s => s.NumberOfDays).DefaultIfEmpty(0).Sum();

			if (existingHours >= 1)
			{
				return true;
			}
			else
			{
				existingHours *= workHours;
			}

			return existingHours + leaveHours > workHours;
		}

		public static bool GetValidBirthdayDate(DateTime date, DateTime birthDate)
		{
			birthDate = birthDate.AddYears(date.Year - birthDate.Year);

			if (date == birthDate)
			{
				return true;
			}

			int i = date > birthDate ? -1 : 1;

			date = date.AddDays(i);

			while (date != birthDate)
			{
				if (!GetValidWorkDay(date))
				{
					date = date.AddDays(i);
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		public static bool BirthdayPeriodAlreadyTaken(int userID, DateTime date, DateTime birthDate)
		{
			birthDate = birthDate.AddYears(date.Year - birthDate.Year);

			bool alreadyTaken = false;
			bool workDay = false;

			date = birthDate;

			//Checking on actual birthday
			alreadyTaken = IsDuplicateLeaveType(userID, date, (int)LeaveTypeEnum.BirthDay);
			if (alreadyTaken)
				return true;

			//Checking first workday before birthday
			date = birthDate.AddDays(-1);

			while (!workDay)
			{
				if (GetValidWorkDay(date))
				{
					alreadyTaken = IsDuplicateLeaveType(userID, date, (int)LeaveTypeEnum.BirthDay);
					if (alreadyTaken)
						return true;
					else
						workDay = true;
				}
				else
				{
					date = date.AddDays(-1);
					workDay = false;
				}
			}

			date = birthDate.AddDays(1);
			workDay = false;

			//Checking first workday after birthday
			while (!workDay)
			{
				if (GetValidWorkDay(date))
				{
					alreadyTaken = IsDuplicateLeaveType(userID, date, (int)LeaveTypeEnum.BirthDay);
					if (alreadyTaken)
						return true;
					else
						workDay = true;
				}
				else
				{
					date = date.AddDays(1);
					workDay = false;
				}
			}

			return false;
		}

		#endregion LEAVEPARTIAL

		#region LEAVEBALANCE

		internal static void SaveLeaveBalance(Leave leave)
		{
			if (leave.LeaveID == 0)
				DB.Leaves.Add(leave);

			DB.SaveChanges();
		}

		internal static void DeleteLeaveBalances(int userID)
		{
			foreach (var lb in DB.LeaveBalances.Where(lb => lb.UserID == userID).ToList())
			{
				DB.LeaveBalances.Remove(lb);
				DB.SaveChanges();
			}
		}

		internal static LeaveBalance GetLeaveBalance(int userID, LeaveTypeEnum leaveType)
		{
			int Count = DB.LeaveBalances
									.Where(lb => lb.UserID == userID && lb.LeaveType == (int)leaveType)
									.Count();

			if (Count == 0)
				return null;

			var balanceDate = DB.LeaveBalances
									.Where(lb => lb.UserID == userID && lb.LeaveType == (int)leaveType)
									.Max(lb => lb.BalanceDate);

			return DB.LeaveBalances
					.SingleOrDefault(lb => lb.UserID == userID && lb.LeaveType == (int)leaveType && lb.BalanceDate == balanceDate);
		}

		internal static LeaveBalance GetLeaveBalanceForUserForDate(int userID, LeaveTypeEnum leaveType, DateTime date) => DB.LeaveBalances
											.Where(lb => lb.UserID == userID && lb.LeaveType == (int)leaveType && lb.BalanceDate == date)
											.SingleOrDefault();

		#endregion LEAVEBALANCE

		#region LEAVE MANUAL ADJUSTMENT

		internal static LeaveManualAdjustment GetLeaveManualAdjustmentByID(int id) => DB.LeaveManualAdjustments
											.Where(lma => lma.LeaveManualAdjustmentID == id)
											.SingleOrDefault();

		internal static LeaveManualAdjustment GetLeaveManualAdjustmentForUserForDate(int userID, LeaveTypeEnum leaveType, DateTime date) => DB.LeaveManualAdjustments
											.Where(lma => lma.UserID == userID && lma.LeaveTypeID == (int)leaveType && lma.EffectiveDate == date)
											.FirstOrDefault();

		public static IList<LeaveManualAdjustment> GetLeaveManualAdjustmentForUserForDateRange(int userID, LeaveTypeEnum leaveType, DateTime dateFrom, DateTime dateTo) => DB.LeaveManualAdjustments
				.Where(lma => lma.UserID == userID && lma.LeaveTypeID == (int)leaveType && lma.EffectiveDate >= dateFrom && lma.EffectiveDate <= dateTo)
				.ToList();

		public static void SaveLeaveManualAdjustment(LeaveManualAdjustment lma)
		{
			if (lma.LeaveManualAdjustmentID == 0)
				DB.LeaveManualAdjustments.Add(lma);

			DB.SaveChanges();
		}

		public static void DeleteLeaveManualAdjustment(LeaveManualAdjustment lma)
		{
			DB.LeaveManualAdjustments.Remove(lma);
			DB.SaveChanges();
		}

		#endregion LEAVE MANUAL ADJUSTMENT

		#region USER ALLOCATION

		public static UserAllocation GetUserAllocationByID(int id) => DB.UserAllocations
											.Where(ua => ua.UserAllocationID == id)
											.SingleOrDefault();

		public static UserAllocation GetUserAllocationByUserAndActivityID(int activityID, int userID) => DB.UserAllocations
									.Where(ua => ua.UserID == userID && ua.ActivityID == activityID)
									.OrderByDescending(ua => ua.StartDate)
									.FirstOrDefault();

		public static IEnumerable<UserAllocation> GetUserAllocationByUserID(int userID) => DB.UserAllocations
											.Where(ua => ua.UserID == userID);

		public static IEnumerable<UserAllocation> GetActiveUserAllocationByUserID(int userID) => (from ua in DB.UserAllocations
																								  join a in DB.Activities on ua.ActivityID equals a.ActivityID
																								  where ua.UserID == userID
																									 && ua.ScheduledEndDate == null
																									 && ua.ConfirmedEndDate == null
																									 && a.IsActive == true
																								  select new
																								  {
																									  ua.UserAllocationID,
																									  ua.UserID,
																									  ua.ActivityID,
																									  ua.ChargeRate,
																									  ua.StartDate,
																									  ua.ScheduledEndDate,
																									  ua.ConfirmedEndDate,
																									  ua.LastModifiedBy,
																									  ua.IsHidden
																								  }).ToList()
																								  .Select(x => new UserAllocation()
																								  {
																									  UserAllocationID = x.UserAllocationID,
																									  UserID = x.UserID,
																									  ActivityID = x.ActivityID,
																									  ChargeRate = x.ChargeRate,
																									  StartDate = x.StartDate,
																									  ScheduledEndDate = x.ScheduledEndDate,
																									  ConfirmedEndDate = x.ConfirmedEndDate,
																									  LastModifiedBy = x.LastModifiedBy,
																									  IsHidden = x.IsHidden
																								  });

		public static void SaveUserAllocation(UserAllocation ua)
		{
			if (ua.UserAllocationID == 0)
				DB.UserAllocations.Add(ua);

			DB.SaveChanges();
		}

		public static void DeleteUserAllocation(UserAllocation ua)
		{
			DB.UserAllocations.Remove(ua);
			DB.SaveChanges();
		}

		#endregion USER ALLOCATION

		#region PASSWORD OPERATIONS

		public static User GetForForgottenPassword(string userSearchString)
		{
			if (string.IsNullOrEmpty(userSearchString))
				return null;

			userSearchString = userSearchString.ToLower();

			return DB.Users
					.SingleOrDefault(u => (u.UserName.ToLower() == userSearchString || u.Email.ToLower() == userSearchString || u.FirstName.ToLower() + " " + u.LastName.ToLower() == userSearchString) && u.IsActive);
		}

		internal static User GetByForgottenPasswordToken(Guid token) => DB.Users
					.SingleOrDefault(u => u.ForgottenPasswordToken == token);

		#endregion PASSWORD OPERATIONS

		#region TIMESHEET APPROVALS

		public static IEnumerable<WorkAllocationExceptionModel> GetTimeCaptureExceptions(int userID, DateTime dateFrom, DateTime dateTo) => from u in DB.Users
																																			join ur in DB.UserRoles on u.UserID equals ur.UserID
																																			from wd in DB.WorkDays
																																			join wa in DB.WorkAllocationsWithLeave on new { u.UserID, DayWorked = wd.WorkDate } equals new { wa.UserID, wa.DayWorked } into waExists
																																			from waL in waExists.DefaultIfEmpty()
																																			join ws in DB.WorkSessionOverviews on new { u.UserID, DayWorked = wd.WorkDate } equals new { ws.UserID, ws.DayWorked } into wsExists
																																			from wsL in wsExists.DefaultIfEmpty()
																																			where wd.WorkDate >= dateFrom && wd.WorkDate <= dateTo && wd.IsWorkable
																																				  && ur.RoleID == 8
																																				  && u.UserID == userID
																																				  && (waL == null || (wsL == null && waL == null) || wsL.HoursDiff != 0)
																																			select new WorkAllocationExceptionModel
																																			{
																																				UserName = u.UserName,
																																				FullName = u.FullName,
																																				WorkDate = wd.WorkDate,
																																				ExceptionDetail = waL == null ? "No work allocation for day" : wsL == null ? "No work session for day" : "Over or under allocation of work hours",
																																				AllocationDifference = waL == null ? 0 - u.WorkHours : wsL == null ? 0 : wsL.HoursDiff
																																			};

		public static IEnumerable<WorkAllocationExceptionModel> GetDivisionTimeCaptureExceptions(DateTime dateFrom, DateTime dateTo, int divisionID) => from u in DB.Users
																																						join ur in DB.UserRoles on u.UserID equals ur.UserID
																																						from wd in DB.WorkDays
																																						join wa in DB.WorkAllocationsWithLeave on new { u.UserID, DayWorked = wd.WorkDate } equals new { wa.UserID, wa.DayWorked } into waExists
																																						from waL in waExists.DefaultIfEmpty()
																																						join ws in DB.WorkSessionOverviews on new { u.UserID, DayWorked = wd.WorkDate } equals new { ws.UserID, ws.DayWorked } into wsExists
																																						from wsL in wsExists.DefaultIfEmpty()
																																						where wd.WorkDate >= dateFrom && wd.WorkDate <= dateTo
																																							  && wd.IsWorkable
																																							  && ur.RoleID == 8
																																							  && u.IsActive
																																							  && u.DivisionID == divisionID
																																							  && (waL == null || (wsL == null && waL == null) || wsL.HoursDiff != 0)
																																						orderby u.FullName
																																						select new WorkAllocationExceptionModel
																																						{
																																							UserName = u.UserName,
																																							FullName = u.FullName,
																																							EmploymentDate = u.EmployStartDate,
																																							WorkDate = wd.WorkDate,
																																							ExceptionDetail = waL == null ? "No work allocation for day" : wsL == null ? "No work session for day" : "Over or under allocation of work hours",
																																							AllocationDifference = waL == null ? 0 - u.WorkHours : wsL == null ? 0 : wsL.HoursDiff
																																						};

		public static IEnumerable<WorkAllocationExceptionModel> GetAllTimeCaptureExceptions(DateTime dateFrom, DateTime dateTo)
		{
			var p = from u in DB.Users
					join ur in DB.UserRoles on u.UserID equals ur.UserID
					from wd in DB.WorkDays
					join wa in DB.WorkAllocationsWithLeave on new { u.UserID, DayWorked = wd.WorkDate } equals new { wa.UserID, wa.DayWorked } into waExists
					from waL in waExists.DefaultIfEmpty()
					join ws in DB.WorkSessionOverviews on new { u.UserID, DayWorked = wd.WorkDate } equals new { ws.UserID, ws.DayWorked } into wsExists
					from wsL in wsExists.DefaultIfEmpty()
					where wd.WorkDate >= dateFrom && wd.WorkDate <= dateTo
						  && wd.IsWorkable
						  && u.IsActive
						  && ur.RoleID == 8
						  && (waL == null || (wsL == null && waL == null) || wsL.HoursDiff != 0)
					orderby u.FullName
					select new WorkAllocationExceptionModel
					{
						UserName = u.UserName,
						FullName = u.FullName,
						EmploymentDate = u.EmployStartDate,
						WorkDate = wd.WorkDate,
						ExceptionDetail = waL == null ? "No work allocation for day" : wsL == null ? "No work session for day" : "Over or under allocation of work hours",
						AllocationDifference = waL == null ? 0 - u.WorkHours : wsL == null ? 0 : wsL.HoursDiff
					};

			return from u in DB.Users
				   join ur in DB.UserRoles on u.UserID equals ur.UserID
				   from wd in DB.WorkDays
				   join wa in DB.WorkAllocationsWithLeave on new { u.UserID, DayWorked = wd.WorkDate } equals new { wa.UserID, wa.DayWorked } into waExists
				   from waL in waExists.DefaultIfEmpty()
				   join ws in DB.WorkSessionOverviews on new { u.UserID, DayWorked = wd.WorkDate } equals new { ws.UserID, ws.DayWorked } into wsExists
				   from wsL in wsExists.DefaultIfEmpty()
				   where wd.WorkDate >= dateFrom && wd.WorkDate <= dateTo
						 && wd.IsWorkable
						 && u.IsActive
						 && ur.RoleID == 8
						 && (waL == null || (wsL == null && waL == null) || wsL.HoursDiff != 0)
				   orderby u.FullName
				   select new WorkAllocationExceptionModel
				   {
					   UserName = u.UserName,
					   FullName = u.FullName,
					   EmploymentDate = u.EmployStartDate,
					   WorkDate = wd.WorkDate,
					   ExceptionDetail = waL == null ? "No work allocation for day" : wsL == null ? "No work session for day" : "Over or under allocation of work hours",
					   AllocationDifference = waL == null ? 0 - u.WorkHours : wsL == null ? 0 : wsL.HoursDiff
				   };
		}

		public static IEnumerable<EncentivizeModel> GetDivisionTimeCaptureEncentivize(DateTime dateFrom, DateTime dateTo, int divisionID) => (from u in DB.Users
																																			  join ur in DB.UserRoles on u.UserID equals ur.UserID
																																			  from wd in DB.WorkDays
																																			  join wa in DB.WorkAllocationsWithLeave on new { u.UserID, DayWorked = wd.WorkDate } equals new { wa.UserID, wa.DayWorked } into waExists
																																			  from waL in waExists.DefaultIfEmpty()
																																			  join ws in DB.WorkSessionOverviews on new { u.UserID, DayWorked = wd.WorkDate } equals new { ws.UserID, ws.DayWorked } into wsExists
																																			  from wsL in wsExists.DefaultIfEmpty()
																																			  where wd.WorkDate >= dateFrom && wd.WorkDate <= dateTo
																																					&& wd.IsWorkable
																																					&& ur.RoleID == 8
																																					&& u.IsActive
																																					&& u.DivisionID == divisionID
																																					&& (waL != null || (wsL != null && waL != null) || wsL.HoursDiff == 0)
																																			  orderby u.UserName
																																			  select new EncentivizeModel
																																			  {
																																				  EmailAddress = u.Email,
																																				  Attended = "Yes",
																																				  FirstName = u.FirstName,
																																				  LastName = u.LastName,
																																				  PointsAwarded = 40
																																			  }).OrderBy(tk => tk.LastName).ThenBy(tg => tg.FirstName).Distinct();

		public static IEnumerable<EncentivizeModel> GetAllTimeEncentivize(DateTime dateFrom, DateTime dateTo) => (from u in DB.Users
																												  join ur in DB.UserRoles on u.UserID equals ur.UserID
																												  from wd in DB.WorkDays
																												  join wa in DB.WorkAllocationsWithLeave on new { u.UserID, DayWorked = wd.WorkDate } equals new { wa.UserID, wa.DayWorked } into waExists
																												  from waL in waExists.DefaultIfEmpty()
																												  join ws in DB.WorkSessionOverviews on new { u.UserID, DayWorked = wd.WorkDate } equals new { ws.UserID, ws.DayWorked } into wsExists
																												  from wsL in wsExists.DefaultIfEmpty()
																												  join uwA in DB.UserWorkAllocationExceptions on new { u.UserID, DayWorked = wd.WorkDate } equals new { uwA.UserID, uwA.DayWorked } into uwExists
																												  from uwAE in uwExists.DefaultIfEmpty()
																												  where wd.WorkDate >= dateFrom && wd.WorkDate <= dateTo
																														&& wd.IsWorkable
																														&& ur.RoleID == 8
																														&& u.IsActive
																														&& uwAE == null
																														//uwA.HoursWorked == wsL.HoursDiff &&
																														//(wsL != null) || wsL.HoursDiff == 0
																														&& (waL != null || (wsL != null && waL != null) || wsL.HoursDiff == 0)
																												  orderby u.UserName
																												  select new EncentivizeModel
																												  {
																													  EmailAddress = u.Email,
																													  Attended = "Yes",
																													  FirstName = u.FirstName,
																													  LastName = u.LastName,
																													  PointsAwarded = 40
																												  }).Distinct().OrderBy(tk => tk.LastName).ThenBy(tg => tg.FirstName);

		internal static TimesheetApproval GetTimesheetApprovalRecordForDate(int userID, DateTime date) => DB.TimesheetApprovals
					.SingleOrDefault(x => x.UserID == userID && x.MonthDate == date);

		internal static void LockTimesheet(TimesheetApproval record)
		{
			if (DB.TimesheetApprovals.Any(x => x.UserID == record.UserID && x.MonthDate == record.MonthDate))
				return;

			DB.TimesheetApprovals.Add(record);
			DB.SaveChanges();
		}

		internal static void UnlockTimesheet(int userID, DateTime dateFrom, DateTime dateTo)
		{
			var list = DB.TimesheetApprovals
											.Where(x => x.UserID == userID && x.MonthDate >= dateFrom && x.MonthDate <= dateTo)
											.ToList();

			if (list.Count <= 0)
				return;

			list.ForEach(x => DB.TimesheetApprovals.Remove(x));

			DB.SaveChanges();
		}

		#endregion TIMESHEET APPROVALS

		#region EXPENSE APPROVALS

		internal static List<ExpenseApproval> GetExpenseApprovalRecordsForDate(int userID, DateTime date) => DB.ExpenseApprovals
					.Where(x => x.UserID == userID && x.MonthDate == date).ToList();

		public static bool IsExpenseApproved(int userID, DateTime date, int expenseID)
		{
			var expense = DB.Expenses.FirstOrDefault(e => e.ExpenseDate.Year == date.Year
												&& e.ExpenseDate.Month == date.Month
												&& e.ExpenseDate.Day == date.Day
												&& e.UserID == userID
												&& e.ExpenseID == expenseID
												&& e.Locked);

			return expense != null;
		}

		internal static void LockExpense(ExpenseApproval record)
		{
			if (DB.ExpenseApprovals.Any(x => x.UserID == record.UserID && x.MonthDate == record.MonthDate))
				return;

			DB.ExpenseApprovals.Add(record);
			DB.SaveChanges();
		}

		internal static void UnlockExpense(int userID, DateTime dateFrom, DateTime dateTo)
		{
			var list = DB.ExpenseApprovals
											.Where(x => x.UserID == userID && x.MonthDate >= dateFrom && x.MonthDate <= dateTo)
											.ToList();

			if (list.Count <= 0)
				return;

			list.ForEach(x => DB.ExpenseApprovals.Remove(x));

			DB.SaveChanges();
		}

		#endregion EXPENSE APPROVALS

		#region EXCEL REPORT MODEL

		public static IEnumerable<UserReportModel> GetUserReportModel() => from u in DB.Users
																		   select new UserReportModel
																		   {
																			   UserName = u.UserName,
																			   FirstName = u.FirstName,
																			   LastName = u.LastName,
																			   PayRollRefNo = u.PayrollRefNo
																		   };

		#endregion EXCEL REPORT MODEL

		#region JSON LOOKUPS

		public static List<JsonLookup> AutoComplete(string partialEmail, int topCount) => (from u in DB.Users
																						   where u.IsActive && (u.UserName.StartsWith(partialEmail) || u.Email.StartsWith(partialEmail) || u.FirstName.Contains(partialEmail) || u.LastName.Contains(partialEmail))
																						   select new JsonLookup
																						   {
																							   id = u.UserID,
																							   label = u.FullName,
																							   value = u.FullName
																						   }).Distinct()
					.OrderBy(u => u.label)
					.Take(topCount)
					.ToList();

		public static List<JsonLookup> AutoCompleteUser(string partialUser, int topCount) => (from u in DB.Users
																							  where u.UserName.StartsWith(partialUser) || u.FirstName.StartsWith(partialUser)
																							  select new JsonLookup
																							  {
																								  id = u.UserID,
																								  label = u.FullName,
																								  value = u.FullName
																							  }).Distinct()
					.OrderBy(u => u.label)
					.Take(topCount)
					.ToList();

		#endregion JSON LOOKUPS

		#region SYSTEM USER

		internal static IEnumerable<User> GetSystemUsers() => DB.Users
					.Where(u => u.IsSystemUser);

		#endregion SYSTEM USER

		#region USER COST TO COMPANY

		internal static UserCostToCompany GetUserCostToCompanyForMonth(int userID, int period) => DB.UserCostToCompanies
					.SingleOrDefault(c => c.UserID == userID && c.Period == period);

		#endregion USER COST TO COMPANY

		#region TICKETMANAGER

		public static IEnumerable<User> GetAssignedUsersByTicketID(int ticketID) => (from u in DB.Users
																					 join tua in DB.TicketAssignmentChanges on u.UserID equals tua.ToUser
																					 where tua.TicketID == ticketID
																					 select u)
					 .Distinct();

		#endregion TICKETMANAGER
	}

	public class JsonLookup
	{
		public int id { get; set; }
		public string label { get; set; }
		public string value { get; set; }
	}

	public class JsonLookupDescription
	{
		public string value { get; set; }
	}
}