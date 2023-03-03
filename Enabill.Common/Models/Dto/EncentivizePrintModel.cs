using System;
using System.Collections.Generic;

namespace Enabill.Models.Dto
{
	public class EncentivizePrintModel
	{
		#region INITIALIZATION

		public EncentivizePrintModel(DateTime dateFrom, DateTime dateTo, int divisionID)
		{
			//not sure why this is needed, since its called somewhere else
			//EncentivizeReport = divisionID != 0 ? UserRepo.GetDivisionTimeCaptureEncentivize(dateFrom, dateTo, divisionID).ToList() : UserRepo.GetAllTimeEncentivize(dateFrom, dateTo).ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<EncentivizeModel> EncentivizeReport { get; set; }

		#endregion PROPERTIES
	}
}