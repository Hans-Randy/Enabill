using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("BillingMethods")]
	public class BillingMethod
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int BillingMethodID { get; set; }

		[Required, MinLength(2), MaxLength(50)]
		public string BillingMethodName { get; set; }

		#endregion PROPERTIES
	}
}