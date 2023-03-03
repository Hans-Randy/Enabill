using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("Departments")]
	public class Department
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int DepartmentID { get; set; }

		[Required, MaxLength(50)]
		public string DepartmentName { get; set; }

		#endregion PROPERTIES
	}
}