namespace Enabill.Models
{
	public class UserLeave
	{
		#region PROPERTIES

		public Leave Leave { get; set; }
		public User Manager { get; set; }
		public User User { get; set; }

		#endregion PROPERTIES
	}
}