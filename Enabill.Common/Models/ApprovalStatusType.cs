using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("ApprovalStatus")]
	public class ApprovalStatus
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int ApprovalStatusID { get; set; }

		[Required, MaxLength(50)]
		public string ApprovalStatusName { get; set; }

		#endregion PROPERTIES
	}
}