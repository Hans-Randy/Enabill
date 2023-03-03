using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Models.Dto;
using Enabill.Repos;
using Newtonsoft.Json;
using NLog;
using ServiceStack.Text;

namespace Enabill.Models
{
	[Table("Users")]
	public class User
	{
		#region LOGGER

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		#endregion LOGGER

		#region PROPERTIES

		//public User EnableEditing(User user);

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int UserID { get; set; }

		[Required]
		public bool CanLogin { get; set; }

		[Required]
		public bool IsActive { get; set; }

		public bool IsLeaveUser => this.EmploymentTypeID != (int)EmploymentTypeEnum.HourlyContractor;

		[Required]
		public bool IsSystemUser { get; set; }

		[Required]
		public int BillableIndicatorID { get; set; }

		[Required]
		public int DivisionID { get; set; }

		[Required, EnumDataType(typeof(EmploymentTypeEnum))]
		public int EmploymentTypeID { get; set; }

		[Required]
		public int RegionID { get; set; }

		public int? ManagerID { get; set; }

		[Required(ErrorMessage = "Percentage Allocation is required.")]
		[Range(1, 100, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
		public int PercentageAllocation { get; set; }

		[Required]
		public double AnnualLeaveTakeOn { get; set; }

		[Required]
		public double FlexiBalanceTakeOn { get; set; }

		[Required, MaxLength(128)]
		[RegularExpression("^[_A-Za-z0-9-\\+]+(\\.[_A-Za-z0-9-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$", ErrorMessage = "E-mail is not valid")]
		public string Email { get; set; }

		[MaxLength(255)]
		public string ExternalRef { get; set; }

		[Required, MinLength(2), MaxLength(50)]
		public string FirstName { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public string FullName { get; set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		[Required, MinLength(2), MaxLength(50)]
		public string LastName { get; set; }

		[MaxLength(200)]
		public string Password { get; set; }

		[MaxLength(10)]
		public string PayrollRefNo { get; set; }

		[MaxLength(20)]
		public string Phone { get; set; }

		[Required]
		public DateTime EmployStartDate { get; set; }

		[DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
		public DateTime? BirthDate { get; set; }

		public DateTime? EmployEndDate { get; set; }

		//This token is generated when a user requests to regain their password, and then validation is
		//done between the code received and this code, set to null after new password saved
		public Guid? ForgottenPasswordToken { get; private set; }

		private bool mustResetPwd;

		[Required]
		public bool MustResetPwd
		{
			get
			{
				if (this.Password == null || this.Password == Helpers.HashSha512(EnabillSettings.DefaultUserPassword))
					return true;
				return this.mustResetPwd;
			}

			set => this.mustResetPwd = value;
		}

		/*
		[NotMapped]
		private DateTime _employStartDate { get; set; }
		[Required]
		public DateTime EmployStartDate
		{
			get { return _employStartDate; }
			set {
				if (EnableEditing() == null)
			}
		}
		*/

		[NotMapped]
		public Division Division { get; set; }

		[NotMapped]
		public EmploymentType EmploymentType { get; set; }

		[NotMapped]
		public Region Region { get; set; }

		public IList<Leave> Leaves { get; set; }

		public IList<WorkAllocation> WorkAllocations { get; set; }

		#endregion PROPERTIES

		#region INSTANTIATION

		public int GetRoleBW() => this.GetRoles().Sum(r => r.RoleID);

		private User manager;

		public User Manager
		{
			get
			{
				if (this.ManagerID == null)
					return null;
				if (this.manager == null)
					this.manager = UserRepo.GetByID(this.ManagerID.Value);
				return this.manager;
			}
		}

		public bool IsFlexiTimeUser
		{
			get
			{
				if (this.EmploymentTypeID == (int)EmploymentTypeEnum.HourlyContractor)
					return false;

				if (this.ExternalRef == Code.Constants.COMPANYNAME)
					return false;

				if (!this.HasRole(UserRoleType.TimeCapturing))
					return false;

				return true;

				//return false;
			}
		}

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public string UserName { get; set; }

		public BillableIndicator BillableIndicator { get; set; }

		[Required]
		public virtual double WorkHours { get; set; }

		#endregion INSTANTIATION

		#region USER

		public static List<User> GetAll(User userRequesting)
		{
			if (userRequesting.HasRole(UserRoleType.SystemAdministrator) || userRequesting.HasRole(UserRoleType.Accountant))
			{
				return UserRepo.GetAll()
						.OrderBy(u => u.FirstName)
						.ThenBy(u => u.LastName)
						.ToList();
			}

			if (userRequesting.HasRole(UserRoleType.Manager))
			{
				return UserRepo.GetStaffForManager(userRequesting.UserID)
						.OrderBy(u => u.FirstName)
						.ThenBy(u => u.LastName)
						.ToList();
			}

			return new List<User>();
		}

		public static List<User> FilterByName(User userRequesting, string searchString, bool isActive = true)
		{
			if (userRequesting.HasRole(UserRoleType.SystemAdministrator) || userRequesting.HasRole(UserRoleType.Accountant))
			{
				return UserRepo.FilterAllByName(searchString, isActive)
						.OrderBy(u => u.FirstName)
						.ThenBy(u => u.LastName)
						.ToList();
			}

			if (userRequesting.HasRole(UserRoleType.Manager))
			{
				return UserRepo.FilterByNameForManager(userRequesting.UserID, searchString)
						.OrderBy(u => u.FirstName)
						.ThenBy(u => u.LastName)
						.ToList();
			}

			if (userRequesting.HasRole(UserRoleType.ProjectOwner))
			{
				return UserRepo.FilterByNameForPM(userRequesting.UserID, searchString)
						.OrderBy(u => u.FirstName)
						.ThenBy(u => u.LastName)
						.ToList();
			}

			return new List<User>();
		}

		//returns the amount of hours a user should work per day at a certain time
		public double WorkHoursOnDate(DateTime date)
		{
			if (date.Date >= DateTime.Today.ToFirstDayOfMonth())
				return this.WorkHours;

			var uH = this.GetUserHistory(date);
			if (uH == null)
				return this.WorkHours;
			return uH.WorkHoursPerDay;
		}

		public void Activate(User userActivating)
		{
			if (!userActivating.HasRole(UserRoleType.SystemAdministrator))
				throw new UserRoleException("You do not have the required permissions to activate this user");

			if (this.IsActive)
				return;

			this.IsActive = true;
			this.EmployEndDate = null;
			this.Save(userActivating);
		}

		//public void Deactivate(User userDeactivating)
		public bool Deactivate(User userDeactivating)
		{
			if (!this.IsActive)
				return true;

			if (userDeactivating.FirstName == "ProcessUser")
			{
				this.IsActive = false;
				this.CanLogin = false;
				this.MustResetPwd = false;
				this.Save(userDeactivating);
			}
			else
			{
				if (!userDeactivating.HasRole(UserRoleType.SystemAdministrator) && !userDeactivating.IsSystemUser)
					throw new UserRoleException("You do not have the required permissions to deactivate this user");

				if (this.GetProjectsForProjectOwner().Count > 0)
					throw new UserManagementException("This user cannot be deactivated because he/she is still the owner of at least one project. Please reassign the project/s to a different user before deactivating this user.");

				if (this.GetStaffOfManager().Count > 0)
					throw new UserManagementException("This user cannot be deactivated as he/she is still the manager of at least one user. Please reassign the user/s a new manager before deactivating this user.");

				//this.ManagerID = null; non nullable DB field hence the error occurs when attempting to deactivate
			}

			return true;
		}

		public User Save(User userSaving)
		{
			var originalUser = UserRepo.GetByID(this.UserID);

			if (this.UserID > 0 && originalUser == null)
				throw new UserManagementException("This user could not be found. Please try again.");

			if (this.UserID == 0 && !userSaving.HasRole(UserRoleType.SystemAdministrator))
				throw new UserRoleException("You do not have the required permissions to create a user account. Action cancelled.");

			if (this.WorkHours < 0 && this.EmploymentTypeID == (int)EmploymentTypeEnum.HourlyContractor)
				throw new UserManagementException("The daily work hours for an hourly contracted user cannot be set as a negetive value. Save cancelled.");

			if (this.WorkHours <= 0 && this.EmploymentTypeID != (int)EmploymentTypeEnum.HourlyContractor)
				throw new UserManagementException("The daily work hours for permanent and monthly contracted users cannot be set to zero or less. Save cancelled.");

			if (this.WorkHours > EnabillSettings.MaxDailyWorkHours)
				throw new UserManagementException(string.Format($"Specifying the user's daily work hours to be greater than {EnabillSettings.MaxDailyWorkHours} hours is not accepted. Action cancelled."));

			if (this.WorkHours % 0.25 != 0)
				throw new UserManagementException("The user's work hours must be a multiple of 0.25 (15 minutes). Action cancelled.");

			if (this.EmployStartDate > this.EmployEndDate)
				throw new UserManagementException("The employee's start date cannot be later than their end date. Action cancelled.");

			//  using (TransactionScope ts = new TransactionScope())
			//  {
			//Hourly contractors do not have a flexitime balance
			if (this.EmploymentTypeID == (int)EmploymentTypeEnum.HourlyContractor)
				this.FlexiBalanceTakeOn = 0;

			if (userSaving.HasRole(UserRoleType.SystemAdministrator))
			{
				this.LastModifiedBy = userSaving == null ? "System" : userSaving.FullName;
				UserRepo.Save(this);
				// ts.Complete();
				return this;
			}

			if (userSaving.CanManage(this) && originalUser != null)
			{
				originalUser.FirstName = this.FirstName;
				originalUser.LastName = this.LastName;
				originalUser.BillableIndicatorID = this.BillableIndicatorID;
				originalUser.CanLogin = this.CanLogin;
				originalUser.DivisionID = this.DivisionID;
				originalUser.Email = this.Email;
				originalUser.EmploymentTypeID = this.EmploymentTypeID;
				originalUser.EmployStartDate = this.EmployStartDate;
				originalUser.ExternalRef = this.ExternalRef;
				originalUser.MustResetPwd = this.MustResetPwd;
				originalUser.Phone = this.Phone;
				originalUser.RegionID = this.RegionID;
				originalUser.WorkHours = this.WorkHours;
				originalUser.AnnualLeaveTakeOn = this.AnnualLeaveTakeOn;
				originalUser.FlexiBalanceTakeOn = this.FlexiBalanceTakeOn;
				originalUser.PayrollRefNo = this.PayrollRefNo;

				originalUser.LastModifiedBy = userSaving.FullName;
				UserRepo.Save(originalUser);
				// ts.Complete();

				return originalUser;
			}

			if (this.UserID == userSaving.UserID && originalUser != null)
			{
				originalUser.Password = this.Password;
				originalUser.LastModifiedBy = userSaving == null ? "System" : userSaving.FullName;

				UserRepo.Save(originalUser);
				//   ts.Complete();
				return originalUser;
			}
			//}

			if (this.UserID > 0 && !userSaving.HasRole(UserRoleType.Manager))
				throw new UserRoleException("You do not have the required permission to edit a user. Save Cancelled.");

			throw new UserRoleException(string.Format($"You are not the manager of {originalUser.FullName} and therefore cannot update his/her details"));
		}

		public static User GetNew() => new User()
		{
			IsActive = true,
			EmployStartDate = DateTime.Now.ToFirstDayOfMonth().AddMonths(1),
			MustResetPwd = true,
			CanLogin = true,
			WorkHours = 8,
			PercentageAllocation = 100
		};

		public static User GetByForgottenPasswordToken(Guid token) => UserRepo.GetByForgottenPasswordToken(token);

		public static User GetByEmail(string email) => UserRepo.GetByEmail(email);

		public static User GetSystemUser() => UserRepo.GetSystemUsers()
					.FirstOrDefault(u => u.IsSystemUser);

		public DateTime ConfigureDate(DateTime startDate)
		{
			if (startDate < EnabillSettings.SiteStartDate)
				startDate = EnabillSettings.SiteStartDate;

			if (startDate < this.EmployStartDate)
				startDate = this.EmployStartDate;

			return startDate;
		}

		public void ConfigureDates(DateTime origStartDate, DateTime origEndDate, out DateTime startDate, out DateTime endDate, bool returnEndOfMonthDate = true)
		{
			if (origStartDate < this.EmployStartDate)
				origStartDate = this.EmployStartDate;
			if (origStartDate < EnabillSettings.SiteStartDate)
				origStartDate = EnabillSettings.SiteStartDate;

			if (origEndDate < origStartDate)
			{
				if (returnEndOfMonthDate)
					origEndDate = origStartDate.ToLastDayOfMonth();
				else
					origEndDate = origStartDate;
			}

			startDate = origStartDate;
			endDate = origEndDate;
		}

		public bool IsDateValidForUser(DateTime date) => date >= EnabillSettings.SiteStartDate && date >= this.EmployStartDate;

		#endregion USER

		#region USER HISTORIES

		private UserHistory GetUserHistory(DateTime date) => UserRepo.GetUserHistory(this.UserID, date.ToPeriod());

		private void UpdateUserHistoryRecords(DateTime startDate)
		{
			//No need yet to check the userExec, private
			for (var date = startDate.ToFirstDayOfMonth(); date < DateTime.Today.ToFirstDayOfMonth(); date = date.AddMonths(1))
			{
				var uH = this.GetUserHistory(date);
				if (uH == null)
					this.CreateNewUserHistory(date);
			}
		}

		private UserHistory CreateNewUserHistory(DateTime date) => this.CreateNewUserHistory(date, this.WorkHours);

		private UserHistory CreateNewUserHistory(DateTime date, double workHours)
		{
			var uH = new UserHistory()
			{
				UserID = this.UserID,
				Period = date.ToPeriod(),
				WorkHoursPerDay = workHours
			};

			UserRepo.SaveUserHistory(uH);
			return uH;
		}

		#endregion USER HISTORIES

		#region PASSWORD OPERATIONS

		public void SetForgottenPasswordToken()
		{
			this.ForgottenPasswordToken = Guid.NewGuid();
			this.LastModifiedBy = "Password Recovery Operations";
			UserRepo.Save(this);
		}

		public void ResetForgottenPasswordToken()
		{
			this.ForgottenPasswordToken = null;
			this.LastModifiedBy = "Password Recovery Operations";

			UserRepo.Save(this);
		}

		#endregion PASSWORD OPERATIONS

		#region ROLE MANAGEMENT

		public List<Role> GetRoles() => UserRepo.GetRoles(this.UserID)
					.OrderBy(r => r.RoleName)
					.ToList();

		public List<Role> GetOtherRoles() => UserRepo.GetOtherRoles(this.UserID)
					.OrderBy(r => r.RoleName)
					.ToList();

		public static List<User> GetByRoleBW(int roleID) => UserRepo.GetByRoleBW(roleID)
					.OrderBy(u => u.FirstName)
					.ThenBy(u => u.LastName)
					.ThenBy(u => u.UserName)
					.ToList();

		public virtual bool HasRole(UserRoleType userRoleType) => this.GetRoles()
					.Exists(r => r.RoleID == (int)userRoleType);

		public void AssignRole(User assigningUser, UserRoleType userRoleType)
		{
			if (!assigningUser.HasRole(UserRoleType.SystemAdministrator))
				throw new UserRoleException("You do not have the required permissions to assign roles to a user.");

			if (this.HasRole(userRoleType))
				return;

			var ur = UserRole.GetNew(assigningUser, this.UserID, (int)userRoleType);
			UserRepo.AssignRoleToUser(ur);
		}

		public void RemoveRole(User assigningUser, UserRoleType userRoleType)
		{
			if (!assigningUser.HasRole(UserRoleType.SystemAdministrator))
				throw new UserManagementException("You do not have the required permissions to remove roles from a user.");

			if (userRoleType == UserRoleType.ProjectOwner && this.GetProjectsForProjectOwner().Count > 0)
				throw new UserManagementException("The 'Project Owner' role can not be removed from this user because he/she is still the owner of at least one project. Please assign these projects to a different user before attempting to remove this role.");

			if (userRoleType == UserRoleType.Manager && this.GetStaffOfManager().Count > 0)
				throw new UserManagementException("The 'Manager' role can not be removed from this user because he/she is still the manager of at least one user. Please assign a new manager to the user/s before attempting to remove this role.");

			UserRepo.RevokeRoleFromUser(this.UserID, (int)userRoleType);
		}

		#endregion ROLE MANAGEMENT

		#region CAN MANAGE ETC

		public virtual bool CanManage(User staffUser)
		{
			if (this.IsSystemUser)
				return true;

			if (this.HasRole(UserRoleType.SystemAdministrator))
				return true;

			if (this.HasRole(UserRoleType.Accountant))
				return true;

			if (this.IsOwnerOfUserProject(staffUser))
				return true;

			if (this.HasRole(UserRoleType.Manager))
				return true;

			if (this.UserID == staffUser.ManagerID || staffUser.SecondaryManagers.Select(u => u.UserID).Contains(this.UserID))
				return true;

			return false;
		}

		internal bool CanManage(Client client)
		{
			if (this.IsSystemUser)
				return true;

			if (this.HasRole(UserRoleType.SystemAdministrator))
				return true;

			if (this.HasRole(UserRoleType.ProjectOwner) && client.GetProjects().Any(p => p.ProjectOwnerID == this.UserID))
				return true;

			return false;
		}

		internal bool CanManage(Contact contact)
		{
			if (contact == null)
				throw new ArgumentNullException(nameof(contact));
			if (this.IsSystemUser)
				return true;

			if (this.HasRole(UserRoleType.SystemAdministrator))
				return true;

			return false;
		}

		public bool CanManage(Project project)
		{
			if (this.IsSystemUser)
				return true;

			if (this.HasRole(UserRoleType.SystemAdministrator) || this.HasRole(UserRoleType.ProjectOwner))
				return true;

			if (project.ProjectOwnerID == this.UserID && this.HasRole(UserRoleType.ProjectOwner) && project.ProjectID > 0)
				return true;

			return false;
		}

		public bool CanManage(Region region)
		{
			if (this.IsSystemUser)
				return true;

			if (this.HasRole(UserRoleType.SystemAdministrator))
				return true;

			return false;
		}

		internal bool CanManage(Activity activity) => this.CanManage(activity.GetProject());

		internal bool CanManage(WorkAllocation wA)
		{
			if (this.IsSystemUser)
				return true;

			if (this.HasRole(UserRoleType.SystemAdministrator))
				return true;

			if (wA.UserID == this.UserID)
				return true;

			//if the user can manage the user that owns the worksession, return true
			if (this.CanManage(wA.GetUser()))
				return true;

			//if the user can manage the project that owns the activity that the worksession is made for, return true
			if (this.CanManage(wA.GetProject()))
				return true;

			return false;
		}

		public bool CanManage(Invoice invoice)
		{
			if (invoice == null)
				return false;

			if (this.IsSystemUser)
				return true;

			if (this.HasRole(UserRoleType.SystemAdministrator))
				return true;

			if (this.HasRole(UserRoleType.InvoiceAdministrator))
			{
				// An invoice will only be for one invoice rule and thus one project
				if (invoice.IsGlobalFixedCost)
				{
					if (this.UserID == invoice.GetProject().InvoiceAdminID)
						return true;
				}
				else if (invoice.GetProjects().Any(p => p.InvoiceAdminID == this.UserID) || invoice.IsAdHoc)
				{
					return true;
				}
			}

			return this.HasRole(UserRoleType.Accountant) && invoice.IsReady;
		}

		internal bool CanManage(InvoiceRule invoiceRule)
		{
			if (this.IsSystemUser)
				return true;

			if (this.HasRole(UserRoleType.SystemAdministrator))
				return true;

			if (this.HasRole(UserRoleType.InvoiceAdministrator))
			{
				//if creating a new rule
				if (invoiceRule.InvoiceRuleID == 0)
					return true;

				//fixed cost rules have a projectID and the invAdmin must manage that project
				if (invoiceRule.IsGlobalFixedCost)
				{
					if (this.UserID == invoiceRule.GetProject().InvoiceAdminID)
						return true;
				}
				else if (invoiceRule.GetProjects().Any(p => this.CanManageInvoicesRulesForProject(p.Project)))
				{
					return true;
				}
			}

			return false;
		}

		internal bool CanManageInvoicesRulesForProject(Project project)
		{
			//TODO: Introduce new Role: Global InvoiceAdmin, so that these ppl can view all invoices, eg Leon will see all, Eric can only see Gallo
			if (this.IsSystemUser)
				return true;

			if (this.HasRole(UserRoleType.SystemAdministrator))
				return true;

			if (this.HasRole(UserRoleType.InvoiceAdministrator) && this.UserID == project.InvoiceAdminID)
				return true;

			return false;
		}

		internal bool CanManageInvoicesForProject(Project project)
		{
			//TODO: Introduce new Role: Global InvoiceAdmin, so that these ppl can view all invoices, eg Leon will see all, Eric can only see Gallo
			if (this.IsSystemUser)
				return true;

			if (this.HasRole(UserRoleType.SystemAdministrator))
				return true;

			if (this.HasRole(UserRoleType.InvoiceAdministrator) && this.UserID == project.InvoiceAdminID)
				return true;

			return false;
		}

		public bool CanViewInvoices()
		{
			if (this.IsSystemUser)
				return true;

			if (this.HasRole(UserRoleType.SystemAdministrator))
				return true;

			if (this.HasRole(UserRoleType.InvoiceAdministrator))
				return true;

			if (this.HasRole(UserRoleType.Accountant))
				return true;

			return false;
		}

		public bool CanViewInvoice(Invoice invoice)
		{
			if (this.IsSystemUser)
				return true;

			if (this.HasRole(UserRoleType.SystemAdministrator))
				return true;

			if (this.HasRole(UserRoleType.InvoiceAdministrator) && invoice.GetProjects().Any(p => this.CanManageInvoicesForProject(p)))
				return true;

			if (this.HasRole(UserRoleType.Accountant))
			{
				if (invoice.InvoiceStatusID == (int)InvoiceStatusType.Ready || invoice.InvoiceStatusID == (int)InvoiceStatusType.Complete)
					return true;
			}

			return false;
		}

		internal bool CanManage(Leave leave)
		{
			if (this.IsSystemUser)
				return true;

			if (this.UserID == leave.UserID)
				return true;

			var owner = UserRepo.GetByID(leave.UserID);
			return this.CanManage(owner);
		}

		#endregion CAN MANAGE ETC

		#region USER PREFERENCES

		public UserPreference GetUserPreference()
		{
			//This function gets the existing record, or creates a new one
			var userPreference = UserPreferenceRepo.GetByUserID(this.UserID);
			if (userPreference != null)
				return userPreference;

			userPreference = UserPreference.GetNew(this);
			userPreference.Save();

			return userPreference;
		}

		public DateTime GetDayStartTime(DateTime date)
		{
			var userPreference = this.GetUserPreference();

			if (!userPreference.DefaultWorkSessionStartTime.HasValue)
				return date.Date.AddHours(9);

			date = date.AddHours(userPreference.DefaultWorkSessionStartTime.Value.Hour);
			return date.AddMinutes(userPreference.DefaultWorkSessionStartTime.Value.Minute);
		}

		public DateTime GetDayEndTime(DateTime date)
		{
			var userPreference = this.GetUserPreference();

			if (!userPreference.DefaultWorkSessionEndTime.HasValue)
				return this.GetDayStartTime(date).AddHours(this.WorkHours);

			date = date.AddHours(userPreference.DefaultWorkSessionEndTime.Value.Hour);
			return date.AddMinutes(userPreference.DefaultWorkSessionEndTime.Value.Minute);
		}

		public void ChangeInvoiceIndexDateSelector(User userChanging, int toChangeTo)
		{
			//if the value to be changed to does not equal 1 or 0, make it default 1
			if (toChangeTo != 0)
				toChangeTo = 1;

			var up = this.GetUserPreference();
			up.InvoiceIndexDateSelector = toChangeTo;

			up.Save(userChanging);
		}

		#endregion USER PREFERENCES

		#region MANAGER

		public List<User> GetManagerList() => UserRepo.GetManagerList()
			.OrderBy(u => u.FullName)
			.ToList();

		public List<User> GetStaffOfManager() => UserRepo.GetStaffForManager(this.UserID)
					.OrderBy(u => u.LastName)
					.ThenBy(u => u.FirstName)
					.ToList();

		public List<User> GetStaffOfManager(UserRoleType userRoleType) => UserRepo.GetStaffForManager(this.UserID, userRoleType)
					.OrderBy(u => u.LastName)
					.ThenBy(u => u.FirstName)
					.ToList();

		public void AssignManagerToUser(User userAssigning, int managingUserID)
		{
			if (this.UserID == managingUserID)
				throw new UserManagementException("The user cannot be assigned as his/her own manager. Assignment cancelled.");

			var managingUser = UserRepo.GetByID(managingUserID);
			if (managingUser == null)
				throw new NullReferenceException("The user being assigned as manager does not exist.");

			this.AssignManagerToUser(userAssigning, managingUser);
		}

		public void AssignManagerToUser(User userAssigning, User managingUser)
		{
			if (!userAssigning.HasRole(UserRoleType.SystemAdministrator))
				throw new UserRoleException("You do not have the required permissions to change a user's manager.");

			if (managingUser == null)
			{
				this.manager = null;
				this.ManagerID = 0;
				return;
			}

			if (!managingUser.HasRole(UserRoleType.Manager))
			{
				throw new UserRoleException(
						string.Format($"{managingUser.FullName} does not have the '{UserRoleType.Manager}' role and therefore cannot be assigned as the manager of {this.FullName}")
					);
			}

			this.ManagerID = managingUser.UserID;
		}

		#endregion MANAGER

		#region SECONDARY MANAGER

		private List<User> secondaryManagers { get; set; }

		[NotMapped]
		public List<User> SecondaryManagers
		{
			get
			{
				if (this.secondaryManagers == null)
					this.secondaryManagers = this.GetSecondaryManagers();

				return this.secondaryManagers;
			}
		}

		public List<User> GetSecondaryManagers() => UserRepo.GetSecondaryManagers(this.UserID)
					.OrderBy(m => m.FirstName)
					.ThenBy(m => m.LastName)
					.ToList();

		public void AssignSecondaryManagerToUser(User userAssigning, int managingUserID)
		{
			var managingUser = UserRepo.GetByID(managingUserID);
			if (managingUser == null)
				throw new NullReferenceException("The user being assigned as manager does not exist.");

			this.AssignSecondaryManagerToUser(userAssigning, managingUser);
		}

		public void AssignSecondaryManagerToUser(User userAssigning, User managingUser)
		{
			if (!userAssigning.HasRole(UserRoleType.SystemAdministrator))
				throw new UserRoleException("You do not have the required permissions to assign a secondary manager to a user.");

			if (managingUser == null)
				return;

			if (!managingUser.HasRole(UserRoleType.Manager))
			{
				throw new UserRoleException(
						string.Format($"{managingUser.FullName} does not have the 'Manager' role and therefore cannot be assigned as a secondary manager of {this.FullName}")
					);
			}

			var model = SecondaryManagerAllocation.CreateNew(managingUser.UserID, this.UserID);
			UserRepo.AssignSecondaryManagerToUser(model);
		}

		#endregion SECONDARY MANAGER

		#region PROJECT OWNER

		public List<Project> GetProjectsForProjectOwner()
		{
			if (!this.HasRole(UserRoleType.ProjectOwner))
				return new List<Project>();

			return UserRepo.GetProjectsForProjectOwner(this.UserID)
					.Where(p => p.IsProjectActive)
					.ToList();
		}

		public bool IsOwnerOfUserProject(User user)
		{
			foreach (var project in UserRepo.GetProjects(user.UserID))
			{
				if (this.CanManage(project))
					return true;
			}

			return false;
		}

		#endregion PROJECT OWNER

		#region PROJECT

		public List<Project> GetProjects() => UserRepo.GetProjects(this.UserID)
					.OrderBy(p => p.ProjectName)
					.ToList();

		#endregion PROJECT

		#region ACTIVITY

		public Activity GetActivityByID(int activityID) => UserRepo.GetActivityByID(activityID);

		public List<UserActivity> GetActivities() => this.GetActivities(DateTime.Today);

		public List<UserActivity> GetActivities(DateTime refDate) => UserRepo.GetActivities(this.UserID, refDate.Date)
					.OrderBy(c => c.ClientName)
					.ThenBy(p => p.ProjectName)
					.ThenBy(a => a.ActivityName)
					.ToList();

		public List<Activity> GetActivitiesForDateSpan(DateTime startDate, DateTime endDate) => UserRepo.GetActivitiesForDateSpan(this.UserID, startDate.Date, endDate.Date)
					.ToList();

		public List<UserActivity> GetPastActivities(DateTime refDate) => UserRepo.GetPastActivities(this.UserID, refDate)
					.ToList();

		public List<UserActivity> GetFutureActivities(DateTime refDate) => UserRepo.GetFutureActivities(this.UserID, refDate)
					.ToList();

		public void AssignDefaultActivities(User assigningUser, User userBeingAssignedToActivity)
		{
			var activityList = UserRepo.GetDefaultActivities();

			foreach (int activityID in activityList)
			{
				if (!this.IsAssignedToActivity(activityID))
				{
					var ua = new UserAllocation()
					{
						UserID = userBeingAssignedToActivity.UserID,
						ActivityID = activityID,
						ChargeRate = 0,
						StartDate = userBeingAssignedToActivity.EmployStartDate,
						ScheduledEndDate = null,
						ConfirmedEndDate = null
					};

					ua.LastModifiedBy = assigningUser.FullName;
					UserAllocationRepo.Save(ua);
				}
			}
		}

		public void RemoveDefaultActivities(User user)
		{
			var activityList = UserRepo.GetDefaultActivities();

			foreach (int activityID in activityList)
			{
				if (this.IsAssignedToActivity(activityID))
				{
					var userAllocation = UserRepo.GetUserAllocationByUserAndActivityID(activityID, user.UserID);

					if (userAllocation != null)
						user.DeleteUserAllocation(userAllocation);
				}
			}
		}

		public void UpdateUserAllocationsStartDate(string lastModifiedName, int userID, DateTime startDate)
		{
			var userAllocationList = UserRepo.GetActiveUserAllocationByUserID(userID).ToList();

			foreach (var ua in userAllocationList)
			{
				ua.StartDate = startDate;
				ua.LastModifiedBy = lastModifiedName;
				UserAllocationRepo.Save(ua);
			}
		}

		public bool IsAssignedToActivity(int activityID) => this.IsAssignedToActivity(activityID, DateTime.Today);

		public bool IsAssignedToActivity(int activityID, DateTime refDate) => this.GetActivities(refDate)
					.Select(a => a.ActivityID)
					.Contains(activityID);

		public double ActivityRate(int activityID) => this.ActivityRate(activityID, DateTime.Today);

		public double ActivityRate(int activityID, DateTime refDate) => this.GetActivities(refDate).SingleOrDefault(a => a.ActivityID == activityID)?
					.ChargeRate ?? default;

		public List<ActivityDetail> GetWorkedActivitiesForDateSpan(DateTime startDate, DateTime endDate) => UserRepo.GetWorkedActivitiesForDateSpan(this.UserID, startDate.Date, endDate.Date);

		public List<ActivityDetail> GetWorkedActivitiesForDateSpanForWorkSessionStatus(DateTime startDate, DateTime endDate, int workSessionStatus) => UserRepo.GetWorkedActivitiesForDateSpanForWorkSessionStatus(this.UserID, startDate.Date, endDate.Date, workSessionStatus);

		public void DeleteUserAllocation(UserAllocation userAllocation)
		{
			var activity = ActivityRepo.GetByID(userAllocation.ActivityID);
			if (UserRepo.GetWorkAllocationsAssignedToActivity(activity.ProjectID, userAllocation.ActivityID, userAllocation.UserID, DateTime.Now).ToList().Count > 0)
				throw new ActivityAdminException("You cannot delete the user allocation as time has already been allocated to it. Action cancelled.");

			UserRepo.DeleteUserAllocation(userAllocation);
		}

		#endregion ACTIVITY

		#region EXPENSES

		public List<ExpenseDetail> GetExpensesForDateSpan(DateTime startDate, DateTime endDate) => UserRepo.GetExpensesForDateSpan(this.UserID, startDate.Date, endDate.Date);

		public int ExpenseStatus(int userID) => UserRepo.GetExpenseStatus(userID);

		#endregion EXPENSES

		#region WORKSESSION

		public List<WorkSession> GetWorkSessions(DateTime date) => UserRepo.GetWorkSessionsForDate(this.UserID, date)
					.ToList();

		public List<WorkSession> GetWorkSessionsForDateSpan(DateTime startDate, DateTime endDate) => UserRepo.GetWorkSessionsForDateSpan(this.UserID, startDate.Date, endDate.Date)
					.ToList();

		public WorkSession GetWorkSession(int workSessionID) => UserRepo.GetWorkSessionByID(this.UserID, workSessionID);

		public string GetLastWorkSession() => WorkSessionRepo.GetLastWorkSessionForUser(this.UserID);

		public void DeleteWorkSession(User userDeleting, int workSessionID)
		{
			var workSession = WorkSessionRepo.GetByID(workSessionID);
			if (workSession == null)
				throw new UserWorkSessionException("An error occurred while trying to locate the work session. Please try again.");

			this.DeleteWorkSession(userDeleting, workSession);
		}

		public void DeleteWorkSession(User userDeleting, WorkSession workSession)
		{
			if (this.UserID != userDeleting.UserID && !userDeleting.HasRole(UserRoleType.SystemAdministrator) && !userDeleting.CanManage(this))
				throw new UserRoleException(string.Format($"You do not have the required permissions to remove {this.FullName}'s work session. Remove cancelled."));

			if (!workSession.CanDelete)
				throw new UserWorkSessionException("This work session cannot be removed because the timesheet for the month in which this work session is has been locked.");

			//Delete all workallocation if a worksession is deleted, where only 1 worksession exist
			if (this.GetWorkSessions(workSession.WorkDate).Count == 1 && this.GetWorkAllocations(workSession.WorkDate).Count > 0)
				UserRepo.DeleteAllWorkAllocation(this.GetWorkAllocations(workSession.WorkDate));

			UserRepo.DeleteWorkSession(workSession);
		}

		public WorkSession CaptureWorkSession(User userSaving, DateTime startTime, DateTime endTime, double lunchTime)
		{
			if (userSaving.UserID != this.UserID && !userSaving.CanManage(this))
				throw new UserRoleException(string.Format($"You do not have the required permissions to manage {this.FullName}'s work sessions. Action cancelled."));

			if (!this.HasRole(UserRoleType.TimeCapturing))
			{
				if (this.UserID == userSaving.UserID)
					throw new UserRoleException(string.Format($"You do not have the '{UserRoleType.TimeCapturing}' role and therefore cannot create a work session."));
				else
					throw new UserRoleException(string.Format($"{this.FullName} does not have the '{UserRoleType.TimeCapturing}' role and therefore, work sessions cannot be created for him/her."));
			}

			var nws = new WorkSession()
			{
				UserID = this.UserID,
				StartTime = startTime,
				EndTime = endTime,
				LunchTime = lunchTime,
				//Default status is Exceptions as the time would not have been allocated yet.
				//Once allocation is saved and no exceptions exist status will be update to UnApproved
				WorkSessionStatusID = 3
			};

			nws.LastModifiedBy = userSaving.LastModifiedBy;
			this.SaveWorkSession(nws, userSaving);
			return nws;
		}

		public void UpdateWorkSession(User userUpdating, WorkSession workSession)
		{
			if (this.UserID != userUpdating.UserID && !userUpdating.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to update someone else's work sessions.");

			if (!this.HasRole(UserRoleType.TimeCapturing))
			{
				if (this.UserID == userUpdating.UserID)
					throw new UserRoleException(string.Format($"You do not have the '{UserRoleType.TimeCapturing}' role and therefore cannot update a work session."));
				else
					throw new UserRoleException(string.Format($"{this.FullName} does not have the '{UserRoleType.TimeCapturing}' role and therefore, work sessions cannot be updated for him/her."));
			}

			workSession.LastModifiedBy = userUpdating.FullName;
			this.SaveWorkSession(workSession, userUpdating);
		}

		public void ApproveWorkSession(WorkSession workSession, User userUpdating)
		{
			if (this.UserID != userUpdating.UserID && !userUpdating.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to update someone else's work sessions.");

			if (!this.HasRole(UserRoleType.TimeCapturing))
			{
				if (this.UserID == userUpdating.UserID)
					throw new UserRoleException(string.Format($"You do not have the '{UserRoleType.TimeCapturing}' role and therefore cannot update a work session."));
				else
					throw new UserRoleException(string.Format($"{this.FullName} does not have the '{UserRoleType.TimeCapturing}' role and therefore, work sessions cannot be updated for him/her."));
			}

			workSession.WorkSessionStatusID = 2;
			workSession.LastModifiedBy = userUpdating.FullName;
			this.SaveWorkSession(workSession, userUpdating);
		}

		public void ApproveNonWorkSession(NonWorkSession nonWorkSession, User userUpdating)
		{
			if (this.UserID != userUpdating.UserID && !userUpdating.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to update someone else's work sessions.");

			if (!this.HasRole(UserRoleType.TimeCapturing))
			{
				if (this.UserID == userUpdating.UserID)
					throw new UserRoleException(string.Format($"You do not have the '{UserRoleType.TimeCapturing}' role and therefore cannot update a work session."));
				else
					throw new UserRoleException(string.Format($"{this.FullName} does not have the '{UserRoleType.TimeCapturing}' role and therefore, work sessions cannot be updated for him/her."));
			}

			nonWorkSession.LastModifiedBy = userUpdating.FullName;
			this.SaveNonWorkSession(nonWorkSession);
		}

		public void UnApproveWorkSession(WorkSession workSession, User userUpdating)
		{
			if (this.UserID != userUpdating.UserID && !userUpdating.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to update someone else's work sessions.");

			if (!this.HasRole(UserRoleType.TimeCapturing))
			{
				if (this.UserID == userUpdating.UserID)
					throw new UserRoleException(string.Format($"You do not have the '{UserRoleType.TimeCapturing}' role and therefore cannot update a work session."));
				else
					throw new UserRoleException(string.Format($"{this.FullName} does not have the '{UserRoleType.TimeCapturing}' role and therefore, work sessions cannot be updated for him/her."));
			}

			workSession.WorkSessionStatusID = 1;
			workSession.LastModifiedBy = userUpdating.FullName;
			this.SaveWorkSession(workSession, userUpdating);
		}

		public void SaveWorkSession(WorkSession workSession, User userSaving)
		{
			if (workSession.StartTime.Date < EnabillSettings.SiteStartDate)
				throw new EnabillSettingsException("You cannot capture a work session on a date before the start of the site.");

			if (!workSession.CanEdit)
				throw new UserWorkSessionException(string.Format($"{this.FullName}'s timesheet is closed for {workSession.WorkDate.ToMonthName()} {workSession.WorkDate.Year} has been locked. Work sessions can not be created or edited."));

			if (!workSession.IsEndDateGreaterThanStartDate())
				throw new UserWorkSessionException("End time cannot be before or equal to the start time.");

			if (workSession.EndTime.Date != workSession.StartTime.Date)
				throw new UserWorkSessionException("Your end time cannot be on a different day than the start day.");

			if (workSession.LunchTime < 0)
				throw new UserWorkSessionException("Lunch can not be set to a negative value. Please revise the value inserted for lunch.");

			if (workSession.LunchTime % 0.25 != 0)
				throw new UserWorkSessionException("Lunch can only be recorded in multiples of 0.25 (15 minutes). Please revise the value inserted for lunch.");

			// Commented out as some clients want their staff to capture their time a month ahead in the case of the December holidays.
			//if (workSession.StartTime > DateTime.Today.ToLastDayOfMonth().AddHours(24))
			//	throw new UserWorkSessionException("You cannot capture a work session in the future.");

			if (workSession.TotalTime < 0)
				throw new UserWorkSessionException("Time allocated to lunch cannot be greater than the total hours of the work session.");

			if (workSession.TotalTime == 0)
				throw new UserWorkSessionException("You cannot create a work session for lunch taken.");

			if (this.IsFlexiDayTakenOnDate(workSession.StartTime.Date))
			{
				//delete the approved flexiday if time captured on this day
				this.RemoveFlexiDay(userSaving, this.GetFlexiDay(workSession.StartTime.Date));
			}

			if (this.IsFlexiDayTakenOnDatePending(workSession.StartTime.Date))
			{
				//delete pending the flexiday if time captured on this day
				this.RemoveFlexiDay(userSaving, this.GetFlexiDayPending(workSession.StartTime.Date));
			}

			var workSessions = this.GetWorkSessions(workSession.StartTime.Date).Where(ws => ws.WorkSessionID != workSession.WorkSessionID).ToList();
			if (workSessions.Any(ws => ws.DoesOverlap(workSession)))
				throw new UserWorkSessionException("You cannot capture a work session that overlaps with another on the same day.");

			UserRepo.SaveWorkSession(workSession);
		}

		public void SaveNonWorkSession(NonWorkSession nonWorkSession)
		{
			if (!nonWorkSession.CanEdit)
				throw new UserWorkSessionException(string.Format($"{this.FullName}'s timesheet is closed for {nonWorkSession.Date.ToMonthName()} {nonWorkSession.Date.Year} has been locked. Work sessions can not be created or edited."));

			UserRepo.SaveNonWorkSession(nonWorkSession);
		}

		public double GetTotalWorkTime(DateTime dateTime) => this.GetWorkSessions(dateTime.Date).Sum(ws => ws.TotalTime);

		public double GetUnallocatedTime(DateTime dateTime) => this.GetTotalWorkTime(dateTime) - this.GetAllocatedTime(dateTime);

		public double GetAllocatedTime(DateTime dateTime) => this.GetWorkAllocations(dateTime).Sum(tu => tu.HoursWorked);

		#endregion WORKSESSION

		#region NONWORKSESSION

		public void UpdateNonWorkSession(User userUpdating, NonWorkSession nonWorkSession)
		{
			if (this.UserID != userUpdating.UserID && !userUpdating.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to update someone else's non work sessions.");

			if (!this.HasRole(UserRoleType.TimeCapturing))
			{
				if (this.UserID == userUpdating.UserID)
					throw new UserRoleException(string.Format($"You do not have the '{UserRoleType.TimeCapturing}' role and therefore cannot update a non work session."));
				else
					throw new UserRoleException(string.Format($"{this.FullName} does not have the '{UserRoleType.TimeCapturing}' role and therefore, non work sessions cannot be updated for him/her."));
			}

			nonWorkSession.LastModifiedBy = userUpdating.FullName;
			this.SaveNonWorkSession(nonWorkSession);
		}

		#endregion NONWORKSESSION

		#region TIMESHEET APPROVAL

		public bool CanTimesheetLockStatusBeManaged(DateTime date) => date >= EnabillSettings.SiteStartDate && date.Date <= DateTime.Today.ToLastDayOfMonth();

		public bool IsTimesheetLockedForDate(DateTime date)
		{
			//only locked if date is in future but NOT in current month as time is allowed to be captured in advance if in current month
			//if (date < EnabillSettings.SiteStartDate || date.Date > DateTime.Today.ToLastDayOfMonth())
			// Changed to allow to capture time up to one month ahead. Some clients want staff to capture time ahead for the December holidays.
			if (date < EnabillSettings.SiteStartDate || date.Date > DateTime.Today.AddMonths(1).ToLastDayOfMonth())
				return true;

			var record = this.GetTimesheetApprovalRecordForDate(date.ToFirstDayOfMonth());
			return record != null;
		}

		private TimesheetApproval GetTimesheetApprovalRecordForDate(DateTime workDay) => UserRepo.GetTimesheetApprovalRecordForDate(this.UserID, workDay.ToFirstDayOfMonth());

		public void LockTimesheet(User userLocking, DateTime monthDate) => this.LockTimesheets(userLocking, monthDate, monthDate);

		public void LockTimesheets(User userLocking, DateTime dateFrom, DateTime dateTo)
		{
			dateFrom = dateFrom.ToFirstDayOfMonth();
			dateTo = dateTo.ToLastDayOfMonth();

			if (!userLocking.CanManage(this))
				throw new UserRoleException(string.Format($"You do not have the required permissions to lock {this.FullName}'s timesheets. Action cancelled."));

			if (dateFrom.IsInFutureMonth() || dateTo.IsInFutureMonth())
				throw new TimesheetApprovalException("Future timesheets cannot be locked. Action cancelled.");

			for (var date = dateFrom; date <= dateTo; date = date.AddMonths(1))
			{
				var record = new TimesheetApproval()
				{
					UserID = this.UserID,
					MonthDate = date,
					UserManaged = userLocking.FullName,
					DateManaged = DateTime.Now.ToCentralAfricanTime()
				};

				record.LastModifiedBy = userLocking.FullName;
				UserRepo.LockTimesheet(record);
			}
		}

		public void UnlockTimesheet(User userUnlocking, DateTime monthDate) => this.UnlockTimesheets(userUnlocking, monthDate, monthDate);

		public void UnlockTimesheets(User userUnlocking, DateTime dateFrom, DateTime dateTo)
		{
			dateFrom = dateFrom.ToFirstDayOfMonth();
			dateTo = dateTo.ToLastDayOfMonth();

			if (!userUnlocking.CanManage(this))
				throw new UserRoleException(string.Format($"You do not have the required permissions to unlock {this.FullName}'s timesheet. Action cancelled."));

			UserRepo.UnlockTimesheet(this.UserID, dateFrom, dateTo);
		}

		public bool IsTimeCaptured(DateTime dateFrom, DateTime dateTo, User callingUser, out List<WorkAllocationExceptionModel> workAllocationExceptionList)
		{
			bool isTimeCaptured = true;
			bool isException = false;

			workAllocationExceptionList = new List<WorkAllocationExceptionModel>();

			//exclude system users from this process
			if (this.IsSystemUser)
				return isTimeCaptured;

			workAllocationExceptionList = UserRepo.GetTimeCaptureExceptions(this.UserID, dateFrom, dateTo).ToList();

			if (workAllocationExceptionList.Count > 0)
			{
				isTimeCaptured = false;
				isException = true;
			}

			var workSessions = UserRepo.GetWorkSessionsForDateSpan(this.UserID, dateFrom, dateTo).ToList();

			foreach (var wd in WorkDayRepo.GetWorkableDays(true, dateFrom, dateTo).ToList())
			{
				foreach (var ws in workSessions.Where(s => s.WorkDate == wd.WorkDate).ToList())
				{
					if (workAllocationExceptionList.Where(we => we.WorkDate == wd.WorkDate).Distinct().Any())
					{
						//set exception status if applicable. revert exception status if not applicable not sure why this occurs, as updating your ws should correct the status, but it does
						ws.WorkSessionStatusID = 3;// isException ? 3 : !isException && ws.WorkSessionStatusID == 3 ? 1 : ws.WorkSessionStatusID;
						this.SaveWorkSession(ws, callingUser);
					}
					else
					{
						ws.WorkSessionStatusID = ws.WorkSessionStatusID == 3 ? 1 : ws.WorkSessionStatusID;
						this.SaveWorkSession(ws, callingUser);
					}
				}
			}

			#region OLD SLOW CODE

			/*
			foreach (WorkDay wd in WorkDayRepo.GetWorkableDays(true, dateFrom, dateTo).ToList())
			{
				if (!this.IsAnyLeaveTakenOnDate(wd.WorkDate) && !this.IsFlexiDayTakenOnDate(wd.WorkDate))
				{
					if (this.GetTotalWorkTime(wd.WorkDate) == 0)
					{
						//worksession missing
						WorkAllocationExceptionModel wae = new WorkAllocationExceptionModel();
						wae.UserName = this.UserName;
						wae.WorkDate = wd.WorkDate;
						wae.ExceptionDetail = "Worksession not captured.";
						workAllocationExceptionList.Add(wae);
						isTimeCaptured = false;
						isException = true;
					}
					else if (this.GetUnallocatedTime(wd.WorkDate) > 0)
					{
						//unallocated/overallocated time
						WorkAllocationExceptionModel wae = new WorkAllocationExceptionModel();
						wae.UserName = this.UserName;
						wae.WorkDate = wd.WorkDate;
						wae.ExceptionDetail = "Activity Time Under Allocated.";
						workAllocationExceptionList.Add(wae);
						isTimeCaptured = false;
						isException = true;
					}
					else if (this.GetUnallocatedTime(wd.WorkDate) < 0)
					{
						//overallocated time
						WorkAllocationExceptionModel wae = new WorkAllocationExceptionModel();
						wae.UserName = this.UserName;
						wae.WorkDate = wd.WorkDate;
						wae.ExceptionDetail = "Activity Time Over Allocated.";
						workAllocationExceptionList.Add(wae);
						isTimeCaptured = false;
						isException = true;
					}

					List<WorkSession> workSessions = UserRepo.GetWorkSessionsForDate(this.UserID, wd.WorkDate).ToList();
					foreach (WorkSession ws in workSessions)
					{
						//set exception status if applicable. revert exception status if not applicable not sure why this occurs, as updating your ws should correct the status, but it does
						ws.WorkSessionStatusID = isException ? 3 : !isException && ws.WorkSessionStatusID == 3 ? 1 : ws.WorkSessionStatusID;
						this.SaveWorkSession(ws, callingUser);
					}

					//reset before processing next day. not using isTimeCaptured as that is a specific to the user wherease isexception is specific to the user & workday
					isException = false;
				}
			}
			*/

			#endregion OLD SLOW CODE

			return isTimeCaptured;
		}

		#endregion TIMESHEET APPROVAL

		#region EXPENSE APPROVAL

		public bool CanExpenseLockStatusBeManaged(DateTime date) => date >= EnabillSettings.SiteStartDate && date.Date >= DateTime.Today.ToLastDayOfMonth();

		public bool IsExpenseLockedForDate(DateTime date)
		{
			//only locked if date is in future but NOT in current month as time is allowed to be captured in advance if in current month
			if (date < EnabillSettings.SiteStartDate || date.Date > DateTime.Today.ToLastDayOfMonth())
				return true;

			var record = this.GetExpenseApprovalRecordsForDate(date.ToFirstDayOfMonth());
			return record != null;
		}

		private List<ExpenseApproval> GetExpenseApprovalRecordsForDate(DateTime workDay) => UserRepo.GetExpenseApprovalRecordsForDate(this.UserID, workDay.ToFirstDayOfMonth());

		public void LockExpense(User userLocking, DateTime monthDate) => this.LockExpenses(userLocking, monthDate, monthDate);

		public void LockExpenses(User userLocking, DateTime dateFrom, DateTime dateTo)
		{
			dateFrom = dateFrom.ToFirstDayOfMonth();
			dateTo = dateTo.ToLastDayOfMonth();

			if (!userLocking.CanManage(this))
				throw new UserRoleException(string.Format($"You do not have the required permissions to lock {this.FullName}'s expense. Action cancelled."));

			if (dateFrom.IsInFutureMonth() || dateTo.IsInFutureMonth())
				throw new ExpenseApprovalException("Future expenses cannot be locked. Action cancelled.");

			for (var date = dateFrom; date <= dateTo; date = date.AddMonths(1))
			{
				var record = new ExpenseApproval()
				{
					UserID = this.UserID,
					MonthDate = date,
					UserManaged = userLocking.FullName,
					DateManaged = DateTime.Now.ToCentralAfricanTime()
				};

				record.LastModifiedBy = userLocking.FullName;
				UserRepo.LockExpense(record);
			}
		}

		public void UnlockExpense(User userUnlocking, DateTime monthDate) => this.UnlockTimesheets(userUnlocking, monthDate, monthDate);

		public void UnlockExpenses(User userUnlocking, DateTime dateFrom, DateTime dateTo)
		{
			dateFrom = dateFrom.ToFirstDayOfMonth();
			dateTo = dateTo.ToLastDayOfMonth();

			if (!userUnlocking.CanManage(this))
				throw new UserRoleException(string.Format($"You do not have the required permissions to unlock {this.FullName}'s timesheet. Action cancelled."));

			UserRepo.UnlockExpense(this.UserID, dateFrom, dateTo);
		}

		#endregion EXPENSE APPROVAL

		#region WORKALLOCATION

		public void SaveWorkAllocation(User userSavingWA, WorkAllocation wA)
		{
			if (this.UserID != userSavingWA.UserID && !userSavingWA.CanManage(wA))
				throw new UserRoleException("You do not have the required permissions to save an allocation for another user.");

			if (wA.HoursWorked >= 20)
				throw new WorkAllocationException("Allocations can not be captured if longer than 20hours. Please break this allocation into smaller time frames. Save cancelled.");

			if (wA.HoursWorked <= 0)
				throw new WorkAllocationException("Work allocations can not be equal to or less than 0. Save cancelled.");

			if ((this.GetWorkAllocations(wA.DayWorked).Where(wa => wa.WorkAllocationID != wA.WorkAllocationID).Sum(wa => wa.HoursWorked) + wA.HoursWorked) >= 24)
				throw new WorkAllocationException("By saving this work allocation, the total hours worked for this day is greater than 23 hours and 45 minutes which is not allowed. Save cancelled.");

			//Validation moved to javascript to cater for training details requirement
			//if ((string.IsNullOrWhiteSpace(wA.Remark) || string.IsNullOrEmpty(wA.Remark)) && wA.GetActivity().MustHaveRemarks)
			//    throw new WorkAllocationException("The remark on the allocation cannot be empty or contain only spaces. Action cancelled.");

			if (wA.DayWorked < EnabillSettings.SiteStartDate)
				throw new EnabillSettingsException("You cannot capture a work allocation for a day before the start date of the site.");

			if (!this.HasRole(UserRoleType.TimeCapturing))
			{
				if (this.UserID == userSavingWA.UserID)
					throw new UserRoleException("You do not have the 'TimeCapturer' role and therefore cannot capture a work allocation.");
				else
					throw new UserRoleException(this.FullName + " does not have the 'TimeCapturer' role and therefore, a work allocation cannot be captured.");
			}

			if (!wA.CanEdit)
				throw new WorkAllocationException("This work allocation has been invoiced and therefore, it cannot be edited.");

			var activity = wA.GetActivity();
			var project = wA.GetProject();

			if (!string.IsNullOrEmpty(wA.TicketReference))
			{
				var ticket = TicketRepo.GetByReference(wA.TicketReference);
				if (ticket == null || ticket.ClientID != project.GetClient().ClientID)
				{
					string message = "Ticket Reference <b> " + wA.TicketReference + "</b> is not linked to " + project.GetClient().ClientName + ". Please select a valid ticket reference.";
					throw new WorkAllocationException(message);
				}
				else
				{
					ticket.TimeSpent += wA.HoursWorked;
					ticket.Save();
				}
			}

			//Validation moved to javascript to cater for training details requirement
			//if (string.IsNullOrEmpty(wA.Remark) && activity.MustHaveRemarks)
			//    throw new WorkAllocationException(string.Format(
			//                                        $"A remark is required on work allocations submitted against {project.GetClient().ClientName} - {project.ProjectName} - {activity.ActivityName}. Action cancelled."));                                        , , , ));

			if (wA.HoursWorked % 0.25 != 0)
				throw new WorkAllocationException("Allocations can only be captured in multiples of 0.25. (15 minutes) Please make sure that all your allocations are correct then resubmit please.");

			if (wA.DayWorked < project.StartDate)
				throw new ProjectManagementException("The date for which this allocation is made is before the start date of the project. Save cancelled.");

			if (project.ConfirmedEndDate.HasValue && wA.DayWorked.Date > project.ConfirmedEndDate.Value.Date)
				throw new ProjectManagementException("The date for which this allocation is made is after the confirmed end date of the project. Save cancelled.");

			if (wA.Period == 0)
				wA.Period = wA.DayWorked.ToPeriod();

			if (wA.WorkAllocationID == 0)
			{
				//wa.HourRate = ActivityRate(wa.ActivityID);
				wA.UserCreated = userSavingWA.FullName;
				wA.DateCreated = DateTime.Now.ToUniversalTime().AddHours(2);
				wA.WorkAllocationType = (int)WorkAllocationType.UserCreated;
			}
			else
			{
				wA.UserModified = userSavingWA.FullName;
				wA.DateModified = DateTime.Now.ToUniversalTime().AddHours(2);
			}

			if (wA.GetActivity().ActivityName == "Training" && (int)TrainingCategoryType.PleaseSelect == wA.TrainingCategoryID)
				throw new WorkAllocationException("Please select a Training Category. Save cancelled.");

			if (wA.GetActivity().ActivityName == "Training" && string.IsNullOrWhiteSpace(wA.TrainerName))
				throw new WorkAllocationException("Please specify a Trainer name. Save cancelled.");

			if (wA.GetActivity().ActivityName == "Training" && (int)TrainingCategoryType.ExternalTrainingCourse == wA.TrainingCategoryID && string.IsNullOrWhiteSpace(wA.TrainingInstitute))
				throw new WorkAllocationException("Please specify a Training Institution name. Save cancelled.");

			wA.LastModifiedBy = userSavingWA.FullName;
			UserRepo.SaveWorkAllocation(wA);
		}

		public void DeleteWorkAllocation(User userDeleting, WorkAllocation wA)
		{
			if (this.UserID != userDeleting.UserID && !userDeleting.CanManage(wA))
				throw new UserRoleException("You do not have the required permissions to delete this work allocation");

			if (!wA.CanDelete)
				throw new WorkAllocationException("This allocation has been invoiced and cannot be deleted.");

			var note = WorkAllocationRepo.GetNote(wA.WorkAllocationID);
			if (note != null)
			{
				UserRepo.DeleteNote(note);
			}

			UserRepo.DeleteWorkAllocation(wA);
		}

		public List<WorkAllocation> GetWorkAllocations(DateTime date) => UserRepo.GetWorkAllocationsForDate(this.UserID, date)
					.ToList();

		public List<WorkAllocation> GetWorkAllocations(DateTime fromDate, DateTime toDate) => UserRepo.GetWorkAllocationsForSpan(this.UserID, fromDate, toDate).ToList();

		public WorkAllocation GetWorkAllocation(int workAllocationID) => UserRepo.GetWorkAllocation(this.UserID, workAllocationID);

		public List<WorkAllocation> GetWorkAllocationsForActivityForDate(Activity activity, DateTime workDay) => this.GetWorkAllocationsForActivityForDate(activity.ActivityID, workDay);

		public List<WorkAllocation> GetWorkAllocationsForActivityForDate(int activityID, DateTime workDay) => UserRepo.GetWorkAllocationsForActivityForDate(this.UserID, activityID, workDay)
					.ToList();

		public List<WorkAllocationExtendedModel> GetDetailedWorkAllocationsForActivityForDate(Activity activity, DateTime workDay)
		{
			var model = new List<WorkAllocationExtendedModel>();
			this.GetWorkAllocations(activity, workDay)
						.ToList()
						.ForEach(wa =>
									model.Add(WorkAllocation.GetExtendedDetail(wa))
								);

			return model;
		}

		private List<WorkAllocation> GetWorkAllocations(Activity activity, DateTime date) => UserRepo.GetWorkAllocationsForActivityForDate(this.UserID, activity.ActivityID, date)
					.ToList();

		public List<WorkAllocation> GetLastWorkAllocationDateForUserForActivity(int activityID) => UserRepo.GetLastWorkAllocationDateForUserForActivity(this.UserID, activityID).ToList();

		#endregion WORKALLOCATION

		#region NOTES

		public List<Note> GetNotes() => this.GetNotes(DateTime.Today.ToFirstDayOfMonth(), DateTime.Now);

		public List<Note> GetNotes(DateTime startDate, DateTime endDate) => UserRepo.GetNotes(this.UserID, startDate, endDate)
					.ToList();

		public Note GetNoteForWorkAllocation(int workAllocationID)
		{
			var workAllocation = WorkAllocationRepo.GetByID(workAllocationID);
			if (workAllocation == null)
				throw new Exception("The work allocation for this note was not found, transaction cancelled");

			var activity = ActivityRepo.GetByID(workAllocation.ActivityID);
			if (!activity.CanHaveNotes)
				return null;

			var project = ProjectRepo.GetByID(activity.ProjectID);
			var note = UserRepo.GetNoteForWorkAllocation(this.UserID, workAllocationID);

			return note ?? new Note() { WorkAllocationID = workAllocationID };
		}

		public List<Note> GetNotesForProject(int projectID) => UserRepo.GetNotesForProject(this.UserID, projectID)
					.ToList();

		public List<Note> GetNotesForActivity(int activityID)
		{
			var activity = ActivityRepo.GetByID(activityID);
			if (activity == null)
				throw new Exception("Activity was not found");

			return UserRepo.GetNotesForActivity(this.UserID, activityID)
					.ToList();
		}

		public void SaveNote(User userSaving, Note note, int workAllocationID)
		{
			var wa = this.GetWorkAllocation(workAllocationID);
			if (wa == null)
				throw new NullReferenceException("WorkAllocation could not be found. Save cancelled.");

			var activity = ActivityRepo.GetByID(wa.ActivityID);
			if (activity == null)
				throw new NullReferenceException("Activity could not be found. Save cancelled.");

			if (userSaving.UserID != wa.UserID && !userSaving.CanManage(wa))
				throw new UserRoleException("You do not have the required permission to save a note for another user");

			note.WorkAllocationID = workAllocationID;

			note.LastModifiedBy = userSaving.FullName;
			UserRepo.SaveNote(note);
		}

		public List<NoteDetailModel> GetDetailedNotes(DateTime dateFrom, DateTime dateTo) => UserRepo.GetDetailedNotes(this.UserID, dateFrom.Date, dateTo.Date)
					.ToList();

		#endregion NOTES

		#region FLEXIBALANCE

		internal void SaveFlexiBalance(User userSaving, FlexiBalance flexiBalance)
		{
			if (userSaving.UserID != flexiBalance.UserID && !userSaving.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to run this process for other users. Action cancelled.");

			if (flexiBalance.UserID != this.UserID)
				throw new FlexiBalanceException("This FlexiBalance entry has been updated with illegal data. Action cancelled.");

			flexiBalance.Save();
		}

		private FlexiBalance GetFlexiBalancePriorToDate(DateTime date) =>
			//private method so no need to check the user requesting
			UserRepo.GetFlexiBalancePriorToDate(this.UserID, date);

		private void ResetInitialFlexiBalance(DateTime originalDate)
		{
			//Get the original flexi balance entry and delete
			if (!this.IsFlexiTimeUser)
				return;

			var newDate = this.EmployStartDate;
			if (newDate < EnabillSettings.SiteStartDate)
				newDate = EnabillSettings.SiteStartDate.Date;

			var newFlexiBalance = new FlexiBalance();

			var initialBalance = this.GetFlexiBalance(originalDate);
			if (initialBalance != null)
				FlexiBalanceRepo.Delete(initialBalance);

			//Create a new flexi balance entry
			newFlexiBalance = new FlexiBalance()
			{
				UserID = this.UserID,
				CalculatedAdjustment = 0,
				ManualAdjustment = 0, //this.FlexiBalanceTakeOn,
				BalanceDate = newDate,
				FinalBalance = this.FlexiBalanceTakeOn, //take on is an opening balance
				UpdatedBy = "System Operations",
				DateUpdated = null
			};

			newFlexiBalance.Save();
		}

		private void DeleteAllFlexiBalances() => UserRepo.DeleteFlexiBalances(this.UserID);

		#endregion FLEXIBALANCE

		#region FLEXITIME

		// Return the last date a Flexibalance was calculated. (If any)
		public DateTime GetMostRecentFlexiBalanceDate()
		{
			var fb = this.GetMostRecentFlexiBalance();
			if (fb != null)
			{
				return fb.BalanceDate;
			}

			// By default the startDate is the date we start the system or the date employed if it is after that
			var startDate = EnabillSettings.SiteStartDate;
			if (startDate < this.EmployStartDate)
				startDate = this.EmployStartDate;

			return startDate;
		}

		public double GetMostRecentFlexiBalanceHours()
		{
			var flexiBalance = this.GetMostRecentFlexiBalance();
			if (flexiBalance == null)
				return 0;

			return flexiBalance.FinalBalance;
		}

		private FlexiBalance GetMostRecentFlexiBalance() => this.GetMostRecentFlexiBalance(DateTime.Now);

		private FlexiBalance GetMostRecentFlexiBalance(DateTime date)
		{
			var flexiBalance = UserRepo.GetMostRecentFlexiBalance(this.UserID, date);
			if (flexiBalance == null)
				return null;
			if (flexiBalance.BalanceDate < EnabillSettings.SiteStartDate)
				return null;
			if (!this.IsFlexiTimeUser)
				flexiBalance.BalanceDate = DateTime.Today.AddDays(-1);

			return flexiBalance;
		}

		//Calculates the amount of flexitime for each day
		public double CalculateFlexiTimeForDay(DateTime date, bool bookTime = false)
		{
			double UserWorkSessions;
			double UserWorkHours = this.WorkHoursOnDate(date);

			if (bookTime)
				UserWorkSessions = this.GetWorkSessions(date).Sum(ws => ws.TotalTime) > 0 ? this.GetWorkSessions(date).Sum(ws => ws.TotalTime) : UserWorkHours;
			else
				UserWorkSessions = this.GetWorkSessions(date).Sum(ws => ws.TotalTime);

			return UserWorkSessions                                         // get my worksessions total time for the day. If no work sessions do not deduct flexitime as the day may still need to be filled in.
					- (WorkDay.IsDayWorkable(date) ? UserWorkHours : 0)     // minus the amount of hours I was supposed to work on that day
					+ this.GetFlexiBalanceAdjustmentValueForDate(date)      // add any flexibalance adjustments my manager may have made
					+ this.GetAllLeaveHoursOnDate(date);                      // and if leave was taken on this day, we add the number of hours I should have worked that day so that it isn't deducted from my flexitime.
		}

		private double GetLeaveHoursOnDate(DateTime date)
		{
			if (!this.IsLeaveTakenOnDate(date))
				return 0;
			var leave = UserRepo.GetLeaveForDate(this.UserID, ApprovalStatusType.Approved, date.Date);

			return leave.NumberOfHours ?? leave.GetNumberOfHoursForDate();
		}

		private double GetAllLeaveHoursOnDate(DateTime date)
		{
			if (!this.IsAnyLeaveTakenOnDate(date) || !date.IsWorkableDay())
				return 0;

			var leaves = UserRepo.GetLeaveForUserForDates(this.UserID, ApprovalStatusType.Approved, date.Date, date.Date).ToList();

			double leaveHours = 0;

			foreach (var leave in leaves)
				leaveHours += leave.NumberOfHours ?? leave.GetNumberOfHoursForDate();

			return leaveHours;
		}

		public List<Leave> GetLeaveHoursForPeriod(DateTime startDate, DateTime endDate) => UserRepo.GetLeaveForUserForDates(this.UserID, ApprovalStatusType.Approved, startDate, endDate).ToList();

		//Calculates the amount of flexitime for a period of days
		public double CalculateFlexiTimeForDateSpan(DateTime startDate, DateTime endDate)
		{
			double flexiHours = 0;
			Helpers.GetDaysInDateSpan(startDate, endDate)
					.ForEach(day => flexiHours += this.CalculateFlexiTimeForDay(day, true));

			return flexiHours;
		}

		public virtual double CalculateFlexiBalanceOnDate(DateTime date, bool includeDateForCalculation)
		{
			double flexiBalance = 0;
			var startDate = date;

			var fb = this.GetMostRecentFlexiBalance(date);
			if (fb == null)
				return 0;

			flexiBalance = fb.FinalBalance;
			startDate = fb.BalanceDate;

			if (date.IsInCurrentMonth())
				date = DateTime.Now.Date;
			else
				date = date.ToLastDayOfMonth();

			if (!includeDateForCalculation)
				date = date.AddDays(-1);

			flexiBalance += this.CalculateFlexiTimeForDateSpan(startDate, date);
			return flexiBalance;
		}

		//return the record in the database, not the calculated value
		private FlexiBalance GetFixedFlexiBalanceAtMonthStart(DateTime date) => this.GetFlexiBalance(date.ToFirstDayOfMonth());

		//return the record in the database, not the calculated value
		public FlexiBalance GetFlexiBalance(DateTime date) => UserRepo.GetFlexiBalance(this.UserID, date.Date);

		public List<FlexiBalance> GetFlexiBalancesForDateSpan(DateTime startDate, DateTime endDate) => UserRepo.GetFlexiBalancesForDateSpan(this.UserID, startDate.ToFirstDayOfMonth(), endDate.ToLastDayOfMonth())
					.ToList();

		public void RecalculateFlexiTime(int amtOfMonthsToRecalculate)
		{
			var startDate = DateTime.Today.ToFirstDayOfMonth().AddMonths(-1 * amtOfMonthsToRecalculate);
			if (startDate < EnabillSettings.SiteStartDate)
				startDate = EnabillSettings.SiteStartDate;
			if (startDate < this.EmployStartDate)
				startDate = this.EmployStartDate;

			double flexiHours = 0;

			var flexiBalance = this.GetFixedFlexiBalanceAtMonthStart(startDate);
			while (flexiBalance != null && startDate.AddDays(-1) >= this.EmployStartDate && startDate.AddDays(-1) >= EnabillSettings.SiteStartDate)
			{
				startDate = startDate.AddMonths(-1);
				flexiBalance = this.GetFixedFlexiBalanceAtMonthStart(startDate);
			}

			if (flexiBalance != null)
			{
				flexiHours = flexiBalance.FinalBalance;
			}

			this.CalculateFlexiTimeForDateSpan(startDate, DateTime.Today);
		}

		public double CalculateHoursRequiredForDateSpan(DateTime startDate, DateTime dateTo)
		{
			double hoursRequired = 0;

			foreach (var wd in WorkDayRepo.GetWorkableDays(true, startDate, dateTo).ToList())
			{
				hoursRequired += this.WorkHoursOnDate(wd.WorkDate);
			}

			return hoursRequired;
		}

		#endregion FLEXITIME

		#region FLEXIDAY

		public bool IsFlexiDayTakenOnDate(DateTime date) => this.GetFlexiDay(date.Date) != null;

		public bool IsFlexiDayTakenOnDatePending(DateTime date) => this.GetFlexiDayPending(date.Date) != null;

		public virtual FlexiDay BookFlexiDay(User userBooking, string remark, DateTime flexiDate)
		{
			flexiDate = flexiDate.Date;
			int flexiDayApprovalStatus;
			double workDayHoursBefore = UserRepo.GetWorkSessionsForDate(this.UserID, flexiDate).ToList().Sum(ws => ws.TotalTime);
			double flexiBalanceBefore = this.GetFlexiBalance(flexiDate.AddMonths(1).ToFirstDayOfMonth()) == null ? 0 : this.GetFlexiBalance(flexiDate.AddMonths(1).ToFirstDayOfMonth()).FinalBalance;

			if (this.UserID != userBooking.UserID && !userBooking.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to capture a FlexiDay for another user. Action cancelled.");

			if (string.IsNullOrEmpty(remark))
				throw new FlexiDayException("A remark is required. Action cancelled.");

			// Check if already booked
			var fDay = this.GetFlexiDay(flexiDate);
			if (fDay != null)
				return fDay;

			// No booking done on a Non-Working day
			if (!WorkDay.IsDayWorkable(flexiDate))
				return null;

			if (flexiDate < EnabillSettings.EarliestPossibleFlexiDate)
				throw new FlexiDayException("You cannot book a flexiday before " + EnabillSettings.EarliestPossibleFlexiDate.ToExceptionDisplayString());

			if (flexiDate >= EnabillSettings.LatestPossibleFlexiDate)
				throw new FlexiDayException("You cannot book a flexiday after " + EnabillSettings.LatestPossibleFlexiDate.ToExceptionDisplayString());

			if (this.IsLeaveTakenOnDate(flexiDate))
				throw new LeaveException("You already have approved leave on " + flexiDate.ToExceptionDisplayString() + ". Booking of FlexiDay cancelled.");

			if (this.GetApprovedFlexiDaysForDateSpan(flexiDate.ToFirstDayOfMonth(), flexiDate.ToLastDayOfMonth().Date).Count >= EnabillSettings.MaxFlexiDaysPerMonth)
			{
				flexiDayApprovalStatus = (int)ApprovalStatusType.Pending;
			}
			else
			{
				flexiDayApprovalStatus = (int)ApprovalStatusType.Approved;
			}

			var flexiDay = new FlexiDay()
			{
				UserID = this.UserID,
				Remark = remark,
				FlexiDate = flexiDate,
				DateSubmitted = DateTime.Now.ToCentralAfricanTime(),
				ApprovalStatusID = flexiDayApprovalStatus
			};

			flexiDay.LastModifiedBy = userBooking.FullName;
			UserRepo.SaveFlexiDay(flexiDay);
			this.CreateBalanceAuditTrail("FlexiDay", flexiDate, this, workDayHoursBefore, flexiBalanceBefore, userBooking, "booked");

			return flexiDay;
		}

		public bool IsFlexiDayAvailableForMonth(DateTime flexiDate) => this.GetFlexiDaysForDateSpan(flexiDate.ToFirstDayOfMonth(), flexiDate.ToLastDayOfMonth().Date).Count <= EnabillSettings.MaxFlexiDaysPerMonth;

		public FlexiDay GetFlexiDay(DateTime flexiDate) => UserRepo.GetFlexiDayForUserForDate(this.UserID, flexiDate.Date);

		public FlexiDay GetFlexiDayPending(DateTime flexiDate) => UserRepo.GetFlexiDayPending(this.UserID, flexiDate.Date);

		public virtual FlexiDay GetFlexiDay(int flexiDayID) => UserRepo.GetFlexiDayByID(this.UserID, flexiDayID);

		public List<FlexiDay> GetFlexiDay(DateTime startDate, DateTime endDate) => UserRepo.GetFlexiDayForUserForDateSpan(this.UserID, startDate.Date, endDate.Date)
					.ToList();

		public List<FlexiDay> GetFlexiDaysForDateSpan(DateTime startDate, DateTime endDate) => UserRepo.GetFlexiDayForUserForDateSpan(this.UserID, startDate, endDate)
						.OrderBy(fd => fd.FlexiDate)
						.ToList();

		public List<FlexiDay> GetApprovedFlexiDaysForDateSpan(DateTime startDate, DateTime endDate) => UserRepo.GetApprovedFlexiDayForUserForDateSpan(this.UserID, startDate, endDate)
						.OrderBy(fd => fd.FlexiDate)
						.ToList();

		public void RemoveFlexiDay(User userRemoving, int flexiDayID)
		{
			var flexiDay = UserRepo.GetFlexiDayByID(this.UserID, flexiDayID);
			var flexiDate = flexiDay.FlexiDate;
			double workDayHoursBefore = UserRepo.GetWorkSessionsForDate(this.UserID, flexiDate).ToList().Sum(ws => ws.TotalTime);
			double flexiBalanceBefore = this.GetFlexiBalance(flexiDate.AddMonths(1).ToFirstDayOfMonth()) == null ? 0 : this.GetFlexiBalance(flexiDate.AddMonths(1).ToFirstDayOfMonth()).FinalBalance;

			this.RemoveFlexiDay(userRemoving, flexiDay);
			this.CreateBalanceAuditTrail("FlexiDay", flexiDate, this, workDayHoursBefore, flexiBalanceBefore, userRemoving, "deleted");
		}

		public void RemoveFlexiDay(User userRemoving, FlexiDay flexiDay)
		{
			if (!userRemoving.CanManage(this) && this.UserID != userRemoving.UserID)
				throw new UserRoleException("You do not have the required permissions to delete another user's FlexiDays");

			var flexiDate = flexiDay.FlexiDate;
			double workDayHoursBefore = UserRepo.GetWorkSessionsForDate(this.UserID, flexiDate).ToList().Sum(ws => ws.TotalTime);
			double flexiBalanceBefore = this.GetFlexiBalance(flexiDate.AddMonths(1).ToFirstDayOfMonth()) == null ? 0 : this.GetFlexiBalance(flexiDate.AddMonths(1).ToFirstDayOfMonth()).FinalBalance;

			FlexiDayRepo.Delete(flexiDay);
			this.CreateBalanceAuditTrail("FlexiDay", flexiDate, this, workDayHoursBefore, flexiBalanceBefore, userRemoving, "deleted");
		}

		#endregion FLEXIDAY

		#region CREATE BALANCEAUDITTRAIL

		public void CreateBalanceAuditTrail(string type, DateTime workDay, User user, double workDayHoursBefore, double flexiBalanceBefore, User changedBy, string status)
		{
			//Create Audit trail entry if past workallocation changed
			double workDayHoursAfter = UserRepo.GetWorkSessionsForDate(user.UserID, workDay).ToList().Sum(ws => ws.TotalTime);
			bool workDayHoursChanged = workDayHoursBefore != workDayHoursAfter || type == "FlexiDay";
			double workDayHoursChangedBy = workDayHoursAfter - workDayHoursBefore;
			string changeSummary = type + " dated " + workDay.ToExceptionDisplayString() + " " + status + " by " + changedBy.FullName + " on " + DateTime.Today.ToExceptionDisplayString();

			//Only do this is changes is applied to a month other than the current
			int monthsToRecalc = DateTime.Today.Year != workDay.Year ? 12 - workDay.Month + DateTime.Today.Month : DateTime.Today.Month - 1;
			if (workDay.IsInPastMonth() && workDayHoursChanged)
			{
				var flexiBalanceAuditTrail = new BalanceAuditTrail
				{
					UserID = user.UserID,
					BalanceTypeID = (int)BalanceTypeEnum.Flexi,
					BalanceChangeTypeID = type == "Worksession" ? (int)BalanceChangeTypeEnum.WorkSession : (int)BalanceChangeTypeEnum.Flexi,
					BalanceDate = workDay.AddMonths(1).ToFirstDayOfMonth(),
					BalanceBefore = flexiBalanceBefore,
					ChangeSummary = changeSummary,
					ChangedBy = changedBy.UserID,
					DateChanged = DateTime.Today
				};
				UserRepo.GetByID(user.UserID).ExecuteFlexiBalanceLeaveBalanceProcess(changedBy, monthsToRecalc);
				flexiBalanceAuditTrail.BalanceAfter = user.GetFlexiBalance(flexiBalanceAuditTrail.BalanceDate).FinalBalance;
				flexiBalanceAuditTrail.HoursChanged = flexiBalanceAuditTrail.BalanceAfter - flexiBalanceAuditTrail.BalanceBefore;
				BalanceAuditTrailRepo.Save(flexiBalanceAuditTrail);
			}
		}

		#endregion CREATE BALANCEAUDITTRAIL

		#region FLEXITIMEADJUSTMENT

		private double GetFlexiBalanceAdjustmentValueForDate(DateTime date) => UserRepo.GetFlexiBalanceAdjustments(this.UserID, date, date)
					.Sum(fba => fba.Adjustment);

		public FlexiBalanceAdjustment GetFlexiBalanceAdjustmentForDate(DateTime date) => UserRepo.GetFlexiBalanceAdjustmentForDate(this.UserID, date);

		public virtual void AdjustFlexiBalance(User userAdjusting, double adjustmentAmount, string remark, DateTime? dateToImplement = null)
		{
			if (!userAdjusting.CanManage(this))
				throw new UserRoleException(string.Format($"You do not have the required permissions to {this.FullName}'s FlexiBalance."));

			if (adjustmentAmount == 0)
				return;

			if (adjustmentAmount % 0.25 != 0)
				throw new FlexiBalanceAdjustmentException("Adjustments to FlexiBalance must be made in multiples of 0.25 (15 minutes). Action cancelled.");

			if (!dateToImplement.HasValue)
			{
				dateToImplement = DateTime.Today.ToFirstDayOfMonth();

				if (this.EmployStartDate.Date >= DateTime.Today.ToFirstDayOfMonth())
					dateToImplement = this.EmployStartDate.Date;

				if (EnabillSettings.SiteStartDate > dateToImplement.Value)
					dateToImplement = EnabillSettings.SiteStartDate;
			}

			dateToImplement = dateToImplement.Value.Date;
			if (dateToImplement.Value < EnabillSettings.SiteStartDate)
				throw new EnabillSettingsException("The date onwhich to implement the FlexiBalance adjustment cannot be before the site start date: " + EnabillSettings.SiteStartDate.ToExceptionDisplayString());

			if (dateToImplement.Value < this.EmployStartDate)
				throw new EnabillSettingsException("The date onwhich to implement the FlexiBalance adjustment cannot be before the user's employment date: " + this.EmployStartDate.ToExceptionDisplayString());

			var fba = new FlexiBalanceAdjustment()
			{
				UserID = this.UserID,
				Adjustment = adjustmentAmount,
				UserAdjusted = userAdjusting.FullName,
				DateAdjusted = DateTime.Now.ToCentralAfricanTime(),
				ImplementationDate = dateToImplement.Value,
				LastModifiedBy = userAdjusting.FullName,
				Remark = remark
			};

			UserRepo.SaveFlexiBalanceAdjustment(fba);
			//Update the user time from implementation date till current
			this.ExecuteFlexiBalanceProcess(userAdjusting, fba.ImplementationDate);
		}

		public List<FlexiBalanceAdjustment> GetFlexiBalanceAdjustments(DateTime dateFrom, DateTime dateTo, bool includeAdjustmentOnDateTo)
		{
			if (!includeAdjustmentOnDateTo)
				dateTo = dateTo.AddDays(-1);

			if (dateFrom < EnabillSettings.SiteStartDate)
				dateFrom = EnabillSettings.SiteStartDate;

			if (dateFrom > dateTo)
				throw new EnabillSettingsException("A problem has been detected with the request dates. Please revise the dates, then try again.");

			return UserRepo.GetFlexiBalanceAdjustments(this.UserID, dateFrom, dateTo)
					.ToList();
		}

		public FlexiBalanceAdjustment GetFlexiBalanceManualAdjustmentByID(int id) => UserRepo.GetFlexiBalanceManualAdjustmentByID(id);

		#endregion FLEXITIMEADJUSTMENT

		#region LEAVE

		public bool IsLeaveTakenOnDate(DateTime date, LeaveTypeEnum leaveType = LeaveTypeEnum.Annual, ApprovalStatusType approvalStatus = ApprovalStatusType.Approved) => date.IsWorkableDay() && this.GetLeaveForDate(date, leaveType, approvalStatus) != null;

		internal Leave GetLeaveForDate(DateTime date, LeaveTypeEnum leaveType = LeaveTypeEnum.Annual, ApprovalStatusType approvalStatus = ApprovalStatusType.Approved) => UserRepo.GetLeaveForDate(this.UserID, leaveType, approvalStatus, date.Date);

		//The top two function only check for Annual leave
		//The time page should indicate any type of leave taken\approved
		public bool IsAnyLeaveTakenOnDate(DateTime date, ApprovalStatusType approvalStatus = ApprovalStatusType.Approved) => UserRepo.GetLeaveForUserForDates(this.UserID, approvalStatus, date, date).Any();

		public Leave GetAnyLeaveForDate(DateTime date, ApprovalStatusType approvalStatus = ApprovalStatusType.Approved) => UserRepo.GetLeaveForDate(this.UserID, approvalStatus, date.Date);

		private int GetLeaveCountForDateSpan(DateTime startDate, DateTime endDate)
		{
			int count = 0;
			foreach (var day in WorkDay.GetWorkableDays(true, startDate.Date, endDate.Date))
			{
				if (this.IsLeaveTakenOnDate(day.WorkDate))
					count++;
			}
			return count;
		}

		public Leave ApplyForLeave(User userRequesting, LeaveTypeEnum leaveType, DateTime startDate, DateTime endDate, int? leaveHours, string remark)
		{
			startDate = startDate.Date;
			endDate = endDate.Date;

			if (string.IsNullOrEmpty(remark))
				remark = " ";
			//    throw new LeaveException("A remark is required and was not specified. Please configure this, then try again.");

			if (startDate < EnabillSettings.SiteStartDate)
				throw new EnabillSettingsException(string.Format($"You cannot apply for leave for days before {EnabillSettings.SiteStartDate.ToExceptionDisplayString()}"));

			if (startDate < this.EmployStartDate)
				throw new UserManagementException("Leave cannot be applied for on dates before the user's emplyoment date. Action cancelled.");

			if (startDate > endDate)
				throw new LeaveException("The date from which the leave will be taken cannot be after the date to which the leave is take. Action cancelled.");

			ConfigureLeaveDates(ref startDate, ref endDate);

			//Partial Leave checks
			if (startDate == endDate)
			{
				if (!UserRepo.GetValidWorkDay(startDate))
					throw new LeaveException("Selected date is not a valid workday. Action cancelled.");

				if (leaveHours == 0) //single day of leave with no leave hours
					throw new LeaveException("You cannot apply for a single day's leave without specifying the amount of hours taken as leave. Action cancelled.");

				if (leaveHours > this.WorkHoursOnDate(startDate)) //single day of leave with too much leave hours
					throw new LeaveException("You cannot apply for a single day's leave without specifying the amount of hours taken as leave. Action cancelled.");

				if (UserRepo.IsDuplicateLeaveType(this.UserID, startDate, (int)leaveType)) //check that the same leave type is not being duplicated
					throw new LeaveException("You have already applied for this leave type. Action cancelled.");

				if (UserRepo.HoursExceeded(this.UserID, startDate, this.WorkHours, leaveHours)) //check combined total of hours does not exceed work hours
					throw new LeaveException("You cannot exceed the number of work hours.");

				//Birthday leave
				if (leaveType == LeaveTypeEnum.BirthDay)
				{
					if (!this.BirthDate.HasValue)
						throw new LeaveException("Your birthday first needs to be filled in before you can apply for birthday leave. Please contact HR.");

					if (startDate < new DateTime(2017, 6, 1, 0, 0, 0)) //Birthday leave only implement from June 1st, 2017
						throw new LeaveException("You cannot apply for birthday leave prior to June 1st, 2017. Action cancelled.");

					if (!UserRepo.GetValidBirthdayDate(startDate, this.BirthDate.Value))
						throw new LeaveException("Birthday leave needs to be on the workday before, day of, or workday after your birthdate. Action cancelled.");

					if (UserRepo.BirthdayPeriodAlreadyTaken(this.UserID, startDate, this.BirthDate.Value)) //Check if leave alreay applied for over this birthday period
						throw new LeaveException("You have already applied for birthday leave for this period. Action cancelled.");
				}
			}

			//Logic about existing Leave taken in this time frame..
			var leaveList = this.GetLeave(startDate, endDate);
			//if (leaveList.Count > 0)
			if (leaveList.Count > 0 && startDate != endDate && leaveList.Any(l => l.ApprovalStatus != (int)ApprovalStatusType.Withdrawn && l.ApprovalStatus != (int)ApprovalStatusType.Declined))
				throw new LeaveException("You cannot request to take leave that overlaps with previously requested leave. Action cancelled.");

			//Logic about existing FlexiDays taken in this time frame..
			if (this.GetFlexiDay(startDate, endDate).Count > 0)
				throw new FlexiDayException("You cannot request to take leave that overlaps with a previously requested FlexiDay. Action cancelled.");

			//Logic about insufficient days
			if (leaveType == LeaveTypeEnum.Annual || leaveType == LeaveTypeEnum.Sick)
			{
				bool daysAvailable = LeaveBalanceRepo.CheckAvailableDays(this.UserID, this.ExternalRef, (int)leaveType, startDate, endDate, false);
				if (!daysAvailable)
				{
					throw new LeaveException("There are insufficient days available, which includes any pending days.");
				}
			}

			double leaveDays = WorkDay.GetWorkableDays(true, startDate, endDate).Count;

			if (startDate.Date == endDate.Date)
			{
				//if leaveHours = null, then full leave day taken..
				if (!leaveHours.HasValue)
					leaveDays = 1.0;
				else
					leaveDays = leaveHours.Value / this.WorkHoursOnDate(endDate);

				if (leaveDays > 1)
					leaveDays = 1;
			}

			var leaveCycleList = new List<LeaveCycleBalance>();

			foreach (var day in WorkDay.GetWorkableDays(true, startDate.Date, endDate.Date))
			{
				var leaveCycleBalance = LeaveCycleBalanceRepo.GetLeaveCycleBalanceForUserDate(this.UserID, leaveType, day.WorkDate);
				//LeaveCycleBalance leaveCycleBalance = LeaveCycleBalanceRepo.GetLeaveCycleBalanceForUserPeriod(this.UserID, leaveType, day.WorkDate);
				if (leaveCycleBalance != null && !leaveCycleList.Any(lb => lb.LeaveCycleBalanceID == leaveCycleBalance.LeaveCycleBalanceID))
					leaveCycleList.Add(leaveCycleBalance);
			}

			var leave = new Leave()
			{
				UserID = this.UserID,
				Remark = remark,
				DateFrom = startDate,
				DateTo = endDate,
				LeaveType = (int)leaveType,
				DateRequested = DateTime.Now.ToCentralAfricanTime(),
				ApprovalStatus = (int)ApprovalStatusType.Pending,
				ManagedBy = null,
				DateManaged = null,
				NumberOfDays = leaveDays,
				NumberOfHours = leaveHours
			};

			//if the leave days being applied for is added to the closing balance is more than the days allowed, i.e. the opening balance.
			//sick leave cannot be more than 30 for example, so if you are applying for 5 days and you have used up 27 then it shouldn't happen

			if (leaveType == LeaveTypeEnum.Compassionate || leaveType == LeaveTypeEnum.Sick)
			{
				double takenDays = 0f;
				double openingBalance = 0f;
				double closingBalance = 0f;
				double manualAdjustment = 0f;

				if (leaveCycleList.Count > 0)
				{
					takenDays = leaveCycleList.Select(t => t.Taken).Sum();
					openingBalance = leaveCycleList.Select(t => t.OpeningBalance).Sum();
					manualAdjustment = leaveCycleList.Select(t => t.ManualAdjustment).Sum();
					closingBalance = openingBalance - (takenDays + manualAdjustment);
				}

				if ((leaveDays + takenDays + manualAdjustment) <= openingBalance)
				{
					leave.Save(userRequesting);
				}
				else
				{
					string leaveDayStr = leaveDays + " day(s)";
					string closingBalanceStr = string.Empty;
					double rounded = Math.Round(closingBalance, 2);

					//if (closingBalance == 0)
					//    closingBalanceStr = "In the current leave cycle, " + closingBalance + " days have already been taken leaving " + closingBalance + " days available.";
					//else
					closingBalanceStr = ". In the current leave cycle, " + (takenDays + manualAdjustment) + " day(s) have already been taken leaving " + rounded + " day(s) available.";

					throw new LeaveException("Unfortunately you cannot apply for " + leaveDayStr + " " + leaveType.GetDescription().ToLower().Replace("leave", "") + " leave. " + closingBalanceStr);
				}
			}
			else
			{
				leave.Save(userRequesting);
			}

			logger.Debug("APPLYING FOR LEAVE SECTION================================");
			logger.Debug("Leave application passed all required tests. Logging out input values before we return the created leave object");
			logger.Debug($"{userRequesting.FullName} is applied for leave");
			logger.Debug("Dumping out leave model that was saved");
			logger.Debug(leave.Dump());
			logger.Debug("==========================================================");

			return leave;
		}

		private static void ConfigureLeaveDates(ref DateTime startDate, ref DateTime endDate)
		{
			var listOfWorkableDays = WorkDay.GetWorkableDays(true, startDate, endDate).Select(w => w.WorkDate).ToList();
			if (listOfWorkableDays.Count == 0)
				throw new LeaveException("Your leave day/s must contain workable days. This leave request contains only weekend days or public holidays. Action cancelled.");

			while (!listOfWorkableDays.Contains(startDate))
				startDate = startDate.AddDays(1);

			while (!listOfWorkableDays.Contains(endDate))
				endDate = endDate.AddDays(-1);
		}

		public void DeclineLeaveRequest(User userManaging, int leaveID)
		{
			var leave = UserRepo.GetLeaveByIDForUser(this.UserID, leaveID);
			if (leave == null)
				throw new LeaveManagementException("The leave request could not be found. Action cancelled.");

			if (leave.ApprovalStatus == (int)ApprovalStatusType.Approved)
			{
				int monthsToRecalc = DateTime.Today.Year != leave.DateFrom.Year ? 12 - leave.DateFrom.Month + DateTime.Today.Month : DateTime.Today.Month - 1;
				this.ExecuteFlexiBalanceLeaveBalanceProcess(userManaging, monthsToRecalc);
			}

			this.DeclineLeaveRequest(userManaging, leave);
		}

		public void DeclineLeaveRequest(User userManaging, Leave leave) => leave.DeclineLeaveRequest(userManaging);

		public void ApproveLeaveRequest(User userManaging, int leaveID)
		{
			var leave = UserRepo.GetLeaveByIDForUser(this.UserID, leaveID);
			if (leave == null)
				throw new LeaveManagementException("The leave request could not be found. Action cancelled.");

			this.ApproveLeaveRequest(userManaging, leave);
		}

		public void ApproveLeaveRequest(User userManaging, Leave leave) => leave.ApproveLeaveRequest(userManaging);

		public void WithdrawLeave(User userWithdrawing, int leaveID)
		{
			var leave = UserRepo.GetLeaveByIDForUser(this.UserID, leaveID);

			if (leave == null)
			{
				throw new LeaveManagementException("The leave you wish to withdraw could not be found. Action cancelled.");
			}

			//if (leave.LeaveType == (int)Enabill.LeaveTypeEnum.Sick || leave.LeaveType == (int)Enabill.LeaveTypeEnum.Compassionate)
			//{
			//    if (leave.LeaveType != (int)Enabill.LeaveTypeEnum.Annual)
			//    {
			//        LeaveCycleBalance leaveCycleBalance = LeaveCycleBalanceRepo.GetLeaveCycleBalanceForUserDate(leave.UserID, (LeaveTypeEnum)leave.LeaveType, leave.DateFrom);

			//        leaveCycleBalance.Taken = leaveCycleBalance.Taken - leave.NumberOfDays;

			//        leaveCycleBalance.ClosingBalance = leaveCycleBalance.OpeningBalance + leaveCycleBalance.Taken;

			//        LeaveCycleBalanceRepo.Save(leaveCycleBalance);
			//    }
			//}

			this.WithdrawLeave(userWithdrawing, leave);
		}

		public void WithdrawLeave(User userWithdrawing, Leave leave)
		{
			if (leave.ApprovalStatus == (int)ApprovalStatusType.Withdrawn)
			{
				return;
			}

			if (userWithdrawing.UserID != this.UserID)
			{
				throw new LeaveManagementException("You cannot withdraw the leave request of another user. Action cancelled.");
			}

			if (this.UserID != leave.UserID)
			{
				throw new LeaveManagementException("You can not withdraw this leave request as it is not your leave request. Action cancelled.");
			}

			if (leave.ApprovalStatus != (int)ApprovalStatusType.Pending && leave.ApprovalStatus != (int)ApprovalStatusType.Approved)
			{
				throw new LeaveManagementException("This leave withdrawal could not be completed as it has already been managed and is not a pending request. Action cancelled.");
			}

			if (leave.ApprovalStatus == (int)ApprovalStatusType.Approved)
			{
				throw new LeaveManagementException("This leave withdrawal could not be completed as it has already been approved by your manager. Action has been cancelled. Kindly ask your manager to decline the previous leave request.");
			}

			var prevStatus = (ApprovalStatusType)leave.ApprovalStatus;

			leave.ApprovalStatus = (int)ApprovalStatusType.Withdrawn;
			leave.DateManaged = DateTime.Now.ToCentralAfricanTime();
			leave.ManagedBy = this.FullName;

			if (prevStatus == ApprovalStatusType.Approved)
			{
				//if (leave.LeaveType == (int)Enabill.LeaveTypeEnum.Sick || leave.LeaveType == (int)Enabill.LeaveTypeEnum.Compassionate)
				if (leave.LeaveType != (int)LeaveTypeEnum.Annual)
				{
					double taken = leave.NumberOfDays < 1 ? leave.NumberOfDays * -1 : -1;

					this.RecalculateLeaveCycleBalances(this.UserID, (LeaveTypeEnum)leave.LeaveType, leave.DateFrom, leave.DateTo, taken, userWithdrawing);
				}
				//Log an entry in the BalanceAuditTrail before updating the balance. This audit is only to highlight changes that has an impacted a previously calculated balance

				leave.CreateBalanceAuditTrail(leave, userWithdrawing, "withdrawn");
			}

			logger.Debug("WITHDRAWING LEAVE SECTION================================");
			logger.Debug("Leave withdrawal passed all required tests. Logging out the leave object");
			logger.Debug($"Leave: \n{leave.Dump()}");
			logger.Debug("==========================================================");

			leave.Save(this);
			leave.Delete();
		}

		public Leave UpdateLeave(User userUpdating, Leave leave, int? hours)
		{
			if (!userUpdating.CanManage(leave))
				throw new LeaveManagementException("You do not have the required permissions to update this leave entry. Action cancelled.");

			if (leave.ApprovalStatus != (int)ApprovalStatusType.Pending)
				throw new EnabillConsumerException("The details of this leave entry cannot be updated as it is not pending.");

			if (string.IsNullOrEmpty(leave.Remark))
				leave.Remark = string.Empty;

			if (leave.DateFrom < EnabillSettings.SiteStartDate)
				throw new EnabillSettingsException(string.Format($"You cannot apply for leave for days before {EnabillSettings.SiteStartDate.ToExceptionDisplayString()}"));

			if (leave.DateFrom < this.EmployStartDate)
				throw new UserManagementException("Leave cannot be applied for on dates before the user's emplyoment date. Action cancelled.");

			if (leave.DateFrom > leave.DateTo)
				throw new LeaveException("The date from which the leave will be taken cannot be after the date to whcih the leave is take. Action cancelled.");

			var listOfWorkableDays = WorkDay.GetWorkableDays(true, leave.DateFrom, leave.DateTo).Select(w => w.WorkDate).ToList();
			if (listOfWorkableDays.Count == 0)
				throw new LeaveException("Your leave day/s must contain workable days. This leave request contains only weekend days or public holidays. Action cancelled.");

			while (!listOfWorkableDays.Contains(leave.DateFrom))
				leave.DateFrom = leave.DateFrom.AddDays(1);

			while (!listOfWorkableDays.Contains(leave.DateTo))
				leave.DateTo = leave.DateTo.AddDays(-1);

			if (leave.DateFrom == leave.DateTo && hours == 0) //single day of leave with no leave hours
				throw new LeaveException("You cannot apply for a single day's leave without specifying the amount of hours taken as leave.");

			if (leave.DateFrom == leave.DateTo && hours > this.WorkHoursOnDate(leave.DateFrom)) //single day of leave with too much leave hours
				throw new LeaveException("You cannot apply for a single day's leave without specifying the amount of hours taken as leave.");

			//Logic about existing Leave taken in this time frame..
			var leaveList = this.GetLeave(leave.DateFrom, leave.DateTo).Where(l => l.LeaveID != leave.LeaveID).ToList();
			if (leaveList.Count > 0 && leaveList.Any(l => l.ApprovalStatus != (int)ApprovalStatusType.Withdrawn && l.ApprovalStatus != (int)ApprovalStatusType.Declined))
				throw new LeaveException("You cannot request to take leave that overlaps with previously requested leave.");

			//Logic about existing FlexiDays taken in this time frame..
			if (this.GetFlexiDay(leave.DateFrom, leave.DateTo).Count > 0)
				throw new FlexiDayException("You cannot request to take leave that overlaps with a previously requested FlexiDay.");

			double leaveDays = WorkDay.GetWorkableDays(true, leave.DateFrom, leave.DateTo).Count;

			if (leave.IsPartialDay)
				leaveDays = hours.Value / this.WorkHoursOnDate(leave.DateTo);

			leave.NumberOfDays = leaveDays;
			leave.NumberOfHours = hours;

			leave.Save(this);
			return leave;
		}

		public Leave GetLeave(int leaveID) => UserRepo.GetLeaveByIDForUser(this.UserID, leaveID);

		public List<Leave> GetLeave(DateTime dateFrom, DateTime dateTo)
		{
			dateFrom = dateFrom.Date;
			dateTo = dateTo.Date;
			if (dateFrom > dateTo)
			{
				var tempDate = dateFrom;
				dateFrom = dateTo;
				dateTo = tempDate;
			}

			return UserRepo.GetLeaveForUserForDates(this.UserID, dateFrom, dateTo)
					.ToList();
		}

		public List<Leave> GetLeave(LeaveTypeEnum leaveType, DateTime dateFrom, DateTime dateTo)
		{
			//if (this.UserID != userRequesting.UserID)
			//    if (!userRequesting.HasRole(UserRoleType.SystemAdministrator) && !(userRequesting.HasRole(UserRoleType.Manager) && userRequesting.UserID == this.ManagerID))
			//        throw new UserRoleException(string.Format($"You do not have the required permissions to view {this.FullName}'s leave."));

			dateFrom = dateFrom.Date;
			dateTo = dateTo.Date;
			if (dateFrom > dateTo)
			{
				var tempDate = dateFrom;
				dateFrom = dateTo;
				dateTo = tempDate;
			}

			return UserRepo.GetLeaveForUserForDates(this.UserID, leaveType, dateFrom, dateTo)
					.ToList();
		}

		public List<Leave> GetLeave(ApprovalStatusType approvalStatus, DateTime dateFrom, DateTime dateTo)
		{
			//if (this.UserID != userRequesting.UserID)
			//    if (!userRequesting.HasRole(UserRoleType.SystemAdministrator) && !(userRequesting.HasRole(UserRoleType.Manager) && userRequesting.UserID == this.ManagerID))
			//        throw new UserRoleException(string.Format($"You do not have the required permissions to view {this.FullName}'s leave."));

			dateFrom = dateFrom.Date;
			dateTo = dateTo.Date;
			if (dateFrom > dateTo)
			{
				var tempDate = dateFrom;
				dateFrom = dateTo;
				dateTo = tempDate;
			}

			return UserRepo.GetLeaveForUserForDates(this.UserID, approvalStatus, dateFrom, dateTo)
					.ToList();
		}

		public List<Leave> GetLeave(LeaveTypeEnum leaveType, ApprovalStatusType approvalStatus, DateTime dateFrom, DateTime dateTo)
		{
			//if (this.UserID != userRequesting.UserID)
			//    if (!userRequesting.HasRole(UserRoleType.SystemAdministrator) && !(userRequesting.HasRole(UserRoleType.Manager) && userRequesting.UserID == this.ManagerID))
			//        throw new UserRoleException(string.Format($"You do not have the required permissions to view {this.FullName}'s leave."));

			dateFrom = dateFrom.Date;
			dateTo = dateTo.Date;

			if (dateFrom > dateTo)
			{
				var tempDate = dateFrom;
				dateFrom = dateTo;
				dateTo = tempDate;
			}

			return UserRepo.GetLeaveForUserForDates(this.UserID, leaveType, approvalStatus, dateFrom, dateTo)
					.ToList();
		}

		public double GetLeaveTakenTotalInHoursForDateSpan(ApprovalStatusType approvalStatusType, DateTime startDate, DateTime endDate)
		{
			double model = 0;

			//bottleneck
			/*
			Enum.GetValues(typeof(LeaveTypeEnum))
					.Cast<LeaveTypeEnum>()
					.ToList()
					.ForEach(lt =>
							model += GetLeaveTakenTotalInHoursForDateSpan(lt, approvalStatusType, startDate, DateTime.Today)
					);
					*/

			foreach (var l in Enum.GetValues(typeof(LeaveTypeEnum)).Cast<LeaveTypeEnum>().ToList())
			{
				model += this.GetLeaveTakenTotalInHoursForDateSpan(l, approvalStatusType, startDate, endDate);
			}

			return model;
		}

		public double GetLeaveTakenTotalInHoursForDateSpan(LeaveTypeEnum leaveType, ApprovalStatusType approvalStatus, DateTime startDate, DateTime endDate)
		{
			double model = 0;

			//foreach (WorkDay day in WorkDayRepo.GetWorkableDays(true, startDate, endDate).ToList())

			foreach (var day in WorkDayRepo.GetWorkableDays(true, startDate, endDate).ToList())
			{
				var leave = UserRepo.GetLeaveForUserForDate(this.UserID, leaveType, approvalStatus, day.WorkDate);
				if (leave != null)
				{
					if (!leave.NumberOfHours.HasValue)
						model += this.WorkHoursOnDate(day.WorkDate);
					else
						model += leave.NumberOfHours.Value;
				}
			}

			return model;
		}

		public double GetLeaveTakenTotalInDaysForDateSpan(ApprovalStatusType approvalStatusType, DateTime startDate, DateTime endDate)
		{
			double model = 0;

			Enum.GetValues(typeof(LeaveTypeEnum))
					.Cast<LeaveTypeEnum>()
					.ToList()
					.ForEach(lt =>
							model += this.GetLeaveTakenTotalInDaysForDateSpan(lt, approvalStatusType, startDate, endDate)
					);

			return model;
		}

		public double GetLeaveTakenTotalInDaysForDateSpan(LeaveTypeEnum leaveType, ApprovalStatusType approvalStatus, DateTime startDate, DateTime endDate)
		{
			double model = 0;
			foreach (var day in WorkDayRepo.GetWorkableDays(true, startDate, endDate).ToList())
			{
				var leave = UserRepo.GetLeaveForUserForDate(this.UserID, leaveType, approvalStatus, day.WorkDate);
				if (leave != null)
				{
					if (leave.NumberOfHours.HasValue)
						model += leave.NumberOfDays;
					else
						model++;
				}
			}

			return model;
		}

		#endregion LEAVE

		#region LOGGING

		private LogItem logItem;

		[NotMapped]
		public LogItem LogItem { get => this.logItem; set => this.logItem = value; }

		#endregion LOGGING

		#region LEAVE BALANCE

		public double CalculateCurrentLeaveBalance(LeaveTypeEnum leaveType)
		{
			var date = DateTime.Today.ToFirstDayOfMonth();

			var leaveBalanceRecord = this.GetLeaveBalance(leaveType, date);
			if (leaveBalanceRecord == null)
				return 0.0009;

			return leaveBalanceRecord.Balance - this.GetLeave(leaveType, ApprovalStatusType.Approved, date, DateTime.Now).Sum(l => l.NumberOfDays);
		}

		public LeaveBalance GetLeaveBalance(LeaveTypeEnum leaveType)
		{
			var leaveBalance = UserRepo.GetLeaveBalance(this.UserID, leaveType);
			if (leaveBalance == null)
			{
				leaveBalance = new LeaveBalance()
				{
					UserID = this.UserID,
					LeaveCredited = 0,
					LeaveTaken = 0,
					Balance = 0,
					LeaveType = (int)leaveType,
					BalanceDate = DateTime.Today.ToFirstDayOfMonth()
				};
			}

			return leaveBalance;
		}

		public LeaveBalance GetLeaveBalance(LeaveTypeEnum leaveType, DateTime date) => UserRepo.GetLeaveBalanceForUserForDate(this.UserID, leaveType, date.Date);

		public LeaveManualAdjustment GetLeaveManualAdjustment(LeaveTypeEnum leaveType, DateTime date) => UserRepo.GetLeaveManualAdjustmentForUserForDate(this.UserID, leaveType, date.Date);

		public virtual LeaveManualAdjustment GetLeaveManualAdjustmentByID(int id) => UserRepo.GetLeaveManualAdjustmentByID(id);

		private void ResetInitialLeaveBalance(DateTime originalDate)
		{
			if (!this.IsLeaveUser)
				return;

			//Get the original leave balance entry and delete

			var newDate = this.EmployStartDate;
			if (newDate < EnabillSettings.SiteStartDate)
				newDate = EnabillSettings.SiteStartDate;

			var newLeaveBalance = new LeaveBalance();

			foreach (LeaveTypeEnum leaveType in Enum.GetValues(typeof(LeaveTypeEnum)))
			{
				var initialBalance = this.GetLeaveBalance(leaveType, originalDate);
				if (initialBalance != null)
					LeaveBalanceRepo.Delete(initialBalance);

				//Create a new leave balance entry
				if (leaveType == LeaveTypeEnum.Annual)
					newLeaveBalance = this.GetNewInitialLeaveBalance(this.AnnualLeaveTakeOn, newDate);
				else
					newLeaveBalance = this.GetNewInitialLeaveBalance(0, newDate, leaveType);

				newLeaveBalance.Save();
			}
		}

		private LeaveBalance GetNewInitialLeaveBalance(double leaveAmount, DateTime implementationDate, LeaveTypeEnum leaveType = LeaveTypeEnum.Annual) => new LeaveBalance()
		{
			UserID = this.UserID,
			Balance = leaveAmount,
			BalanceDate = implementationDate,
			LeaveType = (int)leaveType,
			LeaveCredited = leaveAmount,
			LeaveTaken = 0
		};

		private void DeleteAllLeaveBalances() => UserRepo.DeleteLeaveBalances(this.UserID);

		#endregion LEAVE BALANCE

		#region LEAVE CYCLE BALANCE

		public void CreateInitialLeaveCycles(User user)
		{
			if (user == null)
				throw new ArgumentNullException(nameof(user));

			//Create Sick leave cycle
			LeaveCycleBalanceRepo.Save(new LeaveCycleBalance()
			{
				UserID = this.UserID,
				LeaveTypeID = (int)LeaveTypeEnum.Sick,
				StartDate = this.EmployStartDate,
				EndDate = this.EmployStartDate.AddYears(3).AddDays(-1),
				OpeningBalance = 30,
				Taken = 0,
				ClosingBalance = 30,
				Active = 1,
				LastUpdatedDate = DateTime.Now
			});

			//Create Compassion leave cycle
			LeaveCycleBalanceRepo.Save(new LeaveCycleBalance()
			{
				UserID = this.UserID,
				LeaveTypeID = (int)LeaveTypeEnum.Compassionate,
				StartDate = this.EmployStartDate,
				EndDate = this.EmployStartDate.AddYears(1).AddDays(-1),
				OpeningBalance = 3,
				Taken = 0,
				ClosingBalance = 3,
				Active = 1,
				LastUpdatedDate = DateTime.Now
			});
		}

		public List<LeaveCycleBalance> LeaveCycleWhereLimitReached(LeaveTypeEnum leaveType, DateTime startDate, DateTime endDate, double taken)
		{
			var leaveCycleList = new List<LeaveCycleBalance>();
			LeaveCycleBalance previousLeaveCycleBalance = null;
			double bookedDays = 0;
			var dummyLeaveCycle = new LeaveCycleBalance();

			foreach (var day in WorkDay.GetWorkableDays(true, startDate.Date, endDate.Date))
			{
				var leaveCycleBalance = LeaveCycleBalanceRepo.GetLeaveCycleBalanceForUserDate(this.UserID, leaveType, day.WorkDate);
				//LeaveCycleBalance leaveCycleBalance = LeaveCycleBalanceRepo.GetLeaveCycleBalanceForUserPeriod(this.UserID, leaveType, day.WorkDate);

				if (leaveCycleBalance == null)
				{
					//if no leave cycle exist make one for display purposes in email if needed
					//Not to be saved to the DB!
					var lastLeaveCycleBalance = LeaveCycleBalanceRepo.GetLastLeaveCycleBalanceForUser(this.UserID, leaveType);
					leaveCycleBalance = dummyLeaveCycle;
					leaveCycleBalance.LeaveCycleBalanceID = 0;
					leaveCycleBalance.StartDate = leaveType == LeaveTypeEnum.Sick ? lastLeaveCycleBalance.EndDate.AddDays(1) : new DateTime(day.WorkDate.Year, 01, 01);
					leaveCycleBalance.EndDate = leaveType == LeaveTypeEnum.Sick ? leaveCycleBalance.StartDate.AddYears(3).AddDays(-1) : leaveCycleBalance.StartDate.AddYears(1).AddDays(-1);
					leaveCycleBalance.ClosingBalance = leaveType == LeaveTypeEnum.Sick ? 30 : 3;
					leaveCycleBalance.Active = 0;
					leaveCycleBalance.OpeningBalance = leaveType == LeaveTypeEnum.Sick ? 30 : 3;
					leaveCycleBalance.LastUpdatedDate = DateTime.Now;
					leaveCycleBalance.LeaveTypeID = (int)leaveType;
					leaveCycleBalance.Taken = taken;
				}

				if (previousLeaveCycleBalance == null || leaveCycleBalance.LeaveCycleBalanceID != previousLeaveCycleBalance.LeaveCycleBalanceID)
					bookedDays = taken;
				else
					bookedDays += taken;

				leaveCycleBalance.Taken = bookedDays;

				if (leaveCycleBalance.ClosingBalance - bookedDays < 0)
				{
					//removing it first to ensure that only one entry exist per unique leave balance with the correct Taken value
					if (leaveCycleList.Contains(leaveCycleBalance))
						leaveCycleList.Remove(leaveCycleBalance);

					leaveCycleList.Add(leaveCycleBalance);
				}

				previousLeaveCycleBalance = leaveCycleBalance;
			}

			return leaveCycleList;
		}

		#endregion LEAVE CYCLE BALANCE

		#region FEEDBACK

		public FeedbackThread CreateNewThread(int feedbackTypeID, int feedbackUrgencyTypeID, string threadSubject, string feedbackPostText)
		{
			if (string.IsNullOrEmpty(threadSubject))
				throw new FeedbackException("The subject on the thread is required.");

			if (string.IsNullOrEmpty(feedbackPostText))
				throw new FeedbackException("You cannot submit a blank post.");

			//using (TransactionScope ts = new TransactionScope())
			//{
			var thread = new FeedbackThread()
			{
				FeedbackTypeID = feedbackTypeID,
				FeedbackUrgencyTypeID = feedbackUrgencyTypeID,
				FeedbackSubject = threadSubject
			};

			FeedbackThreadRepo.Save(thread);

			var post = new FeedbackPost()
			{
				FeedbackThreadID = thread.FeedbackThreadID,
				DateAdded = DateTime.Now.ToCentralAfricanTime(),
				PostText = feedbackPostText,
				UserID = this.UserID
			};

			FeedbackPostRepo.Save(post);

			// ts.Complete();

			return thread;
			//}
		}

		#endregion FEEDBACK

		#region USER COST TO COMPANY SPECIFIC

		public void SetUserCostToCompanyForMonth(string password, DateTime monthDate) => this.SetUserCostToCompanyForMonth(password, monthDate.ToPeriod(), 0);

		public void SetUserCostToCompanyForMonth(string passphrase, int period, double costToCompanyAmount)
		{
			if (!Helpers.ConfirmPassphraseIsValid(passphrase))
				throw new EnabillPassphraseException("Invalid passphrase.");

			var userCostToCompany = this.GetUserCostToCompanyForMonth(period, true);

			userCostToCompany._decCostToCompany = costToCompanyAmount;
			userCostToCompany.Save(passphrase);
		}

		internal UserCostToCompany GetUserCostToCompanyForMonth(int period, bool returnNewModelIfNotExists = false)
		{
			var uctc = UserRepo.GetUserCostToCompanyForMonth(this.UserID, period);

			if (uctc != null)
				return uctc;

			if (!returnNewModelIfNotExists)
				return null;

			return UserCostToCompany.GetNewForUserForMonth(this.UserID, period);
		}

		#endregion USER COST TO COMPANY SPECIFIC

		#region PROCESSES

		public void RecalculateLeaveCycleBalances(int userID, LeaveTypeEnum leaveType, DateTime dateFrom, DateTime dateTo, double taken, User userManaging)
		{
			var lcb = new List<LeaveCycleBalance>();
			logger.Debug(" ");
			logger.Debug($"RecalculateLeaveCycleBalances called. Call stack: \n{Environment.StackTrace}");
			logger.Debug("Logging input parameters");
			logger.Debug($"User ID: {userID}");
			logger.Debug($"Leave Type: {leaveType}");
			logger.Debug($"Date From: {dateFrom}");
			logger.Debug($"Date To: {dateTo}");
			logger.Debug($"Taken: {taken}");
			logger.Debug($"User Managing: {userManaging.FullName}");
			logger.Debug("===============================================");

			var leaveCycleBalance = LeaveCycleBalanceRepo.GetLeaveCycleBalanceForUserPeriod(userID, leaveType, dateFrom);

			if (leaveCycleBalance != null)
			{
				logger.Debug($"LeaveCycleBalance model BEFORE update: {leaveCycleBalance.Dump()}");

				//leaveCycleBalance.Taken = leaveCycleBalance.Taken + taken;
				leaveCycleBalance.ClosingBalance = leaveCycleBalance.OpeningBalance - leaveCycleBalance.Taken;

				logger.Debug($"Leave taken value: {taken}");
				logger.Debug($"LeaveCycleBalance model AFTER update: {leaveCycleBalance.Dump()}");
				logger.Debug("===============================================");

				LeaveCycleBalanceRepo.Save(leaveCycleBalance);
			}

			foreach (var day in WorkDayRepo.GetWorkableDays(true, dateFrom, dateTo).ToList())
			{
				leaveCycleBalance = LeaveCycleBalanceRepo.GetLeaveCycleBalanceForUserDate(userID, leaveType, day.WorkDate);
				if (leaveCycleBalance == null)
				{
					leaveCycleBalance = LeaveCycleBalanceRepo.GetLastLeaveCycleBalanceForUser(userID, leaveType);

					if (leaveCycleBalance != null)
					{
						//Fredrik: okay this makes sense now. If no leave cycle balance existed then check for a previous one and set it to inactive and then create a new one
						//Fredrik: doing the check here
						leaveCycleBalance.Active = DateTime.Today >= leaveCycleBalance.StartDate && DateTime.Today <= leaveCycleBalance.EndDate ? 1 : 0;

						logger.Debug($"OLD LeaveCycleBalance model: {leaveCycleBalance.Dump()}");
						LeaveCycleBalanceRepo.Save(leaveCycleBalance);

						//Fredrik: creating a new leave cycle balance
						var newLeaveCycleBalance = new LeaveCycleBalance
						{
							UserID = leaveCycleBalance.UserID,
							LeaveTypeID = leaveCycleBalance.LeaveTypeID
						};
						newLeaveCycleBalance.StartDate = newLeaveCycleBalance.LeaveTypeID == (int)LeaveTypeEnum.Sick ? leaveCycleBalance.EndDate.AddDays(1) : new DateTime(day.WorkDate.Year, 01, 01);
						newLeaveCycleBalance.EndDate = newLeaveCycleBalance.LeaveTypeID == (int)LeaveTypeEnum.Sick ? newLeaveCycleBalance.StartDate.AddYears(3).AddDays(-1) : newLeaveCycleBalance.StartDate.AddYears(1).AddDays(-1);
						newLeaveCycleBalance.OpeningBalance = newLeaveCycleBalance.LeaveTypeID == (int)LeaveTypeEnum.Sick ? 30 : 3;
						newLeaveCycleBalance.Taken = taken;
						newLeaveCycleBalance.ClosingBalance = newLeaveCycleBalance.OpeningBalance - newLeaveCycleBalance.Taken;
						newLeaveCycleBalance.Active = DateTime.Today >= newLeaveCycleBalance.StartDate && DateTime.Today <= newLeaveCycleBalance.EndDate ? 1 : 0;
						newLeaveCycleBalance.LastUpdatedDate = DateTime.Now;

						logger.Debug($"NEW LeaveCycleBalance model: {leaveCycleBalance.Dump()}");
						logger.Debug("===============================================");

						LeaveCycleBalanceRepo.Save(newLeaveCycleBalance);
						lcb.Add(newLeaveCycleBalance);
					}
				}
			}

			if (leaveCycleBalance != null && !lcb.Any(lb => lb.LeaveCycleBalanceID == leaveCycleBalance.LeaveCycleBalanceID))
				lcb.Add(leaveCycleBalance);

			//}
			//use current leave object's datefrom and dateto to get LCB objects - based on LCB range
			//then use date range in LCBs to get range of ManualAdjustments and use ManualAdjustments to sum and add to LCB
			//update LCB balance to reflect manual adjustment plus taken
			foreach (var lb in lcb)
			{
				this.UpdateLeaveCycleBalanceManualAdjustment(lb, userID, leaveType);
			}
		}

		public void UpdateLeaveCycleBalanceManualAdjustment(LeaveManualAdjustment leaveManualAdjustment)
		{
			//LeaveCycleBalance leaveCycleBalance = LeaveCycleBalanceRepo.GetLeaveCycleBalanceForUserDate(UserID, (LeaveTypeEnum)leaveManualAdjustment.LeaveTypeID, leaveManualAdjustment.EffectiveDate);
			var leaveCycleBalance = LeaveCycleBalanceRepo.GetLeaveCycleBalanceForUserPeriod(this.UserID, (LeaveTypeEnum)leaveManualAdjustment.LeaveTypeID, leaveManualAdjustment.EffectiveDate);
			this.UpdateLeaveCycleBalanceManualAdjustment(leaveCycleBalance, this.UserID, (LeaveTypeEnum)leaveManualAdjustment.LeaveTypeID);
		}

		public void UpdateLeaveCycleBalanceManualAdjustment(LeaveCycleBalance leaveCycleBalance, int userID, LeaveTypeEnum leaveType)
		{
			if (leaveCycleBalance != null)
			{
				var manualAdjustments = UserRepo.GetLeaveManualAdjustmentForUserForDateRange(userID, leaveType, leaveCycleBalance.StartDate, leaveCycleBalance.EndDate);

				double manualAdjustmentTotal = 0.0;
				foreach (var m in manualAdjustments)
				{
					manualAdjustmentTotal += m.ManualAdjustment;
				}
				double total = leaveCycleBalance.OpeningBalance - (leaveCycleBalance.Taken + manualAdjustmentTotal);
				leaveCycleBalance.ManualAdjustment = manualAdjustmentTotal;
				leaveCycleBalance.ClosingBalance = total;

				LeaveCycleBalanceRepo.Save(leaveCycleBalance);
			}
		}

		public void RecalculateLeaveAndFlexiBalances(User userSaving, User originalUser, int numberOfMonths)
		{
			//if the employment date is changed
			//if (this.EmployStartDate != originalUser.EmployStartDate || this.AnnualLeaveTakeOn != originalUser.AnnualLeaveTakeOn || this.FlexiBalanceTakeOn != originalUser.FlexiBalanceTakeOn)
			//{
			if (!this.IsActive)
			{
				return;
			}

			if (this.UserID > 0)
			{
				//this.DeleteAllLeaveBalances();
				//this.DeleteAllFlexiBalances();
				this.ResetInitialLeaveBalance(originalUser.EmployStartDate);//changes need to be made here !!! the original user ends up replicating the current user, so a select on the empstartdate doesnt return the original start date.
				this.ResetInitialFlexiBalance(originalUser.EmployStartDate);
			}
			this.ExecuteFlexiBalanceLeaveBalanceProcess(userSaving, numberOfMonths);
			//}
		}

		public void ExecuteFlexiBalanceLeaveBalanceProcess(User userExecuting, int numberOfMonths)
		{
			if (!this.IsActive)
				return;

			numberOfMonths = Math.Abs(numberOfMonths) * -1;

			var startDate = DateTime.Today.ToFirstDayOfMonth().AddMonths(numberOfMonths);
			startDate = this.ConfigureDate(startDate);

			this.UpdateUserHistoryRecords(startDate);
			this.UpdateLeaveRecordValues(userExecuting, startDate);
			this.ExecuteLeaveBalanceProcess(userExecuting, startDate);
			this.ExecuteFlexiBalanceProcess(userExecuting, startDate);
		}

		public void ExecuteFlexiBalanceProcess(User userExecuting, DateTime startDate)
		{
			if (!this.IsFlexiTimeUser)
				return;

			if (startDate.IsInFutureMonth())
				return;

			double origFlexiBalance = 0;
			double startOfMonthFlexiBalance = 0;
			double rollingFlexiBalance = 0;

			//if (startDate.IsInSameMonth(this.EmployStartDate))
			//    startDate = this.EmployStartDate.AddMonths();

			var flexiBalance = this.GetFlexiBalance(startDate) ?? this.GetFlexiBalancePriorToDate(startDate);

			if (flexiBalance != null)
			{
				rollingFlexiBalance = startOfMonthFlexiBalance = origFlexiBalance = flexiBalance.FinalBalance;
				startDate = flexiBalance.BalanceDate;
			}

			var flexiBalanceForMonth = flexiBalance;

			double openingBal = 0;
			for (var date = startDate; date < DateTime.Today.ToFirstDayOfMonth(); date = date.AddDays(1))
			{
				rollingFlexiBalance += this.CalculateFlexiTimeForDay(date);
				if (date.IsLastDayOfMonth())
				{
					//finish up the current month's record
					flexiBalanceForMonth.CalculatedAdjustment = rollingFlexiBalance - startOfMonthFlexiBalance;
					flexiBalanceForMonth.ManualAdjustment = this.GetFlexiBalanceAdjustments(date.ToFirstDayOfMonth(), date.ToLastDayOfMonth(), true).Sum(fba => fba.Adjustment);
					flexiBalanceForMonth.Save();
					//get opening bal for next month
					openingBal = flexiBalanceForMonth.FinalBalance + flexiBalanceForMonth.CalculatedAdjustment; // +flexiBalanceForMonth.ManualAdjustment;

					//setup the next month's record
					flexiBalanceForMonth = this.GetFlexiBalance(date.AddDays(1));

					if (flexiBalanceForMonth == null)
					{
						flexiBalanceForMonth = new FlexiBalance
						{
							UserID = this.UserID
						};
					}
					else
					{
						flexiBalanceForMonth.UpdatedBy = userExecuting == null ? "System" : userExecuting.FullName;
						flexiBalanceForMonth.DateUpdated = DateTime.Today.ToCentralAfricanTime();
					}

					flexiBalanceForMonth.FinalBalance = openingBal; //set the opening balance
					flexiBalanceForMonth.BalanceDate = date.AddDays(1);
					flexiBalanceForMonth.Save();

					startOfMonthFlexiBalance = rollingFlexiBalance;
				}
			}
		}

		private void ExecuteLeaveBalanceProcess(User userExecuting, DateTime startDate)
		{
			if (!this.IsLeaveUser)
			{
				return;
			}

			if (startDate.IsInFutureMonth())
			{
				return;
			}

			foreach (LeaveTypeEnum leaveType in Enum.GetValues(typeof(LeaveTypeEnum)))
			{
				double startOfMonthLeaveBalance = 0;

				if (startDate != this.EmployStartDate)
				{
					startDate = startDate.ToFirstDayOfMonth();
				}

				var leaveB = this.GetLeaveBalance(leaveType, startDate) ?? this.GetLeaveBalancePriorToDate(leaveType, startDate.AddMonths(-1));

				if (leaveB != null)
				{
					startOfMonthLeaveBalance = leaveB.Balance;
					startDate = leaveB.BalanceDate;
				}

				#region OLD

				//for (DateTime date = startDate.ToFirstDayOfMonth().AddMonths(1); date <= DateTime.Today.ToFirstDayOfMonth(); date = date.AddMonths(1))
				//{
				//    LeaveBalance lB = LeaveBalance.UpdateLeaveBalance(this, leaveType, date, startOfMonthLeaveBalance);

				//    //startOfMonthLeaveBalance is only used for Annual leave, others dont carry a monthly balance
				//    startOfMonthLeaveBalance = lB.Balance;
				//}

				#endregion OLD

				for (var date = startDate.ToFirstDayOfMonth(); date <= DateTime.Today.ToFirstDayOfMonth(); date = date.AddMonths(1))
				{
					var lB = LeaveBalance.UpdateLeaveBalance(this, leaveType, startDate, date, startOfMonthLeaveBalance);

					//startOfMonthLeaveBalance is only used for Annual leave, others dont carry a monthly balance
					startOfMonthLeaveBalance = lB.Balance + lB.LeaveCredited - lB.LeaveTaken;
				}
			}
		}

		private void UpdateLeaveRecordValues(User userUpdating, DateTime startDate)
		{
			var leaveRecords = this.GetLeave(startDate, DateTime.Today.ToLastDayOfMonth());
			leaveRecords.ForEach(l => l.ConfigureLeaveValues(userUpdating, this));
		}

		internal double GetLeaveBalanceCreditAmount(DateTime date)
		{
			double annualLeaveDaysGivenMonthly = EnabillSettings.AnnualLeaveAvailableToStaff / 12.00; //num of months in year
			if (date < EnabillSettings.SiteStartDate)
			{
				return 0;
			}

			//if we're getting the credit for the current month, but the month is not done yet, return zero
			if (date.IsInCurrentMonth())
			{
				return 0;
			}

			if (date < this.EmployStartDate)
			{
				if (date.ToPeriod() != this.EmployStartDate.ToPeriod())
				{
					return 0;
				}
				if (date.ToPeriod() == this.EmployStartDate.ToPeriod())
				{
					date = this.EmployStartDate;
				}
			}
			var workableDays = WorkDay.GetWorkableDays(true, date.ToFirstDayOfMonth(), date.ToLastDayOfMonth());
			int workableDaysInMonth = workableDays.Count;

			var toDate = date.ToLastDayOfMonth();

			int userWorkableDaysInMonth = WorkDay.GetWorkableDays(true, date, toDate).Count;

			var leaveDays = this.GetLeave(LeaveTypeEnum.Unpaid, ApprovalStatusType.Approved, date, date.ToLastDayOfMonth());
			var actualUnpaidLeaveDays = new List<DateTime>();
			//fredrik erasmus - unpaid leave days are not the sum of the NumberOfDays property, its a count of days.
			foreach (var l in leaveDays)
			{
				foreach (var dt in Helpers.EachDay(l.DateFrom, l.DateTo))
				{
					if (workableDays.Any(wd => wd.WorkDate.Date == dt.Date))
					{
						actualUnpaidLeaveDays.Add(dt);
					}
				}
			}

			//double leaveDaysTakenAsUnpaid = leaveDays.Sum(x => x.NumberOfDays);

			double leaveDaysTakenAsUnpaid = actualUnpaidLeaveDays.Count;

			//if(leaveDaysTakenAsUnpaid > userWorkableDaysInMonth)
			//{
			//    leaveDaysTakenAsUnpaid = userWorkableDaysInMonth;
			//}

			//cannot divide by 0!

			//Code commented out below applies to where leave is proportionat to how much unpaid leave has been taken.
			//Currently employees get full leave in spite of any unpaid leave taken. This may change so leave commented code in.
			///*
			double multiplier = 0;

			if (workableDaysInMonth > 0)
				multiplier = (userWorkableDaysInMonth - leaveDaysTakenAsUnpaid) / workableDaysInMonth;

			double result = annualLeaveDaysGivenMonthly * multiplier;

			return Math.Round(result, 2);
		}

		public double GetLeaveBalanceCreditAmountProvisional(DateTime date)
		{
			//the provisional leave credit amount will be used in the Leave Balance Report (Annual) to give
			// the user an idea of what the closing balance will be at the end of the selected month
			//usefull if current month as closing balance would not have been created yet

			double annualLeaveDaysGivenMonthly = GetLeaveDaysCredit(this.ExternalRef);

			if (date < EnabillSettings.SiteStartDate)
				return 0;

			if (date < this.EmployStartDate)
			{
				if (date.ToPeriod() != this.EmployStartDate.ToPeriod())
					return 0;
				if (date.ToPeriod() == this.EmployStartDate.ToPeriod())
					date = this.EmployStartDate;
			}

			int workableDaysInMonth = WorkDay.GetWorkableDays(true, date.ToFirstDayOfMonth(), date.ToLastDayOfMonth()).Count;
			int userWorkableDaysInMonth = WorkDay.GetWorkableDays(true, date, date.ToLastDayOfMonth()).Count;
			double leaveDaysTakenAsUnpaid = this.GetLeave(LeaveTypeEnum.Unpaid, ApprovalStatusType.Approved, date, date.ToLastDayOfMonth()).Sum(x => x.NumberOfDays);
			double leaveDaysTakenAsUnpaidPending = this.GetLeave(LeaveTypeEnum.Unpaid, ApprovalStatusType.Pending, date, date.ToLastDayOfMonth()).Sum(x => x.NumberOfDays);
			double multiplier = (userWorkableDaysInMonth - (leaveDaysTakenAsUnpaid + leaveDaysTakenAsUnpaidPending)) / workableDaysInMonth;

			double result = annualLeaveDaysGivenMonthly * multiplier;

			return Math.Round(result, 2);
		}

		public LeaveBalance GetLeaveBalancePriorToDate(LeaveTypeEnum leaveType, DateTime date) => LeaveBalanceRepo.GetLeaveBalancePriorToDate(this.UserID, leaveType, date);

		public static double GetLeaveDaysCredit(string externalRef)
		{
			double result;
			const double numOfMonthsInYear = 12.00;

			if (externalRef == Code.Constants.COMPANYNAME)
				result = 20 / numOfMonthsInYear; //num of months in year
			else
				result = EnabillSettings.AnnualLeaveAvailableToStaff / numOfMonthsInYear; //num of months in year

			return Math.Truncate(100 * result) / 100;
		}

		#endregion PROCESSES
	}
}