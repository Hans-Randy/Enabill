using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class UserEditModel
	{
		#region INITIALIZATION

		public UserEditModel(User user, int? id)
		{
			this.User = user;
			this.UserRoles = new UserRoleModel(user);
			this.UserAllocations = new UserAllocationModel(user, id);
			this.UserReportEmails = new UserReportEmailModel(user);
			this.UserLeaveManualAdjustments = new UserLeaveManualAdjustmentModel(user);
			this.UserLeaveCycleBalances = new LeaveCycleBalanceModel(user);

			this.Managers = new List<User>();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public LeaveCycleBalanceModel UserLeaveCycleBalances { get; private set; }
		public User User { get; private set; }
		public UserAllocationModel UserAllocations { get; private set; }
		public UserLeaveManualAdjustmentModel UserLeaveManualAdjustments { get; private set; }
		public UserReportEmailModel UserReportEmails { get; private set; }
		public UserRoleModel UserRoles { get; private set; }

		public List<User> Managers { get; private set; }

		#endregion PROPERTIES
	}
}