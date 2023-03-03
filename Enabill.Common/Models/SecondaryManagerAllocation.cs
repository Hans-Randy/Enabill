using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("SecondaryManagerAllocations")]
	public class SecondaryManagerAllocation
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int StaffManagerAllocationID { get; private set; }

		[Required]
		public int ManagerID { get; internal set; }

		[Required]
		public int UserID { get; internal set; }

		#endregion PROPERTIES

		#region SECONDARY MANAGER

		internal static SecondaryManagerAllocation CreateNew(int managingUserID, int userID) => new SecondaryManagerAllocation()
		{
			ManagerID = managingUserID,
			UserID = userID
		};

		#endregion SECONDARY MANAGER
	}
}