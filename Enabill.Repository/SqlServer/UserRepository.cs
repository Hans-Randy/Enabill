using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Alacrity.DataAccess;
using Dapper;
using Enabill.Models;
using Enabill.Repos;
using Enabill.Repository.Interfaces;

namespace Enabill.Repository.SqlServer
{
	public class UserRepository : BaseRepository, IUserRepository
	{
		private User user;

		public User User => this.user;

		private User manager;

		public User Manager
		{
			get => this.manager;
			set => this.manager = value;
		}

		private IList<User> managers;

		public IList<User> Managers => this.managers;

		private IList<User> users;

		public IList<User> Users => this.users;

		private IList<WorkAllocation> workAllocations;

		public IList<WorkAllocation> WorkAllocations => this.workAllocations;

		private IList<Region> regions;

		public IList<Region> Regions => this.regions;

		private IList<BillableIndicator> billableIndicators;

		public IList<BillableIndicator> BillableIndicators => this.billableIndicators;

		private IList<Division> divisions;

		public IList<Division> Divisions => this.divisions;

		private IList<EmploymentType> employmentTypes;

		public IList<EmploymentType> EmploymentTypes => this.employmentTypes;

		private IList<Activity> activities;

		public IList<Activity> Activities => this.activities;

		private IList<Department> departments;

		public IList<Department> Departments => this.departments;
		private IList<Project> projects;

		public IList<Project> Projects => this.projects;

		private IList<Client> clients;

		public IList<Client> Clients => this.clients;

		private IList<TrainingCategory> trainingCategories;

		public IList<TrainingCategory> TrainingCategories => this.trainingCategories;

		private IList<UserWorkAllocation> usersWorkAllocations;

		public IList<UserWorkAllocation> UsersWorkAllocations => this.usersWorkAllocations;

		private IList<WorkDay> workDays;

		public IList<WorkDay> WorkDays => this.workDays;

		private IList<WorkSession> workSessions;

		public IList<WorkSession> WorkSessions => this.workSessions;

		private IList<WorkSession> nonWorkSessions;

		public IList<WorkSession> NonWorkSessions => this.nonWorkSessions;

		private IList<UserWorkDayModel> usersWorkDayModel;

		public IList<UserWorkDayModel> UsersWorkDayModel => this.usersWorkDayModel;
		private IList<Leave> leaves;

		public IList<Leave> Leaves => this.leaves;

		private IList<FlexiDay> flexiDays;

		public IList<FlexiDay> FlexiDays => this.flexiDays;

		public void GetUser(int userId)
		{
			var userResult = this.DbManager.Connection.QueryMultiple("GetUserById", new { UserId = userId }, commandType: CommandType.StoredProcedure);
			this.user = userResult.Read<User>().First();
		}

		public string GetUserFullName(int userId)
		{
			var userResult = this.DbManager.Connection.QueryMultiple("GetUserById", new { UserId = userId }, commandType: CommandType.StoredProcedure);
			var userFullName = userResult.Read<User>().First();
			return userFullName.FullName;
		}

		public void GetManagers() => this.managers = this.DbManager.Connection.Query<User>("GetManagers", commandType: CommandType.StoredProcedure).ToList();

		public void GetUsers() => this.users = this.DbManager.Connection.Query<User>("GetUsers", commandType: CommandType.StoredProcedure).ToList();

