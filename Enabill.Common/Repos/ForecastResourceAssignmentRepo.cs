using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class ForecastResourceAssignmentRepo : BaseRepo
	{
		#region FORECASTRESOURCEASSIGNMENT SPECIFIC

		public static IEnumerable<ForecastResourceAssignment> GetAll() => DB.ForecastResourceAssignments;

		public static IEnumerable<ForecastHeaderMostRecentResourceAssignment> GetForecastMostRecentResourceAssignments() => DB.ForecastHeaderMostRecentResourceAssignments;

		public static IEnumerable<ForecastHeaderMostRecentResourceAssignment> GetForecastMostRecentResourceAssignmentsByDetailID(int forecastDetailID) => DB.ForecastHeaderMostRecentResourceAssignments.Where(r => r.ForecastDetailID == forecastDetailID);

		public static IEnumerable<ForecastResourceAssignment> GetForecastResourceAssignmentsByDetailID(int forecastDetailID) => DB.ForecastResourceAssignments.Where(r => r.ForecastDetailID == forecastDetailID);

		public static ForecastResourceAssignment GetByDetailIDResource(int forecastDetailID, string resource) => DB.ForecastResourceAssignments
				   .SingleOrDefault(r => r.ForecastDetailID == forecastDetailID && r.Resource == resource);

		public static void Save(ForecastResourceAssignment forecastResourceAssignment)
		{
			if (forecastResourceAssignment.ForecastResourceAssignmentID == 0)
				DB.ForecastResourceAssignments.Add(forecastResourceAssignment);
			DB.SaveChanges();
		}

		public static void Delete(ForecastResourceAssignment forecastResourceAssignment)
		{
			if (forecastResourceAssignment == null)
				return;

			DB.ForecastResourceAssignments.Remove(forecastResourceAssignment);
			DB.SaveChanges();
		}

		#endregion FORECASTRESOURCEASSIGNMENT SPECIFIC
	}
}