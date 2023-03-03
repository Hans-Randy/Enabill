using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

//using System.Data.Objects;

namespace Enabill.Repos
{
	public abstract class InvoiceRepo : BaseRepo
	{
		#region INVOICE SPECIFIC

		internal static IEnumerable<Invoice> GetAll(DateTime startDate, DateTime endDate) => DB.Invoices
					.Where(i => (i.DateInvoiced ?? i.DateCreated) >= startDate && (i.DateInvoiced ?? i.DateCreated) <= endDate);

		internal static void Save(Invoice invoice)
		{
			if (invoice.InvoiceID == 0)
				DB.Invoices.Add(invoice);

			DB.SaveChanges();
		}

		internal static void Delete(Invoice invoice)
		{
			DeleteAllInvoiceContacts(invoice.InvoiceID);
			DeleteAllInvoiceCredit(invoice.InvoiceID);

			foreach (var allocation in GetLinkedWorkAllocations(invoice.InvoiceID).ToList())
			{
				allocation.InvoiceID = null;
			}

			DB.SaveChanges();

			var line = InvoiceRuleLineRepo.FetchByInvoice(invoice.InvoiceID);

			if (line != null)
			{
				line.InvoiceID = null;
				InvoiceRuleLineRepo.Save(line);
			}

			DB.Invoices.Remove(invoice);
			DB.SaveChanges();
		}

		internal static Invoice GetByID(int invoiceID) => DB.Invoices
					.SingleOrDefault(i => i.InvoiceID == invoiceID);

		internal static List<Invoice> GetAllOpen() => DB.Invoices
					.Where(i => i.InvoiceStatusID == (int)InvoiceStatusType.Open)
					.ToList();

		internal static Invoice GetLinkedInvoice(int finPeriodID, int invoiceRuleID, int thisBillingMethod) => DB.Invoices.Where(i => i.InvoiceRuleID == invoiceRuleID && i.Period == finPeriodID && i.BillingMethodID != thisBillingMethod).SingleOrDefault();

		#endregion INVOICE SPECIFIC

		#region INVOICE CREDITS

		internal static void SaveInvoiceCredit(InvoiceCredit invCredit)
		{
			if (invCredit == null)
				return;
			try
			{
				if (invCredit.InvoiceCreditID <= 0)
					DB.InvoiceCredits.Add(invCredit);
				DB.SaveChanges();
			}
			catch (Exception ex)
			{
				throw new EnabillDomainException("Error saving invoice credit: " + ex.Message);
			}
		}

		internal static void DeleteInvoiceCredit(InvoiceCredit invCredit)
		{
			if (invCredit == null)
				return;

			DB.InvoiceCredits.Remove(invCredit);
			DB.SaveChanges();
		}

		internal static void DeleteAllInvoiceCredit(int invoiceID)
		{
			foreach (var credit in DB.InvoiceCredits.Where(c => c.InvoiceID == invoiceID).ToList())
			{
				DB.InvoiceCredits.Remove(credit);
			}

			DB.SaveChanges();
		}

		internal static void DeleteAllInvoiceContacts(int invoiceID)
		{
			foreach (var contact in DB.InvoiceContacts.Where(c => c.InvoiceID == invoiceID).ToList())
			{
				DB.InvoiceContacts.Remove(contact);
			}

			DB.SaveChanges();
		}

		#region INVOICE SPECIFIC CREDITS

		internal static InvoiceCredit GetInvoicCreditsForInvoiceLevel(int invoiceID) => DB.InvoiceCredits
					.SingleOrDefault(ic => ic.InvoiceID == invoiceID && ic.WorkAllocationID == null);

		#endregion INVOICE SPECIFIC CREDITS

		#region INVOICE CREDITS FOR WORK ALLOCATIONS

		internal static InvoiceCredit GetInvoiceCreditForWorkAllocation(int invoiceID, int workAllocationID) => DB.InvoiceCredits
					.SingleOrDefault(ic => ic.InvoiceID == invoiceID && ic.WorkAllocationID == workAllocationID);

		internal static IEnumerable<InvoiceCredit> GetWorkAllocationCreditsForInvoice(int invoiceID) => DB.InvoiceCredits
					.Where(ic => ic.InvoiceID == invoiceID && ic.WorkAllocationID.HasValue);

		#endregion INVOICE CREDITS FOR WORK ALLOCATIONS

		#endregion INVOICE CREDITS

		#region USERS

