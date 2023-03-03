using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class ReportRepo : BaseRepo
	{
		#region REPORT SPECIFIC

		public Report GetByID(int reportID) => DB.Reports
					.SingleOrDefault(i => i.ReportID == reportID);

		public static IEnumerable<Report> GetAll() => DB.Reports.OrderBy(r => r.ReportName);

		#endregion REPORT SPECIFIC
	}
}