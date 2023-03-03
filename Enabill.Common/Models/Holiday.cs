using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("Holidays")]
	public class Holiday
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int HolidayID { get; set; }

		[Required]
		public bool IsFixedDate { get; set; }

		[Required]
		public bool IsRepeated { get; set; }

		[Required, MinLength(2), MaxLength(128)]
		public string CreatedBy { get; set; }
		public string LastModifiedBy { get; set; }
		public string DeletedBy { get; set; }
		[Required, MinLength(2), MaxLength(50)]
		public string HolidayName { get; set; }

		[Required]
		[DisplayFormat(DataFormatString = "{0:dd MMM}")]
		public DateTime Date { get; set; }

		[Required]
		public DateTime DateCreated { get; set; }
		public DateTime? DateUpdated { get; set; }
		public StatusEnum Status { get; set; }
		public DateTime? DateDeleted { get; set; }

		#endregion PROPERTIES

		#region METHODS
		
		public bool CanDelete() => !this.IsRepeated && !this.IsFixedDate;

		#endregion METHODS
	}
}