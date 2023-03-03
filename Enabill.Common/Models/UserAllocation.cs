using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("UserAllocations")]
	public class UserAllocation
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int UserAllocationID { get; internal set; }

		public bool IsHidden { get; set; }

		[Required]
		public int ActivityID { get; set; }

		[Required]
		public int UserID { get; internal set; }

		[Required]
		public double ChargeRate { get; set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		public DateTime StartDate { get; set; }
		public DateTime? ConfirmedEndDate { get; set; }
		public DateTime? ScheduledEndDate { get; set; }

		#endregion PROPERTIES

		#region INITIALIZATION

		[NotMapped]
		public DateTime? EndDate
		{
			get
			{
				var project = this.GetProject();
				if (project.ConfirmedEndDate.HasValue)
				{
					if (this.IsConfirmed && this.ConfirmedEndDate.Value.Date <= project.ConfirmedEndDate.Value.Date)
					{
						return this.ConfirmedEndDate.Value.Date;
					}
					if (this.ScheduledEndDate.HasValue && this.ScheduledEndDate.Value.Date <= project.ConfirmedEndDate.Value.Date)
						return this.ScheduledEndDate.Value.Date;

					return project.ConfirmedEndDate.Value.Date;
				}
				else if (project.ScheduledEndDate.HasValue)
				{
					if (this.IsConfirmed && this.ConfirmedEndDate.Value.Date <= project.ScheduledEndDate.Value.Date)
						return this.ConfirmedEndDate.Value.Date;
					if (this.ScheduledEndDate.HasValue && this.ScheduledEndDate.Value.Date <= project.ScheduledEndDate.Value.Date)
						return this.ScheduledEndDate.Value.Date;
				}
				else if (this.IsConfirmed)
				{
					return this.ConfirmedEndDate.Value.Date;
				}
				else if (this.ScheduledEndDate.HasValue)
				{
					return this.ScheduledEndDate.Value.Date;
				}

				return null;
			}
		}

		[NotMapped]
		public bool IsConfirmed => this.ConfirmedEndDate.HasValue;

		#endregion INITIALIZATION

		#region USER ALLOCATION

		public static UserAllocation GetByID(User userRequesting, int userAllocationID)
		{
			if (!userRequesting.HasRole(UserRoleType.SystemAdministrator) && !userRequesting.HasRole(UserRoleType.Manager) && !userRequesting.HasRole(UserRoleType.ProjectOwner))
				throw new UserRoleException("You do not have the required permissions to view user allocations.");

			return UserAllocationRepo.GetByID(userAllocationID);
		}

		public static bool IsActivityHidden(int activityID, int userID) => UserAllocationRepo.IsActivityHidden(activityID, userID);

		#region CAN ADD USER ALLOCATION

		internal static bool CanAddUserAllocation(int activityID, User user, DateTime startDate, DateTime? endDate, bool isConfirmed)
		{
			var userAllocationList = UserAllocationRepo.GetAllForActivityForUser(activityID, user.UserID).ToList();

			DateCheck(user, startDate);

			if (userAllocationList.Count > 0)
			{
				if (!isConfirmed)
				{
					if (userAllocationList.Any(ua => ua.ConfirmedEndDate == null))
						return false;
					if (userAllocationList.Any(ua => ua.ConfirmedEndDate.Value >= startDate))
						return false;
				}
				else
				{
					if (userAllocationList.Any(ua => ua.DoesOverLap(startDate, endDate.Value.Date)))
						return false;
				}
			}

			return true;
		}

		private static void DateCheck(User user, DateTime startDate)
		{
			if (startDate < user.EmployStartDate)
				throw new ActivityAdminException(string.Format($"{user.FullName} cannot be added to any activities because the date is before his/her employment date: {user.EmployStartDate.ToExceptionDisplayString()}"));
			if (startDate < EnabillSettings.SiteStartDate)
				throw new EnabillDomainException(string.Format($"{user.FullName} cannot be added to the selected activity/ies because the start date is before the site start date: {EnabillSettings.SiteStartDate.ToExceptionDisplayString()}"));
		}

		#endregion CAN ADD USER ALLOCATION

		private bool DoesOverLap(DateTime startDate, DateTime endDate) => (this.StartDate >= startDate && this.StartDate <= endDate)
					|| (this.ConfirmedEndDate >= startDate && this.ConfirmedEndDate <= endDate);

		public void SetConfirmedEndDateOnUserAllocationForActivity(User userUpdating, DateTime confirmedEndDate)
		{
			if (!userUpdating.HasRole(UserRoleType.SystemAdministrator) && !userUpdating.CanManage(this.GetUser()) && !userUpdating.CanManage(this.GetProject()))
				throw new UserRoleException("You do not have the required permissions to set the end date on this user allocation.");

			// In the case where a user terminates employment, ensure that the Confirmed End Date is less than the Termination Date
			if (this.ConfirmedEndDate != null || this.ConfirmedEndDate < confirmedEndDate)
				return;

			this.ConfirmedEndDate = confirmedEndDate.Date;

			if (this.ConfirmedEndDate < EnabillSettings.SiteStartDate)
				throw new EnabillSettingsException("The end date cannot be before " + EnabillSettings.SiteStartDate.ToExceptionDisplayString());
			
			if (this.ConfirmedEndDate < this.StartDate)
			{
				if (this.EndDate < this.StartDate)
					this.StartDate = confirmedEndDate;
				else
					throw new ActivityAdminException("The end date cannot be before the start date. Action cancelled.");
			}

			this.LastModifiedBy = userUpdating.FullName;
			UserAllocationRepo.Save(this);
		}

		#endregion USER ALLOCATION

		#region USER

		public User GetUser() => UserRepo.GetByID(this.UserID);

		#endregion USER

		#region PROJECT

		private Project GetProject() => UserAllocationRepo.GetProject(this.ActivityID);

		#endregion PROJECT

		#region ACTIVITY

		public Activity GetActivity() => ActivityRepo.GetByID(this.ActivityID);

		public ActivityDetail GetActivityFullDetail() => ActivityRepo.GetFullDetail(this.ActivityID);

		#endregion ACTIVITY
	}
}