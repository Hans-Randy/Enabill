using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class UserRoleModel
	{
		#region INITIALIZATION

		public UserRoleModel(User user)
		{
			this.UserRoles = user.GetRoles();
			this.UserRolesNotAssigned = user.GetOtherRoles();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<Role> UserRoles { get; private set; }
		public List<Role> UserRolesNotAssigned { get; private set; }

		#endregion PROPERTIES
	}
}