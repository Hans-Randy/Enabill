using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class UserReportEmailModel
	{
		#region INITIALIZATION

		public UserReportEmailModel(User user)
		{
			this.User = user;

			this.Emails = ReportEmailRepo.GetUserReportList(user.UserID).ToList();
			this.Reports = ReportRepo.GetAll().ToList();
			this.Frequencies = FrequencyRepo.GetAll().ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public User User { get; internal set; }

		public List<Frequency> Frequencies { get; internal set; }
		public List<Report> Reports { get; internal set; }
		public List<ReportEmail> Emails { get; internal set; }

		#endregion PROPERTIES
	}
}