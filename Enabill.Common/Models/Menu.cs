using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("Menus")]
	public class Menu
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int MenuID { get; set; }

		[Required]
		public int RoleBW { get; set; }

		public int? CustomSort { get; set; }

		[Required]
		public bool IsActive { get; set; }

		[MaxLength(50)]
		public string Action { get; set; }

		[Required, MaxLength(50)]
		public string Controller { get; set; }

		[Required, MaxLength(512)]
		public string MenuHoverImagePath { get; set; }

		[Required, MaxLength(512)]
		public string MenuImagePath { get; set; }

		[Required, MaxLength(50)]
		public string MenuName { get; set; }

		#endregion PROPERTIES
	}
}