		internal static IEnumerable<User> GetAllUsersLinkedToInvoice(int invoiceID)
		{
			var item = (from wa in GetLinkedWorkAllocations(invoiceID)
						join u in DB.Users on wa.UserID equals u.UserID
						select u).Distinct();

			var model = new List<User>();
			model.AddRange(item);

			return model;
		}

		internal static List<UserPrintModel> GetUsersPrintModel(int invoiceID)
		{
			var list = from u in DB.Users
					   join wa in GetLinkedWorkAllocations(invoiceID) on u.UserID equals wa.UserID
					   join ic in DB.InvoiceCredits on wa.WorkAllocationID equals ic.WorkAllocationID into icLJ
					   from icExists in icLJ.DefaultIfEmpty()
					   select new
					   {
						   User = u,
						   WorkAllocation = wa,
						   Credit = icExists
					   };

			var model = new List<UserPrintModel>();
			UserPrintModel currentEntry = null;
			User user = null;

			foreach (var item in list.OrderBy(u => u.User.UserName).ThenBy(u => u.WorkAllocation.HourlyRate))
			{
				user = item.User;
				currentEntry = model.SingleOrDefault(m => m.User == user && m.Rate == item.WorkAllocation.HourlyRate);

				if (currentEntry == null)
				{
					currentEntry = new UserPrintModel
					{
						User = item.User,
						Rate = item.WorkAllocation.HourlyRate ?? 0.0D
					};
					model.Add(currentEntry);
				}

				if (item.Credit == null)
					currentEntry.Hours += item.WorkAllocation.TotalHours;

				currentEntry.Total += item.WorkAllocation.TotalValue;
				currentEntry.Credits += item.Credit?.CreditAmount ?? 0.0D;
			}

			return model;
		}

		#endregion USERS

		#region WORKALLOCATIONS

		internal static void SaveWorkAllocation(WorkAllocation wa)
		{
			if (wa.WorkAllocationID == 0)
				throw new InvoiceException("A new work allocation was discovered and cannot be saved from the invoice instance. Save cancelled.");

			DB.SaveChanges();
		}

		internal static IEnumerable<WorkAllocation> GetLinkedWorkAllocations(int invoiceID) => from wa in DB.WorkAllocations
																							   where wa.InvoiceID == invoiceID
																							   select wa;

		internal static IEnumerable<WorkAllocation> GetNonLinkedWorkAllocations
		(List<int> activityIDs, DateTime dateTo)
		{
			dateTo = dateTo.Date;

			var model = new List<WorkAllocation>();

			return from wa in DB.WorkAllocations
				   join a in DB.Activities on wa.ActivityID equals a.ActivityID
				   where wa.InvoiceID == null
				   && activityIDs.Contains(a.ActivityID)
				   && wa.DayWorked <= dateTo
				   select wa;
		}

		public static IEnumerable<WorkAllocationExtendedModel> GetLinkedWorkAllocationsExtendedModel(int invoiceID) => (from wa in GetLinkedWorkAllocations(invoiceID)
																														join n in DB.Notes on wa.WorkAllocationID equals n.WorkAllocationID into t_note
																														from note in t_note.DefaultIfEmpty(new Note() { NoteID = 0, NoteText = "" })
																														join a in DB.Activities on wa.ActivityID equals a.ActivityID
																														join p in DB.Projects on a.ProjectID equals p.ProjectID
																														join c in DB.Clients on p.ClientID equals c.ClientID
																														join u in DB.Users on wa.UserID equals u.UserID
																														join ws in DB.WorkSessions on wa.DayWorked equals ws.StartTime.Date
																														where ws.UserID == wa.UserID
																														select (new
																														{
																															workAllocation = wa,
																															workSessionStatusID = ws.WorkSessionStatusID,
																															noteID = note.NoteID,
																															noteText = note.NoteText,
																															activity = a,
																															project = p,
																															client = c,
																															user = u
																														})
					)
					.Distinct()
					.ToList()
					.Select(i => new WorkAllocationExtendedModel()
					{
						WorkAllocation = i.workAllocation,
						WorkSessionStatusID = i.workSessionStatusID,
						NoteText = i.noteText,
						NoteID = i.noteID,
						Activity = new ActivityDetail(i.activity, i.project, i.client, true),
						Project = new ProjectDetail(i.project),
						User = new UserDetails(i.user),
						IsSelected = false
					});

