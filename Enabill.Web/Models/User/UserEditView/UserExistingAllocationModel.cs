using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class UserExistingAllocationModel
	{
		#region INITIALIZATION

		public UserExistingAllocationModel(User user)
		{
			this.UserAllocations = this.LoadUserAllocations(user);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<UserAllocationExtendedModel> UserAllocations { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<UserAllocationExtendedModel> LoadUserAllocations(User user)
		{
			var model = UserAllocationRepo.GetUserAllocationExtendedModel(user.UserID).ToList();
			return model;
		}

		#endregion FUNCTIONS
	}
}