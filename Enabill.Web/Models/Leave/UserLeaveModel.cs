using System;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class UserLeaveModel
	{
		#region INITIALIZATION

		public UserLeaveModel(User leaveUser, Leave leave)
		{
			this.LeaveUserID = leaveUser.UserID;
			this.LeaveUser = leaveUser.FullName;
			this.LeaveID = leave.LeaveID;
			this.LeaveType = ((LeaveTypeEnum)leave.LeaveType).GetEnumDescription();
			this.LeaveApprovalStatusID = leave.ApprovalStatus;
			this.LeaveDateFrom = leave.DateFrom;
			this.LeaveDateTo = leave.DateTo;
			this.LeaveStatus = ((ApprovalStatusType)leave.ApprovalStatus).ToString();
			this.ManagedBy = leave.ManagedBy ?? string.Empty;
			this.LeaveRemark = leave.Remark;
			this.IsPartialDay = leave.IsPartialDay;
			this.NumberOfWorkDays = leave.NumberOfDays.ToDoubleString(false);
			this.NumberOfHours = "-";

			if (leave.IsPartialDay)
				this.NumberOfHours = (leave.NumberOfHours.Value * 1.00).ToDoubleString(false);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool IsPartialDay { get; set; }

		public int LeaveApprovalStatusID { get; private set; }
		public int LeaveID { get; private set; }
		public int LeaveUserID { get; private set; }

		public string LeaveRemark { get; private set; }
		public string LeaveStatus { get; private set; }
		public string LeaveType { get; private set; }
		public string LeaveUser { get; private set; }
		public string NumberOfHours { get; private set; }
		public string NumberOfWorkDays { get; private set; }
		public string ManagedBy { get; private set; }

		public DateTime LeaveDateFrom { get; private set; }
		public DateTime LeaveDateTo { get; private set; }

		#endregion PROPERTIES
	}
}