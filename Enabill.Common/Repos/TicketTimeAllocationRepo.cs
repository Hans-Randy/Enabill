using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class TicketTimeAllocationRepo : BaseRepo
	{
		#region TICKET TIME ALLOCATION

		public static List<TicketTimeAllocation> GetAll(DateTime fromPeriod, DateTime toPeriod) => DB.TicketTimeAllocations.Where(tta => tta.DayWorked >= fromPeriod && tta.DayWorked <= toPeriod).OrderBy(tta => tta.FullName).Distinct().ToList();

		#endregion TICKET TIME ALLOCATION
	}
}