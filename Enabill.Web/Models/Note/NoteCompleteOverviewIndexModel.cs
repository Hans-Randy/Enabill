using System.Collections.Generic;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Web.Models
{
	public class NoteCompleteOverviewIndexModel
	{
		#region INITIALIZATION

		//this model is used for the index view (/Note/Index) for users with both the manager and project owner roles
		public NoteCompleteOverviewIndexModel(User currentUser, List<NoteDetailModel> noteList)
		{
			this.ActivityModel = new NoteActivityModel(currentUser);
			this.UserList = currentUser.GetStaffOfManager();
			this.NoteList = noteList;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public NoteActivityModel ActivityModel { get; private set; }

		public List<NoteDetailModel> NoteList { get; private set; }
		public List<User> UserList { get; private set; }

		#endregion PROPERTIES
	}
}