using System;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class UserFlexiDayModel
	{
		#region INITIALIZATION

		public UserFlexiDayModel(User user, FlexiDay flexiDay)
		{
			this.UserID = user.UserID;
			this.UserFullName = user.FullName;
			this.FlexiDayID = flexiDay.FlexiDayID;
			this.FlexiDate = flexiDay.FlexiDate;
			this.FlexiSubmitDate = flexiDay.DateSubmitted;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int FlexiDayID { get; private set; }
		public int UserID { get; private set; }

		public string UserFullName { get; private set; }

		public DateTime FlexiDate { get; private set; }
		public DateTime FlexiSubmitDate { get; private set; }

		#endregion PROPERTIES
	}
}