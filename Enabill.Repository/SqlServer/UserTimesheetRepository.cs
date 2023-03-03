using System;
using System.Collections.Generic;
using System.Linq;
using Alacrity.DataAccess;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repository.Interfaces;
using Newtonsoft.Json;

namespace Enabill.Repository.SqlServer
{
	public class UserTimesheetRepository : BaseRepository, IUserTimesheetRepository
	{
		public UserTimesheetRepository()
		{
		}

		public UserTimesheetRepository(IDbManager dbManager, IUserRepository userRepository)
			: base(dbManager)
		{
			this.userRepository = userRepository;
		}

		public void SaveUserTimesheet(DateTime month, TimesheetSchedule timesheetSchedule, int userID)
		{
			switch (timesheetSchedule)
			{
				case TimesheetSchedule.LastDayOfTheMonthMinusSevenDays:
					this.timesheetStartDate = month.ToFirstDayOfMonth();
					this.timesheetEndDate = month.ToLastDayOfMonth().AddDays(-7);
					break;

				default:
					this.timesheetStartDate = month.ToFirstDayOfMonth();
					this.timesheetEndDate = month.ToLastDayOfMonth().AddDays(-7);
					break;
			}

			this.userRepository.GetTimesheetByUserID(userID, this.timesheetStartDate, this.timesheetEndDate);
			this.usersWorkDayModel = this.userRepository.UsersWorkDayModel;

			var reasons = this.usersWorkDayModel.GroupBy(tf => tf.Reason).Select(t => new { Reason = t.Key, Count = t.Count() });

			string json = JsonConvert.SerializeObject(reasons);

			this._userTimesheets = new List<UserTimesheet>(){//DbManager.Connection.Query<UserTimesheet>(
			//    "INSERT INTO UserTimesheets(DateCreated, DateUpdated, UserID, Schedule,TimesheetStartDate,TimesheetEndDate,TimesheetObject)VALUES(@DateCreated, @DateUpdated, @UserID,@Schedule,@TimesheetStartDate,@TimesheetEndDate,@TimesheetObject)" + Environment.NewLine +
			//    "SELECT Id, DateCreated, DateUpdated, UserID, Schedule, TimesheetStartDate, TimesheetEndDate, TimesheetObject FROM UserTimesheets",
				new UserTimesheet()
				{
					DateCreated = DateTime.Now,
					DateUpdated = DateTime.Now,
					UserID = userID,
					Schedule = timesheetSchedule,
					TimesheetStartDate = timesheetStartDate,
					TimesheetEndDate = timesheetEndDate,
					//TimesheetObject = json
				} };
		}

		private DateTime timesheetStartDate;

		public DateTime TimesheetStartDate => this.timesheetStartDate;
		private DateTime timesheetEndDate;

		public DateTime TimesheetEndDate => this.timesheetEndDate;
		private IList<UserTimesheet> _userTimesheets;

		public IList<UserTimesheet> UserTimesheets => this._userTimesheets;
		private IUserRepository userRepository;

		public IUserRepository UserRepository => this.userRepository;
		private IList<UserWorkDayModel> usersWorkDayModel;
		public IList<UserWorkDayModel> UsersWorkDayModel => this.usersWorkDayModel;
	}
}