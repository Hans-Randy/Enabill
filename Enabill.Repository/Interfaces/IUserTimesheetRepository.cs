using System;
using System.Collections.Generic;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repository.Interfaces
{
	public interface IUserTimesheetRepository : IBaseRepository
	{
		void SaveUserTimesheet(DateTime month, TimesheetSchedule timesheetSchedule, int userID);

		DateTime TimesheetStartDate { get; }
		DateTime TimesheetEndDate { get; }
		IList<UserTimesheet> UserTimesheets { get; }
		IUserRepository UserRepository { get; }
		IList<UserWorkDayModel> UsersWorkDayModel { get; }
	}
}