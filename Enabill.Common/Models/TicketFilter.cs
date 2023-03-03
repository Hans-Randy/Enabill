using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("TicketFilters")]
	public class TicketFilter
	{
		#region PROPERTIES

		[Key, Required]
		public int TicketFilterID { get; set; }

		[Required]
		public int ProjectID { get; set; }

		[MaxLength(256)]
		public string FromAddress { get; set; }

		[MaxLength(256)]
		public string ToAddress { get; set; }

		[MaxLength(512)]
		public string TicketSubject { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public bool IsFilterMatch(string fromEmail, List<string> toEmailList, string subject)
		{
			FeedbackClientFilterEnum filterNeededBW = 0, filterMatchBW = 0;

			/* From Email */
			if (!string.IsNullOrEmpty(this.FromAddress))
			{
				filterNeededBW |= FeedbackClientFilterEnum.From;

				if (!string.IsNullOrEmpty(fromEmail) && this.FromAddress == fromEmail)
					filterMatchBW |= FeedbackClientFilterEnum.From;
			}

			/* To Email */
			if (!string.IsNullOrEmpty(this.ToAddress))
			{
				filterNeededBW |= FeedbackClientFilterEnum.To;

				if (toEmailList.Contains(this.ToAddress))
					filterMatchBW |= FeedbackClientFilterEnum.To;
			}

			/* Subject */
			if (!string.IsNullOrEmpty(this.TicketSubject))
			{
				filterNeededBW |= FeedbackClientFilterEnum.Subject;

				if (!string.IsNullOrEmpty(subject) && subject.Contains(this.TicketSubject))
					filterMatchBW |= FeedbackClientFilterEnum.Subject;
			}

			return filterNeededBW == filterMatchBW;

			//From Email
			////From Email has value check that values match
			//if (((!string.IsNullOrEmpty(From) & !string.IsNullOrEmpty(fromEmail)) & (From == fromEmail))
			//    //From emails both empty
			//    || (string.IsNullOrEmpty(From) & string.IsNullOrEmpty(fromEmail)))
			//{
			//    /* To Email */
			//    //To Email has value check that value is in email list
			//    if ((!string.IsNullOrEmpty(To) & toEmailList.Contains(To))
			//        // To email empty continue with checks
			//        || (string.IsNullOrEmpty(To)))
			//    {
			//        /* Subject */
			//        if (((!string.IsNullOrEmpty(Subject) & !string.IsNullOrEmpty(subject)) & (Subject == subject))
			//            || (string.IsNullOrEmpty(Subject) & string.IsNullOrEmpty(subject)))
			//        {
			//        }
			//    }
			//}
		}

		#endregion FUNCTIONS
	}
}