using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("UserCostToCompanies")]
	public class UserCostToCompany
	{
		#region PROPERTIES

		[Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int UserCostToCompanyID { get; internal set; }

		[Required]
		public int ModifiedByID { get; set; }

		[Required]
		public int Period { get; set; }

		[Required]
		public int UserID { get; internal set; }

		[Required]
		public byte[] CostToCompany { get; set; }

		public string ModifyReason { get; set; }

		[Required]
		public DateTime ModifiedDate { get; set; }

		#endregion PROPERTIES

		#region INITIALIZATION

		[NotMapped]
		public double? _decCostToCompany { get; set; }

		#endregion INITIALIZATION

		#region COST TO COMPANY SPECIFIC

		public static List<UserCostToCompany> GetAllForMonth(DateTime monthDate) => GetAllForMonth(monthDate.ToPeriod());

		public static List<UserCostToCompany> GetAllForMonth(int period) =>
			/*
				if (!period.IsValidPeriod())
				throw new EnabillDomainException("The specified 'Period' is invalid. Please review the period specified and retry.");
			*/

			UserCostToCompanyRepo.GetAllForMonth(period)
					.ToList();

		public static List<UserCostToCompany> GetAll() => UserCostToCompanyRepo.GetAll()
				   .ToList();

		internal static UserCostToCompany GetNewForUserForMonth(int userID, DateTime monthDate) => GetNewForUserForMonth(userID, monthDate.ToPeriod());

		internal static UserCostToCompany GetNewForUserForMonth(User user, DateTime monthDate) => GetNewForUserForMonth(user.UserID, monthDate.ToPeriod());

		internal static UserCostToCompany GetNewForUserForMonth(User user, int period) => GetNewForUserForMonth(user.UserID, period);

		internal static UserCostToCompany GetNewForUserForMonth(int userID, int period)
		{
			if (!period.IsValidPeriod())
				throw new EnabillDomainException("Supplied period value is not in the correct format, YYYYMM. Action cancelled.");

			return new UserCostToCompany()
			{
				UserCostToCompanyID = 0,
				UserID = userID,
				Period = period,
				CostToCompany = new byte[128]
			};
		}

		public void Save(string passphrase)
		{
			if (!this.Period.IsValidPeriod())
				throw new EnabillDomainException("TODO");

			if (!this._decCostToCompany.HasValue)
				this._decCostToCompany = 0;

			//if (this.CostToCompany.All(ctc => ctc == 0))
			//  this._decCostToCompany = 0;

			if (!this._decCostToCompany.HasValue)
				throw new EnabillDomainException("TODO2");

			this.CostToCompany = ProcessRepo.GetEncryptedValue(passphrase, this._decCostToCompany.Value);
			UserCostToCompanyRepo.Save(this);
		}

		public double GetCostToCompanyAmount(string passphrase, out bool validPassphrase)
		{
			validPassphrase = false;
			string value = ProcessRepo.GetDecryptedValue(passphrase, this.CostToCompany);
			if (string.IsNullOrEmpty(value))
				return 0;

			validPassphrase = true;
			return double.Parse(value);
		}

		#endregion COST TO COMPANY SPECIFIC

		#region MONTHLY CONFIGURATION

		public static void ConfigureUserCostToCompanyForMonth(string passphrase, DateTime monthToRunFor)
		{
			if (!Helpers.ConfirmPassphraseIsValid(passphrase))
				throw new EnabillPassphraseException("The passphrase provided is incorrect. Action Cancelled.");

			// Get list of users who were active in the month
			var activeUsersInMonth = UserHistory.GetAllUsersInPeriod(monthToRunFor.ToPeriod());
			if (activeUsersInMonth.Count <= 0)
				return;

			foreach (var user in activeUsersInMonth)
			{
				var userCostToCompany = user.GetUserCostToCompanyForMonth(monthToRunFor.ToPeriod());
				if (userCostToCompany != null)
					continue;

				// Create new records for users who are not in current month's userCostToCompany from userHistory list
				userCostToCompany = GetNewForUserForMonth(user, monthToRunFor);
				// Get the previous month's costToCompany record for each new user and if exists, use these values, else create a 0 value for the user for the month and save...
				var prevMonthCostToCompany = user.GetUserCostToCompanyForMonth(monthToRunFor.AddMonths(-1).ToPeriod());

				if (prevMonthCostToCompany != null)
					userCostToCompany._decCostToCompany = double.Parse(ProcessRepo.GetDecryptedValue(passphrase, prevMonthCostToCompany.CostToCompany));

				userCostToCompany.ModifyReason = "Copied from previous month";
				userCostToCompany.ModifiedDate = DateTime.Now;
				userCostToCompany.Save(passphrase);
			}

			// Delete any entries in the userCostToCompany for users that are not found in the user history table for the month, as records are only inserted into the userHistory table for users who are active for that month...
			foreach (var toBeDeleted in GetAllForMonthToBeRemoved())
				toBeDeleted.Delete();
		}

		private void Delete() => UserCostToCompanyRepo.Delete(this);

		private static List<UserCostToCompany> GetAllForMonthToBeRemoved() => UserCostToCompanyRepo.GetAllForMonthToBeRemoved()
					.ToList();

		#endregion MONTHLY CONFIGURATION
	}
}