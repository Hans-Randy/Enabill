using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class FeedbackIndexModel
	{
		#region INITIALIZATION

		public FeedbackIndexModel(User currentUser, FeedbackThread thread)
		{
			this.FeedbackOpenList = this.LoadList(currentUser, true);
			this.FeedbackClosedList = this.LoadList(currentUser, false);

			if (thread == null)
				this.FeedbackInstance = new FeedbackInstanceModel(currentUser, FeedbackThread.GetNew());
			else
				this.FeedbackInstance = new FeedbackInstanceModel(currentUser, thread);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public FeedbackInstanceModel FeedbackInstance { get; private set; }

		public List<FeedbackListModel> FeedbackClosedList { get; private set; }
		public List<FeedbackListModel> FeedbackOpenList { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<FeedbackListModel> LoadList(User currentUser, bool isOpen)
		{
			var model = new List<FeedbackListModel>();

			FeedbackThread.GetAll(currentUser, isOpen).ForEach(t => model.Add(new FeedbackListModel(t)));

			return model.OrderByDescending(m => m.Thread.DateStarted).ToList();
		}

		#endregion FUNCTIONS
	}
}