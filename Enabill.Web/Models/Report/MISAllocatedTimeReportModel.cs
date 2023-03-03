using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class MISAllocatedTimeReportModel
	{
		#region INITIALIZATION

		public MISAllocatedTimeReportModel(DateTime dateFrom, DateTime dateTo, int divisionID, int departmentID)
		{
			this.MISAllocatedTimeDateFrom = InputHistory.GetDateTime(HistoryItemType.MISAllocatedTimeDateFrom, DateTime.Today.AddDays(-7)).Value;
			this.MISAllocatedTimeDateTo = InputHistory.GetDateTime(HistoryItemType.MISAllocatedTimeDateTo, DateTime.Today).Value;
			this.MISAllocatedTimeReport = divisionID != 0 && departmentID == 0 ? WorkAllocationRepo.GetMISAllocatedTimeForDivision(dateFrom, dateTo, divisionID, Settings.Current.Passphrase).ToList() : divisionID == 0 && departmentID != 0 ? WorkAllocationRepo.GetMISAllocatedTimeForDepartment(dateFrom, dateTo, departmentID, Settings.Current.Passphrase).ToList() : divisionID != 0 && departmentID != 0 ? WorkAllocationRepo.GetMISAllocatedTimeForDivisionDepartment(dateFrom, dateTo, divisionID, departmentID, Settings.Current.Passphrase).ToList() : WorkAllocationRepo.GetMISAllocatedTimeForPeriod(dateFrom, dateTo, Settings.Current.Passphrase).ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public DateTime MISAllocatedTimeDateFrom { get; set; }
		public DateTime MISAllocatedTimeDateTo { get; set; }

		public List<MISAllocatedTimeModel> MISAllocatedTimeReport { get; set; }

		#endregion PROPERTIES
	}
}