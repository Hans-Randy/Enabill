using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("FlexiBalances")]
	public class FlexiBalance
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int FlexiBalanceID { get; protected set; }

		[Required]
		public int UserID { get; internal set; }

		[Required]
		public double CalculatedAdjustment { get; internal set; }

		[Required]
		public double FinalBalance { get; internal set; }

		[Required]
		public double ManualAdjustment { get; internal set; }

		[MaxLength(128)]
		public string UpdatedBy { get; internal set; }

		public DateTime? DateUpdated { get; internal set; }

		[Required]
		public DateTime BalanceDate { get; internal set; }

		#endregion PROPERTIES

		#region FLEXIBALANCE

		public static void RecalculateFlexiBalanceForUser(User userRecalculating, User userToBeRecalculated, int monthsToRecalculateFrom)
		{
			if (userRecalculating.UserID != userToBeRecalculated.UserID && !userRecalculating.HasRole(UserRoleType.SystemAdministrator) && !(userRecalculating.HasRole(UserRoleType.Manager) && userToBeRecalculated.ManagerID == userRecalculating.UserID))
			{
				throw new UserRoleException("You do not have the required permissions to recalculate another user's FlexiBalance.");
			}

			if (monthsToRecalculateFrom < 1)
				throw new FlexiBalanceException("To recalculate a user's flexi balance, you must provide a valid amount of months to recalculate for.");

			if (monthsToRecalculateFrom <= 6)
				throw new FlexiBalanceException("To recalculate a user's flexi balance, you must provide an amount of months less than or equal to 6.");

			var startDate = DateTime.Today.ToFirstDayOfMonth().AddMonths(-1 * monthsToRecalculateFrom);

			if (startDate < EnabillSettings.SiteStartDate)
				startDate = EnabillSettings.SiteStartDate;

			if (startDate < userToBeRecalculated.EmployStartDate)
				startDate = userToBeRecalculated.EmployStartDate;

			var initialFlexiBalance = userToBeRecalculated.GetFlexiBalance(startDate.AddDays(-1));
			double startingBalance = 0;
			double balance = 0;

			if (initialFlexiBalance != null)
				startingBalance = initialFlexiBalance.FinalBalance;

			foreach (var date in Helpers.GetDaysInDateSpan(startDate, DateTime.Today))
			{
				balance += userToBeRecalculated.CalculateFlexiTimeForDay(date);

				var flexiBalance = userToBeRecalculated.GetFlexiBalance(date) ?? new FlexiBalance();

				double manualAdjustments = userToBeRecalculated.GetFlexiBalanceAdjustments(date.AddMonths(-1), date.AddDays(-1), true).Sum(x => x.Adjustment);
				double finalBalance = balance + manualAdjustments;

				flexiBalance.UserID = userToBeRecalculated.UserID;
				flexiBalance.BalanceDate = date;
				flexiBalance.CalculatedAdjustment = balance - startingBalance;
				flexiBalance.ManualAdjustment = manualAdjustments;
				flexiBalance.FinalBalance = finalBalance;
				flexiBalance.UpdatedBy = userRecalculating.FullName;
				flexiBalance.DateUpdated = DateTime.Today;

				if (date.IsLastDayOfMonth())
				{
					userToBeRecalculated.SaveFlexiBalance(userRecalculating, flexiBalance);
				}
			}
		}

		internal void Save()
		{
			var user = this.GetUser();
			var flexiBalanceTemp = user.GetFlexiBalance(this.BalanceDate);

			if (this.FlexiBalanceID == 0)
			{
				if (flexiBalanceTemp != null)
				{
					this.FlexiBalanceID = flexiBalanceTemp.FlexiBalanceID;
				}
			}
			else
			{
				if (flexiBalanceTemp != null && flexiBalanceTemp.FlexiBalanceID != this.FlexiBalanceID)
					throw new FlexiBalanceException("This FlexiBalance has updated a previously created FlexiBalance record and this is not allowed. Action cancelled.");
			}

			FlexiBalanceRepo.Save(this);
		}

		#endregion FLEXIBALANCE

		#region USER

		private User GetUser() => UserRepo.GetByID(this.UserID);

		#endregion USER
	}
}