using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Repository.Interfaces;

namespace Enabill.Web.Models
{
	public class TimeApproveModel
	{
		#region INITIALIZATION

		public TimeApproveModel()
		{
		}

		public TimeApproveModel(IUserRepository userRepository)
		{
			this.userRepository = userRepository;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		private int managerID;
		public int ManagerID => this.managerID;

		private DateTime dateFrom;
		public DateTime DateFrom => this.dateFrom;

		private DateTime dateTo;
		public DateTime DateTo => this.dateTo;

		private TimesheetSchedule _timesheetSchedule;
		public TimesheetSchedule TimesheetSchedule => this._timesheetSchedule;

		private UserWorkDayStatus userWorkDayStatus;
		public UserWorkDayStatus UserWorkDayStatus => this.userWorkDayStatus;

		private User user;

		public User User
		{
			get => this.user;
			set => this.user = value;
		}

		private IUserRepository userRepository;

		private IList<User> users;
		public IList<User> Users => this.users;

		private IList<UserWorkDayModel> usersWorkDayModel;
		public IList<UserWorkDayModel> UsersWorkDayModel => this.usersWorkDayModel;

		#endregion PROPERTIES

		#region FUNCTIONS

		public void GetUsersWorkSessions(DateTime? dateFrom, DateTime? dateTo, int managerID, UserWorkDayStatus status)
		{
			this.dateFrom = dateFrom ?? DateTime.Now.ToFirstDayOfMonth();

			this.dateTo = dateTo ?? DateTime.Now;

			this.managerID = managerID;
			this.userRepository.GetTimesheets(this.dateFrom, this.dateTo, managerID, status);
			this.usersWorkDayModel = this.userRepository.UsersWorkDayModel;

			foreach (var u in this.usersWorkDayModel)
			{
				if (u.IsBeforeStartDay)
				{
					u.Reason = WorkDayReason.BeforeStartDate;
				}
				else if (u.IsAfterEndDate)
				{
					u.Reason = WorkDayReason.AfterEndDate;
				}
				else if (u.WorkDay.IsWorkable)
				{
					if (u.IsLeaveDay)
					{
						u.Reason = WorkDayReason.LeaveDay;
					}
					else if (u.IsPendingLeaveDay)
					{
						u.Reason = WorkDayReason.LeaveDay;
					}
					else if (u.IsFlexiDay)
					{
						u.Reason = WorkDayReason.FlexiDay;
					}
					else if (u.TotalTime > 0 && u.AllocatedTime > 0 && u.TotalTime != u.AllocatedTime)
					{
						u.Reason = WorkDayReason.IncorrectTimeAllocation;
					}
					else if (u.TotalTime == 0)
					{
						u.Reason = WorkDayReason.NoTimeAllocation;
					}
					else
					{
						u.Reason = WorkDayReason.CorrectTimeAllocation;
					}
				}
				else
				{
					if (u.TotalTime > 0 && u.TotalTime != u.AllocatedTime)
					{
						u.Reason = WorkDayReason.IncorrectTimeAllocation;
					}
					else if (u.TotalTime > 0 && u.AllocatedTime > 0 && u.TotalTime == u.AllocatedTime)
					{
						u.Reason = WorkDayReason.CorrectTimeAllocation;
					}
					else
					{
						u.Reason = WorkDayReason.NotWorkable;
					}
				}
			}

			this.users = this.userRepository.Users;
			this.user = this.userRepository.Users.FirstOrDefault();
			this.userWorkDayStatus = status;
		}

		public void GetUserWorkSessions(DateTime? dateFrom, DateTime? dateTo, int userID, UserWorkDayStatus status, TimesheetSchedule timesheetSchedule)
		{
			this.dateFrom = dateFrom ?? DateTime.Now.ToFirstDayOfMonth();

			this.dateTo = dateTo ?? DateTime.Now;

			this.userRepository.GetTimesheetByUserID(userID, this.dateFrom, this.dateTo);
			this.usersWorkDayModel = this.userRepository.UsersWorkDayModel;

			foreach (var u in this.usersWorkDayModel)
			{
				if (u.IsBeforeStartDay)
				{
					u.Reason = WorkDayReason.BeforeStartDate;
				}
				else if (u.IsAfterEndDate)
				{
					u.Reason = WorkDayReason.AfterEndDate;
				}
				else if (u.WorkDay.IsWorkable)
				{
					if (u.IsLeaveDay)
					{
						u.Reason = WorkDayReason.LeaveDay;
					}
					else if (u.IsPendingLeaveDay)
					{
						u.Reason = WorkDayReason.LeaveDay;
					}
					else if (u.IsFlexiDay)
					{
						u.Reason = WorkDayReason.FlexiDay;
					}
					else if (u.TotalTime > 0 && u.AllocatedTime > 0 && u.TotalTime != u.AllocatedTime)
					{
						u.Reason = WorkDayReason.IncorrectTimeAllocation;
					}
					else if (u.TotalTime == 0)
					{
						u.Reason = WorkDayReason.NoTimeAllocation;
					}
					else
					{
						u.Reason = WorkDayReason.CorrectTimeAllocation;
					}
				}
				else
				{
					if (u.TotalTime > 0 && u.TotalTime != u.AllocatedTime)
					{
						u.Reason = WorkDayReason.IncorrectTimeAllocation;
					}
					else if (u.TotalTime > 0 && u.AllocatedTime > 0 && u.TotalTime == u.AllocatedTime)
					{
						u.Reason = WorkDayReason.CorrectTimeAllocation;
					}
					else
					{
						u.Reason = WorkDayReason.NotWorkable;
					}
				}
			}

			this.users = this.userRepository.Users;
			this.user = this.userRepository.User;
			this.userWorkDayStatus = status;
			this._timesheetSchedule = timesheetSchedule;
		}

		#endregion FUNCTIONS
	}
}