		public void GetUsersWorkAllocations(DateTime? dateFrom, DateTime? dateTo, int? managerId, int? userId, int? divisionId, int? clientId, int? projectId, int? activityId, int? employmentTypeId, int pageSize, int pageNumber, bool includeLeave, bool includeManagers = false)
		{
			var userWorkAllocationResult = this.DbManager.Connection.QueryMultiple("GetUsersWorkAllocationsWithLeave", new { DateFrom = dateFrom, DateTo = dateTo, IncludeLeave = includeLeave, IncludeManagers = includeManagers, ManagerID = managerId, UserID = userId }, commandType: CommandType.StoredProcedure);
			this.users = userWorkAllocationResult.Read<User>().ToList();
			if (includeManagers)
				this.managers = userWorkAllocationResult.Read<User>().ToList();
			this.workAllocations = userWorkAllocationResult.Read<WorkAllocation>().ToList();
			this.regions = userWorkAllocationResult.Read<Region>().ToList();
			this.billableIndicators = userWorkAllocationResult.Read<BillableIndicator>().ToList();
			this.divisions = userWorkAllocationResult.Read<Division>().ToList();
			this.employmentTypes = userWorkAllocationResult.Read<EmploymentType>().ToList();
			this.activities = userWorkAllocationResult.Read<Activity>().ToList();
			this.departments = userWorkAllocationResult.Read<Department>().ToList();
			this.projects = userWorkAllocationResult.Read<Project>().ToList();
			this.clients = userWorkAllocationResult.Read<Client>().ToList();
			this.trainingCategories = userWorkAllocationResult.Read<TrainingCategory>().ToList();

			if (includeLeave)
				this.leaves = userWorkAllocationResult.Read<Leave>().ToList();

			this.usersWorkAllocations = new List<UserWorkAllocation>();

			foreach (var p in this.projects)
			{
				p.Client = this.clients.FirstOrDefault(c => c.ClientID == p.ClientID);
				p.Department = this.departments.FirstOrDefault(d => d.DepartmentID == p.DepartmentID);
			}

			foreach (var a in this.activities)
			{
				a.Department = this.departments.FirstOrDefault(d => d.DepartmentID == a.DepartmentID);
				a.Region = this.regions.FirstOrDefault(r => r.RegionID == a.RegionID);
				a.Project = this.projects.FirstOrDefault(p => p.ProjectID == a.ProjectID);
			}

			foreach (var w in this.workAllocations)
			{
				w.Activity = this.activities.FirstOrDefault(a => a.ActivityID == w.ActivityID);
				w.TrainingCategory = this.trainingCategories.FirstOrDefault(tc => tc.TrainingCategoryID == w.TrainingCategoryID);
			}

			foreach (var u in this.users)
			{
				u.WorkAllocations = this.workAllocations.Where(wa => wa.UserID == u.UserID).ToList();
				u.Region = this.regions.FirstOrDefault(r => r.RegionID == u.RegionID);
				u.BillableIndicator = this.billableIndicators.FirstOrDefault(b => b.BillableIndicatorID == u.BillableIndicatorID);
				u.Division = this.divisions.FirstOrDefault(d => d.DivisionID == u.DivisionID);
				u.EmploymentType = this.employmentTypes.FirstOrDefault(e => e.EmploymentTypeID == u.EmploymentTypeID);

				foreach (var w in u.WorkAllocations)
				{
					this.usersWorkAllocations.Add(new UserWorkAllocation
					{
						UserId = w.UserID,
						FullNameManager = u.ManagerID.Value == 0 ? "" : this.GetUserFullName(u.ManagerID.Value),
						FullName = u.FullName,
						DivisionId = u.Division.DivisionID,
						DivisionName = u.Division.DivisionName,
						RegionId = w.Activity.Region.RegionID,
						RegionName = w.Activity.Region.RegionName,
						DepartmentId = w.Activity.Department.DepartmentID,
						DepartmentName = w.Activity.Department.DepartmentName,
						ClientId = w.Activity.Project.ClientID,
						ClientName = w.Activity.Project.GetClient().ClientName,
						ProjectId = w.Activity.Project.ProjectID,
						ProjectName = w.Activity.Project.ProjectName,
						ActivityId = w.Activity.ActivityID,
						ActivityName = w.Activity.ActivityName,
						DayWorked = w.DayWorked,
						HoursWorked = w.HoursWorked,
						WorkHours = u.WorkHours,
						PercentageAllocation = u.PercentageAllocation,
						Period = w.Period,
						EmploymentTypeId = u.EmploymentTypeID,
						EmploymentType = u.EmploymentType.EmploymentTypeName,
						TotalHours = w.TotalHours,
						TotalHoursWorkable = WorkDayRepo.GetTotalWorkDays(dateFrom, dateTo) * u.WorkHours,
						TrainingInstitute = w.TrainingInstitute,
						TrainerName = w.TrainerName,
						TrainingType = w.TrainingCategory.TrainingCategoryName == "Please select" ? "" : w.TrainingCategory.TrainingCategoryName,
						Remark = w.Remark,
						WorkAllocationId = w.WorkAllocationID
					});
				}

				if (includeLeave)
				{
					u.Leaves = this.Leaves.Where(l => l.UserID == u.UserID).ToList();

					foreach (var l in u.Leaves)
					{
						this.usersWorkAllocations.Add(new UserWorkAllocation
						{
							UserId = l.UserID,
							FullNameManager = u.ManagerID.Value == 0 ? "" : this.GetUserFullName(u.ManagerID.Value),
							FullName = u.FullName,
							DivisionId = u.Division.DivisionID,
							DivisionName = u.Division.DivisionName,
							RegionId = u.Region.RegionID,
							RegionName = u.Region.RegionName,
							DepartmentId = 0,
							DepartmentName = "",
							ClientId = 0,
							ClientName = "",
							ProjectId = 0,
							ProjectName = "",
							ActivityId = 0,
							ActivityName = ((LeaveTypeEnum)l.LeaveType).GetEnumDescription(),
							DayWorked = l.WorkDate ?? l.DateFrom,
							HoursWorked = l.NumberOfHours ?? 0,
							PercentageAllocation = u.PercentageAllocation,
							Period = l.DateFrom.ToPeriod(),
							EmploymentTypeId = u.EmploymentTypeID,
							EmploymentType = u.EmploymentType.EmploymentTypeName,
							TotalHours = 0,
							TotalHoursWorkable = 0,
							TrainingInstitute = "",
							TrainerName = "",
							TrainingType = "",
							Remark = "",
							WorkAllocationId = 0
						});
					}
				}
			}

			if (divisionId.HasValue && divisionId.Value > 0)
				this.usersWorkAllocations = this.usersWorkAllocations.Where(u => u.DivisionId == divisionId.Value).ToList();

			if (clientId.HasValue && clientId.Value > 0)
				this.usersWorkAllocations = this.usersWorkAllocations.Where(u => u.ClientId == clientId.Value).ToList();

			if (projectId.HasValue && projectId.Value > 0)
			{
				this.usersWorkAllocations = this.usersWorkAllocations.Where(u => u.ProjectId == projectId).ToList();
			}

			if (activityId.HasValue && activityId.Value > 0)
			{
				this.usersWorkAllocations = this.usersWorkAllocations.Where(u => u.ActivityId == activityId).ToList();
			}

			if (employmentTypeId.HasValue && employmentTypeId.Value > 0)
			{
				this.usersWorkAllocations = this.usersWorkAllocations.Where(u => u.EmploymentTypeId == employmentTypeId.Value).ToList();
			}
		}

