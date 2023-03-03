using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("ExpenseCategoryTypes")]
	public class ExpenseCategoryType
	{
		#region PROPERTIES

		[Key]
		[EnumDataType(typeof(ExpenseCategoryTypeEnum))]
		public int ExpenseCategoryTypeID { get; internal set; }

		[Required, MinLength(2), MaxLength(50)]
		public string ExpenseCategoryTypeName { get; internal set; }

		#endregion PROPERTIES
	}
}