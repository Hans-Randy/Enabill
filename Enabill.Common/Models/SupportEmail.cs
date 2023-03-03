using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("vwSupportEmails")]
	public class SupportEmail
	{
		#region PROPERTIES

		[Key]
		public string UKey { get; internal set; }

		public int ClientID { get; internal set; }
		public int ProjectID { get; internal set; }

		public string SupportEmailAddress { get; internal set; }

		#endregion PROPERTIES
	}
}