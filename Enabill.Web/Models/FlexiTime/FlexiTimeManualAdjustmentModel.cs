using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class FlexiTimeManualAdjustmentModel
	{
		#region INITIALIZATION

		public FlexiTimeManualAdjustmentModel(User user, DateTime dateFrom, DateTime dateTo)
		{
			this.User = user;
			this.FlexiManualAdjustments = UserRepo.GetFlexiBalanceAdjustments(user.UserID, dateFrom, dateTo).ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public User User { get; internal set; }

		public List<FlexiBalanceAdjustment> FlexiManualAdjustments { get; internal set; }

		#endregion PROPERTIES
	}
}