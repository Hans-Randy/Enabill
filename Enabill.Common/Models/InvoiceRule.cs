using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("InvoiceRules")]
	public class InvoiceRule
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int InvoiceRuleID { get; internal set; }

		[Required]
		public bool InvoiceAdditionalHours { get; set; }

		[Required]
		public bool PrintCredits { get; set; }

		[Required]
		public bool PrintTimeSheet { get; set; }

		[Required]
		public bool ShowHoursOnInvoice { get; set; }

		[Required]
		public int InvoiceSubCategoryID { get; set; }

		[Required]
		[EnumDataType(typeof(BillingMethodType))]
		public int BillingMethodID { get; set; }

		[Required]
		public int ClientID { get; set; }

		[Required]
		public int DefaultContactID { get; set; }

		[Required]
		public int PrintTicketRemarkOptionID { get; set; }

		public int? AccrualPeriods { get; set; }
		public int? LastInvoiceID { get; set; }
		public int? LastWorkingDay { get; set; }    // Last working day up to which WorkAllocations will be invoiced
		public int? HoursPaidFor { get; set; }
		public int? InvoiceDay { get; set; }        // Typical date on for the invoice document.\
		public int? PrintLayoutTypeID { get; set; }
		public int? PrintOptionTypeID { get; set; }
		public int? ProjectID { get; set; }

		public double? InvoiceAmountExclVAT { get; set; }

		[MaxLength(500)]
		public string Description { get; set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; set; }

		[MaxLength(50)]
		public string OrderNo { get; set; }

		[Required, MinLength(3), MaxLength(50)]
		public string UserCreated { get; set; }

		[Required]
		public DateTime DateCreated { get; set; }

		[Required]
		public DateTime DateFrom { get; set; }

		public DateTime? ConfirmedEndDate { get; internal set; }
		public DateTime? DateTo { get; set; }
		public DateTime? LastInvoiceDate { get; set; }

		public virtual Client Client { get; internal set; }
		public virtual Project Project { get; internal set; }
		public virtual InvoiceSubCategory InvoiceSubCategory { get; internal set; }

		#endregion PROPERTIES

		#region INITIALIZATION

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
		public IEnumerable<InvoiceRuleLine> InvoiceRuleLines => InvoiceRuleLineRepo.FetchByInvoiceRule(this.InvoiceRuleID);

		[NotMapped]
		public IEnumerable<Contact> Contacts => InvoiceRuleRepo.GetContacts(this.InvoiceRuleID);

		[NotMapped]
		public InvoiceCategory InvoiceCategory => InvoiceCategoryRepo.GetByID(this.InvoiceSubCategory.InvoiceCategoryID);

		[NotMapped]
		public bool IsActive => !this.DateTo.HasValue || this.DateTo.Value > DateTime.Now;

		[NotMapped]
		public bool CanEdit => this.IsActive;

		[NotMapped]
		public bool CanDelete => this.GetInvoices().Count <= 0;

		[NotMapped]
		//public string ProjectName => this.ProjectID.HasValue
		//	? this.GetProjects().SingleOrDefault(p => p.Project.ProjectID == this.ProjectID).Project.ProjectName
		//	: "";

		public string ProjectName => this.ProjectID.HasValue
			? this.GetProjects().SingleOrDefault(p => p.Project.ProjectID == this.ProjectID).Project.ProjectName
			: "";

		[NotMapped]
		public List<string> ProjectNames => this.ProjectID.HasValue
					? new List<string>() { this.GetProjects().SingleOrDefault(p => p.Project.ProjectID == this.ProjectID).Project.ProjectName }
					: this.GetProjects().Select(p => p.Project.ProjectName).ToList();

		[NotMapped]
		public bool CanCreateInvoiceFromRule
		{
			get
			{
				if (this.GetInvoices().Any(i => i.InvoiceStatusID == (int)InvoiceStatusType.Open))
					return false;

				if (this.IsFixedCost)
				{
					var costLines = this.GetInvoiceRuleLines();
					if ((costLines.Sum(l => l.AccrualAmount) != this.InvoiceAmountExclVAT)
						|| (costLines.Sum(l => l.CustomerAmount) != this.InvoiceAmountExclVAT))
					{
						return false;
					}
				}

				//else
				{
					if (this.GetActivities().Count == 0)
						return false;
				}

				return true;
			}
		}

		#endregion INITIALIZATION

		#region INVOICERULE

		public static InvoiceRule GetByID(int id) => InvoiceRuleRepo.GetByID(id);

		public static List<InvoiceRule> GetAll(DateTime refDate) => InvoiceRuleRepo.GetAll(refDate)
					.ToList();

		public static List<InvoiceRule> GetActive() => InvoiceRuleRepo.GetAll(FinPeriod.GetCurrentFinPeriod().FinPeriodID).ToList();

		public static InvoiceRule GetDefaultRule(User userCreating, Client client, Project project, BillingMethodType billingType)
		{
			switch (billingType)
			{
				case BillingMethodType.FixedCost:
					return GetDefaultFixedCostRule(userCreating, client, project);

				case BillingMethodType.MonthlyFixedCost:
					return GetDefaultMonthlyFixedCostRule(userCreating, client, project);

				case BillingMethodType.TimeMaterial:
					return GetDefaultTimeMaterialRule(userCreating, client, project);

				case BillingMethodType.SLA:
					return GetDefaultSLARule(userCreating, client, project);

				case BillingMethodType.ActivityFixedCost:
					return GetDefaultActivityFixedCostRule(userCreating, client, project);

				default:
					throw new InvoiceRuleException("No valid method");
			}
		}

		//public static InvoiceRule GetDefaultRule(User userCreating, Client client, BillingMethodType billingType)
		//{
		//	switch (billingType)
		//	{
		//		case BillingMethodType.TimeMaterial:
		//			return GetDefaultTimeMaterialRule(userCreating, client);

		//		case BillingMethodType.SLA:
		//			return GetDefaultSLARule(userCreating, client);

		//		case BillingMethodType.ActivityFixedCost:
		//			return GetDefaultActivityFixedCostRule(userCreating, client);

		//		default:
		//			throw new InvoiceRuleException("No valid method");
		//	}
		//}

		internal static InvoiceRule GetDefaultFixedCostRule(User userCreating, Client client, Project project) => new InvoiceRule()
		{
			ClientID = client.ClientID,
			BillingMethodID = (int)BillingMethodType.FixedCost,
			UserCreated = userCreating.UserName,
			DateCreated = DateTime.Now,
			DateFrom = DateTime.Today.ToFirstDayOfMonth(),
			InvoiceAdditionalHours = true,
			ShowHoursOnInvoice = true,
			InvoiceAmountExclVAT = project.ProjectValue ?? 0.0D,
			ProjectID = project.ProjectID,
			AccrualPeriods = 1,
			PrintOptionTypeID = 5, //Default Description Only
			PrintLayoutTypeID = 3,
			PrintCredits = false,
			PrintTimeSheet = false,
			
		};

		internal static InvoiceRule GetDefaultMonthlyFixedCostRule(User userCreating, Client client, Project project) => new InvoiceRule()
		{
			ClientID = client.ClientID,
			BillingMethodID = (int)BillingMethodType.MonthlyFixedCost,
			UserCreated = userCreating.UserName,
			DateCreated = DateTime.Now,
			DateFrom = DateTime.Today.ToFirstDayOfMonth(),
			InvoiceAdditionalHours = true,
			ShowHoursOnInvoice = true,
			InvoiceAmountExclVAT = project.ProjectValue ?? 0.0D,
			ProjectID = project.ProjectID,
			AccrualPeriods = 0,
			PrintOptionTypeID = 1,
			PrintLayoutTypeID = 3,
			PrintCredits = false,
			PrintTimeSheet = true
		};

		internal static InvoiceRule GetDefaultActivityFixedCostRule(User userCreating, Client client, Project project) => new InvoiceRule()
		{
			ClientID = client.ClientID,
			BillingMethodID = (int)BillingMethodType.ActivityFixedCost,
			UserCreated = userCreating.UserName,
			DateCreated = DateTime.Now,
			DateFrom = DateTime.Today.ToFirstDayOfMonth(),
			InvoiceAdditionalHours = true,
			ShowHoursOnInvoice = true,
			InvoiceAmountExclVAT = 0.0D,
			ProjectID = project.ProjectID,
			AccrualPeriods = 0,
			PrintOptionTypeID = 5,
			PrintLayoutTypeID = 3,
			PrintCredits = false,
			PrintTimeSheet = true
		};

		internal static InvoiceRule GetDefaultTimeMaterialRule(User userCreating, Client client, Project project) => new InvoiceRule()
		{
			ClientID = client.ClientID,
			BillingMethodID = (int)BillingMethodType.TimeMaterial,
			UserCreated = userCreating.UserName,
			DateCreated = DateTime.Now,
			DateFrom = DateTime.Today.ToFirstDayOfMonth(),
			InvoiceAdditionalHours = true,
			ProjectID = project.ProjectID,
			ShowHoursOnInvoice = true,
			PrintOptionTypeID = 1,
			PrintLayoutTypeID = 3,
			PrintCredits = false,
			PrintTimeSheet = true
		};

		internal static InvoiceRule GetDefaultSLARule(User userCreating, Client client, Project project) => new InvoiceRule()
		{
			ClientID = client.ClientID,
			BillingMethodID = (int)BillingMethodType.SLA,
			UserCreated = userCreating.UserName,
			DateCreated = DateTime.Now,
			DateFrom = DateTime.Today.ToFirstDayOfMonth(),
			InvoiceAdditionalHours = true,
			ShowHoursOnInvoice = true,
			InvoiceAmountExclVAT = 0,
			ProjectID = project.ProjectID,
			HoursPaidFor = 0,
			PrintOptionTypeID = 5,
			PrintLayoutTypeID = 3,
			PrintCredits = false,
			PrintTimeSheet = true
		};

		public void Save(User userSaving)
		{
			if (!userSaving.HasRole(UserRoleType.InvoiceAdministrator) && !userSaving.HasRole(UserRoleType.SystemAdministrator))
				throw new UserRoleException("You do not have the required permissions to create/edit an invoice rule. Save cancelled.");

			if (this.BillingMethodID == (int)BillingMethodType.AdHoc)
				throw new InvoiceRuleException("AdHoc invoices cannot be made from a rule, therefore, AdHoc invoice rules are not saved.");

			if (this.DateFrom < EnabillSettings.SiteStartDate)
				this.DateFrom = EnabillSettings.SiteStartDate;

			if (this.DateTo.HasValue && this.DateTo < this.DateFrom)
				throw new InvoiceRuleException("The deactivation date cannot be set to a date before the activation date. Action cancelled.");

			if (this.ClientID == 0)
				throw new InvoiceRuleException("You must select a valid client for the invoice rule.");

			this.Client = ClientRepo.GetByID(this.ClientID);
			if (this.Client == null)
				throw new InvoiceRuleException("You must select a valid client for the invoice rule.");

			if (!this.Client.IsActive)
				throw new InvoiceRuleException("You cannot create an invoice rule for an inactive client.");

			if (this.DefaultContactID == 0)
				throw new InvoiceRuleException("You must select a client contact for the invoice rule.");

			/*
			DefaultContact = ContactRepo.GetByID(this.DefaultContactID);
			if (DefaultContact == null)
				throw new InvoiceRuleException("You must select a valid Contact for the invoice rule.");

			if (DefaultContact.ClientID != ClientID)
				throw new InvoiceRuleException("You must select a Contact from " + Client.ClientName + " for the invoice rule.");

			if (!DefaultContact.IsActive)
				throw new InvoiceRuleException("You must select an active contact on this invoice rule.");
			*/

			//if (this.BillingMethodID == (int)BillingMethodType.FixedCost)
			//    if (this.GetInvoices().Count > 0)
			//    {
			//        InvoiceRule origInvRule = InvoiceRuleRepo.GetByID(this.InvoiceRuleID);
			//        this.InvoiceAmountExclVAT = origInvRule.InvoiceAmountExclVAT;
			//        this.AccrualPeriods = origInvRule.AccrualPeriods;
			//    }

			this.InvoiceSubCategory = InvoiceSubCategoryRepo.GetByID(this.InvoiceSubCategoryID);
			if (this.InvoiceSubCategory == null)
				throw new InvoiceRuleException("You must select a valid Invoice Sub Category.");

			if (this.IsGlobalFixedCost && this.InvoiceAmountExclVAT == 0)
				throw new InvoiceRuleException("You must specify an invoice amount exclusive of VAT for a fixed cost invoice rule. Save cancelled.");

			if ((this.BillingMethodID == (int)BillingMethodType.SLA && this.HoursPaidFor == null) || this.HoursPaidFor == 0)
				throw new InvoiceRuleException("You must specifiy the amount of hours covered by the SLA rule");

			if (this.InvoiceRuleID == 0)
			{
				this.DateCreated = DateTime.Now.ToUniversalTime().AddHours(2);
				this.UserCreated = userSaving.FullName;
			}

			this.LastModifiedBy = userSaving.FullName;
			InvoiceRuleRepo.Save(this);
		}

		public void Delete(User userRequesting)
		{
			if (!userRequesting.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to delete an invoice rule. Delete cancelled.");

			this.Delete();
		}

		private void Delete() => InvoiceRuleRepo.Delete(this);

		public static List<InvoiceRule> GetForClient(int clientID) => InvoiceRuleRepo.GetForClient(clientID)
					.ToList();

		public static List<InvoiceRule> GetForClient(int clientID, BillingMethodType billingMethodType, bool status) => InvoiceRuleRepo.GetForClient(clientID, (int)billingMethodType, status)
					.ToList();

		public static List<InvoiceRule> GetForClient(int clientID, int billingMethodBWTotal, bool status) => InvoiceRuleRepo.GetForClient(clientID, billingMethodBWTotal, status)
					.ToList();

		public static List<InvoiceRule> GetForClientAndProject(int clientID, int billingMethodBWTotal, bool status, int projectID) => InvoiceRuleRepo.GetForClientAndProject(clientID, billingMethodBWTotal, status, projectID)
		.ToList();

		#endregion INVOICERULE

		#region INVOICE RULE LINES

		internal void AddInvoiceRuleLine(InvoiceRuleLine line)
		{
			if (!this.IsFixedCost)
				throw new InvoiceException("Cannot add invoice period for non Fixed-cost billing methods.");

			if (InvoiceRuleLineRepo.FetchByInvoiceRule(this.InvoiceRuleID).Any(l => l.Period == line.Period))
				throw new InvoiceException(string.Format($"There is already a line configured for period {line.Period}"));

			InvoiceRuleLineRepo.Save(line);
		}

		public void GenerateInvoiceRuleLines(User userGenerating)
		{
			if (!this.IsFixedCost)
				return;

			if (!userGenerating.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to manage this invoice rule. Action cancelled.");

			if (!this.CanEdit)
				throw new InvoiceRuleException("This invoice rule can not be edited as it is inactive. Action cancelled.");

			if (this.GetInvoiceRuleLines().Count > 0)
			{
				if (this.InvoiceRuleLines.Any(irl => irl.InvoiceID.HasValue))
				{
					//throw new InvoiceRuleException("Cannot alter accrual line rules. Invoices has already been made using the existing configuration.");
					this.UpdateInvoiceRuleLines();

					return;
				}

				InvoiceRuleLineRepo.DeleteRange(this.InvoiceRuleLines.ToList());
			}

			var project = this.GetProjects().FirstOrDefault();

			if (project == null)
				return;

			int period = 1;

			InvoiceRuleLine line;

			while (period <= this.AccrualPeriods)
			{
				line = new InvoiceRuleLine();

				if (this.AccrualPeriods == 1)
					line.CustomerAmount = this.InvoiceAmountExclVAT ?? 0;
				else if ((period == 1) || (period == this.AccrualPeriods))
					line.CustomerAmount = (this.InvoiceAmountExclVAT ?? 0.0D) / 2.0D;
				else
					line.CustomerAmount = 0.0D;

				line.InvoiceRuleID = this.InvoiceRuleID;
				line.DefaultDescription += project.Project.ProjectName;

				if (this.AccrualPeriods.HasValue && (this.AccrualPeriods.Value > 0) && this.InvoiceAmountExclVAT.HasValue)
					line.AccrualAmount += Math.Round(this.InvoiceAmountExclVAT.Value / this.AccrualPeriods.Value, 2);
				else
					line.AccrualAmount = 0.0D;

				line.Period = period;

				this.AddInvoiceRuleLine(line);

				period++;
			}
		}

		private void UpdateInvoiceRuleLines()
		{
			int originalCount = this.InvoiceRuleLines.Count();
			int newCount = this.AccrualPeriods.Value;
			int difference = newCount - originalCount;

			if (difference == 0)
			{
				return;
			}
			else if (difference > 0)
			{
				for (int k = 1; k <= difference; k++)
				{
					this.AddInvoiceRuleLine(new InvoiceRuleLine()
					{
						InvoiceRuleID = this.InvoiceRuleID,
						Period = originalCount + k,
						CustomerAmount = 0,
						AccrualAmount = 0,
						DefaultDescription = "Fixed cost line",
						InvoiceID = null
					});
				}

				return;
			}

			//else if less
			//TODO: this is going to require much logic etc

			var remainingLines = this.InvoiceRuleLines.Where(irl => irl.Period <= newCount).ToList();

			//if any of the lines in the list that would remain after the change is not yet invoiced, then we just delete
			if (remainingLines.Any(irl => !irl.InvoiceID.HasValue))
			{
				InvoiceRuleLineRepo.DeleteRange(this.InvoiceRuleLines.Where(irl => irl.Period > newCount).ToList());

				return;
			}

			//else we need to check the values to validate any deletes
			if (remainingLines.Sum(irl => irl.CustomerAmount) == this.InvoiceAmountExclVAT && remainingLines.Sum(irl => irl.AccrualAmount) == this.InvoiceAmountExclVAT)
			{
				InvoiceRuleLineRepo.DeleteRange(this.InvoiceRuleLines.Where(irl => irl.Period > newCount).ToList());

				return;
			}

			throw new InvoiceRuleException("Error");
		}

		internal List<InvoiceRuleLine> GetInvoiceRuleLines() => InvoiceRuleRepo.GetInvoiceRuleLines(this.InvoiceRuleID).OrderBy(irl => irl.Period).ToList();

		internal void SaveInvoiceRuleLine(InvoiceRuleLine line)
		{
			if (line == null)
				return;

			var lines = new List<InvoiceRuleLine> { line };

			this.SaveInvoiceRuleLines(lines);
		}

		public void SaveInvoiceRuleLines(List<InvoiceRuleLine> lines)
		{
			var editableLines = lines.Where(l => l.CanEdit).ToList();
			InvoiceRuleLineRepo.SaveAll(lines);
		}

		#endregion INVOICE RULE LINES

		#region INVOICE

		public List<Invoice> GetInvoices() => InvoiceRuleRepo.GetInvoices(this.InvoiceRuleID).ToList();

		public Invoice GetCurrentInvoice() => InvoiceRuleRepo.GetInvoiceForDate(this.InvoiceRuleID, DateTime.Now);

		public Invoice GetLatestInvoice() => InvoiceRuleRepo.GetInvoices(this.InvoiceRuleID).OrderByDescending(i => i.DateTo).FirstOrDefault();

		public Invoice GetLatestInvoiceForFinPeriod(FinPeriod period) => InvoiceRuleRepo.GetInvoices(this.InvoiceRuleID).Where(i => i.Period == period.FinPeriodID).OrderByDescending(i => i.DateTo).FirstOrDefault();

		public Invoice GetLatestOpenInvoice() => InvoiceRuleRepo.GetInvoices(this.InvoiceRuleID).Where(i => i.InvoiceStatusID == (int)InvoiceStatusType.Open).OrderByDescending(i => i.DateTo).FirstOrDefault();

		/// <summary>
		/// Determines the next invoice period. Item1 is the dateFrom, Item2 is the dateTo and Item3 is the period - only used for Fixed Cost invoicing
		/// </summary>
		/// <returns></returns>
		public Tuple<DateTime, DateTime, int> GetNextInvoicePeriod()
		{
			var currentInvoice = this.GetLatestInvoice(); //this.GetCurrentInvoice();

			//determine the next period to invoice
			DateTime dateFrom;
			DateTime dateTo;
			int period;

			//determine the next invoice date and period from the current invoice
			if (currentInvoice == null)
			{
				dateFrom = this.DateFrom;
				period = 1;
			}
			else
			{
				dateFrom = currentInvoice.DateTo.AddDays(1);
				period = (int)Math.Ceiling((dateFrom.AddDays(15) - this.DateFrom).TotalDays / 30.4);
			}

			dateTo = dateFrom.AddMonths(1).AddDays(-1);

			return new Tuple<DateTime, DateTime, int>(dateFrom, dateTo, period);
		}

		public Invoice CreateInvoice(User userCreating, DateTime invoiceDate)
		{
			if (!userCreating.CanManage(this))
				throw new InvoiceException("You do not have the required permissions to create invoices.");

			if (!this.Client.IsActive)
				throw new InvoiceRuleException("Invoices cannot be created for inactive clients");

			if (this.GetActivities(userCreating).Count == 0 && !this.IsGlobalFixedCost)
				throw new InvoiceException("Invoices cannot be created from an invoice rule that does not have activities linked to it.");

			var currentInvoice = this.GetCurrentInvoice();

			if (currentInvoice?.InvoiceStatusID == (int)InvoiceStatusType.Open)
				throw new InvoiceException("An open invoice made from this invoice rule is still open and must be closed before a new invoice can be created from this rule.");

			if (this.DateTo.HasValue && (currentInvoice != null) && (currentInvoice.DateTo >= this.DateTo.Value))
				throw new InvoiceException(string.Format($"The last period for this rule has already been invoiced. See invoice {currentInvoice.InvoiceID}."));

			// Determine the next period to invoice
			DateTime dateFrom;
			DateTime dateTo;
			int period;

			var nextPeriod = this.GetNextInvoicePeriod();
			dateFrom = nextPeriod.Item1;
			dateTo = nextPeriod.Item2;
			period = nextPeriod.Item3;

			// Now that we have the period, get the appropriate invoice rule line
			var line = InvoiceRuleLineRepo.FetchByInvoiceRulePeriod(this.InvoiceRuleID, period);

			double amountToInvoice = 0.0D;
			double amountToAccrual = 0.0D;

			if (this.IsFixedCost)
			{
				if (line == null)
				{
					throw new InvoiceException("No invoice rule periods configured for the fixed cost rule");
				}
				else
				{
					// Use the line to determine how much should be invoiced...
					amountToInvoice = line.CustomerAmount;
					amountToAccrual = line.AccrualAmount;
				}
			}
			else if (this.IsMonthlyFixedCost)
			{
				amountToInvoice = this.InvoiceAmountExclVAT ?? 0.0D;
				amountToAccrual = this.InvoiceAmountExclVAT ?? 0.0D;
			}
			else
			{
				amountToInvoice = this.InvoiceAmountExclVAT ?? 0.0D;
				amountToAccrual = 0.0D;
			}

			var currentPeriod = FinPeriod.GetCurrentFinPeriod();

			var invoice = new Invoice
			{
				InvoiceRuleID = this.InvoiceRuleID,
				ProjectID = this.ProjectID,
				BillingMethodID = this.BillingMethodID,
				InvoiceSubCategoryID = this.InvoiceSubCategoryID,
				ClientID = this.ClientID,
				InvoiceContactID = this.DefaultContactID,
				ClientName = this.Client.ClientName,
				//InvoiceContactName = this.GetDefaultContact().ContactName,
				InvoiceContactName = Helpers.DecryptString(this.GetDefaultContact().ContactName, Enabill.Code.Constants.PASSPHRASE),
				OrderNo = this.OrderNo,
				DateFrom = dateFrom,
				DateTo = dateTo,
				InvoiceStatusID = (int)InvoiceStatusType.Open,
				InvoiceDate = invoiceDate.Date,
				Period = currentPeriod.FinPeriodID, //invoiceDate.ToPeriod(),

				Description = this.Description,

				HoursPaidFor = this.HoursPaidFor,
				VATRate = this.Client.GetVATRate(),

				ProvisionalAccrualAmount = 0,
				ProvisionalIncomeAmount = 0,
				ProjectedAmountExcl = 0,

				InvoiceAmountExclVAT = 0,
				AccrualExclVAT = 0,

				PrintOptionTypeID = this.PrintOptionTypeID,
				PrintLayoutTypeID = this.PrintLayoutTypeID,
				PrintCredits = this.PrintCredits,
				PrintTimeSheet = this.PrintTimeSheet,
				PrintTicketRemarkOptionID = this.PrintTicketRemarkOptionID
			};

			invoice.Save(userCreating);

			// Add the contacts
			invoice.AddContacts(this.Contacts);

			// Save the rule line since the invoiceLineID has been updated
			if (invoice.IsFixedCost)
				line.InvoiceID = invoice.InvoiceID;

			// Save the rule line since the invoiceLineID has been updated
			this.SaveInvoiceRuleLine(line);

			return invoice;
		}

		#endregion INVOICE

		#region PROJECTS

		public void AddProject(Project project)
		{
			if (!this.IsGlobalFixedCost)
				throw new InvoiceRuleException("Projects can only be assigned to a Fixed Cost rule.");

			if ((this.BillingMethodID != (int)BillingMethodType.TimeMaterial)
				&& (this.GetProjects().Count > 1))
			{
				throw new InvoiceRuleException("Multiple projects are only allowed on Time & Material invoice rules.");
			}

			this.ProjectID = project.ProjectID;
		}

		internal List<ProjectSelectModel> GetProjects()
		{
			var model = new List<ProjectSelectModel>();
			var projects = new List<Project>();

			if (this.ProjectID.HasValue)
			{
				model.Add(new ProjectSelectModel()
				{
					Project = ProjectRepo.GetByID(this.ProjectID.Value),
					IsSelected = true
				});
			}
			else
			{
				foreach (var activity in this.GetActivities())
				{
					var project = activity.GetProject();
					if (!projects.Contains(project))
						projects.Add(project);
				}

				projects.ForEach(p => model.Add
				(
					new ProjectSelectModel() { Project = p, IsSelected = true }
				));
			}

			return model;
		}

		public void UpdateProject(int projectID)
		{
			if (!this.IsGlobalFixedCost)
				throw new InvoiceRuleException("Projects can only be added to a Fixed Cost invoice rule.");

			if (!this.GetProjects().Any(psm => psm.Project.ProjectID == projectID))
				return;

			var p = ProjectRepo.GetByID(projectID);
			this.AddProject(p);
		}

		public Project GetProject()
		{
			if (!this.ProjectID.HasValue)
				return null;

			if (!this.IsGlobalFixedCost)
				return null;

			return ProjectRepo.GetByID(this.ProjectID.Value);
		}

		#endregion PROJECTS

		#region ACTIVITIES

		public List<Activity> GetActivities(User userRequesting)
		{
			if (!userRequesting.CanManage(this))
				return new List<Activity>();

			return this.GetActivities();
		}

		internal List<Activity> GetActivities()
		{
			//Fixed Cost InvoiceRules store a projectID and select activities for the project
			if (this.IsGlobalFixedCost)
			{
				if (!this.ProjectID.HasValue)
					throw new InvoiceRuleException("This invoice rule is a fixed cost invoice rule, but has not been linked to a project. Please discuss this with an administrator.");

				return InvoiceRuleRepo.GetActivitiesForProject(this.ProjectID.Value).OrderBy(a => a.ActivityName).ToList();
			}

			return InvoiceRuleRepo.GetActivities(this.InvoiceRuleID).ToList();
		}

		public List<ActivityDetail> GetDetailedActivities()
		{
			if (this.InvoiceRuleID == 0)
				return new List<ActivityDetail>();

			return ClientRepo.GetActivitiesByBMForInvRule(this.ClientID, this.InvoiceRuleID, (BillingMethodType)this.BillingMethodID)
				.OrderBy(a => a.ActivityName)
				.ToList();
		}

		public void AddActivity(Activity activity)
		{
			if (this.IsGlobalFixedCost)
				throw new InvoiceRuleException("You cannot assign activities to a fixed cost rule. Only projects can be assigned to rules of type 'Fixed Cost'.");

			var project = activity.GetProject();

			if (this.BillingMethodID != project.BillingMethodID)
			{
				throw new InvoiceRuleException("This invoice rule is set as a/an " + ((BillingMethodType)this.BillingMethodID)
						+ ". Only activities of the same billing method type can be linked to this invoice rule.");
			}

			if (this.ClientID != project.ClientID)
				throw new InvoiceRuleException("You cannot link activities from another client to an invoice rule for this client.");

			if (ActivityRepo.IsActivityAssignedToAnInvoiceRule(activity.ActivityID))
			{
				if (ActivityRepo.IsActivityAssignedToThisInvoiceRule(this.InvoiceRuleID, activity.ActivityID))
					return;
				else
					throw new InvoiceRuleException("The activity being linked to this invoice rule already belongs to another invoice rule.");
			}

			InvoiceRuleRepo.AddActivity(this.InvoiceRuleID, activity.ActivityID);
		}

		public void AddActivities(IEnumerable<Activity> activities)
		{
			if (this.IsGlobalFixedCost)
				throw new InvoiceRuleException("You cannot assign activities to a fixed cost rule. Only projects can be assigned to rules of type 'Fixed Cost'.");

			foreach (var activity in activities)
			{
				var project = activity.GetProject();

				if (this.BillingMethodID != project.BillingMethodID)
				{
					throw new InvoiceRuleException("This invoice rule is set as a/an " + ((BillingMethodType)this.BillingMethodID)
							+ ". Only activities of the same billing method type can be linked to this invoice rule.");
				}

				if (this.ClientID != project.ClientID)
					throw new InvoiceRuleException("You cannot link activities from another client to an invoice rule for this client.");

				if (ActivityRepo.IsActivityAssignedToAnInvoiceRule(activity.ActivityID))
				{
					if (ActivityRepo.IsActivityAssignedToThisInvoiceRule(this.InvoiceRuleID, activity.ActivityID))
						return;
					else
						throw new InvoiceRuleException("The activity being linked to this invoice rule already belongs to another invoice rule.");
				}
			}

			InvoiceRuleRepo.AddActivities(this.InvoiceRuleID, activities);
		}

		public void AddActivities(string activitiesString)
		{
			if
			(
				this.BillingMethodID != (int)BillingMethodType.AdHoc
				&& this.BillingMethodID != (int)BillingMethodType.SLA
				&& this.BillingMethodID != (int)BillingMethodType.TimeMaterial
				&& this.BillingMethodID != (int)BillingMethodType.Travel
				&& this.BillingMethodID != (int)BillingMethodType.ActivityFixedCost
			)
			{
				throw new InvoiceRuleException("You cannot assign activities to this rule as it is not of type 'Time & Material(T&M)', 'Travel', 'Ad Hoc' or 'SLA'.");
			}

			var activityIDs = activitiesString.ToIntArray();

			var activities = new List<Activity>();

			foreach (int activityID in activityIDs.Distinct())
			{
				var activity = ActivityRepo.GetByID(activityID);

				if (activity == null)
					throw new ActivityAdminException("An activity could not be found.");

				var project = activity.GetProject();

				if (this.BillingMethodID != project.BillingMethodID)
				{
					throw new InvoiceRuleException("This invoice rule is set as a/an " + ((BillingMethodType)this.BillingMethodID)
							+ ". Only activities of the same billing method type can be linked to this invoice rule.");
				}

				if (this.ClientID != project.ClientID)
					throw new InvoiceRuleException("You cannot link activities from another client to an invoice rule for this client.");

				if (ActivityRepo.IsActivityAssignedToAnInvoiceRule(activity.ActivityID))
				{
					if (ActivityRepo.IsActivityAssignedToThisInvoiceRule(this.InvoiceRuleID, activity.ActivityID))
						continue;
					else
						throw new InvoiceRuleException("The activity being linked to this invoice rule already belongs to another invoice rule.");
				}

				activities.Add(activity);
			}

			InvoiceRuleRepo.AddActivities(this.InvoiceRuleID, activities);
		}

		public void UpdateActivities(string activityIDsString)
		{
			var activityIDs = activityIDsString.ToIntArray();
			var activities = this.GetActivities().Select(a => a.ActivityID).ToList();

			activities = activities.Except(activityIDs).ToList();

			this.AddActivities(activityIDsString);
			this.RemoveActivities(activities);
		}

		private void RemoveActivities(List<int> activities) => InvoiceRuleRepo.RemoveActivities(this.InvoiceRuleID, activities);

		#endregion ACTIVITIES

		#region CONTACTS

		public void AddBccContact(Contact contact)
		{
			if (contact.ClientID != this.ClientID)
				throw new InvoiceRuleException(contact.ContactName + " is not a contact of " + this.Client.ClientName + ". Adding of contact cancelled.");

			if (this.DefaultContactID == contact.ContactID)
				throw new InvoiceRuleException(contact.ContactName + " is set as the default contact for this rule and cannot be added to the email BCC list");

			InvoiceRuleRepo.AddBccContact(this.InvoiceRuleID, contact.ContactID);
		}

		public void AddBccContacts(IEnumerable<Contact> contacts)
		{
			foreach (var contact in contacts)
			{
				if (contact.ClientID != this.ClientID)
					throw new InvoiceRuleException(contact.ContactName + " is not a contact of " + this.Client.ClientName + ". Adding of contacts cancelled.");

				if (this.DefaultContactID == contact.ContactID)
					throw new InvoiceRuleException(contact.ContactName + " is set as the default contact for this rule and cannot be added to the email BCC list");
			}

			InvoiceRuleRepo.AddBccContacts(this.InvoiceRuleID, contacts);
		}

		public void AddBccContacts(string contactIDString)
		{
			var contactIDs = contactIDString.ToIntArray();
			var contacts = new List<Contact>();

			foreach (int contactID in contactIDs.Distinct())
			{
				var contact = ContactRepo.GetByID(contactID);

				if (contact.ClientID != this.ClientID)
					throw new InvoiceRuleException(contact.ContactName + " is not a contact of " + this.Client.ClientName + ". Adding of contacts cancelled.");

				if (this.DefaultContactID == contact.ContactID)
					continue;
				//throw new InvoiceRuleException(contact.ContactName + " is set as the default contact for this rule and cannot be added to the email BCC list");

				contacts.Add(contact);
			}

			InvoiceRuleRepo.AddBccContacts(this.InvoiceRuleID, contacts);
		}

		public void UpdateBccContacts(string contactIDsString)
		{
			var contactIDs = contactIDsString.ToIntArray();
			var contacts = this.Contacts.Select(c => c.ContactID).ToList();

			contacts = contacts.Except(contactIDs).ToList();

			this.AddBccContacts(contactIDsString);
			this.RemoveBccContacts(contacts);
		}

		private void RemoveBccContacts(List<Contact> contacts) => InvoiceRuleRepo.RemoveBccContacts(this.InvoiceRuleID, contacts);

		private void RemoveBccContacts(List<int> contactIDs) => InvoiceRuleRepo.RemoveBccContacts(this.InvoiceRuleID, contactIDs);

		public Contact GetDefaultContact() => ContactRepo.GetByID(this.DefaultContactID);

		#endregion CONTACTS
	}
}