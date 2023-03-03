using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("BalanceChangeTypes")]
	public class BalanceChangeType
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int BalanceChangeTypeID { get; set; }

		[Required]
		public string BalanceChangeTypeName { get; set; }

		#endregion PROPERTIES
	}
}