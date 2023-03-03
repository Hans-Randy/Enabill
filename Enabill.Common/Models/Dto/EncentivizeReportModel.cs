using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Models
{
	public class EncentivizeReportModel
	{
		#region INITIALIZATION

		public EncentivizeReportModel(DateTime? dateFrom, DateTime? dateTo)
		{
			this.WorkDays = EncentivizeReportRepo.GetWorkDays(dateFrom, dateTo);
			this.ReportUsers = EncentivizeReportRepo.GetReportRepoUsers().OrderBy(ru => ru.FullName).ToList();
			this.NonReportUsers = EncentivizeReportRepo.GetNonReportRepoUsers();
			this.WorkSessions = EncentivizeReportRepo.GetWorkSessions(dateFrom, dateTo);
			this.WorkAllocations = EncentivizeReportRepo.GetWorkAllocations(dateFrom, dateTo);
			this.LeaveAllocations = EncentivizeReportRepo.GetLeaveAllocations(dateFrom, dateTo);
			this.LeaveTypes = EncentivizeReportRepo.GetLeaveTypes();
			this.UsersWorkDays = new List<UserWorkDay>();
			this.UserWorkDaysExcelModel = new List<UserWorkDayExcelModel>();

			foreach (var user in this.ReportUsers)
			{
				var u = user;
				var userWorkDay = new UserWorkDay
				{
					UserName = u.UserName,
					UserId = u.UserID,
					FullName = u.FullName,
					EmailAddress = u.Email,
					Attended = "Yes",
					UserWorkDaysSessions = new List<UserWorkDayWorkSession>()
				};

				foreach (var workDay in this.WorkDays)
				{
					var wd = workDay;
					userWorkDay.UserWorkDaysSessions.Add(new UserWorkDayWorkSession()
					{
						WorkDay = wd.WorkDate,

						WorkSession = ((IEnumerable<WorkSession>)this.WorkSessions).Where(ww =>
						{
							if (ww.UserID == u.UserID)
								return ww.StartTime.Day == wd.WorkDate.Day;
							else
								return false;
						}).ToList(),

						WorkAllocations = ((IEnumerable<WorkAllocation>)this.WorkAllocations).Where(ww =>
						{
							if (ww.UserID == u.UserID)
								return ww.DayWorked.Day == wd.WorkDate.Day;
							else
								return false;
						}).ToList()
					});
				}

				userWorkDay.LeaveAllocations = ((IEnumerable<Leave>)this.LeaveAllocations).Where(la => la.UserID == u.UserID).ToList();

				foreach (var workDayWorkSession in userWorkDay.UserWorkDaysSessions)
				{
					var wd = workDayWorkSession;

					if (wd.WorkSession != null)
					{
						foreach (var workSession in wd.WorkSession)
							wd.Duration += (workSession.EndTime - workSession.StartTime).TotalHours - workSession.LunchTime;
					}

					if (wd.WorkAllocations != null)
						wd.Hours = ((IEnumerable<WorkAllocation>)wd.WorkAllocations).Select(wa => wa.HoursWorked).Sum();

					if (((IEnumerable<Leave>)userWorkDay.LeaveAllocations).Any(la => la.DateFrom.Day == wd.WorkDay.Day))
					{
						wd.Leave = ((IEnumerable<Leave>)userWorkDay.LeaveAllocations).First(la => la.DateFrom.Day == wd.WorkDay.Day);
					}
					else if (((IEnumerable<Leave>)userWorkDay.LeaveAllocations).Any(la => la.DateTo.Day == wd.WorkDay.Day))
					{
						wd.Leave = ((IEnumerable<Leave>)userWorkDay.LeaveAllocations).First(la => la.DateTo.Day == wd.WorkDay.Day);
					}
					else if (((IEnumerable<Leave>)userWorkDay.LeaveAllocations).Any(la =>
					{
						if (la.DateFrom.Day <= wd.WorkDay.Day)
							return la.DateTo.Day > wd.WorkDay.Day;
						else
							return false;
					}))
					{
						wd.Leave = ((IEnumerable<Leave>)userWorkDay.LeaveAllocations).First(la =>
						{
							if (la.DateFrom.Day <= wd.WorkDay.Day)
								return la.DateTo.Day > wd.WorkDay.Day;
							else
								return false;
						});
					}

					wd.HoursDiff = wd.Hours - wd.Duration;

					if (wd.Leave == null)
						wd.WorkHours = user.WorkHours;
				}

				userWorkDay.TotalWorkSessions = userWorkDay.UserWorkDaysSessions.Select(k => k.WorkSession.Count).Sum();
				userWorkDay.TotalByLeaveType = new Dictionary<int, int>();
				userWorkDay.TotalByLeaveType = userWorkDay.UserWorkDaysSessions.Where(l => l.Leave != null).GroupBy(tf => tf.Leave.LeaveType).ToDictionary(gdc => gdc.Key, gdc => gdc.Count());
				userWorkDay.TotalWorkHours = userWorkDay.UserWorkDaysSessions.Select(uw => uw.WorkHours).Sum();
				userWorkDay.TotalWorkDays = this.WorkDays.Count;
				userWorkDay.TotalWorkSessionDurationHours = userWorkDay.UserWorkDaysSessions.Select(l => l.Duration).Sum();
				userWorkDay.TotalHoursWorked = userWorkDay.UserWorkDaysSessions.Select(t => t.Hours).Sum();
				userWorkDay.TotalHoursDiff = userWorkDay.UserWorkDaysSessions.Count(t => t.HoursDiff != 0);

				if (userWorkDay.TotalHoursDiff == 0 && userWorkDay.TotalWorkSessionDurationHours > 0 && userWorkDay.TotalHoursWorked > 0)
				{
					userWorkDay.IsEligible = true;
					userWorkDay.PointsAwarded = 40;
				}

				this.UsersWorkDays.Add(userWorkDay);
			}

			this.TotalEligibleUsers = ((IEnumerable<UserWorkDay>)this.UsersWorkDays).Count(tf => tf.IsEligible);

			this.UserWorkDaysExcelModel = ((IEnumerable<UserWorkDayExcelModel>)((IEnumerable<UserWorkDay>)this.UsersWorkDays).Where(t => t.IsEligible).Select(b => new UserWorkDayExcelModel()
			{
				Attended = b.Attended,
				FullName = b.FullName,
				EmailAddress = b.EmailAddress,
				PointsAwarded = b.PointsAwarded
			}).OrderBy(l => l.FullName)).ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int MidMonth { get; set; }
		public int MidMonthTotalEligbleUsers { get; set; }
		public int TotalEligibleUsers { get; set; }
		public int TotalMidMonthUsers { get; set; }

		public List<User> NonReportUsers { get; set; }
		public List<User> ReportUsers { get; set; }
		public List<UserWorkDay> UsersWorkDays { get; set; }
		public List<UserWorkDayExcelModel> UserWorkDaysExcelModel { get; set; }
		public List<WorkAllocation> WorkAllocations { get; set; }
		public List<WorkDay> WorkDays { get; set; }
		public List<WorkSession> WorkSessions { get; set; }
		public List<Leave> LeaveAllocations { get; set; }
		public List<LeaveType> LeaveTypes { get; set; }

		#endregion PROPERTIES
	}

	public class UserWorkDay
	{
		#region PROPERTIES

		public bool IsEligible { get; set; }

		public int PointsAwarded { get; set; }
		public int TotalWorkDays { get; set; }
		public int TotalWorkSessions { get; set; }
		public int UserId { get; set; }

		public double TotalHoursDiff { get; set; }
		public double TotalHoursWorked { get; set; }
		public double TotalWorkHours { get; set; }
		public double TotalWorkSessionDurationHours { get; set; }

		public string Attended { get; set; }
		public string EmailAddress { get; set; }
		public string FirstName { get; set; }
		public string FullName { get; set; }
		public string LastName { get; set; }
		public string UserName { get; set; }

		public UserWorkDaySessionStatus Status { get; set; }

		public Dictionary<int, int> TotalByLeaveType { get; set; }

		public List<Leave> LeaveAllocations { get; set; }
		public List<UserWorkDayWorkSession> UserWorkDaysSessions { get; set; }

		#endregion PROPERTIES
	}

	public class UserWorkDayWorkSession
	{
		#region PROPERTIES

		public double Duration { get; set; }
		public double Hours { get; set; }
		public double HoursDiff { get; set; }
		public double WorkHours { get; set; }

		public string StatusMessage { get; set; }

		public DateTime WorkDay { get; set; }

		public UserWorkDaySessionStatus Status { get; set; }

		public FlexiDay FlexiDay { get; set; }
		public Leave Leave { get; set; }

		public List<WorkAllocation> WorkAllocations { get; set; }
		public List<WorkSession> WorkSession { get; set; }

		#endregion PROPERTIES
	}

	public enum UserWorkDaySessionStatus
	{
		[Description("Exception")]
		Exception = 1,

		[Description("Approved")]
		Approved = 2,

		[Description("Unapproved")]
		UnApproved = 3,

		[Description("All")]
		All = 5
	}
}