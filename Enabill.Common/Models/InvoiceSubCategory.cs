using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("InvoiceSubCategories")]
	public class InvoiceSubCategory
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int InvoiceSubCategoryID { get; set; }

		[Required]
		public int InvoiceCategoryID { get; set; }

		[MaxLength(10)]
		public string RefCode { get; set; }

		[Required, MinLength(3), MaxLength(100)]
		public string SubCategoryName { get; set; }

		public virtual InvoiceCategory InvoiceCategory { get; internal set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public void Save() => InvoiceSubCategoryRepo.Save(this);

		#endregion FUNCTIONS
	}
}