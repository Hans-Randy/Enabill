using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("WorkSessionStatus")]
	public class WorkSessionStatus
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int WorkSessionStatusID { get; private set; }

		public string StatusName { get; private set; }

		#endregion PROPERTIES
	}
}