		public static IEnumerable<WorkAllocationExtendedModel> GetNonLinkedWorkAllocationsExtendedModel(List<int> activityIDs, DateTime dateTo) => (from wa in GetNonLinkedWorkAllocations(activityIDs, dateTo)
																																					join n in DB.Notes on wa.WorkAllocationID equals n.WorkAllocationID into t_note
																																					from note in t_note.DefaultIfEmpty(new Note() { NoteID = 0, NoteText = "" })
																																					join a in DB.Activities on wa.ActivityID equals a.ActivityID
																																					join p in DB.Projects on a.ProjectID equals p.ProjectID
																																					join c in DB.Clients on p.ClientID equals c.ClientID
																																					join u in DB.Users on wa.UserID equals u.UserID
																																					join ws in DB.WorkSessions on wa.DayWorked equals ws.StartTime.Date
																																					where ws.UserID == wa.UserID
																																					select (new
																																					{
																																						workAllocation = wa,
																																						workSessionStatusID = ws.WorkSessionStatusID,
																																						noteID = note.NoteID,
																																						noteText = note.NoteText,
																																						activity = a,
																																						project = p,
																																						client = c,
																																						user = u
																																					})
					)
					.Distinct()
					.ToList()
					.Select(i => new WorkAllocationExtendedModel()
					{
						WorkAllocation = i.workAllocation,
						WorkSessionStatusID = i.workSessionStatusID,
						NoteText = i.noteText,
						NoteID = i.noteID,
						Activity = new ActivityDetail(i.activity, i.project, i.client, true),
						Project = new ProjectDetail(i.project),
						User = new UserDetails(i.user),
						IsSelected = false
					});

		internal static List<WorkSessionExtendedModel> GetWorkSessionsExtendedModel(int invoiceID)
		{
			var data = from ws in DB.WorkSessions
					   from i in DB.Invoices
					   join wa in DB.WorkAllocations on new { User = ws.UserID, i.InvoiceID, Date = DbFunctions.TruncateTime(ws.StartTime) } equals new { User = wa.UserID, InvoiceID = wa.InvoiceID.Value, Date = DbFunctions.TruncateTime(wa.DayWorked) } into allocations
					   from allocGroup in allocations
					   join u in DB.Users on ws.UserID equals u.UserID
					   orderby ws.UserID, ws.StartTime, ws.WorkSessionID
					   where i.InvoiceID == invoiceID
					   select new { ws, u, allocations };

			var model = new List<WorkSessionExtendedModel>();
			var modelItem = new WorkSessionExtendedModel();

			foreach (var item in data)
			{
				modelItem = model.SingleOrDefault(m => m.User.UserID == item.u.UserID && m.DayWorked == item.ws.StartTime.Date);

				if (modelItem == null)
				{
					modelItem = new WorkSessionExtendedModel()
					{
						User = item.u,
						DayWorked = item.ws.StartTime.Date
					};

					model.Add(modelItem);
				}

				modelItem.WorkSessions.Add(item.ws);

				foreach (var wa in item.allocations)
				{
					if (!modelItem.WorkAllocations.Any(a => a.WorkAllocationID == wa.WorkAllocationID))
						modelItem.WorkAllocations.Add(wa);
				}
			}

			return model;
		}

		internal static IEnumerable<WorkAllocationExceptionModel> GetWorkAllocationExceptions(int invoiceID) => from e in DB.InvoiceWorkAllocationExceptions
																												join u in DB.Users on e.UserID equals u.UserID
																												join ur in DB.UserRoles on e.UserID equals ur.UserID
																												join r in DB.Roles on ur.RoleID equals r.RoleID
																												where e.InvoiceID == invoiceID
																												&& r.RoleID == (int)UserRoleType.TimeCapturing
																												select new WorkAllocationExceptionModel
																												{
																													UserName = u.UserName,
																													WorkDate = e.WorkDate,
																													ExceptionDetail = e.Exception
																												};

		#endregion WORKALLOCATIONS

		#region WORKSESSIONS

		public static IEnumerable<WorkSession> GetUnApprovedWorkSessionsLinkedToInvoicePeriod(List<int> activityIDs, DateTime dateFrom, DateTime dateTo)
		{
			dateFrom = dateFrom.Date;
			dateTo = dateTo.Date;

			return from wa in DB.WorkAllocations
				   join a in DB.Activities on wa.ActivityID equals a.ActivityID
				   join ws in DB.WorkSessions on wa.DayWorked equals DbFunctions.TruncateTime(ws.StartTime)
				   where activityIDs.Contains(a.ActivityID)
				   && wa.UserID == ws.UserID
				   && wa.DayWorked <= dateTo
				   && wa.DayWorked >= dateFrom
				   && ws.WorkSessionStatusID != 2
				   select ws;
		}

