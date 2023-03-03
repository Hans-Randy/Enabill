using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("WorkDays")]
	public class WorkDay
	{
		
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public DateTime WorkDate { get; private set; }

		[Required]
		public bool IsWorkable { get; private set; }

		#endregion PROPERTIES

		#region METHODS

		public WorkDay()
		{
		}

		public WorkDay(DateTime date, bool workable)
		{
			this.WorkDate = date;
			this.IsWorkable = workable;
		}

		public void SetIsWorkable(bool isWorkable) => this.IsWorkable = isWorkable;

		#endregion METHODS

		#region WORK DAY

		public static List<WorkDay> GetWorkableDays(bool isWorkable, DateTime startDate, DateTime endDate)
		{
			if (startDate.Date < EnabillSettings.SiteStartDate)
				startDate = EnabillSettings.SiteStartDate;

			return WorkDayRepo.GetWorkableDays(isWorkable, startDate.Date, endDate.Date)
					.ToList();
		}

		public static bool IsDayWorkable(DateTime date)
		{
			if (date.Date < EnabillSettings.SiteStartDate)
				return false;

			return WorkDayRepo.IsDayWorkable(date.Date);
		}

		#endregion WORK DAY

	}
}