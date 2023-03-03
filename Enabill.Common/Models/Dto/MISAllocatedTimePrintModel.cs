using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models.Dto
{
	public class MISAllocatedTimePrintModel
	{
		#region INITIALIZATION

		public MISAllocatedTimePrintModel(DateTime dateFrom, DateTime dateTo, int divisionID, int departmentID)
		{
			//NOTE the User Cost to Company is password protected, hence the scheduled report will return 0 for cost to company as
			//the job does not know the password. User will have run via the Web where they can enter the password to decrypt CTC
			this.MISAllocatedTimeReport = divisionID != 0 && departmentID == 0 ? WorkAllocationRepo.GetMISAllocatedTimeForDivision(dateFrom, dateTo, divisionID, "").ToList() : divisionID == 0 && departmentID != 0 ? WorkAllocationRepo.GetMISAllocatedTimeForDepartment(dateFrom, dateTo, departmentID, "password").ToList() : divisionID != 0 && departmentID != 0 ? WorkAllocationRepo.GetMISAllocatedTimeForDivisionDepartment(dateFrom, dateTo, divisionID, departmentID, "").ToList() : WorkAllocationRepo.GetMISAllocatedTimeForPeriod(dateFrom, dateTo, "").ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<MISAllocatedTimeModel> MISAllocatedTimeReport { get; set; }

		#endregion PROPERTIES
	}
}