		public static IEnumerable<User> GetUsersLinkedToInvoicePeriod(List<int> activityIDs, DateTime dateFrom, DateTime dateTo)
		{
			dateFrom = dateFrom.Date;
			dateTo = dateTo.Date;

			return (from ua in DB.UserAllocations
					join a in DB.Activities on ua.ActivityID equals a.ActivityID
					join u in DB.Users on ua.UserID equals u.UserID
					where activityIDs.Contains(a.ActivityID)
					&& ua.StartDate <= dateTo
					&& (ua.ConfirmedEndDate > dateTo || !ua.ConfirmedEndDate.HasValue)
					select u).Distinct();
		}

		#endregion WORKSESSIONS

		#region PROJECT

		internal static Project GetProject(int projectID) => DB.Projects
					.SingleOrDefault(p => p.ProjectID == projectID);

		internal static IEnumerable<Project> GetProjects(int? invoiceRuleID) => (from ir in DB.InvoiceRules
																				 join ia in DB.InvoiceRuleActivities on ir.InvoiceRuleID equals ia.InvoiceRuleID
																				 join a in DB.Activities on ia.ActivityID equals a.ActivityID
																				 join p in DB.Projects on a.ProjectID equals p.ProjectID
																				 where ir.InvoiceRuleID == invoiceRuleID
																				 select p
				   )
				   .Distinct();

		#endregion PROJECT

		#region ACTIVITIES

		internal static IEnumerable<Activity> GetActivities(int invoiceID) => from wa in DB.WorkAllocations
																			  join a in DB.Activities on wa.ActivityID equals a.ActivityID
																			  where wa.InvoiceID == invoiceID
																			  select a;

		internal static List<ActivityPrintModel> GetActivitiesPrintModel(int invoiceID)
		{
			var list = from a in DB.Activities
					   join wa in
						   from waSel in DB.WorkAllocations
						   join icSel in DB.InvoiceCredits on waSel.WorkAllocationID equals icSel.WorkAllocationID into icLJ
						   from icExists in icLJ.DefaultIfEmpty()
						   select new { waSel, icExists }
							on a.ActivityID equals wa.waSel.ActivityID
					   join p in DB.Projects on a.ProjectID equals p.ProjectID
					   group wa by new { a, wa.waSel.HourlyRate, wa.waSel.InvoiceID, p.ProjectCode } into waGroup
					   orderby waGroup.Key.a.ActivityID
					   where waGroup.Key.InvoiceID.HasValue && waGroup.Key.InvoiceID == invoiceID
					   select new ActivityPrintModel()
					   {
						   Activity = waGroup.Key.a,
						   Rate = waGroup.Key.HourlyRate ?? 0,
						   ProjectCode = waGroup.Key.ProjectCode,
						   Hours = waGroup.Sum(g => g.waSel.HoursBilled ?? g.waSel.HoursWorked),
						   Credits = waGroup.Sum(g => (g.icExists == null) ? 0.0D : g.icExists.CreditAmount),
						   ExclVATAmount = waGroup.Sum(g => (g.waSel.HoursBilled ?? g.waSel.HoursWorked) * g.waSel.HourlyRate) ?? 0
					   }
					   ;

			return list.ToList();
		}

		/*
		internal static void AddActivity(int invoiceID, int activityID)
		{
			if (DB.InvoiceActivities.Any(ia => ia.InvoiceID == invoiceID && ia.ReferenceActivityID == activityID))
				return;

			Activity act = ActivityRepo.GetByID(activityID);
			DB.InvoiceActivities.Add(new InvoiceActivity() { InvoiceID = invoiceID, ReferenceActivityID = activityID, ActivityName = act.ActivityName, ProjectName = act.GetProject().ProjectName, DepartmentName = DepartmentRepo.GetByID(act.DepartmentID).DepartmentName, RegionName = RegionRepo.GetByID(act.RegionID).RegionName });
			DB.SaveChanges();
		}

		internal static void AddActivities(int invoiceID, List<Activity> activities)
		{
			foreach (var activity in activities)
			{
				if (DB.InvoiceActivities.Any(ia => ia.InvoiceID == invoiceID && ia.ReferenceActivityID == activity.ActivityID))
					continue;

				string projectName = activity.GetProject().ProjectName;
				string regionName = RegionRepo.GetByID(activity.RegionID).RegionName;
				string departmentName = DepartmentRepo.GetByID(activity.DepartmentID).DepartmentName;

				DB.InvoiceActivities.Add(new InvoiceActivity() { InvoiceID = invoiceID, ReferenceActivityID = activity.ActivityID, ActivityName = activity.ActivityName, ProjectName = projectName, DepartmentName = departmentName, RegionName = regionName });
			}
			DB.SaveChanges();
		}
		*/

