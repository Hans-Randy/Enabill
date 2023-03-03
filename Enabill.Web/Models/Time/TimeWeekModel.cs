using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Enabill.Models;
using Enabill.Models.Dto;
using static Enabill.Models.WorkDay;

namespace Enabill.Web.Models
{
	public class TimeWeekModel
	{
		#region INITIALIZATION

		public TimeWeekModel(User user, DateTime startWorkDate, string threeState, string callingPage = "")
		{
			this.User = user;
			this.StartDate = startWorkDate;
			this.ThreeState = threeState;
			this.CallingPage = callingPage;

			this.TimeDailyModels = this.DailyModels(startWorkDate, callingPage);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool IsMondayWorkable => IsDayWorkable(this.StartDate);
		public bool IsTuesdayWorkable => IsDayWorkable(this.StartDate.AddDays(1));
		public bool IsWednesdayWorkable => IsDayWorkable(this.StartDate.AddDays(2));
		public bool IsThursdayWorkable => IsDayWorkable(this.StartDate.AddDays(3));
		public bool IsFridayWorkable => IsDayWorkable(this.StartDate.AddDays(4));

		public bool DayLockedMonday { get; private set; }
		public bool DayLockedTuesday { get; private set; }
		public bool DayLockedWednesday { get; private set; }
		public bool DayLockedThursday { get; private set; }
		public bool DayLockedFriday { get; private set; }
		public bool DayLockedSaturday { get; private set; }
		public bool DayLockedSunday { get; private set; }

		public string ThreeState { get; private set; }

		public string CallingPage { get; private set; }

		public DateTime StartDate { get; set; }

		public User User { get; private set; }

		public List<TimeDayModel> TimeDailyModels { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<TimeDayModel> DailyModels(DateTime startWorkDate, string callingPage)
		{
			var dailyModels = new List<TimeDayModel>();
			int j = DateTime.DaysInMonth(startWorkDate.Year, startWorkDate.Month);
			DateTime weekDate;

			for (int i = 0; i <= 6; i++)
			{
				weekDate = this.WeekDate(startWorkDate, i, j);
				dailyModels.Add(new TimeDayModel(this.User, weekDate, callingPage));
			}

			return dailyModels;
		}

		private DateTime WeekDate(DateTime startWorkDate, int addDays, int daysInMonth)
		{
			if (startWorkDate.Day + addDays <= daysInMonth)
			{
				startWorkDate = startWorkDate.AddDays(addDays);
			}
			else
			{
				addDays = startWorkDate.Day + addDays - daysInMonth - 1;
				startWorkDate = startWorkDate.AddMonths(1).ToFirstDayOfMonth().AddDays(addDays);
			}

			return startWorkDate;
		}

		public string GetWorkListJson()
		{
			var output = new List<WeeklyModel>();

			foreach (var dailyModel in this.TimeDailyModels)
			{
				switch (dailyModel.WorkDay.DayOfWeek)
				{
					case DayOfWeek.Monday:
						this.DayLockedMonday = dailyModel.IsTimesheetLockedForDay;
						break;

					case DayOfWeek.Tuesday:
						this.DayLockedTuesday = dailyModel.IsTimesheetLockedForDay;
						break;

					case DayOfWeek.Wednesday:
						this.DayLockedWednesday = dailyModel.IsTimesheetLockedForDay;
						break;

					case DayOfWeek.Thursday:
						this.DayLockedThursday = dailyModel.IsTimesheetLockedForDay;
						break;

					case DayOfWeek.Friday:
						this.DayLockedFriday = dailyModel.IsTimesheetLockedForDay;
						break;

					case DayOfWeek.Saturday:
						this.DayLockedSaturday = dailyModel.IsTimesheetLockedForDay;
						break;

					case DayOfWeek.Sunday:
						this.DayLockedSunday = dailyModel.IsTimesheetLockedForDay;
						break;

					default:
						break;
				}

				foreach (var wa in dailyModel.WorkAllocations)
				{
					if (output.Find(x => x.ActivityId == wa.Activity.ActivityID) != null)
					{
						if (!(wa.WorkAllocation.HoursWorked > 0))
							continue;

						if (wa.WorkAllocation.DayWorked.DayOfWeek == DayOfWeek.Monday)
						{
							if (output.Find(x => x.ActivityId == wa.Activity.ActivityID).Monday == null)
							{
								output.Find(x => x.ActivityId == wa.Activity.ActivityID).Monday = wa.WorkAllocation.HoursWorked;
							}
							else
							{
								output.Find(x => x.ActivityId == wa.Activity.ActivityID).Monday += wa.WorkAllocation.HoursWorked;
							}

							continue;
						}

						if (wa.WorkAllocation.DayWorked.DayOfWeek == DayOfWeek.Tuesday)
						{
							if (output.Find(x => x.ActivityId == wa.Activity.ActivityID).Tuesday == null)
							{
								output.Find(x => x.ActivityId == wa.Activity.ActivityID).Tuesday = wa.WorkAllocation.HoursWorked;
							}
							else
							{
								output.Find(x => x.ActivityId == wa.Activity.ActivityID).Tuesday += wa.WorkAllocation.HoursWorked;
							}

							output.Find(x => x.ActivityId == wa.Activity.ActivityID).RemarkTuesday = wa.WorkAllocation.Remark;

							continue;
						}

						if (wa.WorkAllocation.DayWorked.DayOfWeek == DayOfWeek.Wednesday)
						{
							if (output.Find(x => x.ActivityId == wa.Activity.ActivityID).Wednesday == null)
							{
								output.Find(x => x.ActivityId == wa.Activity.ActivityID).Wednesday = wa.WorkAllocation.HoursWorked;
							}
							else
							{
								output.Find(x => x.ActivityId == wa.Activity.ActivityID).Wednesday += wa.WorkAllocation.HoursWorked;
							}

							output.Find(x => x.ActivityId == wa.Activity.ActivityID).RemarkWednesday = wa.WorkAllocation.Remark;

							continue;
						}

						if (wa.WorkAllocation.DayWorked.DayOfWeek == DayOfWeek.Thursday)
						{
							if (output.Find(x => x.ActivityId == wa.Activity.ActivityID).Thursday == null)
							{
								output.Find(x => x.ActivityId == wa.Activity.ActivityID).Thursday = wa.WorkAllocation.HoursWorked;
							}
							else
							{
								output.Find(x => x.ActivityId == wa.Activity.ActivityID).Thursday += wa.WorkAllocation.HoursWorked;
							}

							output.Find(x => x.ActivityId == wa.Activity.ActivityID).RemarkThursday = wa.WorkAllocation.Remark;

							continue;
						}

						if (wa.WorkAllocation.DayWorked.DayOfWeek == DayOfWeek.Friday)
						{
							if (output.Find(x => x.ActivityId == wa.Activity.ActivityID).Friday == null)
							{
								output.Find(x => x.ActivityId == wa.Activity.ActivityID).Friday = wa.WorkAllocation.HoursWorked;
							}
							else
							{
								output.Find(x => x.ActivityId == wa.Activity.ActivityID).Friday += wa.WorkAllocation.HoursWorked;
							}

							output.Find(x => x.ActivityId == wa.Activity.ActivityID).RemarkFriday = wa.WorkAllocation.Remark;

							continue;
						}

						if (wa.WorkAllocation.DayWorked.DayOfWeek == DayOfWeek.Saturday)
						{
							if (output.Find(x => x.ActivityId == wa.Activity.ActivityID).Saturday == null)
							{
								output.Find(x => x.ActivityId == wa.Activity.ActivityID).Saturday = wa.WorkAllocation.HoursWorked;
							}
							else
							{
								output.Find(x => x.ActivityId == wa.Activity.ActivityID).Saturday += wa.WorkAllocation.HoursWorked;
							}

							output.Find(x => x.ActivityId == wa.Activity.ActivityID).RemarkSaturday = wa.WorkAllocation.Remark;

							continue;
						}

						if (wa.WorkAllocation.DayWorked.DayOfWeek == DayOfWeek.Sunday)
						{
							if (output.Find(x => x.ActivityId == wa.Activity.ActivityID).Sunday == null)
							{
								output.Find(x => x.ActivityId == wa.Activity.ActivityID).Sunday = wa.WorkAllocation.HoursWorked;
							}
							else
							{
								output.Find(x => x.ActivityId == wa.Activity.ActivityID).Sunday += wa.WorkAllocation.HoursWorked;
							}

							output.Find(x => x.ActivityId == wa.Activity.ActivityID).RemarkSunday = wa.WorkAllocation.Remark;

							continue;
						}
					}
					else
					{
						output.Add(new WeeklyModel
						{
							Client = wa.Activity.ClientName,
							Project = wa.Activity.ProjectName,
							Value = wa.Activity.ActivityName,
							ActivityId = wa.Activity.ActivityID,
							DayLocked = dailyModel.IsTimesheetLockedForDay,
							MustHaveRemarks = wa.Activity.MustHaveRemarks,

							Monday = GetValueForDay(wa, DayOfWeek.Monday),
							Tuesday = GetValueForDay(wa, DayOfWeek.Tuesday),
							Wednesday = GetValueForDay(wa, DayOfWeek.Wednesday),
							Thursday = GetValueForDay(wa, DayOfWeek.Thursday),
							Friday = GetValueForDay(wa, DayOfWeek.Friday),
							Saturday = GetValueForDay(wa, DayOfWeek.Saturday),
							Sunday = GetValueForDay(wa, DayOfWeek.Sunday),

							RemarkMonday = wa.WorkAllocation.Remark,
							RemarkTuesday = null,
							RemarkWednesday = null,
							RemarkThursday = null,
							RemarkFriday = null,
							RemarkSaturday = null,
							RemarkSunday = null,
							IsHidden = IsActivityHidden(wa.Activity.ActivityID, this.User.UserID)
						});
					}
				}
			}

			return new JavaScriptSerializer().Serialize(output);
		}

		public string IsWorkableStyle(bool workable) => !workable ? "weekend_or_holiday" : "";

		private static double? GetValueForDay(WorkAllocationExtendedModel wa, DayOfWeek dayOfWeek) => wa.WorkAllocation.DayWorked.DayOfWeek == dayOfWeek && wa.WorkAllocation.HoursWorked > 0
			? wa.WorkAllocation.HoursWorked
			: (double?)null;

		private static bool IsActivityHidden(int activityID, int userID) => UserAllocation.IsActivityHidden(activityID, userID);

		#endregion FUNCTIONS
	}

	public class WeeklyModel
	{
		#region PROPERTIES

		public bool IsHidden { get; set; }
		public bool MustHaveRemarks { get; set; }
		public bool DayLocked { get; set; }

		public int ActivityId { get; set; }

		public double? Monday { get; set; }
		public double? Tuesday { get; set; }
		public double? Wednesday { get; set; }
		public double? Thursday { get; set; }
		public double? Friday { get; set; }
		public double? Saturday { get; set; }
		public double? Sunday { get; set; }

		public string Client { get; set; }
		public string Project { get; set; }
		public string RemarkMonday { get; set; }

		public string RemarkTuesday { get; set; }
		public string RemarkWednesday { get; set; }
		public string RemarkThursday { get; set; }
		public string RemarkFriday { get; set; }
		public string RemarkSaturday { get; set; }
		public string RemarkSunday { get; set; }
		public string Value { get; set; }

		#endregion PROPERTIES
	}
}