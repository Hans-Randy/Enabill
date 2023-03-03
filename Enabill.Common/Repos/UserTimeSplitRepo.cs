using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public class UserTimeSplitRepo : BaseRepo
	{
		#region USER TIME SPLIT

		public static List<UserTimeSplitReportModel> GetAll(DateTime fromPeriod, DateTime toPeriod, int clientID, string project, string activity, string employmentType, string department = "0", int userID = 0)
		{
			var data = from uts in DB.UserTimeSplits
					   join u in DB.Users on uts.UserID equals u.UserID
					   join e in DB.EmploymentTypes on u.EmploymentTypeID equals e.EmploymentTypeID
					   where uts.DayWorked >= fromPeriod
							  && uts.DayWorked <= toPeriod
					   select (new UserTimeSplitReportModel()
					   {
						   WorkAllocationID = uts.WorkAllocationID,
						   UserID = uts.UserID,
						   FullName = uts.FullName,
						   EmploymentType = e.EmploymentTypeName,
						   DivisionName = uts.DivisionName,
						   RegionName = uts.RegionName,
						   DepartmentName = uts.DepartmentName,
						   ClientID = uts.ClientID,
						   ClientName = uts.ClientName,
						   ProjectID = uts.ProjectID,
						   ProjectName = uts.ProjectName,
						   ActivityID = uts.ActivityID,
						   ActivityName = uts.ActivityName,
						   Period = uts.Period,
						   DayWorked = uts.DayWorked,
						   HoursWorked = uts.HoursWorked,
						   Remark = uts.Remark,
						   TrainingType = uts.TrainingType,
						   TrainerName = uts.TrainerName,
						   TrainingInstitute = uts.TrainingInstitute
					   }
					   );

			if (clientID != 0)
				data = data.Where(d => d.ClientID == clientID);

			if (employmentType != "0")
				data = data.Where(d => d.EmploymentType == employmentType);

			if (project != "0")
				data = data.Where(d => d.ProjectName == project);

			if (activity != "0")
				data = data.Where(d => d.ActivityName.Contains(activity));

			if (department != "0")
				data = data.Where(d => d.DepartmentName == department);

			if (userID != 0)
				data = data.Where(d => d.UserID == userID);

			return data.Distinct().OrderBy(u => u.FullName).ToList();
		}

		public static List<UserTimeSplitReportModel> GetAll(DateTime? fromPeriod = null, DateTime? toPeriod = null, int? divisionID = null, int? managerID = null, int? userID = null, int? clientID = null, int? projectID = null, int? activityID = null, int? employmentTypeID = null)
		{
			if (!fromPeriod.HasValue)
				fromPeriod = DateTime.Today.AddMonths(-1).ToFirstDayOfMonth();

			if (!toPeriod.HasValue)
				toPeriod = DateTime.Now.ToLastDayOfMonth();

			// Get full report for those having the Forecast Administrator role. otherwise per manager.
			if (managerID.HasValue)
			{
				var user = UserRepo.GetByID(managerID.Value);

				if (user.HasRole(UserRoleType.ForecastAdministrator))
					managerID = null;
			}

			var data = from uts in DB.UserTimeSplits
					   join u in DB.Users on uts.UserID equals u.UserID
					   join e in DB.EmploymentTypes on u.EmploymentTypeID equals e.EmploymentTypeID
					   where uts.DayWorked >= fromPeriod
							   && uts.DayWorked <= toPeriod
					   select (new UserTimeSplitReportModel()
					   {
						   WorkAllocationID = uts.WorkAllocationID,
						   ManagerID = u.ManagerID.Value,
						   UserID = uts.UserID,
						   FullName = uts.FullName,
						   EmploymentType = e.EmploymentTypeName,
						   EmploymentTypeID = e.EmploymentTypeID,
						   DivisionName = uts.DivisionName,
						   DivisionID = u.DivisionID,
						   RegionName = uts.RegionName,
						   DepartmentName = uts.DepartmentName,
						   ClientID = uts.ClientID,
						   ClientName = uts.ClientName,
						   ProjectID = uts.ProjectID,
						   ProjectName = uts.ProjectName,
						   ActivityID = uts.ActivityID,
						   ActivityName = uts.ActivityName,
						   Period = uts.Period,
						   DayWorked = uts.DayWorked,
						   HoursWorked = uts.HoursWorked,
						   Remark = uts.Remark,
						   TrainingType = uts.TrainingType,
						   TrainerName = uts.TrainerName,
						   TrainingInstitute = uts.TrainingInstitute
					   }
					   );

			if (divisionID.HasValue && divisionID.Value != 0)
				data = data.Where(d => d.DivisionID == divisionID.Value);

			if (managerID.HasValue && managerID.Value != 0)
				data = data.Where(d => d.ManagerID == managerID.Value);

			if (userID.HasValue && userID.Value != 0)
				data = data.Where(d => d.UserID == userID.Value);

			if (clientID.HasValue && clientID.Value != 0)
				data = data.Where(d => d.ClientID == clientID.Value);

			if (projectID.HasValue && projectID.Value != 0)
				data = data.Where(d => d.ProjectID == projectID.Value);

			if (activityID.HasValue && activityID.Value != 0)
				data = data.Where(d => d.ActivityID == activityID.Value);

			if (employmentTypeID.HasValue && employmentTypeID.Value != 0)
			{
				data = data.Where(d => d.EmploymentTypeID == employmentTypeID.Value);
			}

			return data.Distinct().OrderBy(u => u.FullName).ToList();
		}

		#endregion USER TIME SPLIT
	}
}