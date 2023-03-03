using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("UserHistories")]
	public class UserHistory
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int UserHistoryID { get; internal set; }

		[Required]
		public int Period { get; internal set; }

		[Required]
		public int UserID { get; internal set; }

		[Required]
		public double WorkHoursPerDay { get; internal set; }

		#endregion PROPERTIES

		#region USER HITORY SPECIFIC

		public static List<UserHistory> GetAllForPeriod(int period)
		{
			if (!period.IsValidPeriod())
				return new List<UserHistory>();

			return UserHistoryRepo.GetAllForPeriod(period)
					.ToList();
		}

		#endregion USER HITORY SPECIFIC

		internal static List<User> GetAllUsersInPeriod(int period)
		{
			if (!period.IsValidPeriod())
				return new List<User>();

			return UserHistoryRepo.GetUsersInPeriod(period)
					.OrderBy(u => u.FullName)
					.ToList();
		}
	}
}