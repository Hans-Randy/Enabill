using System;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class VATHistoryRepo : BaseRepo
	{
		#region VAT HISTORY SPECIFIC

		internal static VATHistory GetForDate(DateTime refDate)
		{
			var list = DB.VATHistories
					.Where(var => var.ImplementationDate <= refDate)
					.ToList();

			if (list.Count <= 0)
				return null;

			var maxDate = list.Max(v => v.ImplementationDate);

			return list.SingleOrDefault(v => v.ImplementationDate == maxDate);
		}

		#endregion VAT HISTORY SPECIFIC
	}
}