using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class RoleRepo : BaseRepo
	{
		#region ROLE SPECIFIC

		public static Role GetByID(int roleID) => DB.Roles.SingleOrDefault(r => r.RoleID == roleID);

		public static Role GetByName(string roleName) => DB.Roles
					.SingleOrDefault(r => r.RoleName == roleName);

		public static IEnumerable<Role> GetAll() => DB.Roles;

		internal static void Save(Role role)
		{
			if (!DB.Roles.Any(r => r.RoleID == role.RoleID))
				DB.Roles.Add(role);
			DB.SaveChanges();
		}

		#endregion ROLE SPECIFIC
	}
}