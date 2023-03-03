using System.Collections.Generic;

namespace Enabill.Models
{
	public class UserReport
	{
		#region PROPERTIES

		public User User { get; set; }

		public IList<Report> Reports { get; set; }

		#endregion PROPERTIES
	}
}