using System.Collections.Generic;

namespace Enabill.Models
{
	public class ActivityWrapper : IActivity
	{
		#region FUNCTIONS

		public List<string> GetDistinctActivityNames() => Activity.GetDistinctActivityNames();

		public List<string> GetDistinctActivityNamesForClientID(int clientID) => Activity.GetDistinctActivityNamesForClientID(clientID);

		public List<string> GetDistinctActivityNamesForProjectName(string projectName) => Activity.GetDistinctActivityNamesForProjectName(projectName);

		#endregion FUNCTIONS
	}
}