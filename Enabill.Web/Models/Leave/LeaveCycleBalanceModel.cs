using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class LeaveCycleBalanceModel
	{
		#region INITIALIZATION

		public LeaveCycleBalanceModel(User user)
		{
			this.UserLeaveCycleBalances = LeaveCycleBalanceRepo.GetLeaveCycleBalance(user.UserID).ToList();
		}

		public LeaveCycleBalanceModel(LeaveTypeEnum leaveType)
		{
			this.LeaveCycleBalances = this.LoadLeaveCycles(leaveType);
			this.LeaveType = leaveType;
		}

		public LeaveCycleBalanceModel(LeaveTypeEnum leaveType, DateTime date)
		{
			this.LeaveBalances = this.LoadLeaveBalances(leaveType, date);
			this.LeaveOtherBalances = this.LoadLeaveOtherBalances(leaveType, date);
			this.LeaveType = leaveType;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public LeaveTypeEnum LeaveType { get; private set; }

		public List<LeaveBalanceSummarisedReportModel> LeaveBalances { get; private set; }
		public List<LeaveCycleBalance> UserLeaveCycleBalances { get; private set; }
		public List<LeaveCycleExtendedReportModel> LeaveCycleBalances { get; private set; }
		public List<LeaveOtherReportModel> LeaveOtherBalances { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<LeaveCycleExtendedReportModel> LoadLeaveCycles(LeaveTypeEnum leaveType)
		{
			var LeaveCycleBalances = new List<LeaveCycleExtendedReportModel>();

			var users = UserRepo.GetAllActiveOrRecentlyInactive().ToList();

			foreach (var user in users)
			{
				var leaveList = LeaveCycleBalanceRepo.GetLastestCycleExtendedReportModel(user.UserID, leaveType);

				if (leaveList != null)
					LeaveCycleBalances.Add(leaveList);
			}

			return LeaveCycleBalances;
		}

		private List<LeaveBalanceSummarisedReportModel> LoadLeaveBalances(LeaveTypeEnum leaveType, DateTime date)
		{
			var LeaveBalances = new List<LeaveBalanceSummarisedReportModel>();
			return LeaveBalanceRepo.GetLeaveBalanceExtendedReportForDate(leaveType, date).ToList();
		}

		private List<LeaveOtherReportModel> LoadLeaveOtherBalances(LeaveTypeEnum leaveType, DateTime date)
		{
			var LeaveOtherBalances = new List<LeaveOtherReportModel>();
			return LeaveBalanceRepo.GetLeaveOtherReportForDate(leaveType, date).ToList();
		}

		#endregion FUNCTIONS
	}
}