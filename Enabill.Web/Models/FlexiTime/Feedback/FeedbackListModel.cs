using Enabill.Models;

namespace Enabill.Web.Models
{
	public class FeedbackListModel
	{
		#region INITIALIZATION

		public FeedbackListModel(FeedbackThread thread)
		{
			this.Thread = thread;
			this.FeedbackUser = thread.GetThreadUser().FullName;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public string FeedbackUser { get; internal set; }

		public FeedbackThread Thread { get; internal set; }

		#endregion PROPERTIES
	}
}