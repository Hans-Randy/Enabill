using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class UserLeaveManualAdjustmentModel
	{
		#region INITIALIZATION

		public UserLeaveManualAdjustmentModel(User user)
		{
			this.User = user;

			this.LeaveManualAdjustments = UserRepo.GetLeaveManualAdjustments(user.UserID).ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public User User { get; internal set; }

		public List<LeaveManualAdjustment> LeaveManualAdjustments { get; internal set; }

		#endregion PROPERTIES
	}
}