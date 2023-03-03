using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("vwUserLastWorkSessions")]
	public class UserLastWorkSession
	{
		#region PROPERTIES

		[Key]
		public int UserID { get; set; }

		public DateTime LastWorkSessionDate { get; set; }

		#endregion PROPERTIES
	}
}