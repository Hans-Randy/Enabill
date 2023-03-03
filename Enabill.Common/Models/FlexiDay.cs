using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("FlexiDays")]
	public class FlexiDay
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int FlexiDayID { get; internal set; }

		[EnumDataType(typeof(ApprovalStatusType))]
		public int ApprovalStatusID { get; internal set; }

		[Required]
		public int UserID { get; internal set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		[MaxLength(200)]
		public string Remark { get; set; }

		[Required]
		public DateTime DateSubmitted { get; internal set; }

		[Required]
		public DateTime FlexiDate { get; internal set; }

		#endregion PROPERTIES

		#region FLEXIDAY

		public void ApproveFlexiDayRequest(User userManaging)
		{
			var user = UserRepo.GetByID(this.UserID);

			if (!userManaging.CanManage(user))
				throw new UserRoleException(string.Format($"You do not have the required permissions to approve flexiday for {user.FullName}. Action cancelled."));

			this.ApprovalStatusID = (int)ApprovalStatusType.Approved;
			FlexiDayRepo.Save(this);
		}

		public void DeclineFlexiDayRequest(User userManaging)
		{
			var user = UserRepo.GetByID(this.UserID);

			if (!userManaging.CanManage(user))
				throw new UserRoleException(string.Format($"You do not have the required permissions to decline the flexiday for {user.FullName}. Action cancelled."));

			this.ApprovalStatusID = (int)ApprovalStatusType.Declined;
			FlexiDayRepo.Save(this);
		}

		public void WithdrawFlexiDay(int flexiDayID)
		{
			var flexiDay = UserRepo.GetFlexiDayByID(this.UserID, flexiDayID);

			if (flexiDay == null)
				throw new FlexiDayException("The flexiday you wish to withdraw could not be found. Action cancelled.");

			FlexiDayRepo.Delete(flexiDay);
		}

		#endregion FLEXIDAY
	}
}