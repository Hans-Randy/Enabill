using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models.Dto
{
	public class WorkAllocationExceptionPrintModel
	{
		#region INITIALIZATION

		public WorkAllocationExceptionPrintModel(DateTime dateFrom, DateTime dateTo, int divisionID)
		{
			this.WorkAllocationExceptionReport = divisionID != 0 ? UserRepo.GetDivisionTimeCaptureExceptions(dateFrom, dateTo, divisionID).ToList() : UserRepo.GetAllTimeCaptureExceptions(dateFrom, dateTo).ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<WorkAllocationExceptionModel> WorkAllocationExceptionReport { get; set; }

		#endregion PROPERTIES
	}
}