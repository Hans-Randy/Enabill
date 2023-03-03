using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models.Dto
{
	public class DSActivityReportModel
	{
		#region INITIALIZATION

		public DSActivityReportModel(DateTime dateFrom, DateTime dateTo)
		{
			this.DSActivityReport = this.LoadDSActivityReportModel(dateFrom, dateTo);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<UserTimeSplitReportModel> DSActivityReport { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<UserTimeSplitReportModel> LoadDSActivityReportModel(DateTime dateFrom, DateTime dateTo) => UserTimeSplitRepo.GetAll(dateFrom, dateTo, 0, "0", "0", "0", "Development Services").ToList();

		#endregion FUNCTIONS
	}
}