		#endregion ACTIVITIES

		#region CONTACTS

		internal static IEnumerable<Contact> GetContacts(int invoiceID) => DB.InvoiceContacts
				.Where(ic => ic.InvoiceID == invoiceID)
				.Select(ic => ic.Contact);

		internal static void AddContact(int invoiceID, int contactID)
		{
			if (DB.InvoiceContacts.Any(ic => ic.InvoiceID == invoiceID && ic.ContactID == contactID))
				return;

			DB.InvoiceContacts.Add(new InvoiceContact { InvoiceID = invoiceID, ContactID = contactID });
			DB.SaveChanges();
		}

		internal static void AddContacts(int invoiceID, IEnumerable<Contact> contacts)
		{
			foreach (var contact in contacts)
			{
				if (DB.InvoiceContacts.Any(ic => ic.InvoiceID == invoiceID && ic.ContactID == contact.ContactID))
					continue;

				DB.InvoiceContacts.Add(new InvoiceContact() { InvoiceID = invoiceID, ContactID = contact.ContactID });
			}

			DB.SaveChanges();
		}

		#endregion CONTACTS

		#region RELATED INVOICES

		internal static List<Invoice> GetClientRelatedInvoices(Invoice invoice) => DB.Invoices.Where(i => i.ClientID == invoice.ClientID
																													  && i.InvoiceID != invoice.InvoiceID
									 )
									 .OrderBy(i => i.InvoiceDate).ToList();

		internal static List<Invoice> GetRuleRelatedInvoices(Invoice invoice) => DB.Invoices.Where(i => i.InvoiceRuleID == invoice.InvoiceRuleID
																													&& i.InvoiceID != invoice.InvoiceID
									 )
									 .OrderBy(i => i.InvoiceDate).ToList();

		#endregion RELATED INVOICES

		#region EXPORT TO CSV

		public static List<InvoiceExportModel> GetInvoicesByPeriod(int? period)
		{
			string docDate = period.ToString().Substring(0, 4) + "/" + period.ToString().Substring(4, 2) + "/" + period.ToLastDayOfMonthFromPeriod();

			return (from i in DB.Invoices
					join invs in DB.InvoiceStatus on i.InvoiceStatusID equals invs.InvoiceStatusID
					join wa in DB.WorkAllocations on i.InvoiceID equals wa.InvoiceID into wa_join
					from wa in wa_join.DefaultIfEmpty()
					join c in DB.Clients on i.ClientID equals c.ClientID
					join a in DB.Activities on wa.ActivityID equals a.ActivityID into a_join
					from a in a_join.DefaultIfEmpty()
					join cdc in DB.ClientDepartmentCode on i.ClientDepartmentCodeID equals cdc.ClientDepartmentCodeID into cdc_join
					from cdc in cdc_join.DefaultIfEmpty()
					join g in DB.GLAccounts on i.GLAccountID equals g.GLAccountID into g_join
					from g in g_join.DefaultIfEmpty()
					where
					  i.Period == period && invs.StatusName == "Ready"
					group new { c, cdc, wa, a, i, g } by new
					{
						c.ClientName,
						cdc.DepartmentCode,
						c.AccountCode,
						wa.HourlyRate,
						a.ActivityName,
						i.Description,
						g.GLAccountCode,
						i.InvoiceAmountExclVAT
					} into g
					select new InvoiceExportModel
					{
						Customer = g.Key.ClientName,
						DocumentDate = docDate, //(2013/03/31)
						HoursWorked = g.Sum(p => (int?)(p.wa.HoursBilled ?? p.wa.HoursWorked)) ?? 0,
						DepartmentCode = g.Key.DepartmentCode,
						CustomerCode = g.Key.AccountCode,
						Rate = g.Key.HourlyRate ?? 0,
						Description = g.Key.ActivityName ?? g.Key.Description.Replace(",", " "),
						GLCode = g.Key.GLAccountCode,
						Amount = (g.Sum(p => p.wa.HoursBilled ?? p.wa.HoursWorked) * g.Key.HourlyRate == 0 ? null : g.Sum(p => p.wa.HoursBilled ?? p.wa.HoursWorked) * g.Key.HourlyRate) ?? g.Key.InvoiceAmountExclVAT
					}
						).ToList();
		}

		#endregion EXPORT TO CSV
	}
}