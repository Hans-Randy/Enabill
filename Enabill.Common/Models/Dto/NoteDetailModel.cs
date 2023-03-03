namespace Enabill.Models.Dto
{
	public class NoteDetailModel
	{
		#region PROPERTIES

		public Activity Activity { get; internal set; }
		public Project Project { get; internal set; }
		public User User { get; internal set; }
		public Note Note { get; internal set; }
		public WorkAllocation WorkAllocation { get; internal set; }

		#endregion PROPERTIES
	}
}