using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("BalanceTypes")]
	public class BalanceType
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int BalanceTypeID { get; set; }

		[Required]
		public string BalanceTypeName { get; set; }

		#endregion PROPERTIES
	}
}