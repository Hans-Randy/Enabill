using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class DivisionRepo : BaseRepo
	{
		#region DIVISION SPECIFIC

		public static Division GetByID(int divisionID) => DB.Divisions
					.SingleOrDefault(d => d.DivisionID == divisionID);

		public static Division GetByName(string divisionName) => DB.Divisions
					.SingleOrDefault(r => r.DivisionName == divisionName);

		public static IEnumerable<Division> GetActive() => DB.Divisions
					.Where(d => d.IsActive == true);

		public static IEnumerable<Division> GetAll() => DB.Divisions;

		internal static void Save(Division division)
		{
			if (division.DivisionID == 0)
				DB.Divisions.Add(division);

			DB.SaveChanges();
		}

		internal static void Delete(Division division)
		{
			try
			{
				DB.Divisions.Remove(division);
				DB.SaveChanges();
			}
			catch
			{
				throw new NullReferenceException("The division could not be found in the records");
			}
		}

		#endregion DIVISION SPECIFIC
	}
}