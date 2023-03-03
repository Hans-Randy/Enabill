using System.Collections.Generic;
using System.Linq;

namespace Enabill.Repos
{
	public abstract class EmploymentTypeRepo : BaseRepo
	{
		#region EMPLOYMENTTYPE SPECIFIC

		public static List<string> GetAll() => (from ep in DB.EmploymentTypes
												select ep.EmploymentTypeName)
					 .Distinct()
					 .ToList();

		#endregion EMPLOYMENTTYPE SPECIFIC
	}
}