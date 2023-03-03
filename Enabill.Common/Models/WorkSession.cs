using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("WorkSessions")]
	public class WorkSession
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int WorkSessionID { get; set; }

		[Required]
		public int UserID { get; set; }

		[Required, EnumDataType(typeof(WorkSessionStatusType))]
		public int WorkSessionStatusID { get; set; }

		[Required]
		public double LunchTime { get; set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		[Required]
		public DateTime EndTime { get; set; }

		[Required]
		public DateTime StartTime { get; set; }

		#endregion PROPERTIES

		#region INITIALIZATION

		[NotMapped]
		public DateTime WorkDate => this.StartTime.Date;

		[NotMapped]
		public bool CanEdit => !UserRepo.GetByID(this.UserID).IsTimesheetLockedForDate(this.WorkDate);

		[NotMapped]
		public bool CanDelete => this.CanEdit;

		[NotMapped]
		public double TotalTime => this.EndTime.Subtract(this.StartTime).TotalHours - this.LunchTime;

		[NotMapped]
		public double GrossTime => this.EndTime.Subtract(this.StartTime).TotalHours;

		#endregion INITIALIZATION

		#region WORK SESSION

		public bool DoesOverlap(WorkSession ws) => (this.StartTime >= ws.StartTime && this.StartTime <= ws.EndTime)
				|| (this.EndTime >= ws.StartTime && this.EndTime <= ws.EndTime)
				|| (ws.StartTime >= this.StartTime && ws.StartTime <= this.EndTime)
				|| (ws.EndTime >= this.StartTime && ws.EndTime <= this.EndTime);

		public bool IsEndDateGreaterThanStartDate() => this.EndTime > this.StartTime;

		#endregion WORK SESSION
	}

	[Table("vwWorkSessionOverview")]
	public class WorkSessionOverview
	{
		#region PROPERTIES

		[Key]
		public int UserID { get; set; }

		public bool IsWorkable { get; set; }

		public double Duration { get; set; }
		public double HoursWorked { get; set; }
		public double HoursDiff { get; set; }

		public DateTime DayWorked { get; set; }

		#endregion PROPERTIES
	}
}