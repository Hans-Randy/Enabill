using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class UserAllocationModel
	{
		#region INITIALIZATION

		public UserAllocationModel(User user, int? id)
		{
			if (id != null)
			{
				this.UserActivities = this.LoadUserActivities(user);
				this.MaxNumberOfAllocations = this.LoadMaxNumberOfAllocations(this.UserActivities);
				this.MaxNumberOfActivities = this.LoadMaxNumberOfActivities(this.UserActivities);
				this.MaxNumberOfProjects = this.LoadMaxNumberOfProjects(this.UserActivities);
				this.MaxNumberOfClients = this.UserActivities.Count;
			}

			this.User = user;
			this.UserExistingAllocations = new UserExistingAllocationModel(user);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int MaxNumberOfActivities { get; internal set; }
		public int MaxNumberOfAllocations { get; internal set; }
		public int MaxNumberOfClients { get; internal set; }
		public int MaxNumberOfProjects { get; internal set; }

		public User User { get; internal set; }
		public UserExistingAllocationModel UserExistingAllocations { get; internal set; }

		public List<ClientWrapperModel> UserActivities { get; internal set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<ClientWrapperModel> LoadUserActivities(User user)
		{
			var model = new List<ClientWrapperModel>();

			ClientRepo.GetAll().OrderBy(r => r.ClientName).ToList().ForEach(client => model.Add(new ClientWrapperModel(client, user)));

			//model.Where(m => m.MaxNumberOfAllocations == 0).ToList().ForEach(m => model.Remove(m));

			return model;
		}

		private int LoadMaxNumberOfAllocations(List<ClientWrapperModel> userActivities)
		{
			int returnValue = 0;

			userActivities.ForEach(c => returnValue += c.MaxNumberOfAllocations);

			return returnValue;
		}

		private int LoadMaxNumberOfActivities(List<ClientWrapperModel> userActivities)
		{
			int returnValue = 0;

			userActivities.ForEach(c => returnValue += c.MaxNumberOfActivities);

			return returnValue;
		}

		private int LoadMaxNumberOfProjects(List<ClientWrapperModel> userActivities)
		{
			int returnValue = 0;

			userActivities.ForEach(c => returnValue += c.MaxNumberOfProjects);

			return returnValue;
		}

		#endregion FUNCTIONS
	}
}