		//public void InsertUserNonWorkSessions(DateTime? dateFrom, DateTime? dateTo, int userId) => this.DbManager.Connection.Execute("InsertNonWorkSessionDaysByUserID", new { @DateTimeFrom = dateFrom, @DateTimeTo = dateTo, UserId = userId }, commandType: CommandType.StoredProcedure);
		public void InsertUserNonWorkSessions(DateTime? dateFrom, DateTime? dateTo, int userId) => this.DbManager.Connection.Execute("InsertNonWorkSessionDaysByUserID", new { @DateFrom = dateFrom, @DateTo = dateTo, UserId = userId }, commandType: CommandType.StoredProcedure);

		public void GetTimesheets(DateTime dateFrom, DateTime dateTo, int managerID, UserWorkDayStatus status)
		{
			var usersResult = this.DbManager.Connection.QueryMultiple("GetTimesheets", new { DateFrom = dateFrom.Date, DateTo = dateTo.Date }, commandType: CommandType.StoredProcedure);
			this.users = usersResult.Read<User>().Where(u => u.IsActive).ToList();

			if (managerID > 0 && this.users.Any(u => u.ManagerID == managerID))
				this.users = this.users.Where(u => u.ManagerID == managerID).ToList();

			this.users = this.users.OrderBy(u => u.UserName).ToList();
			this.workAllocations = usersResult.Read<WorkAllocation>().ToList();
			this.workSessions = usersResult.Read<WorkSession>().ToList();
			this.workDays = usersResult.Read<WorkDay>().Where(t => t.WorkDate >= dateFrom && t.WorkDate <= dateTo).ToList();
			this.leaves = usersResult.Read<Leave>().ToList();
			this.flexiDays = usersResult.Read<FlexiDay>().ToList();
			this.usersWorkDayModel = new List<UserWorkDayModel>();
			var leaveLookup = new List<LeaveDayModel>();

			//Got all my data I needed
			foreach (var u in this.users)
			{
				foreach (var w in this.workDays)
				{
					var uwd = new UserWorkDayModel
					{
						WorkDay = w,
						User = u
					};
					this.usersWorkDayModel.Add(uwd);
				}
			}

			this.BuildLeaveAssociatedLookup(leaveLookup);
			this.BuildUserWorkDayModel(leaveLookup);
			this.CheckLeaveAllocation();
			this.SetUserWorkDayStatus();

			if (status != UserWorkDayStatus.All)
			{
				this.usersWorkDayModel = this.usersWorkDayModel.Where(u => u.Status == status).OrderBy(t => t.User.UserName).ToList();
			}
			else
			{
				this.usersWorkDayModel = this.usersWorkDayModel.OrderBy(uw => uw.User.UserName).ToList();
			}
		}

