using System.Collections.Generic;
using System.Linq;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public class PercentageAllocationReportRepo : BaseRepo
	{
		public static List<PercentageAllocationModel> ExecutePercentageAllocationReport(int year, int month, int finPeriod, string managerID) => DB.Database.SqlQuery<PercentageAllocationModel>("EXEC PercentageAllocationReport_LSP {0}, {1}, {2}, {3}", year, month, finPeriod, managerID).ToList();
	}
}