using System.Collections.Generic;

namespace Enabill.Models.Dto
{
	public class ProjectActivityUserDetail
	{
		#region PROPERTIES

		public Activity Activity;

		public List<UserDetails> ActivityUsers;
		public List<UserDetails> UsersNotAssignedToActivity;

		#endregion PROPERTIES
	}

	public class UserDetails
	{
		#region INITIALIZATION

		public UserDetails(User u)
		{
			this.UserID = u.UserID;
			this.FirstName = u.FirstName;
			this.LastName = u.LastName;
			this.FullName = u.FullName;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int UserID;

		public string FirstName;
		public string LastName;
		public string FullName;

		#endregion PROPERTIES
	}
}