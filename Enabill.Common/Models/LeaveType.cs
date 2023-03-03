using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("LeaveTypes")]
	public class LeaveType
	{
		#region PROPERTIES

		[Key]
		[EnumDataType(typeof(LeaveTypeEnum))]
		public int LeaveTypeID { get; internal set; }

		[Required, MinLength(2), MaxLength(64)]
		public string LeaveTypeName { get; internal set; }

		#endregion PROPERTIES
	}
}