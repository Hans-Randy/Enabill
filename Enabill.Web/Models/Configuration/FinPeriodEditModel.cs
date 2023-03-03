using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class FinPeriodEditModel
	{
		#region INITIALIZATION

		public FinPeriodEditModel(string getAll)
		{
			this.FinPeriods = this.LoadPeriods(getAll);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<FinPeriod> FinPeriods;

		#endregion PROPERTIES

		#region FUNCTIONS

		public List<FinPeriod> LoadPeriods(string getAll) => FinPeriod.GetAll(getAll);

		#endregion FUNCTIONS
	}
}