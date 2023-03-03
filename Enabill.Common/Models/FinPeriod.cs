using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("FinPeriods")]
	public class FinPeriod
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int FinPeriodID { get; set; }

		[Required]
		public bool IsCurrent { get; set; }

		[Required]
		public DateTime DateFrom { get; set; }

		[Required]
		public DateTime DateTo { get; set; }

		#endregion PROPERTIES

		#region FINPERIOD

		public void Save() => FinPeriodRepo.Save(this);

		public static List<FinPeriod> GetAll(string getAll) => FinPeriodRepo.GetAll(getAll).ToList();

		public static FinPeriod GetCurrentFinPeriod() => FinPeriodRepo.GetCurrentFinPeriod();

		public static FinPeriod GetFinPeriodByID(int finPeriod) => FinPeriodRepo.GetFinPeriodByID(finPeriod);

		public static string AddNewFinPeriod(DateTime dateFrom, DateTime dateTo, bool isCurrent)
		{
			FinPeriod newPeriod = null;
			string message = dateFrom.Month.ToMonthName();
			var period = GetFinPeriodByID(dateFrom.ToPeriod());

			if (period == null)
			{
				var currentPeriod = GetCurrentFinPeriod();
				currentPeriod.IsCurrent = false;
				currentPeriod.Save();

				newPeriod = new FinPeriod() { DateFrom = dateFrom, DateTo = dateTo, IsCurrent = isCurrent };
				newPeriod.Save();
				message += " successfully added.";
			}
			else
			{
				message += " already exists.";
			}

			return message;
		}

		public static void UpdateFinPeriod(int finPeriodID)
		{
			var currentPeriod = GetCurrentFinPeriod();
			currentPeriod.IsCurrent = false;
			currentPeriod.Save();

			var finPeriod = GetFinPeriodByID(finPeriodID);
			finPeriod.IsCurrent = true;
			finPeriod.Save();
		}

		#endregion FINPERIOD
	}
}