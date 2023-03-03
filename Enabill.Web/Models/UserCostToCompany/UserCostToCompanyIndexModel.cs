using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Web.Models
{
	public class UserCostToCompanyIndexModel
	{
		#region INITIALIZATION

		public UserCostToCompanyIndexModel(string passphrase, int period)
		{
			this.Period = period;
			this.MonthDate = period.ToDateFromPeriod();
			this.UserCTCList = this.LoadCostToCompanyModel(period);
			this.Month = Month.GetByPeriod(period);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int Period { get; private set; }

		public DateTime MonthDate { get; private set; }

		public Month Month { get; private set; }

		public List<UserCostToCompanyExtendedModel> UserCTCList { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<UserCostToCompanyExtendedModel> LoadCostToCompanyModel(int period)
		{
			var model = new List<UserCostToCompanyExtendedModel>();

			foreach (var uctc in UserCostToCompany.GetAllForMonth(period))
				model.Add(new UserCostToCompanyExtendedModel(uctc));

			return model.OrderBy(m => m.User.FullName)
						.ToList();
		}

		#endregion FUNCTIONS
	}
}