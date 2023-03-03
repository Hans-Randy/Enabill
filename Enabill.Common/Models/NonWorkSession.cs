using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("NonWorkSessions")]
	public class NonWorkSession
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int NonWorkSessionID { get; set; }

		[Required]
		public int UserID { get; set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		[Required]
		public DateTime Date { get; set; }

		[Required, EnumDataType(typeof(WorkSessionStatusType))]

		#endregion PROPERTIES

		#region INITIALIZATION

		//[NotMapped]
		//public DateTime WorkDate { get { return StartTime.Date; } }

		[NotMapped]
		public bool CanEdit => !UserRepo.GetByID(this.UserID).IsTimesheetLockedForDate(this.Date);

		[NotMapped]
		public bool CanDelete => this.CanEdit;

		#endregion INITIALIZATION
	}
}