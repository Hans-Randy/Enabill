using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("Activities")]
	public class Activity
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ActivityID { get; internal set; }

		// Indicates if a user can add long text notes (like in POI) to the activity when allocating time to it.
		[Required]
		public bool CanHaveNotes { get; set; }

		[Required]
		public bool IsActive { get; internal set; }

		// If true, means that Activitiy has been Deactivated and once past the latest Confirmed End Date, the Activity will be made Inactive
		[Required]
		public bool IsDeactivated { get; internal set; }

		// If true, will form part of the set of Activities that a new Time Capturer will get.
		[Required]
		public bool IsDefault { get; set; }

		// Indicates if this activity must have remarks entered next to the time allocated to it.
		[Required]
		public bool MustHaveRemarks { get; set; }

		[Required]
		public int DepartmentID { get; set; }

		[Required]
		public int ProjectID { get; internal set; }

		// Region where staff is allocated to perform this activity.
		[Required]
		public int RegionID { get; set; }

		[Required, MaxLength(50)]
		public string ActivityName { get; set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		public Department Department { get; set; }

		public Project Project { get; set; }

		public Region Region { get; set; }

		#endregion PROPERTIES

		#region ACTIVITY

		public static List<string> GetDistinctActivityNames() => ActivityRepo.GetDistinctActivityNames()
					.ToList();

		public static List<string> GetDistinctActivityNamesForClientID(int clientID) => ActivityRepo.GetDistinctActivityNamesForClientID(clientID)
					.ToList();

		public static List<string> GetDistinctActivityNamesForProjectName(string projectName) => ActivityRepo.GetDistinctActivityNamesForProjectName(projectName)
					.ToList();

		public static Activity GetByID(int activityID) => ActivityRepo.GetByID(activityID);

		public string GetFullName()
		{
			var model = ActivityRepo.GetFullDetail(this.ActivityID);

			return string.Format($"{model.ClientName}<br />{model.ProjectName}<br />{model.ActivityName}");
		}

		#endregion ACTIVITY

		#region PROJECT

		public Project GetProject() => ProjectRepo.GetByID(this.ProjectID);

		#endregion PROJECT

		#region USER

		// List of users (with Detail) that are assigned to this activity.
		public List<ActivityUserDetail> GetUsersAssigned() => this.GetUsersAssigned(DateTime.Today);

		public List<ActivityUserDetail> GetUsersAssigned(DateTime refDate) => UserAllocationRepo.GetUsersAssignedModel(this.ActivityID, refDate)
					.OrderBy(ua => ua.UserFullName)
					.ThenBy(ua => ua.StartDate)
					.ToList();

		public List<ActivityUserDetail> GetUsersAssigned(DateTime fromDate, DateTime toDate) => UserAllocationRepo.GetUsersAssignedModel(this.ActivityID, fromDate, toDate)
					.OrderBy(ua => ua.UserFullName)
					.ThenBy(ua => ua.StartDate)
					.ToList();

		public List<ActivityUserDetail> GetPastUsersAssigned() => UserAllocationRepo.GetPastUsersAssignedModel(this.ActivityID, DateTime.Today)
					.OrderBy(ua => ua.UserFullName)
					.ThenBy(ua => ua.StartDate)
					.ToList();

		public List<ActivityUserDetail> GetFutureUsersAssigned() => UserAllocationRepo.GetFutureUsersAssignedModel(this.ActivityID, DateTime.Today)
					.OrderBy(ua => ua.UserFullName)
					.ThenBy(ua => ua.StartDate)
					.ToList();

		// List of users (with Detail) that are assigned to this activity.
		public List<ActivityUserDetail> GetUsersNotAssigned() => this.GetUsersNotAssigned(DateTime.Today);

		public List<ActivityUserDetail> GetUsersNotAssigned(DateTime refDate) => UserAllocationRepo.GetUsersNotAssignedModel(this.ActivityID, refDate)
					.ToList();

		#endregion USER

		#region INVOICE RULE

		internal bool IsLinkedToInvoiceRule()
		{
			var list = InvoiceRuleActivityRepo.GetByActivityID(this.ActivityID).ToList();
			var model = new List<InvoiceRuleActivity>();

			foreach (var item in list)
			{
				var invRule = InvoiceRuleRepo.GetByID(item.InvoiceRuleID);

				if (invRule.IsActive)
					model.Add(item);
			}

			if (model.Count > 0)
				return true;

			if (this.GetProject().IsGlobalFixedCost && InvoiceRuleRepo.GetByProjectID(this.ProjectID).ToList().Count > 0)
				return true;

			return false;
		}

		#endregion INVOICE RULE
	}
}