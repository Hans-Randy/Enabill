using System;
using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Repository.Interfaces
{
	public interface IUserRepository : IBaseRepository
	{
		User User { get; }
		User Manager { get; }
		IList<User> Managers { get; }
		IList<User> Users { get; }
		IList<WorkAllocation> WorkAllocations { get; }
		IList<Region> Regions { get; }
		IList<BillableIndicator> BillableIndicators { get; }
		IList<Division> Divisions { get; }
		IList<EmploymentType> EmploymentTypes { get; }
		IList<Activity> Activities { get; }
		IList<Department> Departments { get; }
		IList<Project> Projects { get; }
		IList<Client> Clients { get; }
		IList<TrainingCategory> TrainingCategories { get; }
		IList<UserWorkAllocation> UsersWorkAllocations { get; }
		IList<WorkDay> WorkDays { get; }
		IList<WorkSession> WorkSessions { get; }
		IList<WorkSession> NonWorkSessions { get; }
		IList<Leave> Leaves { get; }
		IList<FlexiDay> FlexiDays { get; }
		IList<UserWorkDayModel> UsersWorkDayModel { get; }

		void GetManagers();

		void GetUser(int userId);

		void GetUsers();

		void GetUsersWorkAllocations(DateTime? dateFrom, DateTime? dateTo, int? managerId, int? userId, int? divisionId, int? clientId, int? projectId, int? activityId, int? employmentTypeId, int pageSize, int pageNumber, bool includeLeave, bool includeManagers);

		void InsertUserNonWorkSessions(DateTime? dateFrom, DateTime? dateTo, int userId);

		void GetTimesheets(DateTime dateFrom, DateTime dateTo, int managerID, UserWorkDayStatus status);

		void GetTimesheetByUserID(int userId, DateTime dateFrom, DateTime dateTo);

		void GetUserById(int userId);
	}
}