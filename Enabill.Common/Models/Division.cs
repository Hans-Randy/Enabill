using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("Divisions")]
	public class Division
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int DivisionID { get; set; }

		[Required]
		public bool IsActive { get; set; }

		[Required, MaxLength(50)]
		public string DivisionName { get; set; }

		[Required, MaxLength(6)]
		public string DivisionCode { get; set; }

		#endregion PROPERTIES
	}
}