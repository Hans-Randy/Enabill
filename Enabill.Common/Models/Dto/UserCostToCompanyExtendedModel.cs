using System;
using Enabill.Repos;

namespace Enabill.Models.Dto
{
	public class UserCostToCompanyExtendedModel
	{
		#region INITIALIZATION

		public UserCostToCompanyExtendedModel(UserCostToCompany model)
		{
			this.CostToCompany = model;
			this.User = UserRepo.GetByID(model.UserID);
			this.MonthDate = model.Period.ToDateFromPeriod();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public DateTime MonthDate { get; private set; }

		public User User { get; private set; }
		public UserCostToCompany CostToCompany { get; private set; }

		#endregion PROPERTIES
	}
}