using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class PrintTicketRemarkOptionRepo : BaseRepo
	{
		public static IEnumerable<PrintTicketRemarkOption> GetAll() => DB.PrintTicketRemarkOptions;

		public static Dictionary<int, string> GetPrintTicketRemarksOptionsExtendedNames()
		{
			var model = new Dictionary<int, string>();

			foreach (var po in GetAll().ToList())
			{
				model.Add(po.PrintTicketRemarkOptionID, po.PrintTicketRemarkOptionName);
			}

			return model;
		}
	}
}