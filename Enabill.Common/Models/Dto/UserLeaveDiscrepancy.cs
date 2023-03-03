using System;

namespace Enabill.Models.Dto
{
	public class UserLeaveDiscrepancy
	{
		#region PROPERTIES

		public int LeaveType;
		public int UserID;

		public float Number;
		public float Taken;

		public string UserName;

		public DateTime DateFrom;
		public DateTime DateTo;
		public DateTime EndDate;

		#endregion PROPERTIES
	}
}