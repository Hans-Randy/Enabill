using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("CurrencyType")]
	public class CurrencyType
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int CurrencyTypeID { get; set; }

		[Required, MaxLength(50)]
		public string CurrencyName { get; set; }

		[Required, MaxLength(3)]
		public string CurrencyISO { get; set; }

		#endregion PROPERTIES
	}
}
