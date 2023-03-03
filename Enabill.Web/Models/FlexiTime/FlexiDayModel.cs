using System;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class FlexiDayModel
	{
		#region INITIALIZATION

		public FlexiDayModel(User flexiDayUser, FlexiDay flexiDay)
		{
			this.FlexiDayUserID = flexiDay.UserID;
			this.FlexiDayUser = flexiDayUser.FullName;
			this.FlexiDaysID = flexiDay.FlexiDayID;
			this.FlexiDayApprovalStatusID = flexiDay.ApprovalStatusID;
			this.FlexiDayDate = flexiDay.FlexiDate;
			this.FlexiDayApprovalStatus = ((ApprovalStatusType)flexiDay.ApprovalStatusID).ToString();
			this.FlexiDayRemark = flexiDay.Remark;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int FlexiDayApprovalStatusID { get; private set; }
		public int FlexiDaysID { get; private set; }
		public int FlexiDayUserID { get; private set; }

		public string FlexiDayApprovalStatus { get; private set; }
		public string FlexiDayRemark { get; private set; }
		public string FlexiDayUser { get; private set; }

		public DateTime FlexiDayDate { get; private set; }

		#endregion PROPERTIES
	}
}