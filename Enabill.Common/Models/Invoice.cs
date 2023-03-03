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
	[Table("Invoices")]
	public class Invoice
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int InvoiceID { get; internal set; }

		public bool IsInternal { get; set; }

		[Required]
		public bool IsTimeApproved { get; set; }

		[Required]
		public bool PrintCredits { get; set; }

		[Required]
		public bool PrintTimeSheet { get; set; }

		[Required]
		public int BillingMethodID { get; internal set; }

		[Required]
		public int ClientID { get; internal set; }

		[Required, EnumDataType(typeof(InvoiceStatusType))]
		public int InvoiceStatusID { get; set; }

		[Required]
		public int InvoiceSubCategoryID { get; set; }

		[Required]
		public int Period { get; set; }

		[Required]
		public int PrintTicketRemarkOptionID { get; set; }

		public int? ClientDepartmentCodeID { get; set; }
		public int? GLAccountID { get; set; }
		public int? HoursPaidFor { get; set; }
		public int? InvoiceContactID { get; internal set; }
		public int? InvoiceRuleID { get; internal set; }
		public int? PrintLayoutTypeID { get; set; }
		public int? PrintOptionTypeID { get; set; }
		public int? ProjectID { get; set; }

		[Required]
		public double AccrualExclVAT { get; set; }

		[Required]
		public double InvoiceAmountExclVAT { get; set; }

		[Required]
		public double InvoiceAmountInclVAT { get; set; }

		[Required]
		public double VATRate { get; set; }

		[Required]
		public double VATAmount { get; set; }

		//system calculated values
		public double? ProjectedAmountExcl { get; internal set; }

		public double? ProvisionalAccrualAmount { get; internal set; }
		public double? ProvisionalIncomeAmount { get; internal set; }

		[MaxLength(50)]
		public string ClientAccountCode { get; set; }

		[MaxLength(100)]
		public string ClientName { get; internal set; }

		[MaxLength(50)]
		public string CustomerRef { get; set; }

		[MaxLength(500)]
		public string Description { get; set; }

		[MaxLength(30)]
		public string ExternalInvoiceNo { get; set; }

		[MaxLength(128)]
		public string InvoiceContactName { get; set; }

		[MaxLength(50)]
		public string OrderNo { get; set; }

		[MaxLength(50)]
		public string OurRef { get; set; }

		[Required, MinLength(3), MaxLength(50)]
		public string UserCreated { get; internal set; }

		[MaxLength(50)]
		public string UserInvoiced { get; internal set; }

		[Required]
		public DateTime DateCreated { get; internal set; }

		[Required]
		public DateTime DateFrom { get; set; }

		[Required]
		public DateTime DateTo { get; set; }

		public DateTime? DateInvoiced { get; internal set; }
		public DateTime? InvoiceDate { get; set; }

		public virtual Client Client { get; internal set; }

		//public virtual ClientDepartmentCode ClientDepartmentCode { get; internal set; }
		//public virtual GLAccount GLAccountCode { get; internal set; }
		//public InvoiceCategory InvoiceCategory { get { return InvoiceSubCategoryRepo.GetParent(this.InvoiceSubCategoryID); } }
		//public virtual ICollection<InvoiceLine> InvoiceLines { get; internal set; }

		[NotMapped]
		public string BillingMethodValue
		{
			get
			{
				switch (this.BillingMethodID)
				{
					case (int)BillingMethodType.FixedCost:
						return BillingMethodType.FixedCost.GetDescription();

					case (int)BillingMethodType.MonthlyFixedCost:
						return BillingMethodType.MonthlyFixedCost.GetDescription();

					case (int)BillingMethodType.ActivityFixedCost:
						return BillingMethodType.ActivityFixedCost.GetDescription();

					case (int)BillingMethodType.NonBillable:
						return BillingMethodType.NonBillable.GetDescription();

					case (int)BillingMethodType.SLA:
						return BillingMethodType.SLA.GetDescription();

					case (int)BillingMethodType.TimeMaterial:
						return BillingMethodType.TimeMaterial.GetDescription();

					case (int)BillingMethodType.Travel:
						return BillingMethodType.Travel.GetDescription();

					default:
						return BillingMethodType.AdHoc.GetDescription();
				}
			}
		}

		#endregion PROPERTIES

		#region INITIALIZATION

		[NotMapped]
		public double WorkAllocationCreditAmount => this.GetInvoiceCreditsForWorkAllocations()
							.Sum(i => i.CreditAmount);

		[NotMapped]
		public double InvoiceCreditAmount
		{
			get
			{
				var invCredit = this.GetInvoiceCreditForInvoiceLevel();
				if (invCredit == null)
					return 0.00D;
				return invCredit.CreditAmount;
			}
		}

		[NotMapped]
		public double TotalInvoiceCreditAmount => this.WorkAllocationCreditAmount + this.InvoiceCreditAmount;

		[NotMapped]
		public double ProvisionalNettAmount
		{
			get
			{
				if (!this.ProvisionalIncomeAmount.HasValue)
					return 0 - this.TotalInvoiceCreditAmount;

				return this.ProvisionalIncomeAmount.Value - this.TotalInvoiceCreditAmount;
			}
		}

		[NotMapped]
		public bool IsTM => this.BillingMethodID == (int)BillingMethodType.TimeMaterial;

		[NotMapped]
		public bool IsFixedCost => this.BillingMethodID == (int)BillingMethodType.FixedCost;

		[NotMapped]
		public bool IsMonthlyFixedCost => this.BillingMethodID == (int)BillingMethodType.MonthlyFixedCost;

		[NotMapped]
		public bool IsActivityFixedCost => this.BillingMethodID == (int)BillingMethodType.ActivityFixedCost;

		[NotMapped]
		public bool IsGlobalFixedCost => this.IsFixedCost || this.IsMonthlyFixedCost;

		[NotMapped]
		public bool IsSLA => this.BillingMethodID == (int)BillingMethodType.SLA;

		[NotMapped]
		public bool IsTravel => this.BillingMethodID == (int)BillingMethodType.Travel;

		[NotMapped]
		public bool IsAdHoc => this.BillingMethodID == (int)BillingMethodType.AdHoc;

		[NotMapped]
		public bool IsNonBillable => this.BillingMethodID == (int)BillingMethodType.NonBillable;

		[NotMapped]
		public bool IsOpen => this.InvoiceStatusID == (int)InvoiceStatusType.Open;

		[NotMapped]
		public bool IsInProgress => this.InvoiceStatusID == (int)InvoiceStatusType.InProgress;

		[NotMapped]
		public bool IsReady => this.InvoiceStatusID == (int)InvoiceStatusType.Ready;

		[NotMapped]
		public bool IsComplete => this.InvoiceStatusID == (int)InvoiceStatusType.Complete;

		[NotMapped]
		public bool CanBeMovedToOpen =>
				//if (!string.IsNullOrEmpty(this.ExternalInvoiceNo))
				//    return false;
				this.IsInProgress;

		[NotMapped]
		public bool CanBeMovedToInProgress
		{
			get
			{
				if (this.InvoiceID < 1)
					return false;

				if (!string.IsNullOrEmpty(this.ExternalInvoiceNo))
					return false;

				if (!this.IsReady && !this.IsOpen)
					return false;

				return true;
			}
		}

		[NotMapped]
		public bool CanBeMovedToReady
		{
			get
			{
				if (this.InvoiceID < 1)
					return false;

				if (!this.IsInProgress && !this.IsComplete)
					return false;

				return true;
			}
		}

		[NotMapped]
		public bool CanBeMovedToComplete
		{
			get
			{
				if (string.IsNullOrEmpty(this.ExternalInvoiceNo))
					return false;

				if (!this.IsReady)
					return false;

				string accountCode = this.Client.AccountCode;

				return !string.IsNullOrEmpty(accountCode) && !string.IsNullOrWhiteSpace(accountCode);
			}
		}

		[NotMapped]
		public IEnumerable<Activity> Activities => InvoiceRepo.GetActivities(this.InvoiceID);

		[NotMapped]
		public IEnumerable<Contact> Contacts => InvoiceRepo.GetContacts(this.InvoiceID);

		[NotMapped]
		public InvoiceRule InvoiceRule => this.InvoiceRuleID.HasValue ? InvoiceRuleRepo.GetByID(this.InvoiceRuleID.Value) : null;

		[NotMapped]
		public List<InvoiceStatusType> NextAllowableInvoiceStatusType {
			get
			{
				if ((InvoiceStatusType)this.InvoiceStatusID == InvoiceStatusType.Open)
				{
					return new List<InvoiceStatusType> { InvoiceStatusType.InProgress, InvoiceStatusType.Ready, InvoiceStatusType.Complete };
				}

				if ((InvoiceStatusType)this.InvoiceStatusID == InvoiceStatusType.InProgress)
				{
					return new List<InvoiceStatusType> { InvoiceStatusType.Ready, InvoiceStatusType.Complete };
				}

				if ((InvoiceStatusType)this.InvoiceStatusID == InvoiceStatusType.Ready)
				{
					return new List<InvoiceStatusType> { InvoiceStatusType.Complete };
				}

				return new List<InvoiceStatusType> { InvoiceStatusType.Complete };
			}
		}

		#endregion INITIALIZATION

		#region INVOICE

		public static Invoice GetByID(User userRequesting, int invoiceID)
		{
			var i = InvoiceRepo.GetByID(invoiceID);

			if (!userRequesting.CanManage(i))
				return null;

			return i;
		}

		public static List<Invoice> GetAll()
		{
			var list = new List<Invoice>();
			return InvoiceRepo.GetAll(DateTime.Parse("2011-01-01"), DateTime.Today.ToLastDayOfMonth()).OrderByDescending(i => i.InvoiceID).ToList();
		}

		public static Invoice CreateInvoice(User userCreating, BillingMethodType billingTypeID, Client client, Contact contact, string customerRef, string orderNo, string ourRef, DateTime dateFrom, DateTime dateTo, double amountExcl, int subcategoryID, Project project = null) => CreateInvoiceForType(userCreating, billingTypeID, client, contact, customerRef, orderNo, ourRef, dateFrom, dateTo, amountExcl, subcategoryID, null);

		public static Invoice CreateInvoice(User userCreating, BillingMethodType billingTypeID, Client client, Contact contact, string customerRef, string orderNo, string ourRef, DateTime dateFrom, DateTime dateTo, double amountExcl, int subcategoryID, InvoiceRule rule) => CreateInvoiceForType(userCreating, billingTypeID, client, contact, customerRef, orderNo, ourRef, dateFrom, dateTo, amountExcl, subcategoryID, rule);

		public static Invoice CreateInvoice(User userCreating, Client client, Contact contact, Project project, string customerRef, string orderNo, string ourRef, InvoiceSubCategory subCategory, DateTime dateFrom, DateTime dateTo, int period, DateTime invoiceDate, bool isInternal)
		{
			var invoice = new Invoice()
			{
				ClientID = client.ClientID,
				BillingMethodID = (int)BillingMethodType.AdHoc,
				ClientName = client.ClientName,
				CustomerRef = customerRef,
				ProjectID = (project == null) ? null : (int?)project.ProjectID,
				OrderNo = orderNo,
				DateFrom = dateFrom,
				DateTo = dateTo,
				DateCreated = DateTime.Today.Date,
				InvoiceDate = invoiceDate,
				InvoiceContactID = contact.ContactID,
				InvoiceContactName = null,
				InvoiceStatusID = (int)InvoiceStatusType.Open,
				InvoiceSubCategoryID = subCategory.InvoiceCategoryID,
				OurRef = ourRef,
				Period = period,
				UserCreated = userCreating.UserName,
				UserInvoiced = userCreating.UserName,
				VATRate = client.GetVATRate(),
				IsInternal = isInternal
			};

			if (!userCreating.CanManage(invoice))
				throw new InvoiceException("You do not have the required permissions to create an invoice");

			invoice.Save();

			return invoice;
		}

		private static Invoice CreateInvoiceForType(User userCreating, BillingMethodType billingTypeID, Client client, Contact contact, string customerRef, string orderNo, string ourRef, DateTime dateFrom, DateTime dateTo, double amountExcl, int subcategoryID, InvoiceRule rule, Project project = null)
		{
			int? ruleID = (rule == null) ? null : (int?)rule.InvoiceRuleID;
			int? projectID = (project == null) ? null : (int?)project.ProjectID;

			return new Invoice()
			{
				BillingMethodID = (int)billingTypeID,
				ClientID = client.ClientID,
				ClientName = client.ClientName,
				CustomerRef = customerRef,
				DateCreated = DateTime.Today,
				DateFrom = dateFrom,
				DateTo = dateTo,
				InvoiceAmountExclVAT = amountExcl,
				InvoiceContactID = contact.ContactID,
				InvoiceContactName = null,
				InvoiceDate = DateTime.Today,
				InvoiceRuleID = ruleID,
				InvoiceStatusID = (int)InvoiceStatusType.Open,
				InvoiceSubCategoryID = subcategoryID,
				OrderNo = orderNo,
				OurRef = ourRef,
				Period = DateTime.Today.ToPeriod(),
				UserCreated = userCreating.UserName,
				UserInvoiced = userCreating.UserName,
				VATRate = client.GetVATRate(),
				IsTimeApproved = billingTypeID == BillingMethodType.AdHoc,
				ProjectID = projectID,
			//urrency= client.GetCurrency(client.CurrencyTypeID).CurrencyISO,
			};
		}

		public void Save(User userSaving, bool reProcess)
		{
			this.Save(userSaving);

			if (reProcess)
				this.ReProcessInvoice();
		}

		public void Save(User userSaving)
		{
			if (!userSaving.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to create/edit an invoice. Save cancelled.");

			using (var ts = new TransactionScope())
			{
				if (this.InvoiceID == 0) //0 = new invoice
				{
					this.UserCreated = userSaving.FullName;
					this.DateCreated = DateTime.Now.ToUniversalTime().AddHours(2);
				}

				//if (this.IsAdHoc && this.InvoiceLines != null && this.InvoiceLines.Count() > 0)
				//    this.InvoiceAmountExclVAT = this.InvoiceLines.Sum(il => il.LineAmountExclVAT);

				this.Save();
				ts.Complete();
			}
		}

		private void Save()
		{
			if (this.DateFrom > this.DateTo)
				throw new InvoiceException("The \"Date From\" cannot be after the \"Date To\". Please revise the invoice dates.");

			if (this.DateFrom < EnabillSettings.SiteStartDate)
				throw new EnabillSettingsException(string.Format($"You cannot invoice for dates before the site start date: {EnabillSettings.SiteStartDate.ToExceptionDisplayString()}"));

			if (!this.Period.IsValidPeriod())
				throw new InvoiceException("The value entered as the period is not a valid period. Please revise this before saving.");

			if (this.IsOpen || this.IsInProgress)
			{
				this.VATAmount = this.InvoiceAmountExclVAT * this.VATRate / 100;
				this.InvoiceAmountInclVAT = this.VATAmount + this.InvoiceAmountExclVAT;
			}

			InvoiceRepo.Save(this);
		}

		public void Delete(User userDeleting)
		{
			if (!userDeleting.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to delete an invoice. Delete cancelled.");

			this.Delete();
		}

		private void CalculateInvoiceAmounts()
		{
			double grossAmt = this.InvoiceAmountExclVAT;
			this.VATAmount = grossAmt * this.VATRate;
			this.InvoiceAmountInclVAT = grossAmt + this.VATAmount;
		}

		private void Delete() => InvoiceRepo.Delete(this);

		public Invoice GetLinkedInvoice()
		{
			if (this.InvoiceRuleID.HasValue)
				return InvoiceRepo.GetLinkedInvoice(this.Period, this.InvoiceRuleID.Value, this.BillingMethodID);

			return null;
		}

		#endregion INVOICE

		#region CONTACTS

		internal void AddContact(int contactID) => InvoiceRepo.AddContact(this.InvoiceID, contactID);

		internal void AddContacts(IEnumerable<Contact> contacts) => InvoiceRepo.AddContacts(this.InvoiceID, contacts);

		public void AddContacts(string contactIDString)
		{
			var contactIDs = contactIDString.ToIntArray();
			var contacts = new List<Contact>();

			//delete all existing contacts before adding the newly selected ones to avoid duplicates
			InvoiceRepo.DeleteAllInvoiceContacts(this.InvoiceID);

			foreach (int contactID in contactIDs.Distinct())
			{
				var contact = ContactRepo.GetByID(contactID);

				if (contact.ClientID != this.ClientID)
					throw new InvoiceException(contact.ContactName + " is not a contact of " + this.Client.ClientName + ". Adding of contacts cancelled.");

				//if (this.DefaultContactID == contact.ContactID)
				//   continue;

				//contacts.Add(contact);

				InvoiceRepo.AddContact(this.InvoiceID, contactID);
			}
		}

		#endregion CONTACTS

		#region STATUS

		public void OpenInvoice(User userExecuting)
		{
			var invoice = this;
			if (!userExecuting.CanManage(invoice))
				throw new UserRoleException("You do not have the required permissions to move the status of an invoice. Action cancelled.");

			if (!invoice.CanBeMovedToOpen)
			{
				if (!string.IsNullOrEmpty(invoice.ExternalInvoiceNo))
					throw new InvoiceException("External Invoice Number has been confirmed. Cannot 'Open' invoice.");

				if (invoice.IsOpen)
					throw new InvoiceException("Invoice is already in the 'Open' status.");

				if (invoice.IsComplete)
					throw new InvoiceException("Invoice has already been completed, and therefore, cannot be moved to 'Open'.");

				invoice.IsInvoiceInternal();
			}

			invoice.IsInvoiceInternal();

			if (invoice.IsInternal && string.IsNullOrEmpty(invoice.ExternalInvoiceNo))
			{
				invoice.ExternalInvoiceNo = "No invoice needed.";
			}

			invoice.InvoiceStatusID = (int)InvoiceStatusType.Open;
			invoice.Save();
			invoice.ReProcessInvoice();
		}

		public void MoveInvoiceToInProgress(User userExecuting)
		{
			var invoice = this;

			if (!userExecuting.CanManage(invoice))
				throw new UserRoleException("You do not have the required permissions to move the status of an invoice. Action cancelled.");

			if (!invoice.CanBeMovedToInProgress)
			{
				if (invoice.InvoiceID < 1)
					throw new InvoiceException("This invoice has not yet been saved. Please save the invoice first before attempting to move this invoice to 'In Progress'.");

				if (!string.IsNullOrEmpty(invoice.ExternalInvoiceNo))
					throw new InvoiceException("The external reference number of the invoice has been set. Please remove this reference number before attempting to move the invoice to 'In Progress'.");

				if (invoice.IsInProgress)
					throw new InvoiceException("Invoice is already in the 'In Progress' state.");

				if (invoice.IsComplete)
					throw new InvoiceException("Invoice has already been completed, and therefore, cannot be moved to 'In Progress'.");

				invoice.IsInvoiceInternal();
			}

			invoice.IsInvoiceInternal();

			if (invoice.IsInternal && string.IsNullOrEmpty(invoice.ExternalInvoiceNo))
			{
				invoice.ExternalInvoiceNo = "No invoice needed.";
			}

			invoice.InvoiceStatusID = (int)InvoiceStatusType.InProgress;
			invoice.InvoiceDate = invoice.InvoiceDate;
			invoice.Save();
			invoice.ReProcessInvoice();
		}

		public void ReadyInvoice(User userExecuting)
		{
			var invoice = this;

			if (!userExecuting.CanManage(invoice))
				throw new UserRoleException("You do not have the required permissions to move the status of an invoice. Action cancelled.");

			if (!invoice.CanBeMovedToReady)
			{
				if (invoice.InvoiceID < 1)
					throw new InvoiceException("This invoice has not yet been saved. Please save the invoice first before attempting to move this invoice to ready.");

				if (invoice.IsOpen)
					throw new InvoiceException("Invoice is in the 'Open' status and will need to be moved to 'In Progress' before being moved to 'Ready'.");

				if (invoice.IsReady)
					throw new InvoiceException("Invoice is already in the ready state.");

				if (invoice.IsComplete)
					throw new InvoiceException("Invoice has already been completed, and therefore, cannot be moved to ready.");

				invoice.IsInvoiceInternal();
			}

			invoice.IsInvoiceInternal();

			if (invoice.IsInternal && string.IsNullOrEmpty(invoice.ExternalInvoiceNo))
				invoice.ExternalInvoiceNo = "No invoice needed.";

			invoice.InvoiceStatusID = (int)InvoiceStatusType.Ready;
			//ClientDepartmentCodeID = this.ClientDepartmentCodeID;
			invoice.ClientDepartmentCodeID = 125; //Hard coded to Alacrity as the client
												  //GLAccountID = this.GLAccountID;
			invoice.GLAccountID = 10; //Hard coded to Consulting Fees
			invoice.InvoiceDate = invoice.InvoiceDate;
			invoice.Save();
		}

		public void CompleteInvoice(User userExecuting)
		{
			if (!userExecuting.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to move the status of an invoice. Action canceled.");

			if (!this.CanBeMovedToComplete)
			{
				if (this.IsOpen)
					throw new InvoiceException("Invoice is currently in the open state and would first have to be moved to 'Ready' before it can be moved to 'Complete'");

				if (string.IsNullOrEmpty(this.ExternalInvoiceNo))
					throw new InvoiceException("External Invoice Reference Number is missing. Cannot complete Invoice.");

				if (string.IsNullOrEmpty(this.Client.AccountCode) || string.IsNullOrWhiteSpace(this.Client.AccountCode))
					throw new InvoiceException("The client to which this invoice belongs does not have a valid account code. Please update " + this.Client.ClientName + "'s account code before trying to move the invoice to 'Complete'");

				if (this.ClientDepartmentCodeID == 0 || this.ClientDepartmentCodeID == null)
					throw new InvoiceException("Client Department Code cannot be moved if no code is selected'.");

				this.IsInvoiceInternal();
			}

			this.InvoiceStatusID = (int)InvoiceStatusType.Complete;
			this.InvoiceContactName = ContactRepo.GetByID(this.InvoiceContactID.Value).ContactName;
			this.ClientName = this.Client.ClientName;
			this.ClientAccountCode = this.Client.AccountCode;
			//ClientDepartmentCodeID = this.ClientDepartmentCodeID;
			this.ClientDepartmentCodeID = 125; //Hard coded to Alacrity as the client
											   //GLAccountID = this.GLAccountID;
			this.GLAccountID = 10; //Hard coded to Consulting Fees
			this.InvoiceDate = this.InvoiceDate;
			this.Save();
		}

		private void IsInvoiceInternal()
		{
			if (!this.IsInternal)
			{
				if (this.ClientDepartmentCodeID == 0 || this.ClientDepartmentCodeID == null)
					throw new InvoiceException("Cannot be moved if no client department code is selected'.");

				if (this.GLAccountID == 0 || this.GLAccountID == null)
					throw new InvoiceException("Cannot be moved if no GL account code is selected'.");
			}
		}

		public void CompleteInvoice(/*User userExecuting, */string externalInvNo)
		{
			//Currently, this method is only being used from the tests ...
			//The consumer doesn't call this function (and doesn't need to) and therefore, this can be removed.

			//Also, this method contains no rules about moving from ready to complete.
			this.ExternalInvoiceNo = externalInvNo;
			this.InvoiceStatusID = (int)InvoiceStatusType.Complete;
		}


		#endregion STATUS

		#region INVOICE LINES (REMOVED)

		/*
		public List<InvoiceLine> GetInvoiceLines()
		{
			return InvoiceRepo.GetInvoiceLines(this.InvoiceID).ToList();
		}

		public void AddLine(User userSaving, int invoiceID, string description, double lineAmountExcl, double quantity)
		{
			if (!userSaving.CanManage(this))
				throw new InvoiceException("You do not have the required permissions to edit an invoice");

			int lineNo;

			if (this.InvoiceLines == null)
				lineNo = 1;
			else
				lineNo = this.InvoiceLines.Count + 1;

			InvoiceLine line = new InvoiceLine()
			{
				InvoiceID = invoiceID,
				Description = description,
				LineAmountExclVAT = Math.Round(lineAmountExcl, 2),
				Quantity = quantity,
				UnitPrice = Math.Round(lineAmountExcl / quantity, 2),
				LineNo = lineNo
			};

			InvoiceLineRepo.Save(line);

			// Sum up Invoice Lines
			if (InvoiceLines != null)
			{
				this.InvoiceAmountExclVAT = InvoiceLines.Sum(x => x.LineAmountExclVAT);
				this.VATAmount = InvoiceAmountExclVAT * this.VATRate;
				this.InvoiceAmountInclVAT = InvoiceAmountExclVAT + this.VATAmount;
				this.Save();
			}

			this.Save(userSaving);
		}

		public void CreateLines(User userCreating, object state = null)
		{
			if (!userCreating.CanManage(this))
				throw new InvoiceException("You do not have the required permissions to edit an invoice.");

			CreateLines(state);
		}

		internal void CreateLines(object state = null)
		{
			if (!this.IsOpen)
				throw new InvoiceException("Cannot create lines if invoice is not open.");

			if (this.InvoiceLines == null)
				this.InvoiceLines = new List<InvoiceLine>();

			if (this.InvoiceLines.Count > 0)
				ResetInvoice();

			switch ((BillingMethodType)this.BillingMethodID)
			{
				case BillingMethodType.TimeMaterial:
					CreateInvoiceLinesTimeAndMaterial(state);
					break;

				case BillingMethodType.FixedCost:
					CreateInvoiceLinesFixedCost(state);
					break;

				case BillingMethodType.SLA:
					CreateInvoiceLinesSLA(state);
					break;

				case BillingMethodType.Travel:
					CreateInvoiceLinesTravel(state);
					break;

				default:
					break;
			}
			this.Save();
		}

		//public void ResetInvoice(User userReseting)
		//{
		//    if (!userReseting.HasRole(UserRoleType.InvoiceAdministrator) && !userReseting.HasRole(UserRoleType.ProjectOwner))
		//        throw new InvoiceException("you do not have the required permissions to update this invoice.");

		//    ResetInvoice();
		//}

		private void ResetWorkAllocations(List<InvoiceLine> lines, int billType)
		{
			List<WorkAllocation> splitAllocations = new List<WorkAllocation>();

			foreach (var il in lines)
			{
				foreach (var wa in il.GetWorkAllocations())
				{
					wa.InvoiceLineID = null;
					wa.HoursBilled = null;

					if (wa.WorkAllocationType == (int)Enabill.WorkAllocationType.SLASplit)
						splitAllocations.Add(wa);

					UserRepo.SaveWorkAllocation(wa);
				}
			}

			for (int j = splitAllocations.Count - 1; j >= 0; j--)
			{
				splitAllocations[j].Delete();
			}

			for (int i = lines.Count - 1; i >= 0; i--)
			{
				lines[i].Delete();
			}
		}

		private void ResetInvoice()
		{
			//try and find the T&M invoice (for overrun) based on this invoice's invoice date
			Invoice slaTMInvoice = InvoiceRuleRepo.GetByID(this.InvoiceRuleID.Value).GetInvoices().Where(inv => inv.BillingMethodID == (int)BillingMethodType.TimeMaterial && inv.InvoiceDate == this.InvoiceDate).SingleOrDefault();

			if (!this.IsOpen)
				throw new InvoiceException("This invoice cannot be updated as it is not in the 'open' phase");
			ResetWorkAllocations(this.InvoiceLines.ToList(), this.BillingMethodID);

			if ((slaTMInvoice != null) && this.IsSLA)
			{
				ResetWorkAllocations(slaTMInvoice.InvoiceLines.ToList(), slaTMInvoice.BillingMethodID);
				slaTMInvoice.Delete();
			}
		}

		private void CreateInvoiceLinesTimeAndMaterial(object state)
		{
			this.AddLines();

			// Sum up Invoice Lines
			if (InvoiceLines != null)
			{
				this.InvoiceAmountExclVAT = InvoiceLines.Sum(x => x.LineAmountExclVAT);
				this.VATAmount = InvoiceAmountExclVAT * this.VATRate;
				this.InvoiceAmountInclVAT = InvoiceAmountExclVAT + this.VATAmount;
				this.Save();
			}
		}

		private void CreateInvoiceLinesFixedCost(object state)
		{
			InvoiceRule ir = InvoiceRuleRepo.GetByID(this.InvoiceRuleID.Value);
			Project project = ir.GetProject();

			InvoiceRuleLine irl = state as InvoiceRuleLine;

			InvoiceLine il = new InvoiceLine
			{
				InvoiceID = this.InvoiceID,
				Description = project.ProjectName + " - " + project.ProjectDesc,
				LineAmountExclVAT = irl.CustomerAmount,
				LineNo = 1
			};

			il.Save();

			irl.InvoiceLineID = il.InvoiceLineID;
		}

		private void CreateInvoiceLinesSLA(object state)
		{
			List<WorkAllocation> workAllocations = new List<WorkAllocation>();

			this.Activities.ToList().ForEach(act =>
						InvoiceRepo.GetLinkedWorkAllocations(act.ActivityID, this.DateFrom, this.DateTo)
							.Where(wa => wa.InvoiceLineID == null)
							.ToList()
							.ForEach(wa =>
									workAllocations.Add(wa)
						)
			);

			bool? invoiceAdditional = InvoiceRuleRepo.GetByID(this.InvoiceRuleID.Value).InvoiceAdditionalHours;
			if ((invoiceAdditional.HasValue) && (invoiceAdditional.Value))
			{
				//split the work allocations between SLA and AdHoc
				List<WorkAllocation> slaAllocations = new List<WorkAllocation>();
				List<WorkAllocation> adHocAllocations = new List<WorkAllocation>();
				double totalHours = 0.0D;
				foreach (WorkAllocation waItem in workAllocations)
				{
					if ((totalHours + waItem.HoursWorked <= this.HoursPaidFor.Value) || (!waItem.IsBillable))
						slaAllocations.Add(waItem);
					else
					{
						double adhocHrs = (totalHours + waItem.HoursWorked) - this.HoursPaidFor.Value;
						//we check the collection as well, because double subtraction yields difference in the < check like 0.0000999999999
						if ((adhocHrs < waItem.HoursWorked) && (adHocAllocations.Count == 0))
						{
							double fillSLA = waItem.HoursWorked - adhocHrs;
							waItem.HoursBilled = fillSLA;

							UserRepo.SaveWorkAllocation(waItem);
							slaAllocations.Add(waItem);

							WorkAllocation tmBit = new WorkAllocation();
							tmBit.WorkAllocationType = (int)Enabill.WorkAllocationType.SLASplit;
							tmBit.HoursWorked = 0.0D;
							tmBit.HoursBilled = adhocHrs;
							//tmBit.HourRate = waItem.HourRate;
							tmBit.IsBillable = waItem.IsBillable;
							tmBit.ActivityID = waItem.ActivityID;
							tmBit.DateCreated = DateTime.Now;
							tmBit.DayWorked = waItem.DayWorked;
							tmBit.Period = waItem.Period;
							tmBit.Remark = waItem.Remark;
							tmBit.UserCreated = waItem.UserCreated;
							tmBit.UserID = waItem.UserID;
							tmBit.ParentWorkAllocationID = waItem.WorkAllocationID;

							UserRepo.SaveWorkAllocation(tmBit);

							adHocAllocations.Add(tmBit);
						}
						else
							adHocAllocations.Add(waItem);
					}

					if (waItem.IsBillable)
						totalHours += waItem.HoursWorked;
				}

				CreateLinesForNormalSLA(slaAllocations);

				if (adHocAllocations.Count > 0)
				{
					Invoice tmInvoice = new Invoice();
					tmInvoice.BillingMethodID = (int)BillingMethodType.TimeMaterial;
					tmInvoice.ClientID = this.ClientID;
					tmInvoice.ClientName = this.ClientName;
					tmInvoice.AddContatcs(this.Contacts);
					tmInvoice.CustomerRef = this.CustomerRef;
					tmInvoice.DateCreated = this.DateCreated;
					tmInvoice.DateFrom = adHocAllocations.Min(a => a.DayWorked);
					tmInvoice.DateInvoiced = this.DateInvoiced;
					tmInvoice.DateTo = adHocAllocations.Max(a => a.DayWorked);
					tmInvoice.HoursPaidFor = (int)Math.Round(adHocAllocations.Sum(a => a.HoursWorked));
					tmInvoice.InvoiceContactID = this.InvoiceContactID;
					tmInvoice.InvoiceDate = this.InvoiceDate;
					tmInvoice.InvoiceRuleID = this.InvoiceRuleID;
					tmInvoice.InvoiceStatusID = (int)InvoiceStatusType.Open;
					tmInvoice.InvoiceSubCategoryID = this.InvoiceSubCategoryID;
					tmInvoice.OrderNo = this.OrderNo;
					tmInvoice.OurRef = this.OurRef;
					tmInvoice.Period = this.Period;
					tmInvoice.UserCreated = this.UserCreated;
					tmInvoice.UserInvoiced = this.UserInvoiced;
					tmInvoice.VATRate = this.VATRate;

					tmInvoice.Save();

					tmInvoice.AddActivities(this.Activities.ToList());

					tmInvoice.AddLines(adHocAllocations);

					// Sum up Invoice Lines
					tmInvoice.InvoiceAmountExclVAT = Math.Round(tmInvoice.InvoiceLines.Sum(x => x.LineAmountExclVAT), 2);
					tmInvoice.VATAmount = tmInvoice.InvoiceAmountExclVAT * tmInvoice.VATRate;
					tmInvoice.InvoiceAmountInclVAT = tmInvoice.InvoiceAmountExclVAT + tmInvoice.VATAmount;
					tmInvoice.Save();
				}
			}
			else
			{
				CreateLinesForNormalSLA(workAllocations);
			}
		}

		private void CreateLinesForNormalSLA(List<WorkAllocation> workAllocations)
		{
			InvoiceRule ir = InvoiceRuleRepo.GetByID(this.InvoiceRuleID.Value);
			Project project = ir.GetProject();

			InvoiceLine il = new InvoiceLine
			{
				InvoiceID = this.InvoiceID,
				Description = "SLA - " + project.ProjectName,
				LineAmountExclVAT = this.InvoiceAmountExclVAT,
				Quantity = (double)ir.HoursPaidFor.Value,
				UnitPrice = Math.Round(this.InvoiceAmountExclVAT / (double)ir.HoursPaidFor.Value, 2),
				LineNo = 1
			};

			il.Save();

			foreach (var wa in workAllocations)
			{
				wa.InvoiceLineID = il.InvoiceLineID;

				UserRepo.SaveWorkAllocation(wa);
			}

			// Sum up Invoice Lines
			this.VATAmount = InvoiceAmountExclVAT * this.VATRate;
			this.InvoiceAmountInclVAT = InvoiceAmountExclVAT + this.VATAmount;
			this.Save();
		}

		private void CreateInvoiceLinesTravel(object state)
		{
			this.AddLines();

			// Sum up Invoice Lines
			if (InvoiceLines != null)
			{
				this.InvoiceAmountInclVAT = this.InvoiceAmountExclVAT = InvoiceLines.Sum(x => x.LineAmountExclVAT);
				this.VATAmount = this.VATRate = 0;
				this.Save();
			}
		}

		private void AddLines(List<WorkAllocation> workAllocations)
		{
			InvoiceRule ir = InvoiceRuleRepo.GetByID(this.InvoiceRuleID.Value);
			//Project project = ir.GetProjects().First().Project;

			int invLine = 1;
			List<InvoiceLine> invoiceLines = new List<InvoiceLine>();
			Dictionary<WorkAllocation, InvoiceLine> map = new Dictionary<WorkAllocation, InvoiceLine>();
			foreach (WorkAllocation wa in workAllocations)
			{
				double unitprice = Math.Round((wa.IsBillable) ? wa.GetUser().ActivityRate(wa.ActivityID) : 0.0D, 2);
				double amount = wa.HoursWorked * unitprice;
				double quantity = wa.HoursBilled ?? wa.HoursWorked;

				User user = wa.GetUser();
				// Create and Invoice Line
				invoiceLines.Add(new InvoiceLine()
				{
					InvoiceID = this.InvoiceID,
					Description = string.Format($"{user.FirstName} {user.LastName}{((string.IsNullOrEmpty(wa.Remark) ? string.Empty : " - " + wa.Remark))}"),    //removed 'N/A' default value - blank string looks better on print
					LineNo = invLine++,
					LineAmountExclVAT = amount,
					Quantity = quantity,
					UnitPrice = unitprice
				});
				map.Add(wa, invoiceLines.Last());
			}
			InvoiceLineRepo.SaveAll(invoiceLines);

			foreach (var wa in map.Keys)
			{
				wa.InvoiceLineID = map[wa].InvoiceLineID;
			}
			UserRepo.SaveAllWorkAllocation(workAllocations);
		}

		private void AddLines()
		{
			if (!this.IsTM && !this.IsTravel)
				throw new InvoiceException("This method is common between adhoc, travel and T&M invoices. No other billing method types can use this method.");
			//Private method, so this error should only display if domain is incorrect

			foreach (Activity act in this.Activities.ToList())
			{
				// We need all WorkAllocations for the activies on this Invoice
				List<WorkAllocation> workAllocations = InvoiceRepo
					.GetLinkedWorkAllocations(act.ActivityID, this.DateFrom, this.DateTo)
					.Where(wa => wa.InvoiceLineID == null)
					.ToList();

				this.AddLines(workAllocations);
			}
		}

		public void SaveInvoiceLines(List<InvoiceLine> invLineList)
		{
			foreach (InvoiceLine invLine in invLineList)
			{
				if (invLine.InvoiceLineID == 0)
					invLine.InvoiceID = this.InvoiceID;
				invLine.Save();
			}
		}

		public void DeleteInvoiceLines(List<int> deletedLineIDs)
		{
			foreach (int id in deletedLineIDs)
			{
				if (this.IsFixedCost)
				{
					InvoiceRuleLine irl = InvoiceRuleLineRepo.FetchByInvoiceLine(id);
					irl.InvoiceLineID = null;
				}

				InvoiceLine invLine = InvoiceLineRepo.GetByID(id);
				invLine.Delete();
			}
		}
		*/

		#endregion INVOICE LINES (REMOVED)

		#region PROCESSES

		/*
		public static void InvoiceRun(User userExecuting, DateTime dateRun)
		{
			if (!userExecuting.HasRole(UserRoleType.SystemAdministrator) && !userExecuting.HasRole(UserRoleType.InvoiceAdministrator))
				throw new UserRoleException("You do not have the required permissions to execute the invoice run");

			foreach (InvoiceRule ir in InvoiceRuleRepo.GetAll(dateRun).ToList())
			{
				if (ir.GetActivities(userExecuting).Count() == 0)
					continue;

				if (ir.GetCurrentInvoice() == null)
				{
					ir.CreateInvoice(userExecuting, dateRun);
				}
			}
		}
		*/

		public void ProcessInvoice()
		{
			if (!this.IsOpen)
				return;

			this.GatherWorkAllocations();
			this.CalculateEstimateValues();

			//reset the invoice date
			//InvoiceDate = DateTime.Today;

			this.Save();
		}

		public void ReProcessInvoice()
		{
			if ((!this.IsOpen) && (!this.IsInProgress))
				return;

			if (this.IsOpen)
			{
				this.ProcessInvoice();

				return;
			}

			var workAllocations = InvoiceRepo.GetLinkedWorkAllocations(this.InvoiceID).ToList();

			foreach (var allocation in workAllocations.Where(wa => wa.DayWorked > this.DateTo))
			{
				allocation.InvoiceID = null;
				WorkAllocationRepo.Save(allocation);
			}

			this.CalculateEstimateValues();
		}

		private void GatherWorkAllocations()
		{
			var activityIDs = this.GetActivitiesFromRule()
										.Select(a => a.ActivityID)
										.ToList();

			var workAllocations = InvoiceRepo.GetLinkedWorkAllocations(this.InvoiceID).ToList();
			workAllocations.AddRange(InvoiceRepo.GetNonLinkedWorkAllocations(activityIDs, this.DateTo));

			if (workAllocations.Any(wa => wa.DayWorked > this.DateTo))
			{
				foreach (var allocation in workAllocations.Where(wa => wa.DayWorked >= this.DateTo))
				{
					allocation.InvoiceID = null;
					WorkAllocationRepo.Save(allocation);
				}

				//and then find items again
				workAllocations = InvoiceRepo.GetLinkedWorkAllocations(this.InvoiceID).ToList();
				workAllocations.AddRange(InvoiceRepo.GetNonLinkedWorkAllocations(activityIDs, this.DateTo));
			}

			this.AddWorkAllocations(workAllocations);
		}

		public void CalculateEstimateValues(User userExecuting)
		{
			if (!userExecuting.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to calculate the invoice values for this invoice.");

			this.CalculateEstimateValues();
		}

		private void CalculateEstimateValues()
		{
			switch (this.BillingMethodID)
			{
				case (int)BillingMethodType.TimeMaterial:
					this.CalculateTimeMaterialEstimates();
					break;

				case (int)BillingMethodType.FixedCost:
					this.CalculateFixedCostEstimates();
					break;

				case (int)BillingMethodType.MonthlyFixedCost:
					this.CalculateMonthlyFixedCostEstimates();
					break;

				case (int)BillingMethodType.ActivityFixedCost:
					this.CalculateActivityFixedCostEstimates();
					break;

				case (int)BillingMethodType.SLA:
					this.CalculateSLAEstimates();
					break;

				default:
					{
						this.ProvisionalAccrualAmount = 0.0D;
						this.ProvisionalIncomeAmount = 0.0D;
						this.ProjectedAmountExcl = 0.0D;
					}
					break;
			}
		}

		private void CalculateTimeMaterialEstimates()
		{
			var allocations = InvoiceRepo.GetLinkedWorkAllocations(this.InvoiceID).ToList();

			double totalValue = 0.0D;
			var maxDate = DateTime.MinValue;

			foreach (var alloc in allocations)
			{
				totalValue += (alloc.HoursBilled ?? alloc.HoursWorked) * alloc.HourlyRate.Value;

				if (alloc.DayWorked > maxDate)
					maxDate = alloc.DayWorked;
			}

			//how many hours are we into the invoice period?
			double totalHours = WorkDay.GetWorkableDays(true, this.DateFrom, DateTime.Now.Date).Count * 8;

			//how many hours in the whole invoice period?
			double allHours = WorkDay.GetWorkableDays(true, this.DateFrom, this.DateTo).Count * 8.0D;   //8 hours per day

			double allValue = 0.0D;

			if (totalHours >= allHours)
			{
				allValue = totalValue;
			}
			else
			{
				//calculate what the estimated value will be
				if (totalHours != 0)
					allValue = (allHours / totalHours) * totalValue;
			}

			this.ProvisionalAccrualAmount = totalValue;
			this.ProvisionalIncomeAmount = totalValue;
			this.ProjectedAmountExcl = allValue;
		}

		private void CalculateFixedCostEstimates()
		{
			this.ProvisionalAccrualAmount = 0.0D;
			this.ProvisionalIncomeAmount = 0.0D;
			this.ProjectedAmountExcl = 0.0D;

			if (!this.InvoiceRuleID.HasValue)
				return;

			var rule = InvoiceRule.GetByID(this.InvoiceRuleID.Value);

			if (rule == null)
				return;

			int period = (int)Math.Ceiling((this.DateFrom.AddDays(15) - rule.DateFrom).TotalDays / 30.4);

			var line = InvoiceRuleLineRepo.FetchByInvoiceRulePeriod(this.InvoiceRuleID.Value, period);

			if (line == null)
				return;

			this.ProvisionalAccrualAmount = line.AccrualAmount;
			this.ProvisionalIncomeAmount = line.CustomerAmount;
			this.ProjectedAmountExcl = line.AccrualAmount;
		}

		private void CalculateMonthlyFixedCostEstimates()
		{
			this.ProvisionalAccrualAmount = 0.0D;
			this.ProvisionalIncomeAmount = 0.0D;
			this.ProjectedAmountExcl = 0.0D;

			if (!this.InvoiceRuleID.HasValue)
				return;

			var rule = InvoiceRule.GetByID(this.InvoiceRuleID.Value);

			if (rule == null)
				return;

			this.ProvisionalAccrualAmount = rule.InvoiceAmountExclVAT;
			this.ProvisionalIncomeAmount = rule.InvoiceAmountExclVAT;
			this.ProjectedAmountExcl = rule.InvoiceAmountExclVAT;
		}

		private void CalculateActivityFixedCostEstimates()
		{
			this.ProvisionalAccrualAmount = 0.0D;
			this.ProvisionalIncomeAmount = 0.0D;
			this.ProjectedAmountExcl = 0.0D;

			if (!this.InvoiceRuleID.HasValue)
				return;

			var rule = InvoiceRule.GetByID(this.InvoiceRuleID.Value);

			if (rule == null)
				return;

			//get all activities for the linked work allocations
			double totalCharge = 0.0D;

			foreach (var activity in this.GetActivitiesFromRule())
			{
				foreach (var userDetail in activity.GetUsersAssigned(this.DateFrom, this.DateTo))
				{
					totalCharge += userDetail.ChargeRate;
				}
			}

			this.ProvisionalAccrualAmount = totalCharge;
			this.ProvisionalIncomeAmount = totalCharge;
			this.ProjectedAmountExcl = totalCharge;
		}

		private void CalculateSLAEstimates()
		{
			if (!this.InvoiceRuleID.HasValue)
				return;

			var rule = InvoiceRule.GetByID(this.InvoiceRuleID.Value);

			this.ProvisionalAccrualAmount = rule.InvoiceAmountExclVAT;
			this.ProvisionalIncomeAmount = rule.InvoiceAmountExclVAT;
			this.ProjectedAmountExcl = rule.InvoiceAmountExclVAT;

			var allocations = InvoiceRepo.GetLinkedWorkAllocations(this.InvoiceID).ToList();
			var tmInvoice = this.GetLinkedInvoice();

			if (tmInvoice != null)
				allocations.AddRange(InvoiceRepo.GetLinkedWorkAllocations(tmInvoice.InvoiceID));

			if ((allocations.Sum(a => a.HoursWorked) > rule.HoursPaidFor) && rule.InvoiceAdditionalHours)
			{
				//if we don't have a TM invoice yet, make it quick...
				if (tmInvoice == null)
					tmInvoice = new Invoice();

				tmInvoice.BillingMethodID = (int)BillingMethodType.TimeMaterial;
				tmInvoice.ClientID = this.ClientID;
				tmInvoice.ClientName = this.ClientName;
				tmInvoice.CustomerRef = this.CustomerRef;
				tmInvoice.DateCreated = this.DateCreated;
				tmInvoice.DateFrom = this.DateFrom;
				tmInvoice.DateInvoiced = this.DateInvoiced;
				tmInvoice.DateTo = this.DateTo;
				tmInvoice.InvoiceContactID = this.InvoiceContactID;
				tmInvoice.InvoiceDate = this.InvoiceDate;
				tmInvoice.InvoiceRuleID = this.InvoiceRuleID;
				tmInvoice.InvoiceStatusID = (int)InvoiceStatusType.Open;
				tmInvoice.InvoiceSubCategoryID = this.InvoiceSubCategoryID;
				tmInvoice.OrderNo = this.OrderNo;
				tmInvoice.OurRef = this.OurRef;
				tmInvoice.Period = this.Period;
				tmInvoice.UserCreated = this.UserCreated;
				tmInvoice.UserInvoiced = this.UserInvoiced;
				tmInvoice.VATRate = this.VATRate;
				tmInvoice.Save();

				tmInvoice.AddContacts(this.Contacts);

				//do we have a previously split allocation?
				WorkAllocation splitAlloc;

				if (allocations.Any(a => a.WorkAllocationType == (int)WorkAllocationType.SLASplit))
				{
					splitAlloc = allocations.Find(a => a.WorkAllocationType == (int)WorkAllocationType.SLASplit);

					foreach (var extraAlloc in allocations.Where(a => a.WorkAllocationType == (int)WorkAllocationType.SLASplit && a.WorkAllocationID != splitAlloc.WorkAllocationID))
					{
						extraAlloc.DeleteInProcess();
					}
				}
				else
				{
					splitAlloc = allocations.SingleOrDefault(a => a.WorkAllocationType == (int)WorkAllocationType.SLASplit);
				}

				if (splitAlloc != null)
				{
					//reset the original work allocation
					var origAlloc = WorkAllocation.GetByID(splitAlloc.ParentWorkAllocationID.Value);
					origAlloc.HoursBilled = null;

					//remove the split from the list and delete it.
					allocations.Remove(splitAlloc);
					splitAlloc.DeleteInProcess();
				}

				//now we first need to split the work allocations into the SLA and T&M
				var tmAllocations = new List<WorkAllocation>();

				double slaHours = 0.0F;
				double tmHours = 0.0F;
				double hoursPaidfor = rule.HoursPaidFor ?? 0.0D;
				WorkAllocation currentAlloc;

				foreach (var alloc in allocations)
				{
					currentAlloc = alloc;
					//reset the invoice ID
					currentAlloc.InvoiceID = this.InvoiceID;

					if (slaHours + currentAlloc.HoursWorked <= rule.HoursPaidFor)
					{
						slaHours += currentAlloc.HoursWorked;
						continue;
					}

					//do we need to split this allocation?
					if ((tmAllocations.Count == 0) && (slaHours + currentAlloc.HoursWorked > hoursPaidfor))
					{
						double diff = slaHours + currentAlloc.HoursWorked - hoursPaidfor;

						//should we split it?
						if (diff > 0)
						{
							//update the current alloc's hours billed only
							currentAlloc.HoursBilled = currentAlloc.HoursWorked - diff;
							InvoiceRepo.SaveWorkAllocation(currentAlloc);

							//we need to create a new allocation for the difference. Zero hours worked but set the billed
							var tmAlloc = new WorkAllocation()
							{
								WorkAllocationType = (int)WorkAllocationType.SLASplit,
								HoursWorked = 0.0D,
								HoursBilled = diff,
								HourlyRate = currentAlloc.HourlyRate,
								ActivityID = currentAlloc.ActivityID,
								DateCreated = DateTime.Now,
								LastModifiedBy = "System",
								DayWorked = currentAlloc.DayWorked,
								Period = currentAlloc.Period,
								Remark = currentAlloc.Remark,
								UserCreated = currentAlloc.UserCreated,
								UserID = currentAlloc.UserID,
								ParentWorkAllocationID = currentAlloc.WorkAllocationID,
							};

							UserRepo.SaveWorkAllocation(tmAlloc);

							currentAlloc = tmAlloc;
						}
					}

					tmHours += currentAlloc.HoursWorked;
					//add it to the list of allocations for T&M
					tmAllocations.Add(currentAlloc);

					//reset hours paid for since it might have changed...
					tmInvoice.HoursPaidFor = (int)tmHours;
					currentAlloc.InvoiceID = tmInvoice.InvoiceID;
				}

				this.Save();

				//calculate the TM values (it's already completely linked up...)
				tmInvoice?.CalculateEstimateValues();
			}
			else if (tmInvoice != null)
			{
				//reset all work allocations
				foreach (var wa in allocations)
					wa.InvoiceID = this.InvoiceID;

				tmInvoice.Delete();
			}
		}

		#endregion PROCESSES

		#region CLIENT

		public Client GetClient(User userRequesting)
		{
			// TODO: this functions should not be public without having the userRequesting as a param...
			if (!userRequesting.CanManage(this))
				return null;

			return this.GetClient();
		}

		internal Client GetClient() => ClientRepo.GetByID(this.ClientID);

		#endregion CLIENT

		#region PROJECT

		public Project GetProject()
		{
			if (!this.ProjectID.HasValue)
				throw new InvoiceException("This invoice is of type 'Fixed Cost' but no project has been assigned to it. Data integrity compromised.");

			return InvoiceRepo.GetProject(this.ProjectID.Value);
		}

		internal List<Project> GetProjects()
		{
			if (this.IsGlobalFixedCost)
				return new List<Project>() { this.GetProject() };

			return InvoiceRepo.GetProjects(this.InvoiceRuleID).OrderBy(p => p.ProjectName).ToList();
		}

		#endregion PROJECT

		#region ACTIVITIES

		public List<ActivityPrintModel> GetActivitiesPrintModel() => InvoiceRepo.GetActivitiesPrintModel(this.InvoiceID).ToList();

		#endregion ACTIVITIES

		#region USERS

		//get distinct list of users linked to the invoice activities
		public List<User> GetAllUsersLinkedToInvoice()
		{
			var model = new List<User>();
			var dateTo = this.DateTo <= this.DateFrom ? DateTime.Today.AddDays(1) : this.DateTo;

			model.AddRange(InvoiceRepo.GetAllUsersLinkedToInvoice(this.InvoiceID));

			return model;
		}

		public List<UserPrintModel> GetUsersPrintModel() => InvoiceRepo.GetUsersPrintModel(this.InvoiceID);

		#endregion USERS

		#region WORK ALLOCATION

		//this method is for display perpos
		public List<WorkAllocationExtendedModel> GetWorkAllocationsExtendedModel()
		{
			var dateTo = this.DateTo <= this.DateFrom ? DateTime.Today.AddDays(1) : this.DateTo;
			var activityIDs = this.GetActivitiesFromRule().Select(a => a.ActivityID).ToList();

			var model = new List<WorkAllocationExtendedModel>();

			model.AddRange(InvoiceRepo.GetLinkedWorkAllocationsExtendedModel(this.InvoiceID));

			if (this.IsOpen || this.IsInProgress)
				model.AddRange(InvoiceRepo.GetNonLinkedWorkAllocationsExtendedModel(activityIDs, dateTo));

			return model.OrderBy(m => m.WorkAllocation.DayWorked)
						.ThenBy(m => m.User.FullName)
						.ToList();
		}

		public List<Activity> GetActivitiesFromRule()
		{
			if (this.IsAdHoc || this.IsNonBillable)
				return new List<Activity>();

			return InvoiceRuleRepo.GetByID(this.InvoiceRuleID.Value).GetActivities();
		}

		public List<WorkAllocation> GetWorkAllocations(User userRequesting)
		{
			if (!userRequesting.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to view these work allocations.");

			return this.GetWorkAllocations();
		}

		internal List<WorkAllocation> GetWorkAllocations()
		{
			var model = new List<WorkAllocation>();

			var dateTo = this.DateTo <= this.DateFrom ? DateTime.Today.AddDays(1) : this.DateTo;

			var activityIDs = this.GetActivitiesFromRule()
										.Select(a => a.ActivityID)
										.ToList();

			model.AddRange(InvoiceRepo.GetLinkedWorkAllocations(this.InvoiceID).ToList());

			if (this.IsOpen || this.IsInProgress)
				model.AddRange(InvoiceRepo.GetNonLinkedWorkAllocations(activityIDs, dateTo).ToList());

			return model;
		}

		public void AddWorkAllocation(WorkAllocation workAllocation)
		{
			workAllocation.InvoiceID = this.InvoiceID;
			workAllocation.HourlyRate = WorkAllocationRepo.GetRate(workAllocation.WorkAllocationID);

			InvoiceRepo.SaveWorkAllocation(workAllocation);
		}

		public void AddWorkAllocations(List<WorkAllocation> workAllocations)
		{
			foreach (var wa in workAllocations)
				this.AddWorkAllocation(wa);
		}

		public void LinkWorkAllocationsAndUpdateCredits(User userExecuting, double invoiceCreditAmount, string linkedWorkAllocations, string credits)
		{
			if (!userExecuting.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to link work allocations to this invoice, nor to update the credits of the work allocations.");

			var workAllocationsToBeLinked = linkedWorkAllocations.ToIntArray();
			var invWorkAllocationCredits = this.GetWorkAllocationCredits();
			var creditsModel = new Dictionary<int, double>();

			foreach (string credit in credits.ToStringArray())
			{
				int index = credit.IndexOf("|");

				if (!int.TryParse(credit.Substring(0, index), out int workAllocationID))
					throw new WorkAllocationException("Error occured while trying to create work allocation credits.");

				if (!double.TryParse(credit.Substring(index + 1, credit.Length - index - 1), out double creditAmount))
					throw new WorkAllocationException("Error occured while trying to create work allocation credits.");

				if (workAllocationsToBeLinked.Contains(workAllocationID))
					creditsModel.Add(workAllocationID, creditAmount);
			}

			using (var ts = new TransactionScope())
			{
				this.UpdateInvoiceCredit(userExecuting, invoiceCreditAmount);
				this.UpdateListOfWorkAllocationsLinkedtoInvoice(workAllocationsToBeLinked);
				this.CreateCredits(userExecuting, creditsModel);

				ts.Complete();
			}
		}

		private void UpdateInvoiceCredit(User userExecuting, double invoiceCreditAmount)
		{
			var invCredit = this.GetInvoiceCreditForInvoiceLevel();

			if (invoiceCreditAmount == 0)
			{
				if (invCredit != null)
					InvoiceRepo.DeleteInvoiceCredit(invCredit);

				return;
			}

			if (invCredit != null)
				invCredit.CreditAmount = invoiceCreditAmount;
			else
				invCredit = this.GetNewInvoiceCredit(userExecuting, invoiceCreditAmount);

			this.SaveInvoiceCredit(userExecuting, invCredit);
		}

		private void SaveInvoiceCredit(User userExecuting, InvoiceCredit invCredit)
		{
			invCredit.LastModifiedBy = userExecuting.FullName;
			InvoiceRepo.SaveInvoiceCredit(invCredit);
		}

		private InvoiceCredit GetNewInvoiceCredit(User userExecuting, double invoiceCreditAmount) => new InvoiceCredit()
		{
			InvoiceCreditID = 0,
			InvoiceID = this.InvoiceID,
			WorkAllocationID = null,
			CreditAmount = invoiceCreditAmount,
			CreatedBy = userExecuting.FullName,
			LastModifiedBy = userExecuting.FullName
		};

		internal void UpdateListOfWorkAllocationsLinkedtoInvoice(List<int> workAllocationsToBeLinked)
		{
			if (this.IsAdHoc)
				throw new InvoiceException("Work Allocations cannot be linked to Ad Hoc invoices.");

			if (this.IsNonBillable)
				throw new InvoiceException("Work Allocations cannot be linked to Non-Billable invoices.");

			if (!this.IsOpen && !this.IsInProgress)
				throw new InvoiceException("This invoice is not in an editable state and therefore, work allocations cannot be removed or added or added to the invoice.");

			var invoiceWorkAllocations = this.GetWorkAllocations();
			var removedAllocations = new List<int>();

			//Get List of workallocationIDs to be removed from invoice
			foreach (var wA in invoiceWorkAllocations)
			{
				if (!workAllocationsToBeLinked.Contains(wA.WorkAllocationID))
					removedAllocations.Add(wA.WorkAllocationID);
			}

			using (var ts = new TransactionScope())
			{
				/**********Link work allocations not linked yet***********/
				var list = new List<WorkAllocation>();

				foreach (int workAllocationID in workAllocationsToBeLinked)
				{
					var wA = WorkAllocationRepo.GetByID(workAllocationID);
					list.Add(wA);
				}

				this.AddWorkAllocations(list);
				////////////////////////////////////////////////////////////

				/**********Remove the linked work allocations no longer to be linked*************/
				list = new List<WorkAllocation>();

				foreach (int workAllocationID in removedAllocations)
				{
					var wA = WorkAllocationRepo.GetByID(workAllocationID);
					list.Add(wA);
				}

				this.RemoveWorkAllocations(list);
				////////////////////////////////////////////////////////////

				ts.Complete();
			}
		}

		private void RemoveWorkAllocations(List<WorkAllocation> list)
		{
			foreach (var wa in list)
			{
				wa.DeleteInvoiceCredit();
				wa.InvoiceID = null;
				InvoiceRepo.SaveWorkAllocation(wa);
			}
		}

		#endregion WORK ALLOCATION

		#region RELATED INVOICES

		public List<Invoice> GetClientRelatedInvoices(Invoice invoice) => InvoiceRepo.GetClientRelatedInvoices(invoice);

		public List<Invoice> GetRuleRelatedInvoices(Invoice invoice) => InvoiceRepo.GetRuleRelatedInvoices(invoice);

		#endregion RELATED INVOICES

		#region INVOICE CREDITS FOR WORK ALLOCATION

		private List<InvoiceCredit> GetWorkAllocationCredits() => InvoiceRepo.GetWorkAllocationCreditsForInvoice(this.InvoiceID)
					.ToList();

		private InvoiceCredit GetInvoiceCreditForWorkAllocation(int workAllocationID) => InvoiceRepo.GetInvoiceCreditForWorkAllocation(this.InvoiceID, workAllocationID);

		private InvoiceCredit GetNewWorkAllocationCredit(User userCreating, int workAllocationID, double creditAmount) => new InvoiceCredit()
		{
			InvoiceID = this.InvoiceID,
			WorkAllocationID = workAllocationID,
			CreditAmount = creditAmount,
			CreatedBy = userCreating.FullName,
			LastModifiedBy = userCreating.FullName
		};

		private void CreateCredits(User userCrediting, Dictionary<int, double> creditsModel) // dictionary: key = workAllocationID, value = credit amount
		{
			if (this.IsAdHoc)
				throw new InvoiceException("Adhoc invoices do not own work allocations and therefore, credits cannot be created on this invoice for work allocations it does not own.");

			if (this.IsNonBillable)
				throw new InvoiceException("Non-billable invoices are impossible.");// we should never get here though - Gavin

			if (this.IsFixedCost)
				throw new InvoiceException("Work allocations on fixed cost invoices cannot be credited. To create a create amount, please credit the invoice, and not the work allocation.");

			if (this.IsMonthlyFixedCost)
				throw new InvoiceException("Work allocations on monthly fixed cost invoices cannot be credited. To create a create amount, please credit the invoice, and not the work allocation.");

			if (!this.IsOpen && !this.IsInProgress)
				throw new InvoiceException("This invoice is not in an editable state and therefore, credits cannot be created for the invoice.");

			var invWorkAllocationCredits = this.GetWorkAllocationCredits();

			using (var ts = new TransactionScope())
			{
				foreach (var credit in creditsModel)
				{
					var invCredit = this.GetNewWorkAllocationCredit(userCrediting, credit.Key, credit.Value);

					if (invWorkAllocationCredits.Select(m => m.WorkAllocationID).Contains(credit.Key))
					{
						invCredit = invWorkAllocationCredits.Single(ic => ic.WorkAllocationID == credit.Key);
						invWorkAllocationCredits.Remove(invCredit);
						invCredit = this.GetInvoiceCreditForWorkAllocation(credit.Key);
						invCredit.CreditAmount = credit.Value;
						invCredit.LastModifiedBy = userCrediting.FullName;
					}

					InvoiceRepo.SaveInvoiceCredit(invCredit);
				}

				foreach (var invCredit in invWorkAllocationCredits)
					InvoiceRepo.DeleteInvoiceCredit(invCredit);

				ts.Complete();
			}
		}

		internal List<InvoiceCredit> GetInvoiceCreditsForWorkAllocations() => InvoiceRepo.GetWorkAllocationCreditsForInvoice(this.InvoiceID)
					.ToList();

		#endregion INVOICE CREDITS FOR WORK ALLOCATION

		#region INVOICE CREDITS

		internal InvoiceCredit GetInvoiceCreditForInvoiceLevel() => InvoiceRepo.GetInvoicCreditsForInvoiceLevel(this.InvoiceID);

		#endregion INVOICE CREDITS

		#region TIME APPROVAL

		public void ApproveTime(User userExecuting)
		{
			if (!userExecuting.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to move the status of an invoice. Action cancelled.");

			var exceptions = InvoiceRepo.GetWorkAllocationExceptions(this.InvoiceID).ToList();

			this.IsTimeApproved = exceptions.Count <= 0;

			#region OLD SLOW CODE

			/*
			//List<User> users = this.GetAllUsersLinkedToInvoice();
			List<int> activityIDs = this.GetActivitiesFromRule().Select(a => a.ActivityID).ToList();
			List<User> users = new List<User>();
			users.AddRange(InvoiceRepo.GetUsersLinkedToInvoicePeriod(activityIDs, this.DateFrom, this.DateTo));
			List<WorkAllocationExceptionModel> userWorkAllocationExceptionList = new List<WorkAllocationExceptionModel>();

			foreach (var ua in users)
			{
				User user = UserRepo.GetByID(ua.UserID);
				if (user.HasRole(UserRoleType.TimeCapturing))
				{
					if (!user.IsTimeCaptured(this.DateFrom, this.DateTo, userExecuting, out userWorkAllocationExceptionList))
					{
						this.IsTimeApproved = false;
						this.Save();
						return;
					}
				}
			}
			this.IsTimeApproved = true;
			this.Save();
			*/

			#endregion OLD SLOW CODE
		}

		public List<WorkAllocationExceptionModel> GetTimeCaptureExceptions() => InvoiceRepo.GetWorkAllocationExceptions(this.InvoiceID).ToList();

		#endregion TIME APPROVAL

		#region PRINT

		public TimesheetTablePrintModel GetTimesheetTable() => new TimesheetTablePrintModel(InvoiceRepo.GetWorkSessionsExtendedModel(this.InvoiceID));

		public TimesheetDetailsPrintModel GetTimesheetDetails() => new TimesheetDetailsPrintModel(InvoiceRepo.GetLinkedWorkAllocationsExtendedModel(this.InvoiceID)
															 .OrderBy(a => a.Activity.ActivityID)
															 .ThenBy(a => a.WorkAllocation.DayWorked)
															 .ThenBy(a => a.User.UserID)
															 .ToList());

		#endregion PRINT
	}

	[Table("vwInvoiceWorkAllocationExceptions")]
	public class InvoiceWorkAllocationException
	{
		[Key]
		public int InvoiceID { get; set; }

		public int InvoiceRuleID { get; set; }
		public int ActivityID { get; set; }
		public string ActivityName { get; set; }
		public int UserID { get; set; }
		public DateTime WorkDate { get; set; }
		public bool IsWorkable { get; set; }
		public double HoursWorked { get; set; }
		public double HoursDiff { get; set; }
		public string Exception { get; set; }
	}
}