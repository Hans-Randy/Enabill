using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("PrintOptions")]
	public class PrintOption
	{
		#region PROPERTIES

		[Required]
		public int PrintOptionID { get; internal set; }

		[Required, MaxLength(100)]
		public string PrintOptionName { get; internal set; }

		#endregion PROPERTIES
	}
}