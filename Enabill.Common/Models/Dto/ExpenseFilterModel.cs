using System.Collections.Generic;

namespace Enabill.Models.Dto
{
	public class ExpenseFilterModel
	{
		#region PROPERTIES

		public bool Locked { get; set; }

		public string FilterText { get; set; }

		public IList<Expense> ExpenseList { get; set; }

		#endregion PROPERTIES
	}
}