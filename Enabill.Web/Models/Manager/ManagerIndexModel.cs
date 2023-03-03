using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class ManagerIndexModel
	{
		#region INITIALIZATION

		public ManagerIndexModel(User manager)
		{
			var staffList = manager.GetStaffOfManager().OrderBy(u => u.FirstName).ToList();
			this.StaffList = this.LoadStaffList(staffList);
			this.AnnualLeaveBalance = this.LoadLeaveBalance(staffList, LeaveTypeEnum.Annual, DateTime.Today.ToFirstDayOfMonth());
			this.RecentLeave = this.LoadRecentLeave(staffList, DateTime.Today.ToPreviousDayOfWeek(DayOfWeek.Monday), DateTime.Today.ToPreviousDayOfWeek(DayOfWeek.Monday).AddDays(21));
			this.RecentFlexiDays = this.LoadRecentFlexiDays(staffList, DateTime.Today.ToFirstDayOfMonth(), DateTime.Today.ToPreviousDayOfWeek(DayOfWeek.Sunday).AddDays(14));
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<UserDetailModel> StaffList { get; private set; }
		public List<UserFlexiBalanceModel> FlexiBalance { get; private set; }
		public List<UserFlexiDayModel> RecentFlexiDays { get; private set; }
		public List<UserLeaveBalanceModel> AnnualLeaveBalance { get; private set; }
		public List<UserLeaveModel> RecentLeave { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<UserDetailModel> LoadStaffList(List<User> staffList)
		{
			var model = new List<UserDetailModel>();

			foreach (var user in staffList)
			{
				model.Add(new UserDetailModel(user));
			}

			return model;
		}

		private List<UserLeaveBalanceModel> LoadLeaveBalance(List<User> staffList, LeaveTypeEnum leaveType, DateTime leaveDate)
		{
			var model = new List<UserLeaveBalanceModel>();

			foreach (var user in staffList)
			{
				model.Add(new UserLeaveBalanceModel(user, user.GetLeaveBalance(LeaveTypeEnum.Annual)));
			}

			return model;
		}

		private List<UserLeaveModel> LoadRecentLeave(List<User> staffList, DateTime dateFrom, DateTime dateTo)
		{
			var model = new List<UserLeaveModel>();

			foreach (var user in staffList)
			{
				user.GetLeave(dateFrom, dateTo)
						.OrderBy(l => l.ApprovalStatus)
						.ToList()
						.ForEach(l => model.Add(new UserLeaveModel(user, l)));
			}

			return model;
		}

		private List<UserFlexiBalanceModel> LoadFlexiBalance(List<User> staffList)
		{
			var model = new List<UserFlexiBalanceModel>();

			foreach (var user in staffList)
			{
				model.Add(new UserFlexiBalanceModel(user));
			}

			return model;
		}

		private List<UserFlexiDayModel> LoadRecentFlexiDays(List<User> staffList, DateTime dateFrom, DateTime dateTo)
		{
			var model = new List<UserFlexiDayModel>();

			foreach (var user in staffList)
			{
				foreach (var flexiDay in user.GetFlexiDaysForDateSpan(dateFrom, dateTo))
				{
					model.Add(new UserFlexiDayModel(user, flexiDay));
				}
			}

			return model;
		}

		#endregion FUNCTIONS
	}
}