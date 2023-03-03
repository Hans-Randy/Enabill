using System.Collections.Generic;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Web.Models
{
	public class NoteOverviewIndexModel
	{
		#region INITIALIZATION

		//this model is used for the index view (/Note/Index) for users with the time capturer role, viewing their own notes
		public NoteOverviewIndexModel(User currentUser, List<NoteDetailModel> noteList)
		{
			this.ActivityModel = new NoteActivityModel(currentUser);
			this.NoteList = noteList;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public NoteActivityModel ActivityModel { get; private set; }

		public List<NoteDetailModel> NoteList { get; private set; }

		#endregion PROPERTIES
	}
}