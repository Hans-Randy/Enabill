using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("Roles")]
	public class Role
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int RoleID { get; set; }

		[Required, MaxLength(50)]
		public string RoleName { get; set; }

		#endregion PROPERTIES

		#region ROLE

		//This method is only begin used by the tests
		public void Save() => RoleRepo.Save(this);

		#endregion ROLE
	}
}