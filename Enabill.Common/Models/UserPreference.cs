using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("UserPreferences")]
	public class UserPreference
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int UserPreferenceID { get; private set; }

		[Required]
		public bool CollapseMyFlexTimeBalance { get; set; } // Home index page

		[Required]
		public bool CollapseMyLeaveBalance { get; set; } // Home index page

		[Required]
		public bool CollapseMyTimesheet { get; set; } // Home index page

		[Required]
		public bool CollapseMyUpcomingLeave { get; set; } // Home index page

		[Required]
		public bool DayView { get; set; } // Day view default, otherwise week view

		//[Required]
		public string ThreeState { get; set; } // Used to determine which filter is applied for the week view grid

		public int InvoiceIndexDateSelector { get; set; } // 0 = dateRange, 1 = period

		[Required]
		public int UserID { get; private set; }

		public double DefaultLunchDuration { get; set; }

		public DateTime? DefaultWorkSessionStartTime { get; set; }

		public DateTime? DefaultWorkSessionEndTime { get; set; }

		#endregion PROPERTIES

		#region USER PREFERENCE

		public static UserPreference GetDefaultPreference() => new UserPreference()
		{
			UserID = 0,
			DefaultWorkSessionStartTime = DateTime.Today.Add(new TimeSpan(8, 00, 0)),
			DefaultWorkSessionEndTime = DateTime.Today.Add(new TimeSpan(17, 00, 0)),
			DefaultLunchDuration = 1,
			InvoiceIndexDateSelector = 1,
			CollapseMyTimesheet = false,
			CollapseMyFlexTimeBalance = false,
			CollapseMyLeaveBalance = false,
			CollapseMyUpcomingLeave = false,
			DayView = false,
			ThreeState = "false"
		};

		//This method only gets a new instance to return... no save
		public static UserPreference GetNew(User user)
		{
			var userPref = GetDefaultPreference();
			userPref.UserID = user.UserID;

			return userPref;
		}

		public void Save(User userSaving)
		{
			if (userSaving.UserID != this.UserID)
				throw new UserPreferenceException("You cannot update the preferences of another user. Save cancelled.");

			this.Save();
		}

		internal void Save()
		{
			if (this.DefaultWorkSessionStartTime.HasValue && this.DefaultWorkSessionEndTime.HasValue && this.DefaultWorkSessionStartTime >= this.DefaultWorkSessionEndTime.Value)
				throw new UserPreferenceException("The start time cannot be after the set end time. Save cancelled.");

			if (!this.DefaultWorkSessionStartTime.HasValue && this.DefaultWorkSessionEndTime.HasValue && this.DefaultWorkSessionEndTime.Value.TimeOfDay <= TimeSpan.FromHours(9))
				throw new UserPreferenceException("If the default start time is not set, the end time cannot be set to a value before 9am.");

			if (this.DefaultLunchDuration < 0)
				throw new UserPreferenceException("The default lunch duration can not be less than zero. Save cancelled.");

			if (this.DefaultLunchDuration % 0.25 != 0)
				throw new UserPreferenceException("The default lunch duration muct be a multiple of 0.25 (15 minutes). Save cancelled.");

			UserPreferenceRepo.Save(this);
		}

		#endregion USER PREFERENCE

		#region COLUMN TOGGLING

		public UserPreference ToggleColumn(CollapseColumnType collapseColumnType)
		{
			switch (collapseColumnType)
			{
				case CollapseColumnType.CollapseMyTimesheet:
					this.ToggleMyTimesheetValue();
					break;

				case CollapseColumnType.CollapseMyFlexiTimeBalance:
					this.ToggleMyFlexiTimeBalanceValue();
					break;

				case CollapseColumnType.CollapseMyLeaveBalance:
					this.ToggleMyLeaveBalanceValue();
					break;

				case CollapseColumnType.CollapseMyUpcomingLeave:
					this.ToggleMyUpcomingLeaveValue();
					break;
			}

			this.Save();

			return this;
		}

		private void ToggleMyTimesheetValue() => this.CollapseMyTimesheet = !this.CollapseMyTimesheet;

		private void ToggleMyFlexiTimeBalanceValue() => this.CollapseMyFlexTimeBalance = !this.CollapseMyFlexTimeBalance;

		private void ToggleMyLeaveBalanceValue() => this.CollapseMyLeaveBalance = !this.CollapseMyLeaveBalance;

		private void ToggleMyUpcomingLeaveValue() => this.CollapseMyUpcomingLeave = !this.CollapseMyUpcomingLeave;

		#endregion COLUMN TOGGLING
	}

	public enum CollapseColumnType
	{
		[Description("Collapse My Timesheet")]
		CollapseMyTimesheet = 1,

		[Description("Collapse My FlexiTime Balance")]
		CollapseMyFlexiTimeBalance = 2,

		[Description("Collapse My Leave Balance")]
		CollapseMyLeaveBalance = 3,

		[Description("Collapse My Upcoming Leave")]
		CollapseMyUpcomingLeave = 4
	}
}