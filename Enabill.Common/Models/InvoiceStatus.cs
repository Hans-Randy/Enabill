using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("InvoiceStatus")]
	public class InvoiceStatus
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int InvoiceStatusID { get; private set; }

		public string StatusName { get; private set; }

		#endregion PROPERTIES
	}
}