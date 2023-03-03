using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("BillableIndicators")]
	public class BillableIndicator
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int BillableIndicatorID { get; internal set; }

		[Required, MinLength(2), MaxLength(50)]
		public string BillableIndicatorName { get; set; }

		#endregion PROPERTIES
	}
}