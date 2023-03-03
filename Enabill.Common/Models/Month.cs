using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("Months")]
	public class Month
	{
		#region PROPERTIES

		[Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int Period { get; internal set; }

		[Required]
		public double OverheadPercentage { get; internal set; }

		#endregion PROPERTIES

		#region MONTH SPECIFIC

		public static Month GetByPeriod(int period) => MonthRepo.GetByPeriod(period);

		public static void CreateForMonth(DateTime monthDate)
		{
			var month = MonthRepo.GetByPeriod(monthDate.ToPeriod());
			if (month != null)
				return;

			month = GetNewForPeriod(monthDate);
			month.Save();
		}

		private void Save() => MonthRepo.Save(this);

		private static Month GetNewForPeriod(DateTime monthDate) => new Month()
		{
			Period = monthDate.ToPeriod(),
			OverheadPercentage = 0
		};

		#endregion MONTH SPECIFIC

		public void SetOverheadPercentage(User userSaving, double overheadPercentage)
		{
			if (!userSaving.HasRole(UserRoleType.SystemAdministrator))
				throw new UserRoleException("You do not have the requried permissions to update the month's overhead percentage. Action cancelled.");

			this.OverheadPercentage = overheadPercentage;
			this.Save();
		}
	}
}