using System.Collections.Generic;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Web.Models
{
	public class NoteProjectOwnerOverviewIndexModel
	{
		#region INITIALIZATION

		//this model is used for the index view (/Note/Index) for users with the project owner roles
		public NoteProjectOwnerOverviewIndexModel(User currentUser, List<NoteDetailModel> noteList)
		{
			this.ActivityModel = new NoteActivityModel(currentUser);
			this.UserList = currentUser.GetStaffOfManager();
			this.NoteList = noteList;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public NoteActivityModel ActivityModel { get; private set; }

		public List<User> UserList { get; private set; }
		public List<NoteDetailModel> NoteList { get; private set; }

		#endregion PROPERTIES
	}
}