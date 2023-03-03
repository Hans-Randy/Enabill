using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("WebHistories")]
	public class WebHistory
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int WebHistoryID { get; private set; }

		public int? UserID { get; private set; }

		[Required, MaxLength(50)]
		public string IPAddress { get; private set; }

		[Required, MaxLength(512)]
		public string RequestUrl { get; private set; }

		[Required, MaxLength(512)]
		public string UserAgent { get; private set; }

		[Required]
		public DateTime RequestDate { get; private set; }

		#endregion PROPERTIES

		#region WEB HISTORY

		public static void CaptureWebHistoryRequest(int? userID, string requestedURL, string userAgent, string ipAddress)
		{
			var webHistory = new WebHistory()
			{
				UserID = userID,
				RequestDate = DateTime.Now.ToCentralAfricanTime(),
				UserAgent = userAgent,
				RequestUrl = requestedURL,
				IPAddress = ipAddress
			};

			WebHistoryRepo.Save(webHistory);
		}

		#endregion WEB HISTORY
	}
}