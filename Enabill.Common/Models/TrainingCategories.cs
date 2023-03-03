using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("TrainingCategories")]
	public class TrainingCategory
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int TrainingCategoryID { get; internal set; }

		public string TrainingCategoryName { get; internal set; }

		#endregion PROPERTIES
	}
}