		public void GetTimesheetByUserID(int userId, DateTime dateFrom, DateTime dateTo)
		{
			var userResult = this.DbManager.Connection.QueryMultiple("GetTimeSheetByUserID", new { UserId = userId, DateFrom = dateFrom, DateTo = dateTo }, commandType: CommandType.StoredProcedure);
			this.users = userResult.Read<User>().ToList();
			this.user = userResult.Read<User>().First();
			this.workAllocations = userResult.Read<WorkAllocation>().ToList();
			this.workSessions = userResult.Read<WorkSession>().ToList();
			this.workDays = userResult.Read<WorkDay>().ToList();
			this.leaves = userResult.Read<Leave>().ToList();
			this.flexiDays = userResult.Read<FlexiDay>().ToList();

			this.usersWorkDayModel = new List<UserWorkDayModel>();
			var leaveLookup = new List<LeaveDayModel>();

			//Got all my data I needed
			foreach (var w in this.workDays)
			{
				var uwd = new UserWorkDayModel
				{
					WorkDay = w,
					User = user
				};
				this.usersWorkDayModel.Add(uwd);
			}

			this.BuildLeaveAssociatedLookup(leaveLookup);
			this.BuildUserWorkDayModel(leaveLookup);
			this.CheckLeaveAllocation();
			this.SetUserWorkDayStatus();
		}

		public void GetUserById(int userId) => this.user = this.DbManager.Connection.Query<User>("GetUserById", new { UserId = userId }, commandType: CommandType.StoredProcedure).First();

		private void BuildLeaveAssociatedLookup(List<LeaveDayModel> leaveLookup)
		{
			foreach (var l in this.leaves)
			{
				for (var date = l.DateFrom; date <= l.DateTo; date = date.AddDays(1))
				{
					leaveLookup.Add(new LeaveDayModel { LeaveDate = date, LeaveType = (LeaveTypeEnum)l.LeaveType, ApprovalStatus = (ApprovalStatusType)l.ApprovalStatus, UserId = l.UserID });
				}
			}
		}

		private void BuildUserWorkDayModel(List<LeaveDayModel> leaveLookup)
		{
			foreach (var w in this.usersWorkDayModel)
			{
				w.HasException = false;
				w.IsBeforeStartDay = false;
				w.WorkSessions = this.workSessions.Where(ws => ws.UserID == w.User.UserID && ws.StartTime.Date == w.WorkDay.WorkDate.Date).ToList();

				if (leaveLookup.Any(l => l.LeaveDate.Date == w.WorkDay.WorkDate.Date && l.UserId == w.User.UserID))
				{
					w.Leave = leaveLookup.Where(l => l.LeaveDate.Date == w.WorkDay.WorkDate.Date && l.UserId == w.User.UserID).ToList();
				}

				w.FlexiDays = this.flexiDays.Where(f => f.FlexiDate.Date == w.WorkDay.WorkDate.Date && f.UserID == w.User.UserID).ToList();
				w.WorkAllocations = this.workAllocations.Where(wa => wa.DayWorked.Date == w.WorkDay.WorkDate.Date && wa.UserID == w.User.UserID).ToList();
			}
		}

