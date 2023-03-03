using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Transactions;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("WorkAllocations")]
	public class WorkAllocation
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int WorkAllocationID { get; set; }

		[Required]
		public int ActivityID { get; set; }

		[Required]
		public int Period { get; set; }

		[Required, EnumDataType(typeof(TrainingCategoryType))]
		public int TrainingCategoryID { get; set; }

		[Required]
		public int UserID { get; set; }

		[Required, EnumDataType(typeof(WorkAllocationType))]
		public int WorkAllocationType { get; set; }

		public int? InvoiceID { get; internal set; }

		//public int? InvoiceLineID { get; internal set; }
		public int? ParentWorkAllocationID { get; internal set; }

		[NotMapped]
		public double AmountExcl { get; private set; }

		[Required]
		public double HoursWorked { get; set; }

		[NotMapped]
		public double TotalHours => this.HoursBilled ?? this.HoursWorked;

		[NotMapped]
		public double TotalValue => (this.HoursBilled ?? this.HoursWorked) * (this.HourlyRate ?? 0.0D);

		public double? HoursBilled { get; set; }
		public double? HourlyRate { get; set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		[MaxLength(200)]
		public string Remark { get; set; }

		[MaxLength(128)]
		public string UserCreated { get; internal set; }

		public string TicketReference { get; set; }
		public string TrainerName { get; set; }
		public string TrainingInstitute { get; set; }

		[MinLength(2), MaxLength(128)]
		public string UserModified { get; internal set; }

		[Required]
		public DateTime DayWorked { get; set; }

		[Required]
		public DateTime DateCreated { get; internal set; }

		public DateTime? DateModified { get; internal set; }

		public Activity Activity { get; set; }

		public TrainingCategory TrainingCategory { get; set; }

		#endregion PROPERTIES

		#region INITIALIZATION

		[NotMapped]
		public bool BelongsToInvoice => this.InvoiceID.HasValue;

		[NotMapped]
		public bool CanEdit => !this.BelongsToInvoice || (this.GetInvoice().InvoiceStatusID == (int)InvoiceStatusType.Open);

		[NotMapped]
		public bool CanEditForInvoicing
		{
			get
			{
				var invoice = this.GetInvoice();
				if (invoice.IsOpen || invoice.IsInProgress)
					return true;

				return this.CanEdit;
			}
		}

		[NotMapped]
		public bool CanBeManagedForInvoice
		{
			get
			{
				if (this.BelongsToInvoice)
				{
					var inv = this.GetInvoice();
					if (inv.IsComplete || inv.IsReady)
						return false;
				}

				//TODO check if timesheet is locked

				return true;
			}
		}

		[NotMapped]
		public bool CanDelete => !this.BelongsToInvoice || (this.GetInvoice().InvoiceStatusID == (int)InvoiceStatusType.Open);

		[NotMapped]
		public double GrossAmount => (this.HoursBilled ?? this.HoursWorked) * this.HourlyRate ?? 0.0D;

		[NotMapped]
		public double CreditAmount => this.GetInvoiceCreditAmount();

		[NotMapped]
		public double NettAmount => this.GrossAmount - this.CreditAmount;

		[NotMapped]
		public bool HasCredit => this.GetInvoiceCredit() != null;

		#endregion INITIALIZATION

		#region WORKALLOCATION

		public static WorkAllocation GetByID(User userRequesting, int id)
		{
			var wA = GetByID(id);

			if (wA == null || !userRequesting.CanManage(wA))
				return null;

			return wA;
		}

		public static WorkAllocation GetByID(int id) => WorkAllocationRepo.GetByID(id);

		public static WorkAllocation GetByUserIdActivityIDWorkDay(int userID, int activityID, DateTime workDay) => WorkAllocationRepo.GetByUserIdActivityIDWorkDay(userID, activityID, workDay);

		public static WorkAllocation GetNew(User currentUser, User user, int activityID, DateTime workDay)
		{
			var activity = ActivityRepo.GetByID(activityID);
			return GetNew(currentUser, user, activity, workDay);
		}

		public static WorkAllocation GetNew(User userCreating, User user, Activity activity, DateTime workDay)
		{
			if (userCreating.UserID != user.UserID && !userCreating.HasRole(UserRoleType.SystemAdministrator) && !userCreating.CanManage(user) && !userCreating.CanManage(activity))
				throw new UserRoleException("You do not have the required permissions to create a work allocation for another user. Create cancelled.");

			return new WorkAllocation
			{
				UserID = user.UserID,
				ActivityID = activity.ActivityID,
				DayWorked = workDay,
				LastModifiedBy = userCreating.FullName
			};
		}

		public void Delete(User userDeleting)
		{
			if (!userDeleting.HasRole(UserRoleType.SystemAdministrator) && !userDeleting.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to delete this work allocation. Delete cancelled.");
			this.Delete();
		}

		internal void Delete()
		{
			if (!this.CanDelete)
				throw new WorkAllocationException("This work allocation belongs to an invoice that is no longer in the Open phase. Remove cancelled.");

			WorkAllocationRepo.Delete(this);
		}

		public void DeleteInProcess()
		{
			if (!this.CanBeManagedForInvoice)
				throw new WorkAllocationException("This work allocation belongs to an invoice that is no longer in the Open or In Progress phase. Remove cancelled.");

			WorkAllocationRepo.Delete(this);
		}

		public static void UpdateHourlyRates(User userUpdating, string hourlyRates)
		{
			if (!userUpdating.HasRole(UserRoleType.SystemAdministrator) && !userUpdating.HasRole(UserRoleType.InvoiceAdministrator))
				throw new UserRoleException("You do not have the required permissions to update the hourly rates of work allocations.");

			using (var ts = new TransactionScope())
			{
				WorkAllocation wa;
				foreach (string item in hourlyRates.ToStringArray())
				{
					int index = item.IndexOf("|");

					if (!int.TryParse(item.Substring(0, index), out int workAllocationID))
						throw new WorkAllocationException("Error occured while trying to update work allocation hourly rate.");

					if (!double.TryParse(item.Substring(index + 1, item.Length - index - 1), out double hourlyRate))
						throw new WorkAllocationException("Error occured while trying to update work allocation hourly rate.");

					wa = WorkAllocationRepo.GetByID(workAllocationID);
					wa.HourlyRate = hourlyRate;
					wa.Save(userUpdating);
				}

				ts.Complete();
			}
		}

		private void Save(User userSaving)
		{
			this.LastModifiedBy = userSaving.FullName;
			double workDayHoursBefore = this.GetUser().GetAllocatedTime(this.DayWorked);
			WorkAllocationRepo.Save(this);
			double workDayHoursAfter = this.GetUser().GetAllocatedTime(this.DayWorked);
			bool workDayHoursChanged = workDayHoursBefore != workDayHoursAfter;
			double workDayHoursChangedBy = workDayHoursBefore - workDayHoursAfter;

			//Only do this is changes is applied to a month other than the current
			int monthsToRecalc = DateTime.Today.Year != this.DayWorked.Year ? 12 - this.DayWorked.Month + DateTime.Today.Month : DateTime.Today.Month - 1;
			if (this.DayWorked.IsInPastMonth() && workDayHoursChanged)
			{
				//Flexi balance audit trail
				var flexiBalance = this.GetUser().GetFlexiBalance(this.DayWorked.AddMonths(1).ToFirstDayOfMonth());
				//BalanceAuditTrail existingFlexiBalanceAuditTrail = BalanceAuditTrailRepo.GetByUniqueKey(this.UserID, this.DayWorked.AddMonths(1).ToFirstDayOfMonth(), (int)BalanceTypeEnum.Flexi, (int)BalanceChangeTypeEnum.Leave);
				var flexiBalanceAuditTrail = new BalanceAuditTrail
				{
					UserID = this.UserID,
					BalanceTypeID = (int)BalanceTypeEnum.Flexi,
					BalanceChangeTypeID = (int)BalanceChangeTypeEnum.Leave,
					BalanceDate = this.DayWorked.AddMonths(1).ToFirstDayOfMonth(),
					BalanceBefore = flexiBalance.FinalBalance,
					ChangeSummary = "WorkAllocation added or changed on workday " + this.DayWorked.ToExceptionDisplayString() + " by " + userSaving.FullName + " on " + DateTime.Today.ToExceptionDisplayString(),
					ChangedBy = userSaving.UserID,
					HoursChanged = workDayHoursChangedBy,
					DateChanged = DateTime.Today
				};
				UserRepo.GetByID(this.UserID).ExecuteFlexiBalanceLeaveBalanceProcess(userSaving, monthsToRecalc);
				flexiBalanceAuditTrail.BalanceAfter = flexiBalance.FinalBalance;
			}
		}

		#endregion WORKALLOCATION

		#region USER

		public User GetUser() => UserRepo.GetByID(this.UserID);

		#endregion USER

		#region ACTIVITY

		public Activity GetActivity() => ActivityRepo.GetByID(this.ActivityID);

		#endregion ACTIVITY

		#region PROJECT

		public Project GetProject() => WorkAllocationRepo.GetProject(this.ActivityID);

		#endregion PROJECT

		#region NOTE

		public Note GetNote(User userRequesting)
		{
			var note = WorkAllocationRepo.GetNote(this.WorkAllocationID);

			if (!userRequesting.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to view this note.");

			if (note == null)
				note = this.GetNewNote();

			return note;
		}

		public NoteDetailModel GetDetailedNote(User userRequesting)
		{
			var model = WorkAllocationRepo.GetDetailedNote(this.WorkAllocationID);

			if (!userRequesting.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to view this note.");

			if (model == null)
				model = this.GetNewDetailedNote();

			return model;
		}

		public void SaveNote(User userSaving, Note note)
		{
			if (!userSaving.CanManage(this))
			{
				if (this.WorkAllocationID == 0)
					throw new UserRoleException("You do not have the required permissions to create a note for another user.");
				else
					throw new UserRoleException("You do not have the required permissions to edit a note for another user.");
			}

			if (note.NoteID == 0 && !note.CanEdit)
				throw new NoteException("You cannot create a note for this allocation because the allocation has been invoiced already.");
			if (note.NoteID > 0 && !note.CanEdit)
				throw new NoteException("This note cannot be updated as it's allocation has been invoiced.");

			note.UserModified = userSaving.FullName;
			note.DateModified = DateTime.Now.ToCentralAfricanTime();

			if (note.NoteText.Length == 0)
			{
				note.Delete(userSaving);
				return;
			}

			NoteRepo.Save(note);
		}

		internal Note GetNewNote() => new Note()
		{
			WorkAllocationID = this.WorkAllocationID,
			LastModifiedBy = "Sys setup"
		};

		internal NoteDetailModel GetNewDetailedNote()
		{
			var activity = this.GetActivity();

			return new NoteDetailModel()
			{
				Note = this.GetNewNote(),
				WorkAllocation = this,
				Activity = activity,
				Project = activity.GetProject(),
				User = this.GetUser(),
			};
		}

		#endregion NOTE

		/*

		#region INVOICE LINE

		public InvoiceLine GetInvoiceLine(User userRequesting)
		{
			return GetInvoiceLine();
		}

		internal InvoiceLine GetInvoiceLine()
		{
			if (this.InvoiceLineID == null)
				return null;

			return InvoiceLineRepo.GetByID(this.InvoiceLineID.Value);
		}

		#endregion INVOICE LINE

		*/

		#region INVOICE

		internal Invoice GetInvoice()
		{
			if (!this.InvoiceID.HasValue || this.InvoiceID.Value <= 0)
				return null;
			return InvoiceRepo.GetByID(this.InvoiceID.Value);
		}

		#endregion INVOICE

		#region INVOICE CREDITS

		public InvoiceCredit GetInvoiceCredit(User userRequesting, bool ifNullReturnNewCredit = false)
		{
			if (!userRequesting.CanManage(this.GetInvoice()))
				throw new UserRoleException("You don not have the required permessions to retrieve this invoice credit.");

			return this.GetInvoiceCredit(ifNullReturnNewCredit);
		}

		internal InvoiceCredit GetInvoiceCredit(bool ifNullReturnNewCredit = false)
		{
			var invCredit = WorkAllocationRepo.GetInvoiceCredit(this.WorkAllocationID);

			if (invCredit != null)
				return invCredit;

			//if invCredit == null then lets do the following
			if (!ifNullReturnNewCredit)
				return null;

			return new InvoiceCredit();
		}

		public double GetInvoiceCreditAmount(User userRequesting)
		{
			if (!userRequesting.CanManage(this.GetInvoice()))
				throw new UserRoleException("You do not have the required permissions to retrieve this invoice credit.");

			return this.GetInvoiceCreditAmount();
		}

		internal double GetInvoiceCreditAmount()
		{
			if (!this.InvoiceID.HasValue || this.InvoiceID <= 0)
				return 0.0D;

			var invCredit = this.GetInvoiceCredit();
			if (invCredit != null)
				return invCredit.CreditAmount;
			return 0.0D;
		}

		internal void DeleteInvoiceCredit()
		{
			if (!this.HasCredit)
				return;

			if (!this.CanEditForInvoicing)
				throw new WorkAllocationException("The credit of this work allocation cannot be deleted as the invoice that contains this work allocation has been processed.");

			WorkAllocationRepo.DeleteInvoiceCredit(this.GetInvoiceCredit());
		}

		#endregion INVOICE CREDITS

		#region EXTENDED DETAIL

		public static WorkAllocationExtendedModel GetExtendedDetail(WorkAllocation wa) => WorkAllocationRepo.GetExtendedModel(wa);

		public static WorkAllocationExtendedModel CreateNewExtendedModel(User user, Activity activity, DateTime workDay)
		{
			var wa = new WorkAllocation
			{
				WorkAllocationType = (int)Enabill.WorkAllocationType.UserCreated,
				UserID = user.UserID,
				ActivityID = activity.ActivityID,
				DayWorked = workDay
			};

			var project = ProjectRepo.GetByID(activity.ProjectID);
			var client = project.GetClient();

			return new WorkAllocationExtendedModel()
			{
				WorkAllocation = wa,
				Activity = new ActivityDetail(activity, project, client, false),
				Project = new ProjectDetail(project),
				Client = client,
				AssociatedProjectTickets = TicketRepo.GetAssociatedProjectTickets(client.ClientID, project.ProjectID).ToList()
			};
		}

		#endregion EXTENDED DETAIL
	}

	[Table("vwWorkAllocationsWithLeave")]
	public class WorkAllocationsWithLeave
	{
		#region PROPERTIES

		[Key]
		public int UserID { get; set; }

		public int ActivityID { get; set; }
		public int Period { get; set; }

		public double HoursWorked { get; set; }

		public string Remark { get; set; }

		public DateTime DayWorked { get; set; }

		#endregion PROPERTIES
	}

	[Table("vwUserWorkAllocationExceptions")]
	public class UserWorkAllocationException
	{
		#region PROPERTIES

		[Key]
		public int UserID { get; set; }

		public double HoursWorked { get; set; }

		public string ActivityName { get; set; }
		public string ClientName { get; set; }
		public DateTime DayWorked { get; set; }
		public string DepartmentName { get; set; }
		public string DivisionName { get; set; }
		public string Period { get; set; }
		public string ProjectName { get; set; }
		public string RegionName { get; set; }
		public string UserName { get; set; }

		#endregion PROPERTIES
	}
}