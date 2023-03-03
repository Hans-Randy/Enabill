using System;

namespace Enabill.Models
{
	public class WorkDayWrapper : IWorkDay
	{
		#region FUNCTIONS

		public bool IsDayWorkable(DateTime date) => WorkDay.IsDayWorkable(date);

		#endregion FUNCTIONS
	}
}