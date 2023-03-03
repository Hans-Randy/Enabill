using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Transactions;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("Projects")]
	public class Project
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ProjectID { get; set; }

		[Required]
		public bool BookInAdvance { get; set; }

		[Required]
		public bool CanHaveNotes { get; set; }

		[Required]
		public bool MustHaveRemarks { get; set; }

		[Required]
		public int BillingMethodID { get; set; }

		[Required]
		public int ClientID { get; set; }

		[Required]
		public int DepartmentID { get; set; }

		[Required]
		public int InvoiceAdminID { get; set; }

		[Required]
		public int ProjectOwnerID { get; set; }

		[Required]
		public int RegionID { get; set; }

		public double? ProjectValue { get; set; }

		[MaxLength(100)]
		public string DeactivatedBy { get; internal set; }

		[MaxLength(50)]
		public string GroupCode { get; set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		[MaxLength(15)]
		public string ProjectCode { get; set; }

		[MaxLength(200)]
		public string ProjectDesc { get; set; }

		[Required, MaxLength(50)]
		public string ProjectName { get; set; }

		[MaxLength(128)]
		public string SupportEmailAddress { get; set; }

		[Required]
		public DateTime CreatedDate { get; set; }

		[Required]
		public DateTime StartDate { get; set; }

		public DateTime? ConfirmedEndDate { get; internal set; }
		public DateTime? DeactivatedDate { get; internal set; }
		public DateTime? ScheduledEndDate { get; set; }


		public string AmountTotal;

		public string Currency;
		public Client Client { get; set; }

		public Department Department { get; set; }

		#endregion PROPERTIES

		#region INITALIZATION

		[NotMapped]
		public bool IsTM => this.BillingMethodID == (int)BillingMethodType.TimeMaterial;

		[NotMapped]
		public bool IsFixedCost => this.BillingMethodID == (int)BillingMethodType.FixedCost;

		[NotMapped]
		public bool IsMonthlyFixedCost => this.BillingMethodID == (int)BillingMethodType.MonthlyFixedCost;

		[NotMapped]
		public bool IsGlobalFixedCost => this.IsFixedCost || this.IsMonthlyFixedCost;

		[NotMapped]
		public bool IsSLA => this.BillingMethodID == (int)BillingMethodType.SLA;

		[NotMapped]
		public bool IsTravel => this.BillingMethodID == (int)BillingMethodType.Travel;

		[NotMapped]
		public bool IsAdHoc => this.BillingMethodID == (int)BillingMethodType.AdHoc;

		[NotMapped]
		public bool IsProjectActive => !this.ScheduledEndDate.HasValue || this.ScheduledEndDate.Value.Date >= DateTime.Today || this.HasActiveUserAllocations || this.HasActiveInvoiceRules;

		public bool HasActiveUserAllocations => ProjectRepo.GetUsersAssignedToProjectModel(this.ProjectID, DateTime.Today) != 0;

		public bool HasActiveInvoiceRules => ProjectRepo.GetInvoiceRulesByProjectID(this.ProjectID) != 0;

		public string ProjectStatus
		{
			get
			{
				string projectStatus = "";
				int userAllocations = ProjectRepo.GetUsersAssignedToProjectModel(this.ProjectID, DateTime.Today);
				int invoiceRules = ProjectRepo.GetInvoiceRulesByProjectID(this.ProjectID);

				if (this.ScheduledEndDate.HasValue && this.ScheduledEndDate.Value.Date < DateTime.Today)
				{
					if (userAllocations > 0)
					{
						projectStatus = "Project end date reached but Active UserAllocations exist. Please check.";
					}
					else
					{
						if (invoiceRules > 0)
							projectStatus = "Project end date reached but Active Invoice Rules exist. Please check.";
						else
							projectStatus = "Inactive";
					}
				}
				else
				{
					projectStatus = "Active";
				}

				if (projectStatus == "Inactive")
					ProjectRepo.GetActivities(this.ProjectID, true).ToList().ForEach(a => this.DeactivateActivity(a));

				return projectStatus;
			}
		}

		public BillingMethod GetBillingMethod => BillingMethodRepo.GetByID(this.BillingMethodID);

		#endregion INITALIZATION

		#region CLIENT

		public Client GetClient() => ClientRepo.GetByID(this.ClientID);

		#endregion CLIENT

		#region PROJECT

		public static List<Project> GetAll() => ProjectRepo.GetAll()
			.OrderBy(c => c.ProjectName)
			.ToList();

		public static Project GetNew() => new Project()
		{
			StartDate = DateTime.Now.ToFirstDayOfMonth(),
			LastModifiedBy = "Sys Setup"
		};

		public static List<string> GetDistinctProjectNames() => ProjectRepo.GetDistinctProjectNames()
					.ToList();

		public static List<string> GetDistinctProjectNamesForClientID(int clientID) => ProjectRepo.GetDistinctProjectNamesForClientID(clientID)
					.ToList();

		public static List<Project> GetSupportProjectsForClient(int clientID) => ProjectRepo.GetSupportProjectsForClient(clientID).ToList();

		public static List<ProjectSearchResult> Search(User userSearching, string q, bool isActive)
		{
			if (userSearching.HasRole(UserRoleType.SystemAdministrator))
				return ProjectRepo.Search(q, 0, isActive);

			if (userSearching.HasRole(UserRoleType.ProjectOwner))
				return ProjectRepo.Search(q, userSearching.UserID, isActive);

			//find out if exception should be thrown or new list ?? throw exception for now
			throw new UserRoleException("You do not have the required permissions to retrieve the data");
			//return new List<ProjectSearchResult>();
		}

		public void EndProject(User userEndingProject /* TODO DateTime implementationDate*/)
		{
			if (!userEndingProject.HasRole(UserRoleType.SystemAdministrator) && !userEndingProject.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to end this project");

			if (this.GetActivities(true).Any(a => a.IsActive))
				throw new ProjectManagementException("There are still active activies for this project. Please deactivate those activities before attempting to deactivate the project.");

			this.ConfirmedEndDate = this.ScheduledEndDate ?? DateTime.Now.ToCentralAfricanTime();
			this.DeactivatedBy = userEndingProject.FullName;
			this.DeactivatedDate = DateTime.Now.ToCentralAfricanTime();
			this.Save(userEndingProject);
		}

		public void Activate(User userActivating)
		{
			if (!userActivating.HasRole(UserRoleType.SystemAdministrator) && !userActivating.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to activate this project.");

			this.ConfirmedEndDate = null;
			this.Save(userActivating);
		}

		public void Save(User userSaving)
		{
			var projectManager = UserRepo.GetByID(this.ProjectOwnerID);

			if (projectManager == null)
				throw new UserManagementException("The user that was set as the project manager could not be found. Save cancelled.");

			if (!projectManager.HasRole(UserRoleType.ProjectOwner))
				throw new UserRoleException(string.Format($"{projectManager.FullName} does not have the '{UserRoleType.ProjectOwner}' role and can therefore not be assigned as this project's owner. Save cancelled."));

			var invoiceUser = UserRepo.GetByID(this.InvoiceAdminID);

			if (invoiceUser == null)
				throw new UserManagementException("The user that was set as the invoice administrator could not be found. Save cancelled.");

			if (!invoiceUser.HasRole(UserRoleType.InvoiceAdministrator))
				throw new UserRoleException(string.Format($"{invoiceUser.FullName} does not have the '{UserRoleType.InvoiceAdministrator}' role and can therefore not be assigned as this project's invoice administrator. Save cancelled."));

			if (userSaving.CanManage(this))
			{
				this.LastModifiedBy = userSaving.FullName;
				this.ConfirmedEndDate = this.ScheduledEndDate;
				ProjectRepo.Save(this);
				return;
			}

			if (this.ProjectID > 0 && userSaving.HasRole(UserRoleType.ProjectOwner))
				throw new UserRoleException("You are not the project owner of this project. Save cancelled");

			if (this.ProjectID > 0)
				throw new UserRoleException("You do not have the required permissions to edit a project. Save cancelled.");

			throw new UserRoleException("You do not have the required permissions to create a project. Save cancelled");
		}

		internal static ProjectDetail GetDetailed(Project project) => new ProjectDetail(project);

		#endregion PROJECT

		#region INVOICES

		public List<Invoice> CreateInvoices(User userCreating, DateTime dateTime)
		{
			if (!userCreating.CanManageInvoicesForProject(this))
				throw new InvoiceException("You do not have the requireed permissions to create invoices for this project. Action cancelled.");

			if (!this.IsProjectActive)
				throw new InvoiceException("You cannot create invoices while the project is Inactive. Action cancelled.");

			if (!this.TimesheetsAreUpToDate(dateTime.ToPeriod()))
				throw new InvoiceException("You cannot create invoices while timesheets are outstanding.");

			var inv = new Invoice
			{
				InvoiceID = 1,
				InvoiceAmountExclVAT = 0,
				DateInvoiced = dateTime
			};

			throw new NullReferenceException("Henk to complete this.");
			return null;// this.Invoices;

			//TODO: This method is not yet complete
		}

		private bool TimesheetsAreUpToDate(int period) =>
			//TODO: Implement timesheets outstanding
			true;

		#endregion INVOICES

		#region ACTIVTIY MANAGEMENT

		public Activity GetNewActivity() => new Activity()
		{
			ProjectID = this.ProjectID,
			RegionID = this.RegionID,
			DepartmentID = this.DepartmentID,
			MustHaveRemarks = this.MustHaveRemarks,
			CanHaveNotes = this.CanHaveNotes,
			IsActive = true,
			LastModifiedBy = "Sys Setup"
		};

		public Activity GetActivity(int activityID) => ProjectRepo.GetActivity(this.ProjectID, activityID);

		public List<Activity> GetActivities() => ProjectRepo.GetActivities(this.ProjectID)
					.OrderBy(a => a.ActivityName)
					.ToList();

		public List<Activity> GetActivities(bool isActive) => ProjectRepo.GetActivities(this.ProjectID, isActive)
					.OrderBy(a => a.ActivityName)
					.ToList();

		public Activity GetDefaultActivity(User userCreating)
		{
			if (!userCreating.HasRole(UserRoleType.SystemAdministrator) && !userCreating.CanManage(this))
				throw new UserRoleException("You don not have the required permissions to create an activity for this project. Action cancelled.");

			var activity = this.GetNewActivity();
			activity.ActivityName = this.ProjectName;

			return activity;
		}

		public void CreateDefaultActivity(User userCreating)
		{
			if (!userCreating.HasRole(UserRoleType.SystemAdministrator) && !userCreating.CanManage(this))
				throw new UserRoleException("You don not have the required permissions to create an activity for this project. Action cancelled.");

			var activity = this.GetNewActivity();
			activity.ActivityName = this.ProjectName;

			this.SaveActivity(userCreating, activity);
		}

		public void DeleteActivity(User userDeleting, Activity activity)
		{
			if (!userDeleting.HasRole(UserRoleType.SystemAdministrator) && !userDeleting.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to remove this activity. Action cancelled.");

			if (this.GetUsersAssignedToActivity(activity.ActivityID).Count > 0)
				throw new ActivityAdminException("You cannot remove an activity while users are allocated to it. Action cancelled.");

			ProjectRepo.DeleteActivity(activity);
		}

		public void SetActivityStatus(User userManaging, Activity activity, bool isDeactivated)
		{
			if (!userManaging.HasRole(UserRoleType.SystemAdministrator) && !userManaging.CanManage(this))
			{
				if (isDeactivated)
					throw new UserRoleException("You do not have the required permissions to deactivate this activity. Action cancelled.");
				else
					throw new UserRoleException("You do not have the required permissions to activate this activity. Action cancelled.");
			}

			if (isDeactivated)
				this.DeactivateActivity(activity);
			else
				this.ActivateActivity(activity);

			this.SaveActivity(userManaging, activity);
		}

		public void SaveActivity(User userSaving, Activity activity)
		{
			if (!userSaving.HasRole(UserRoleType.SystemAdministrator) && !userSaving.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to edit this project's activities. Save cancelled.");

			activity.LastModifiedBy = userSaving.FullName;
			ProjectRepo.SaveActivity(activity);
		}

		private void ActivateActivity(Activity activity)
		{
			activity.IsActive = true;
			activity.IsDeactivated = false;
		}

		private void DeactivateActivity(Activity activity)
		{
			//if (activity.IsLinkedToInvoiceRule())
			//	throw new ActivityAdminException("You cannot deactivate this activity as it is linked to an invoice rule.");

			//if (activity.GetUsersAssigned().Count > 0)
			//	throw new ActivityAdminException("You cannot deactivate this activity as it has been allocated to one or more users.");

			//activity.IsActive = false;
			activity.IsDeactivated = true;
			ProjectRepo.SaveActivity(activity);
		}

		#endregion ACTIVTIY MANAGEMENT

		#region USER ACTIVITY ASSIGNMENT

		public void AssignUsersToActivities(User assigningUser, string userIDs, string activityIDs, double chargeRate, DateTime startDate, DateTime? endDate, bool isConfirmed = false)
		{
			using (var ts = new TransactionScope())
			{
				foreach (int userID in userIDs.ToIntArray())
				{
					var user = UserRepo.GetByID(userID);
					if (user != null)
					{
						foreach (int activityID in activityIDs.ToIntArray())
						{
							var activity = ActivityRepo.GetByID(activityID);
							if (activity != null)
							{
								this.AssignUserToActivity(assigningUser, user, activity, chargeRate, startDate, endDate, isConfirmed);
							}
						}
					}
				}
				ts.Complete();
			}
		}

		//public void AssignUsersToActivity(User assigningUser, string userIDs, Activity activity, double chargeRate, DateTime startDate, DateTime? endDate, bool isConfirmed = false)
		//{
		//    foreach (int userID in userIDs.ToIntArray())
		//    {
		//        User user = UserRepo.GetByID(userID);
		//        if (user != null)
		//            AssignUserToActivity(assigningUser, user, activity, chargeRate, startDate, endDate, isConfirmed);
		//    }
		//}

		public void AssignUserToActivity(User assigningUser, User userBeingAssignedToActivity, Activity activity, double chargeRate, DateTime startDate, DateTime? endDate, bool isConfirmed = false)
		{
			startDate = startDate.Date;

			if (!endDate.HasValue)
				isConfirmed = false;

			if (!assigningUser.HasRole(UserRoleType.SystemAdministrator) && !assigningUser.CanManage(this))
				throw new ActivityAdminException("You do not have the required permissions to allocate consultants to a project.");

			if (!this.IsProjectActive)
				throw new ActivityAdminException("You cannot assign a user to an inactive project");

			if (startDate < EnabillSettings.SiteStartDate)
				throw new EnabillSettingsException("You cannot assign a user to an activity with the start date being before " + EnabillSettings.SiteStartDate.ToExceptionDisplayString());

			if (endDate.HasValue)
			{
				endDate = endDate.Value.Date;

				if (endDate < EnabillSettings.SiteStartDate)
					throw new EnabillSettingsException("You cannot assign a user to an activity with the end date being before " + EnabillSettings.SiteStartDate.ToExceptionDisplayString());

				if (startDate >= endDate)
					throw new EnabillDomainException("The dates are invalid. Please revise the dates and retry.");
			}

			if (userBeingAssignedToActivity == null)
				throw new ActivityAdminException("User was not found.");

			if (!userBeingAssignedToActivity.IsActive)
				throw new ActivityAdminException("You cannot allocate an inactive user to a project/activity.");

			//if (!userBeingAssignedToActivity.HasRole(UserRoleType.TimeCapturing))
			//throw new UserRoleException(string.Format($"{userBeingAssignedToActivity.FullName} can not be assigned to the activity because he/she does not have the '{UserRoleType.TimeCapturing}' role and therefore, cannot be assigned to the activity."));

			if (activity == null)
				throw new ActivityAdminException("Activity was not found.");

			if (!activity.IsActive)
				throw new ActivityAdminException("You cannot assign a user to an inactive activity.");

			var project = activity.GetProject();

			if (this.StartDate.Date < project.StartDate.Date)
				this.StartDate = project.StartDate.Date;

			if (!UserAllocation.CanAddUserAllocation(activity.ActivityID, userBeingAssignedToActivity, startDate, endDate, isConfirmed))
				throw new ActivityAdminException("You cannot allocate this user to the activity because he/she is linked to the activity without a confirmed end date or the dates overlap with an exisiting user allocation. Please revise dates and confirm dates, then try again.");

			var ua = new UserAllocation()
			{
				UserID = userBeingAssignedToActivity.UserID,
				ActivityID = activity.ActivityID,
				ChargeRate = chargeRate,
				StartDate = startDate,
				ScheduledEndDate = null,
				ConfirmedEndDate = null
			};

			if (isConfirmed)
			{
				ua.ScheduledEndDate = ua.ConfirmedEndDate = endDate.Value;
			}
			else if (endDate.HasValue)
			{
				ua.ScheduledEndDate = endDate.Value;
			}

			ua.LastModifiedBy = assigningUser.FullName;
			UserAllocationRepo.Save(ua);
		}

		public List<User> GetUsersAssignedToActivity(int activityID) => this.GetUsersAssignedToActivity(activityID, DateTime.Today);

		public List<User> GetUsersAssignedToActivity(int activityID, DateTime refDate) => UserRepo.GetUsersAssignedToActivity(activityID, refDate.Date)
					.OrderBy(u => u.UserName)
				   .ToList();

		public List<User> GetUsersNotAssignedtoActivity(int activityID) => this.GetUsersNotAssignedtoActivity(activityID, DateTime.Today);

		public List<User> GetUsersNotAssignedtoActivity(int activityID, DateTime refDate)
		{
			if (!this.IsProjectActive)
				return new List<User>();

			return UserRepo.GetUsersNotAssignedToActivity(activityID, refDate.Date)
					.OrderBy(u => u.UserName)
					.ToList();
		}

		public bool IsUserAssignedToActivity(int userID, int activityID) => this.GetUsersAssignedToActivity(activityID, DateTime.Today)
					.Exists(ca => ca.UserID == userID);

		public bool IsUserAssignedToActivity(int userID, int activityID, DateTime refDate) => this.GetUsersAssignedToActivity(activityID, refDate.Date)
					.Exists(ca => ca.UserID == userID);

		public UserAllocation GetUserAllocation(int userAllocationID) => UserAllocationRepo.GetByID(userAllocationID);

		internal UserAllocation GetUserAllocation(int activityID, int userID, DateTime refDate) => UserAllocationRepo.GetForActivityForUserForDate(activityID, userID, refDate);

		public void DeleteUserAllocation(UserAllocation userAllocation)
		{
			var activity = ActivityRepo.GetByID(userAllocation.ActivityID);

			if (UserRepo.GetWorkAllocationsAssignedToActivity(activity.ProjectID, userAllocation.ActivityID, userAllocation.UserID, DateTime.Now).ToList().Count > 0)
				throw new ActivityAdminException("You cannot delete the user allocation as time has already been allocated to it. Action cancelled.");

			ProjectRepo.DeleteUserAllocation(userAllocation);
		}

		#endregion USER ACTIVITY ASSIGNMENT
	}

	public class ProjectSearchResult
	{
		public int ProjectID { get; internal set; }
		public string ProjectName { get; internal set; }
		public string ClientName { get; internal set; }
		public string BillingMethodTypeName { get; internal set; }
		public string Region { get; internal set; }
		public string Department { get; internal set; }
		public string ProjectCode { get; internal set; }
		public bool IsActive { get; internal set; }
		public DateTime? ScheduledEndDate { get; internal set; }
		public double? ProjectValue { get; internal set; }
		public string ProjectOwnerName { get; internal set; }
		public bool IsFixedCost { get; internal set; }
		public List<string> Activities { get; internal set; }
		public Project Project { get; internal set; }
	}

	public class ProjectSelectModelComparer : IEqualityComparer<ProjectSelectModel>
	{
		// Projects are equal if their names and projectID's are equal.
		public bool Equals(ProjectSelectModel x, ProjectSelectModel y)
		{
			//Check whether the compared objects reference the same data.
			if (ReferenceEquals(x, y))
				return true;

			//Check whether any of the compared objects is null.
			if (x is null || y is null)
				return false;

			//Check whether the products' properties are equal.
			return x.Project.ProjectID == y.Project.ProjectID;
		}

		// If Equals() returns true for a pair of objects
		// then GetHashCode() must return the same value for these objects.

		public int GetHashCode(ProjectSelectModel obj)
		{
			//Check whether the object is null
			if (obj is null)
				return 0;

			//Get hash code for the Code field.
			//Calculate the hash code for the product.
			return obj.Project.ProjectID.GetHashCode();
		}
	}
}