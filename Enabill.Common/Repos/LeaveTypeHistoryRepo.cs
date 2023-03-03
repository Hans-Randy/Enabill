using System;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class LeaveTypeHistoryRepo : BaseRepo
	{
		#region LEAVE TYPE HISTORY SPECIFIC

		internal static LeaveTypeHistory GetForTypeAndDate(int leaveTypeID, DateTime refDate) => DB.LeaveTypeHistories
					.SingleOrDefault(x => x.LeaveTypeID == leaveTypeID && x.DateFrom <= refDate && (x.DateTo ?? refDate) >= refDate);

		#endregion LEAVE TYPE HISTORY SPECIFIC
	}
}