using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class ReportEmailRepo : BaseRepo
	{
		#region REPORT EMAIL SPECIFIC

		public static IEnumerable<Report> GetReportList(int frequencyID) => (from re in DB.ReportEmails
																			 join r in DB.Reports on re.ReportID equals r.ReportID
																			 where re.FrequencyID == frequencyID
																			 select r
					).Distinct();

		public static IEnumerable<User> GetReportUserEmailList(int reportID, int frequencyID) => (from re in DB.ReportEmails
																								  join u in DB.Users on re.UserID equals u.UserID
																								  where re.FrequencyID == frequencyID && re.ReportID == reportID && u.IsActive
																								  select u
					).Distinct();

		public static IEnumerable<User> GetReportUserEmailList(int reportID) => (from re in DB.ReportEmails
																				 join u in DB.Users on re.UserID equals u.UserID
																				 where re.ReportID == reportID && u.IsActive
																				 select u
					).Distinct();

		public static IEnumerable<User> GetManagerUserEmailList(int reportID, int frequencyID) => (from re in DB.ReportEmails
																								   join u in DB.Users on re.UserID equals u.UserID
																								   join ur in DB.UserRoles on u.UserID equals ur.UserID
																								   where re.FrequencyID == frequencyID && re.ReportID == reportID && ur.RoleID == 4 && u.IsActive
																								   select u
					).Distinct();

		public static IEnumerable<ReportEmail> GetUserReportList(int userID) => (from re in DB.ReportEmails
																				 join r in DB.Reports on re.ReportID equals r.ReportID
																				 where re.UserID == userID
																				 select re
					).Distinct();

		public static IEnumerable<ReportEmail> GetUserReportList(IList<int> userIDs) => (from re in DB.ReportEmails
																						 join r in DB.Reports on re.ReportID equals r.ReportID
																						 where userIDs.Contains(re.UserID)
																						 select re
					).Distinct();

		public static void Save(ReportEmail reportEmail)
		{
			DB.ReportEmails.Add(reportEmail);
			DB.SaveChanges();
		}

		public static void Delete(ReportEmail reportEmail)
		{
			DB.ReportEmails.Remove(reportEmail);
			DB.SaveChanges();
		}

		#endregion REPORT EMAIL SPECIFIC
	}
}