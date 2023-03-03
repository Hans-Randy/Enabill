using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("ReportEmails")]
	public class ReportEmail
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ReportEmailID { get; internal set; }

		[Required]
		public int FrequencyID { get; set; }

		[Required]
		public int ReportID { get; set; }

		[Required]
		public int UserID { get; set; }

		#endregion PROPERTIES
	}
}