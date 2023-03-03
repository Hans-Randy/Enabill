using System;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class UserFlexiBalanceModel
	{
		#region INITIALIZATION

		public UserFlexiBalanceModel(User user)
		{
			this.UserID = user.UserID;
			this.UserFullName = user.FullName;
			this.FlexiBalance = user.CalculateFlexiBalanceOnDate(DateTime.Today, true);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int UserID { get; private set; }

		public double FlexiBalance { get; private set; }

		public string UserFullName { get; private set; }

		#endregion PROPERTIES
	}
}