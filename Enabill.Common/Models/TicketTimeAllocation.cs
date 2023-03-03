using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("vwTicketTimeAllocation")]
	public class TicketTimeAllocation
	{
		#region PROPERTIES

		[Key]
		public int WorkAllocationID { get; set; }

		public int Period { get; set; }

		public double HoursWorked { get; set; }

		public string ActivityName { get; set; }
		public string ClientName { get; set; }
		public string FullName { get; set; }
		public string ProjectName { get; set; }
		public string Remark { get; set; }
		public string TicketReference { get; set; }
		public string TicketSubject { get; set; }

		public DateTime DateCreated { get; set; }
		public DateTime DayWorked { get; set; }

		#endregion PROPERTIES
	}
}