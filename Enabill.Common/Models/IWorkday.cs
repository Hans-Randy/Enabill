using System;

namespace Enabill.Models
{
	public interface IWorkDay
	{
		#region PROPERTIES

		bool IsDayWorkable(DateTime date);

		#endregion PROPERTIES
	}
}