		private void CheckLeaveAllocation()
		{
			foreach (var w in this.usersWorkDayModel)
			{
				w.AllocatedTime = w.WorkAllocations.Select(wr => wr.HoursWorked).Sum();
				w.TotalTime = w.WorkSessions.Select(wr => wr.TotalTime).Sum();
				w.UnAllocatedTime = w.TotalTime - w.AllocatedTime;

				if (w.Leave != null)
				{
					if (w.Leave.Any(l => l.ApprovalStatus == ApprovalStatusType.Approved))
					{
						w.IsLeaveDay = true;
					}
					else if (w.Leave.Any(l => l.ApprovalStatus == ApprovalStatusType.Pending))
					{
						w.IsPendingLeaveDay = true;
					}
				}
				else
				{
					w.IsLeaveDay = false;
				}

				if (w.FlexiDays != null)
					w.IsFlexiDay = w.FlexiDays.Any(f => (ApprovalStatusType)f.ApprovalStatusID == ApprovalStatusType.Approved);
				else
					w.IsFlexiDay = false;

				if (w.WorkSessions.Any(ws => (WorkSessionStatusType)ws.WorkSessionStatusID == WorkSessionStatusType.Exception))
				{
					w.HasException = true;
				}
				else if (w.WorkDay.IsWorkable && w.WorkSessions.Any(ws => ws.WorkSessionStatusID == 0) && !w.IsLeaveDay && !w.IsFlexiDay)
				{
					w.HasException = true;
				}
				else if (w.WorkSessions.Count == 0 && w.WorkDay.IsWorkable && !w.IsLeaveDay && !w.IsFlexiDay)
				{
					w.HasException = true;
				}
				else if (w.UnAllocatedTime > 0 && w.WorkDay.IsWorkable && !w.IsLeaveDay && !w.IsFlexiDay)
				{
					w.HasException = true;
				}

				if (w.WorkDay.WorkDate.Date <= w.User.EmployStartDate.Date)
				{
					w.HasException = false;
					w.IsBeforeStartDay = true;
				}
				else if (w.User.EmployEndDate.HasValue)
				{
					if (w.WorkDay.WorkDate.Date >= w.User.EmployEndDate.Value.Date)
					{
						w.IsAfterEndDate = true;
					}
				}
				else if (w.WorkDay.IsWorkable)
				{
					if (w.IsFlexiDay)
					{
						w.HasException = false;
					}
					else if (w.IsLeaveDay)
					{
						w.HasException = false;
					}
					else if (w.IsPendingLeaveDay)
					{
						w.HasException = true;
					}
				}
			}
		}

		private void SetUserWorkDayStatus()
		{
			foreach (var w in this.usersWorkDayModel)
			{
				if (w.HasException)
				{
					w.Status = UserWorkDayStatus.Exception;
				}
				else if (w.WorkSessions.Any(ww => (WorkSessionStatusType)ww.WorkSessionStatusID == WorkSessionStatusType.UnApproved))
				{
					w.Status = UserWorkDayStatus.Unapproved;
				}
				else
				{
					w.Status = UserWorkDayStatus.Approved;
				}

				if (!w.WorkDay.IsWorkable)
				{
					w.Reason = WorkDayReason.NotWorkable;
				}
				else if (w.IsLeaveDay)
				{
					w.Reason = WorkDayReason.LeaveDay;
				}
				else if (w.IsFlexiDay)
				{
					w.Reason = WorkDayReason.FlexiDay;
				}
				else if (w.IsBeforeStartDay)
				{
					w.Reason = WorkDayReason.BeforeStartDate;
				}
				else if (w.IsAfterEndDate)
				{
					w.Reason = WorkDayReason.AfterEndDate;
				}
				else if (w.UnAllocatedTime > 0)
				{
					w.Reason = WorkDayReason.CorrectTimeAllocation;
				}
			}
		}

		public UserRepository()
		{
		}

		public UserRepository(IDbManager dbManager)
			: base(dbManager)
		{
		}
	}
}