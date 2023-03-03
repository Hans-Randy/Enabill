using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("InvoiceCategories")]
	public class InvoiceCategory
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int InvoiceCategoryID { get; set; }

		[Required, MinLength(3), MaxLength(50)]
		public string CategoryName { get; set; }

		[MaxLength(10)]
		public string ExternalRef { get; set; }

		public virtual ICollection<InvoiceSubCategory> InvoiceSubCategories { get; internal set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public void Save() => InvoiceCategoryRepo.Save(this);

		#endregion FUNCTIONS
	}
}