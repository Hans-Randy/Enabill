using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("ForecastReferenceDefaults")]
	public class ForecastReferenceDefault
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ForecastReferenceDefaultID { get; set; }

		[Required]
		public int ModifiedByUserID { get; set; }

		[Required]
		public string Reference { get; set; }

		[Required]
		public DateTime EffectiveDate { get; set; }

		#endregion PROPERTIES
	}
}