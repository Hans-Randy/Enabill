using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class PrintOptionRepo : BaseRepo
	{
		public static IEnumerable<PrintOption> GetAll() => DB.PrintOptions;

		public static IEnumerable<PrintOption> GetAllPrintOptions()
		{
			int[] options = new int[] { 1, 2, 5 };

			return DB.PrintOptions.Where(po => options.Contains(po.PrintOptionID));
		}

		public static IEnumerable<PrintOption> GetAllPrintLayouts()
		{
			int[] options = new int[] { 3, 4 };

			return DB.PrintOptions.Where(po => options.Contains(po.PrintOptionID));
		}

		public static Dictionary<int, string> GetPrintOptionsExtendedNames()
		{
			var model = new Dictionary<int, string>();

			foreach (var po in GetAllPrintOptions().ToList())
			{
				model.Add(po.PrintOptionID, po.PrintOptionName);
			}

			return model;
		}

		public static Dictionary<int, string> GetPrintLayoutsExtendedNames()
		{
			var model = new Dictionary<int, string>();

			foreach (var po in GetAllPrintLayouts().ToList())
			{
				model.Add(po.PrintOptionID, po.PrintOptionName);
			}

			return model;
		}
	}
}