using System;
using System.Collections.Generic;

namespace Enabill.Models.Dto
{
	public class WeekPasteDataModel
	{
		#region PROPERTIES

		public DateTime StartDate { get; set; }

		public List<ActivityPostModel> Logs { get; set; }

		#endregion PROPERTIES
	}
}