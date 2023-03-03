using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;
using Enabill.Repository.Interfaces;

namespace Enabill.Web.Models
{
	public class ActivityReportDisplayModel
	{
		private IUserRepository userRepository;

		#region INITIALIZATION

		public ActivityReportDisplayModel(DateTime? fromPeriod = null, DateTime? toPeriod = null, int? divisionID = null, int? managerID = null, int? userID = null, int? clientID = null, int? projectID = null, int? activityID = null, int? employmentTypeID = null)
		{
			this.UserTimeSplitReportModels = UserTimeSplitRepo.GetAll(fromPeriod, toPeriod, divisionID, managerID, userID, clientID, projectID, activityID, employmentTypeID);
		}

		public ActivityReportDisplayModel()
		{
		}

		public ActivityReportDisplayModel(IUserRepository userRepository)
		{
			this.userRepository = userRepository;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		private int? managerId;
		public int? ManagerId => this.managerId;

		private int? userId;
		public int? UserId => this.userId;

		private int? clientId;
		public int? ClientId => this.clientId;

		private int? projectId;
		public int? ProjectId => this.projectId;

		private int? activityId;
		public int? ActivityId => this.activityId;

		private int? divisionId;
		public int? DivisionId => this.divisionId;

		private int? employmentTypeId;
		public int? EmploymentTypeId => this.employmentTypeId;

		private int pageNumber;
		public int PageNumber => this.pageNumber;

		private int pageSize;
		public int PageSize => this.pageSize;

		private int percentageAllocation;
		public int PercentageAllocation => this.percentageAllocation;

		private int totalPages;
		public int TotalPages => this.totalPages;

		private int totalRecords;
		public int TotalRecords => this.totalRecords;

		private double totalHoursWorked;
		public double TotalHoursWorked => this.totalHoursWorked;

		private DateTime dateFrom;
		public DateTime DateFrom => this.dateFrom;

		private DateTime dateTo;
		public DateTime DateTo => this.dateTo;

		private IList<Activity> activities;
		public IList<Activity> Activities => this.activities;

		private IList<Client> clients;
		public IList<Client> Clients => this.clients;

		private IList<Division> divisions;
		public IList<Division> Divisions => this.divisions;

		private IList<EmploymentType> employmentTypes;
		public IList<EmploymentType> EmploymentTypes => this.employmentTypes;

		private IList<Project> projects;
		public IList<Project> Projects => this.projects;

		private IList<User> managers;
		public IList<User> Managers => this.managers;

		private IList<User> users;
		public IList<User> Users => this.users;

		public IList<UserTimeSplitReportModel> UserTimeSplitReportModels
		{
			get;
			set;
		}

		private IList<UserWorkAllocation> pagedUsersWorkAllocations;
		public IList<UserWorkAllocation> PagedUsersWorkAllocations => this.pagedUsersWorkAllocations;

		private IList<UserWorkAllocation> usersWorkAllocations;
		public IList<UserWorkAllocation> UsersWorkAllocations => this.usersWorkAllocations;

		#endregion PROPERTIES

		#region FUNCTIONS

		public void GetUsersWorkAllocations(string dateFrom, string dateTo, int? managerId, int? userId, int? divisionId, int? clientId, int? projectId, int? activityId, int? employmentTypeId, int pageSize, int pageNumber, bool includeLeave = false, bool includeManagers = false)
		{
			var enZA = new CultureInfo("en-ZA");
			var dtFrom = DateTime.Now.ToFirstDayOfMonth();
			var dtTo = DateTime.Now;

			if (!string.IsNullOrEmpty(dateFrom))
			{
				bool d = DateTime.TryParseExact(dateFrom, "yyyy-MM-dd", enZA, DateTimeStyles.None, out dtFrom);

				if (!d)
				{
					dtFrom = DateTime.Now.ToFirstDayOfMonth();
				}
			}
			else
			{
				dtFrom = DateTime.Now.ToFirstDayOfMonth();
			}

			if (!string.IsNullOrEmpty(dateTo))
			{
				bool d = DateTime.TryParseExact(dateTo, "yyyy-MM-dd", enZA, DateTimeStyles.None, out dtTo);

				if (!d)
				{
					dtTo = DateTime.Now.ToLastDayOfMonth();
				}
			}
			else
			{
				dtTo = DateTime.Now.ToLastDayOfMonth();
			}

			this.dateFrom = dtFrom;
			this.dateTo = dtTo;

			this.managerId = managerId;
			this.userId = userId;
			this.divisionId = divisionId;
			this.clientId = clientId;
			this.projectId = projectId;
			this.activityId = activityId;
			this.employmentTypeId = employmentTypeId;

			if (pageNumber == 0)
				pageNumber = 1;

			this.userRepository.GetUsersWorkAllocations(this.dateFrom, this.dateTo, this.managerId, this.userId, divisionId, clientId, projectId, activityId, employmentTypeId, pageSize, pageNumber, includeLeave, includeManagers);
			this.managers = this.userRepository.Managers;
			this.users = this.userRepository.Users;
			this.divisions = this.userRepository.Divisions;
			this.clients = this.userRepository.Clients;
			this.projects = this.userRepository.Projects;
			this.activities = this.userRepository.Activities;
			this.employmentTypes = this.userRepository.EmploymentTypes;

			this.usersWorkAllocations = this.userRepository.UsersWorkAllocations;
			this.usersWorkAllocations = this.usersWorkAllocations.OrderBy(c => c.FullNameManager).ThenBy(c => c.ClientName).ThenBy(c => c.ProjectName).ThenBy(c => c.FullName).ThenBy(c => c.ActivityName).ThenByDescending(c => c.DayWorked).ToList();

			if (pageNumber == 0)
				this.pageNumber = 1;

			this.pageNumber = pageNumber;
			this.totalRecords = this.usersWorkAllocations.Count;
			this.totalHoursWorked = this.usersWorkAllocations.Select(u => u.HoursWorked).Sum();
			this.pageSize = pageSize;

			if (this.totalRecords > 0)
				this.totalPages = (this.totalRecords / pageSize) + 1;
			else
				this.totalPages = 0;

			this.pagedUsersWorkAllocations = this.usersWorkAllocations.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

			if (this.totalRecords == 0)
				this.pageNumber = 0;
		}

		#endregion FUNCTIONS
	}
}