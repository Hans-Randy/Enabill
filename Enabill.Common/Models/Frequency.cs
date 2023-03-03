using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("Frequencies")]
	public class Frequency
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int FrequencyID { get; private set; }

		public string FrequencyName { get; private set; }

		#endregion PROPERTIES
	}
}