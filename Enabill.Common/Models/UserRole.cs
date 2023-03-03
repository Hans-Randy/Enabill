using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("UserRoles")]
	public class UserRole
	{
		#region PROPERTIES

		[Key]
		public int UserRoleID { get; internal set; }

		[Required]
		public int RoleID { get; internal set; }

		[Required]
		public int UserID { get; internal set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		[Required]
		public DateTime DateCreated { get; internal set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		internal static UserRole GetNew(User userRequesting, int userID, int roleID) => new UserRole()
		{
			UserID = userID,
			RoleID = roleID,
			DateCreated = DateTime.Now.ToCentralAfricanTime(),
			LastModifiedBy = userRequesting.FullName
		};

		#endregion FUNCTIONS
	}
}