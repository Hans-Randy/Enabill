using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("EmploymentTypes")]
	public class EmploymentType
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int EmploymentTypeID { get; set; }

		[Required, MaxLength(50)]
		public string EmploymentTypeName { get; set; }

		#endregion PROPERTIES
	}
}