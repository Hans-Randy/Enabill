using System;
using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class ManagerDashboardModel
	{
		#region INITIALIZATION

		public ManagerDashboardModel(User user)
		{
			this.Staff = this.LoadStaff(user);
			this.HasStaff = this.Staff.Count > 0;
			this.PendingLeave = this.LoadPendingLeave(this.Staff);
			this.HasLeaveToApprove = this.PendingLeave.Count > 0;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool HasLeaveToApprove { get; private set; }
		public bool HasStaff { get; private set; }

		public List<User> Staff { get; private set; }
		public List<UserLeaveModel> PendingLeave { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<User> LoadStaff(User user) => user.GetStaffOfManager();

		private List<UserLeaveModel> LoadPendingLeave(List<User> staff)
		{
			var model = new List<UserLeaveModel>();

			foreach (var staffMember in staff)
			{
				foreach (var leave in staffMember.GetLeave(ApprovalStatusType.Pending, DateTime.Now.AddYears(-1), DateTime.Now.AddYears(1)))
				{
					var modelItem = new UserLeaveModel(staffMember, leave);
					model.Add(modelItem);
				}
			}

			return model;
		}

		#endregion FUNCTIONS
	}
}