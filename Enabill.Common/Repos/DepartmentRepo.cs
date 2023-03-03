using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class DepartmentRepo : BaseRepo
	{
		#region DEPARTMENT SPECIFIC

		public static Department GetByID(int departmentID) => DB.Departments
					.SingleOrDefault(d => d.DepartmentID == departmentID);

		public static Department GetByName(string departmentName) => DB.Departments
					.SingleOrDefault(d => d.DepartmentName == departmentName);

		public static IEnumerable<Department> GetAll() => DB.Departments;

		#endregion DEPARTMENT SPECIFIC
	}
}