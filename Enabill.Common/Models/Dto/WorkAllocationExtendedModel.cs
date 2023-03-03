using System.Collections.Generic;

namespace Enabill.Models.Dto
{
	public class WorkAllocationExtendedModel
	{
		#region PROPERTIES

		public bool IsSelected;

		public int WorkSessionStatusID;
		public int? NoteID;

		public string NoteText;

		public ActivityDetail Activity;
		public Client Client;
		public ProjectDetail Project;
		public UserDetails User;
		public WorkAllocation WorkAllocation;

		public List<string> AssociatedProjectTickets;

		#endregion PROPERTIES
	}
}