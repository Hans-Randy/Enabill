using System.Collections.Generic;

namespace Enabill.Models
{
	public class ProjectWrapper : IProject
	{
		public List<string> GetDistinctProjectNames() => Project.GetDistinctProjectNames();

		public List<string> GetDistinctProjectNamesForClientID(int clientID) => Project.GetDistinctProjectNamesForClientID(clientID);
	}
}