using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public abstract class WorkAllocationRepo : BaseRepo
	{
		#region WORKALLOCATION SPECIFIC

		internal static WorkAllocation GetByID(int waID) => DB.WorkAllocations
					.SingleOrDefault(wa => wa.WorkAllocationID == waID);

		internal static WorkAllocation GetByUserIdActivityIDWorkDay(int userID, int activityID, DateTime workDay) => DB.WorkAllocations
					.SingleOrDefault(wa => wa.UserID == userID && wa.ActivityID == activityID && wa.DayWorked == workDay);

		internal static void Delete(WorkAllocation wa)
		{
			var user = UserRepo.GetByID(wa.UserID);
			//Only do this is changes is applied to a month other than the current
			int monthsToRecalc = DateTime.Today.Year != wa.DayWorked.Year ? 12 - wa.DayWorked.Month + DateTime.Today.Month : DateTime.Today.Month - 1;

			if (wa.DayWorked.IsInPastMonth())
				user.ExecuteFlexiBalanceLeaveBalanceProcess(user, monthsToRecalc);

			DB.WorkAllocations.Remove(wa);
			DB.SaveChanges();
		}

		public static IEnumerable<TrainingCategory> GetAllTrainingCategories() => DB.TrainingCategories;

		public static Dictionary<int, string> GetTrainingCategoryExtendedNames()
		{
			var model = new Dictionary<int, string>();

			foreach (var tc in GetAllTrainingCategories().ToList())
			{
				// if (tc.TrainingCategoryID !=0)
				model.Add(tc.TrainingCategoryID, tc.TrainingCategoryName);
			}

			return model;
		}

		internal static double GetRate(int waID)
		{
			var userLink = (from wa in DB.WorkAllocations
							join a in DB.Activities on wa.ActivityID equals a.ActivityID
							join ua in DB.UserAllocations on a.ActivityID equals ua.ActivityID
							where wa.UserID == ua.UserID
								  && wa.WorkAllocationID == waID
								  && wa.DayWorked >= ua.StartDate
								  && ((ua.ConfirmedEndDate == null) || (wa.DayWorked <= ua.ConfirmedEndDate))
							select ua).FirstOrDefault();

			if (userLink == null)
				return 0.0D;

			return userLink.ChargeRate;
		}

		#endregion WORKALLOCATION SPECIFIC

		#region NOTES

		internal static Note GetNote(int workAllocationID) => DB.Notes
					.SingleOrDefault(n => n.WorkAllocationID == workAllocationID);

		#endregion NOTES

		#region WORK ALLOCATION EXTENDED MODELS

		internal static WorkAllocationExtendedModel GetExtendedModel(WorkAllocation wAllocation)
		{
			var list = (from wa in DB.WorkAllocations
						join n in DB.Notes on wa.WorkAllocationID equals n.WorkAllocationID into t_note
						from note in t_note.DefaultIfEmpty()
						join a in DB.Activities on wa.ActivityID equals a.ActivityID
						join p in DB.Projects on a.ProjectID equals p.ProjectID
						join c in DB.Clients on p.ClientID equals c.ClientID
						join u in DB.Users on wa.UserID equals u.UserID
						where wa.WorkAllocationID == wAllocation.WorkAllocationID
						select new
						{
							workAllocation = wa,
							noteID = (int?)note.NoteID,
							noteText = note.NoteText,
							activity = a,
							project = p,
							client = c,
							user = u,
						})
						.ToList();

			return list.Select(m => new WorkAllocationExtendedModel()
			{
				WorkAllocation = m.workAllocation,
				NoteText = m.noteText,
				NoteID = m.noteID,
				Activity = new ActivityDetail(m.activity, m.project, m.client, m.workAllocation.InvoiceID != null),
				Project = new ProjectDetail(m.project),
				Client = m.client,
				User = new UserDetails(m.user),
				AssociatedProjectTickets = TicketRepo.GetAssociatedProjectTickets(m.client.ClientID, m.project.ProjectID).ToList()
			})
						.SingleOrDefault();
		}

		public static IEnumerable<ActivityExtendedReportModel> GetActivityExtendedModelForPeriod(DateTime dateFrom, DateTime dateTo) => (from wa in DB.WorkAllocations
																																		 join a in DB.Activities on wa.ActivityID equals a.ActivityID
																																		 join p in DB.Projects on a.ProjectID equals p.ProjectID
																																		 join c in DB.Clients on p.ClientID equals c.ClientID
																																		 join u in DB.Users on wa.UserID equals u.UserID
																																		 join d in DB.Departments on a.DepartmentID equals d.DepartmentID
																																		 join t in DB.TrainingCategories on wa.TrainingCategoryID equals t.TrainingCategoryID
																																		 where wa.DayWorked >= dateFrom
																																		 && wa.DayWorked <= dateTo
																																		 select (new ActivityExtendedReportModel()
																																		 {
																																			 UserName = u.UserName,
																																			 FullName = u.FullName,
																																			 Date = wa.DayWorked,
																																			 Client = c.ClientName,
																																			 Project = p.ProjectName,
																																			 Department = d.DepartmentName,
																																			 Activity = a.ActivityName,
																																			 HoursAllocated = wa.HoursWorked,
																																			 Remarks = wa.Remark,
																																			 TrainingType = t.TrainingCategoryName == "Please select" ? "" : t.TrainingCategoryName,
																																			 TrainerName = wa.TrainerName,
																																			 TrainingInstitute = wa.TrainingInstitute
																																		 })
					)
					.Distinct()
					.ToList();

		public static IEnumerable<ActivityExtendedReportModel> GetActivityExtendedModelForClient(DateTime dateFrom, DateTime dateTo, int clientID) => (from wa in DB.WorkAllocations
																																					   join a in DB.Activities on wa.ActivityID equals a.ActivityID
																																					   join p in DB.Projects on a.ProjectID equals p.ProjectID
																																					   join c in DB.Clients on p.ClientID equals c.ClientID
																																					   join u in DB.Users on wa.UserID equals u.UserID
																																					   join d in DB.Departments on a.DepartmentID equals d.DepartmentID
																																					   join t in DB.TrainingCategories on wa.TrainingCategoryID equals t.TrainingCategoryID
																																					   where wa.DayWorked >= dateFrom
																																					   && wa.DayWorked <= dateTo
																																					   && c.ClientID == clientID
																																					   select (new ActivityExtendedReportModel()
																																					   {
																																						   UserName = u.UserName,
																																						   FullName = u.FullName,
																																						   Date = wa.DayWorked,
																																						   Client = c.ClientName,
																																						   Project = p.ProjectName,
																																						   Department = d.DepartmentName,
																																						   Activity = a.ActivityName,
																																						   HoursAllocated = wa.HoursWorked,
																																						   Remarks = wa.Remark,
																																						   TrainingType = t.TrainingCategoryName == "Please select" ? "" : t.TrainingCategoryName,
																																						   TrainerName = wa.TrainerName,
																																						   TrainingInstitute = wa.TrainingInstitute
																																					   })
					)
					.Distinct()
					.ToList();

		public static IEnumerable<ActivityExtendedReportModel> GetActivityExtendedModelForUser(DateTime dateFrom, DateTime dateTo, int userID) => (from wa in DB.WorkAllocations
																																				   join a in DB.Activities on wa.ActivityID equals a.ActivityID
																																				   join p in DB.Projects on a.ProjectID equals p.ProjectID
																																				   join c in DB.Clients on p.ClientID equals c.ClientID
																																				   join u in DB.Users on wa.UserID equals u.UserID
																																				   join d in DB.Departments on a.DepartmentID equals d.DepartmentID
																																				   join t in DB.TrainingCategories on wa.TrainingCategoryID equals t.TrainingCategoryID
																																				   where wa.DayWorked >= dateFrom
																																				   && wa.DayWorked <= dateTo
																																				   && u.UserID == userID
																																				   select (new ActivityExtendedReportModel()
																																				   {
																																					   UserName = u.UserName,
																																					   FullName = u.FullName,
																																					   Date = wa.DayWorked,
																																					   Client = c.ClientName,
																																					   Project = p.ProjectName,
																																					   Department = d.DepartmentName,
																																					   Activity = a.ActivityName,
																																					   HoursAllocated = wa.HoursWorked,
																																					   Remarks = wa.Remark,
																																					   TrainingType = t.TrainingCategoryName == "Please select" ? "" : t.TrainingCategoryName,
																																					   TrainerName = wa.TrainerName,
																																					   TrainingInstitute = wa.TrainingInstitute
																																				   })
					)
					.Distinct()
					.ToList();

		public static IEnumerable<ActivityExtendedReportModel> GetActivityExtendedModelForClientProject(DateTime dateFrom, DateTime dateTo, int clientID, string projectName) => (from wa in DB.WorkAllocations
																																												  join a in DB.Activities on wa.ActivityID equals a.ActivityID
																																												  join p in DB.Projects on a.ProjectID equals p.ProjectID
																																												  join c in DB.Clients on p.ClientID equals c.ClientID
																																												  join u in DB.Users on wa.UserID equals u.UserID
																																												  join d in DB.Departments on a.DepartmentID equals d.DepartmentID
																																												  join t in DB.TrainingCategories on wa.TrainingCategoryID equals t.TrainingCategoryID
																																												  where wa.DayWorked >= dateFrom
																																												  && wa.DayWorked <= dateTo
																																												  && c.ClientID == clientID
																																												  && p.ProjectName == projectName
																																												  select (new ActivityExtendedReportModel()
																																												  {
																																													  UserName = u.UserName,
																																													  FullName = u.FullName,
																																													  Date = wa.DayWorked,
																																													  Client = c.ClientName,
																																													  Project = p.ProjectName,
																																													  Department = d.DepartmentName,
																																													  Activity = a.ActivityName,
																																													  HoursAllocated = wa.HoursWorked,
																																													  Remarks = wa.Remark,
																																													  TrainingType = t.TrainingCategoryName == "Please select" ? "" : t.TrainingCategoryName,
																																													  TrainerName = wa.TrainerName,
																																													  TrainingInstitute = wa.TrainingInstitute
																																												  })
					)
					.Distinct()
					.ToList();

		public static IEnumerable<ActivityExtendedReportModel> GetActivityExtendedModelForClientProjectActivity(DateTime dateFrom, DateTime dateTo, int clientID, string projectName, string activityName) => (from wa in DB.WorkAllocations
																																																			   join a in DB.Activities on wa.ActivityID equals a.ActivityID
																																																			   join p in DB.Projects on a.ProjectID equals p.ProjectID
																																																			   join c in DB.Clients on p.ClientID equals c.ClientID
																																																			   join u in DB.Users on wa.UserID equals u.UserID
																																																			   join d in DB.Departments on a.DepartmentID equals d.DepartmentID
																																																			   join t in DB.TrainingCategories on wa.TrainingCategoryID equals t.TrainingCategoryID
																																																			   where wa.DayWorked >= dateFrom
																																																			   && wa.DayWorked <= dateTo
																																																			   && c.ClientID == clientID
																																																			   && p.ProjectName == projectName
																																																			   && a.ActivityName == activityName
																																																			   select (new ActivityExtendedReportModel()
																																																			   {
																																																				   UserName = u.UserName,
																																																				   FullName = u.FullName,
																																																				   Date = wa.DayWorked,
																																																				   Client = c.ClientName,
																																																				   Project = p.ProjectName,
																																																				   Department = d.DepartmentName,
																																																				   Activity = a.ActivityName,
																																																				   HoursAllocated = wa.HoursWorked,
																																																				   Remarks = wa.Remark,
																																																				   TrainingType = t.TrainingCategoryName == "Please select" ? "" : t.TrainingCategoryName,
																																																				   TrainerName = wa.TrainerName,
																																																				   TrainingInstitute = wa.TrainingInstitute
																																																			   })
					)
					.Distinct()
					.ToList();

		public static IEnumerable<ActivityExtendedReportModel> GetActivityExtendedModelForProject(DateTime dateFrom, DateTime dateTo, string projectName) => (from wa in DB.WorkAllocations
																																							  join a in DB.Activities on wa.ActivityID equals a.ActivityID
																																							  join p in DB.Projects on a.ProjectID equals p.ProjectID
																																							  join c in DB.Clients on p.ClientID equals c.ClientID
																																							  join u in DB.Users on wa.UserID equals u.UserID
																																							  join d in DB.Departments on a.DepartmentID equals d.DepartmentID
																																							  join t in DB.TrainingCategories on wa.TrainingCategoryID equals t.TrainingCategoryID
																																							  where wa.DayWorked >= dateFrom
																																							  && wa.DayWorked <= dateTo
																																							  && p.ProjectName == projectName
																																							  select (new ActivityExtendedReportModel()
																																							  {
																																								  UserName = u.UserName,
																																								  FullName = u.FullName,
																																								  Date = wa.DayWorked,
																																								  Client = c.ClientName,
																																								  Project = p.ProjectName,
																																								  Department = d.DepartmentName,
																																								  Activity = a.ActivityName,
																																								  HoursAllocated = wa.HoursWorked,
																																								  Remarks = wa.Remark,
																																								  TrainingType = t.TrainingCategoryName == "Please select" ? "" : t.TrainingCategoryName,
																																								  TrainerName = wa.TrainerName,
																																								  TrainingInstitute = wa.TrainingInstitute
																																							  })
					)
					.Distinct()
					.ToList();

		public static IEnumerable<ActivityExtendedReportModel> GetActivityExtendedModelForActivity(DateTime dateFrom, DateTime dateTo, string activityName) => (from wa in DB.WorkAllocations
																																								join a in DB.Activities on wa.ActivityID equals a.ActivityID
																																								join p in DB.Projects on a.ProjectID equals p.ProjectID
																																								join c in DB.Clients on p.ClientID equals c.ClientID
																																								join u in DB.Users on wa.UserID equals u.UserID
																																								join d in DB.Departments on a.DepartmentID equals d.DepartmentID
																																								join t in DB.TrainingCategories on wa.TrainingCategoryID equals t.TrainingCategoryID
																																								where wa.DayWorked >= dateFrom
																																								&& wa.DayWorked <= dateTo
																																								&& a.ActivityName == activityName
																																								select (new ActivityExtendedReportModel()
																																								{
																																									UserName = u.UserName,
																																									FullName = u.FullName,
																																									Date = wa.DayWorked,
																																									Client = c.ClientName,
																																									Project = p.ProjectName,
																																									Department = d.DepartmentName,
																																									Activity = a.ActivityName,
																																									HoursAllocated = wa.HoursWorked,
																																									Remarks = wa.Remark,
																																									TrainingType = t.TrainingCategoryName == "Please select" ? "" : t.TrainingCategoryName,
																																									TrainerName = wa.TrainerName,
																																									TrainingInstitute = wa.TrainingInstitute
																																								})
					)
					.Distinct()
					.ToList();

		public static IEnumerable<ActivityExtendedReportModel> GetActivityExtendedModelForProjectActivity(DateTime dateFrom, DateTime dateTo, string projectName, string activityName) => (from wa in DB.WorkAllocations
																																														   join a in DB.Activities on wa.ActivityID equals a.ActivityID
																																														   join p in DB.Projects on a.ProjectID equals p.ProjectID
																																														   join c in DB.Clients on p.ClientID equals c.ClientID
																																														   join u in DB.Users on wa.UserID equals u.UserID
																																														   join d in DB.Departments on a.DepartmentID equals d.DepartmentID
																																														   join t in DB.TrainingCategories on wa.TrainingCategoryID equals t.TrainingCategoryID
																																														   where wa.DayWorked >= dateFrom
																																														   && wa.DayWorked <= dateTo
																																														   && p.ProjectName == projectName
																																														   && a.ActivityName == activityName
																																														   select (new ActivityExtendedReportModel()
																																														   {
																																															   UserName = u.UserName,
																																															   FullName = u.FullName,
																																															   Date = wa.DayWorked,
																																															   Client = c.ClientName,
																																															   Project = p.ProjectName,
																																															   Department = d.DepartmentName,
																																															   Activity = a.ActivityName,
																																															   HoursAllocated = wa.HoursWorked,
																																															   Remarks = wa.Remark,
																																															   TrainingType = t.TrainingCategoryName == "Please select" ? "" : t.TrainingCategoryName,
																																															   TrainerName = wa.TrainerName,
																																															   TrainingInstitute = wa.TrainingInstitute
																																														   })
					)
					.Distinct()
					.ToList();

		public static IEnumerable<ActivityExtendedReportModel> GetActivityExtendedModelForClientActivity(DateTime dateFrom, DateTime dateTo, int clientID, string activityName) => (from wa in DB.WorkAllocations
																																													join a in DB.Activities on wa.ActivityID equals a.ActivityID
																																													join p in DB.Projects on a.ProjectID equals p.ProjectID
																																													join c in DB.Clients on p.ClientID equals c.ClientID
																																													join u in DB.Users on wa.UserID equals u.UserID
																																													join d in DB.Departments on a.DepartmentID equals d.DepartmentID
																																													join t in DB.TrainingCategories on wa.TrainingCategoryID equals t.TrainingCategoryID
																																													where wa.DayWorked >= dateFrom
																																													&& wa.DayWorked <= dateTo
																																													&& c.ClientID == clientID
																																													&& a.ActivityName == activityName
																																													select (new ActivityExtendedReportModel()
																																													{
																																														UserName = u.UserName,
																																														FullName = u.FullName,
																																														Date = wa.DayWorked,
																																														Client = c.ClientName,
																																														Project = p.ProjectName,
																																														Department = d.DepartmentName,
																																														Activity = a.ActivityName,
																																														HoursAllocated = wa.HoursWorked,
																																														Remarks = wa.Remark,
																																														TrainingType = t.TrainingCategoryName == "Please select" ? "" : t.TrainingCategoryName,
																																														TrainerName = wa.TrainerName,
																																														TrainingInstitute = wa.TrainingInstitute
																																													})
					)
					.Distinct()
					.ToList();

		#endregion WORK ALLOCATION EXTENDED MODELS

		#region

		public static IEnumerable<ProjectActivityTimeReportModel> GetProjectActivityTimeForProjectManager(DateTime dateFrom, DateTime dateTo, int projectManagerID)
		{
			if (projectManagerID != 0)
			{
				//Retrieve activity time for a specific projectManager
				return (from wa in DB.WorkAllocations
						join a in DB.Activities on wa.ActivityID equals a.ActivityID
						join p in DB.Projects on a.ProjectID equals p.ProjectID
						join c in DB.Clients on p.ClientID equals c.ClientID
						join u in DB.Users on wa.UserID equals u.UserID
						join d in DB.Departments on a.DepartmentID equals d.DepartmentID
						join pm in DB.Users on p.ProjectOwnerID equals pm.UserID
						where wa.DayWorked >= dateFrom
						&& wa.DayWorked <= dateTo
						&& p.ProjectOwnerID == projectManagerID
						select (new ProjectActivityTimeReportModel()
						{
							ProjectManager = "Project Manager - " + pm.FullName,
							Client = c.ClientName,
							Project = p.ProjectName,
							Activity = a.ActivityName,
							Period = wa.Period,
							DayWorked = wa.DayWorked,
							UserName = u.UserName,
							FullName = u.FullName,
							HoursWorked = wa.HoursWorked,
							HoursQuoted = 0,
							TicketReference = wa.TicketReference,
							Remark = wa.Remark
						})
					  )
					  .Distinct()
					  .ToList();
			}
			else
			{
				//Retrieve activity time for a all Project Managers
				return (from wa in DB.WorkAllocations
						join a in DB.Activities on wa.ActivityID equals a.ActivityID
						join p in DB.Projects on a.ProjectID equals p.ProjectID
						join c in DB.Clients on p.ClientID equals c.ClientID
						join u in DB.Users on wa.UserID equals u.UserID
						join d in DB.Departments on a.DepartmentID equals d.DepartmentID
						join pm in DB.Users on p.ProjectOwnerID equals pm.UserID
						where wa.DayWorked >= dateFrom
						&& wa.DayWorked <= dateTo
						select (new ProjectActivityTimeReportModel()
						{
							ProjectManager = "Project Manager - " + pm.FullName,
							Client = c.ClientName,
							Project = p.ProjectName,
							Activity = a.ActivityName,
							Period = wa.Period,
							DayWorked = wa.DayWorked,
							UserName = u.UserName,
							FullName = u.FullName,
							HoursWorked = wa.HoursWorked,
							HoursQuoted = 0,
							TicketReference = wa.TicketReference,
							Remark = wa.Remark
						})
					)
					.Distinct()
					.ToList();
			}
		}

		#endregion

		#region MIS ALLOCATED TIME

		public static List<MISAllocatedTimeModel> GetMISAllocatedTimeForPeriod(DateTime dateFrom, DateTime dateTo, string passPhrase)
		{
			bool isValid = false;

			var data = (from wa in DB.WorkAllocations
						join a in DB.Activities on wa.ActivityID equals a.ActivityID
						join p in DB.Projects on a.ProjectID equals p.ProjectID
						join r in DB.Regions on p.RegionID equals r.RegionID
						join c in DB.Clients on p.ClientID equals c.ClientID
						join u in DB.Users on wa.UserID equals u.UserID
						join dv in DB.Divisions on u.DivisionID equals dv.DivisionID
						join d in DB.Departments on a.DepartmentID equals d.DepartmentID
						join b in DB.BillingMethods on p.BillingMethodID equals b.BillingMethodID
						join wd in DB.WorkableDaysPerPeriods on wa.Period equals wd.Period
						join ctc in DB.UserCostToCompanies on u.UserID equals ctc.UserID
						where wa.DayWorked >= dateFrom
						&& wa.DayWorked <= dateTo
						&& ctc.Period == wa.Period
						select new
						{
							Region = r,
							Department = d,
							Client = c,
							Project = p,
							User = u,
							BillingMethod = b,
							WorkAllocation = wa,
							WorkableDays = wd,
							CostToCompany = ctc
						}
						)
						.Distinct()
						.OrderBy(r => r.Region.RegionName)
						.ThenBy(d => d.Department.DepartmentName)
						.ThenBy(c => c.Client.ClientName)
						.ThenBy(u => u.User.FullName)
						.ThenBy(wd => wd.WorkAllocation.DayWorked)
						.ToList();

			var list = new List<MISAllocatedTimeModel>();

			foreach (var item in data)
			{
				var model = new MISAllocatedTimeModel
				{
					Region = item.Region.RegionName,
					Department = item.Department.DepartmentName,
					Client = item.Client.ClientName,
					Project = item.Project.ProjectName,
					Resource = item.User.FullName,
					BillingMethod = item.BillingMethod.BillingMethodName,
					DayWorked = item.WorkAllocation.DayWorked,
					HoursWorked = item.WorkAllocation.HoursWorked,
					CostToCompany = (item.WorkAllocation.HoursWorked / (item.User.WorkHours * item.WorkableDays.NumberOfDays)) * item.CostToCompany.GetCostToCompanyAmount(passPhrase, out isValid) * 1.33
				};
				list.Add(model);
			}

			return list;
		}

		public static List<MISAllocatedTimeModel> GetMISAllocatedTimeForDivision(DateTime dateFrom, DateTime dateTo, int divisionID, string passPhrase)
		{
			bool isValid = false;

			var data = (from wa in DB.WorkAllocations
						join a in DB.Activities on wa.ActivityID equals a.ActivityID
						join p in DB.Projects on a.ProjectID equals p.ProjectID
						join r in DB.Regions on p.RegionID equals r.RegionID
						join c in DB.Clients on p.ClientID equals c.ClientID
						join u in DB.Users on wa.UserID equals u.UserID
						join dv in DB.Divisions on u.DivisionID equals dv.DivisionID
						join d in DB.Departments on a.DepartmentID equals d.DepartmentID
						join b in DB.BillingMethods on p.BillingMethodID equals b.BillingMethodID
						join wd in DB.WorkableDaysPerPeriods on wa.Period equals wd.Period
						join ctc in DB.UserCostToCompanies on u.UserID equals ctc.UserID
						where wa.DayWorked >= dateFrom
						&& wa.DayWorked <= dateTo
						&& dv.DivisionID == divisionID
						&& ctc.Period == wa.Period
						select new
						{
							Region = r,
							Department = d,
							Client = c,
							Project = p,
							User = u,
							BillingMethod = b,
							WorkAllocation = wa,
							WorkableDays = wd,
							CostToCompany = ctc
						}
						)
						.Distinct()
						.OrderBy(r => r.Region.RegionName)
						.ThenBy(d => d.Department.DepartmentName)
						.ThenBy(c => c.Client.ClientName)
						.ThenBy(u => u.User.FullName)
						.ThenBy(wd => wd.WorkAllocation.DayWorked)
						.ToList();

			var list = new List<MISAllocatedTimeModel>();

			foreach (var item in data)
			{
				var model = new MISAllocatedTimeModel
				{
					Region = item.Region.RegionName,
					Department = item.Department.DepartmentName,
					Client = item.Client.ClientName,
					Project = item.Project.ProjectName,
					Resource = item.User.FullName,
					BillingMethod = item.BillingMethod.BillingMethodName,
					DayWorked = item.WorkAllocation.DayWorked,
					HoursWorked = item.WorkAllocation.HoursWorked,
					CostToCompany = (item.WorkAllocation.HoursWorked / (item.User.WorkHours * item.WorkableDays.NumberOfDays)) * item.CostToCompany.GetCostToCompanyAmount(passPhrase, out isValid) * 1.33
				};
				list.Add(model);
			}

			return list;
		}

		public static List<MISAllocatedTimeModel> GetMISAllocatedTimeForDepartment(DateTime dateFrom, DateTime dateTo, int departmentID, string passPhrase)
		{
			bool isValid = false;

			var data = (from wa in DB.WorkAllocations
						join a in DB.Activities on wa.ActivityID equals a.ActivityID
						join p in DB.Projects on a.ProjectID equals p.ProjectID
						join r in DB.Regions on p.RegionID equals r.RegionID
						join c in DB.Clients on p.ClientID equals c.ClientID
						join u in DB.Users on wa.UserID equals u.UserID
						join dv in DB.Divisions on u.DivisionID equals dv.DivisionID
						join d in DB.Departments on a.DepartmentID equals d.DepartmentID
						join b in DB.BillingMethods on p.BillingMethodID equals b.BillingMethodID
						join wd in DB.WorkableDaysPerPeriods on wa.Period equals wd.Period
						join ctc in DB.UserCostToCompanies on u.UserID equals ctc.UserID
						where wa.DayWorked >= dateFrom
						&& wa.DayWorked <= dateTo
						&& d.DepartmentID == departmentID
						&& ctc.Period == wa.Period
						select new
						{
							Region = r,
							Department = d,
							Client = c,
							Project = p,
							User = u,
							BillingMethod = b,
							WorkAllocation = wa,
							WorkableDays = wd,
							CostToCompany = ctc
						}
						)
						.Distinct()
						.OrderBy(r => r.Region.RegionName)
						.ThenBy(d => d.Department.DepartmentName)
						.ThenBy(c => c.Client.ClientName)
						.ThenBy(u => u.User.FullName)
						.ThenBy(wd => wd.WorkAllocation.DayWorked)
						.ToList();

			var list = new List<MISAllocatedTimeModel>();

			foreach (var item in data)
			{
				var model = new MISAllocatedTimeModel
				{
					Region = item.Region.RegionName,
					Department = item.Department.DepartmentName,
					Client = item.Client.ClientName,
					Project = item.Project.ProjectName,
					Resource = item.User.FullName,
					BillingMethod = item.BillingMethod.BillingMethodName,
					DayWorked = item.WorkAllocation.DayWorked,
					HoursWorked = item.WorkAllocation.HoursWorked,
					CostToCompany = (item.WorkAllocation.HoursWorked / (item.User.WorkHours * item.WorkableDays.NumberOfDays)) * item.CostToCompany.GetCostToCompanyAmount(passPhrase, out isValid) * 1.33
				};
				list.Add(model);
			}

			return list;
		}

		public static List<MISAllocatedTimeModel> GetMISAllocatedTimeForDivisionDepartment(DateTime dateFrom, DateTime dateTo, int divisionID, int departmentID, string passPhrase)
		{
			bool isValid = false;

			var data = (from wa in DB.WorkAllocations
						join a in DB.Activities on wa.ActivityID equals a.ActivityID
						join p in DB.Projects on a.ProjectID equals p.ProjectID
						join r in DB.Regions on p.RegionID equals r.RegionID
						join c in DB.Clients on p.ClientID equals c.ClientID
						join u in DB.Users on wa.UserID equals u.UserID
						join dv in DB.Divisions on u.DivisionID equals dv.DivisionID
						join d in DB.Departments on a.DepartmentID equals d.DepartmentID
						join b in DB.BillingMethods on p.BillingMethodID equals b.BillingMethodID
						join wd in DB.WorkableDaysPerPeriods on wa.Period equals wd.Period
						join ctc in DB.UserCostToCompanies on u.UserID equals ctc.UserID
						where wa.DayWorked >= dateFrom
						&& wa.DayWorked <= dateTo
						&& dv.DivisionID == divisionID
						&& d.DepartmentID == departmentID
						&& ctc.Period == wa.Period
						select new
						{
							Region = r,
							Department = d,
							Client = c,
							Project = p,
							User = u,
							BillingMethod = b,
							WorkAllocation = wa,
							WorkableDays = wd,
							CostToCompany = ctc
						}
						)
						.Distinct()
						.OrderBy(r => r.Region.RegionName)
						.ThenBy(d => d.Department.DepartmentName)
						.ThenBy(c => c.Client.ClientName)
						.ThenBy(u => u.User.FullName)
						.ThenBy(wd => wd.WorkAllocation.DayWorked)
						.ToList();

			var list = new List<MISAllocatedTimeModel>();

			foreach (var item in data)
			{
				var model = new MISAllocatedTimeModel
				{
					Region = item.Region.RegionName,
					Department = item.Department.DepartmentName,
					Client = item.Client.ClientName,
					Project = item.Project.ProjectName,
					Resource = item.User.FullName,
					BillingMethod = item.BillingMethod.BillingMethodName,
					DayWorked = item.WorkAllocation.DayWorked,
					HoursWorked = item.WorkAllocation.HoursWorked,
					CostToCompany = (item.WorkAllocation.HoursWorked / (item.User.WorkHours * item.WorkableDays.NumberOfDays)) * item.CostToCompany.GetCostToCompanyAmount(passPhrase, out isValid) * 1.33
				};
				list.Add(model);
			}

			return list;
		}

		#endregion

		#region NOTE EXTENDED MODELS

		internal static NoteDetailModel GetDetailedNote(int workAllocationID) => (from n in DB.Notes
																				  join wa in DB.WorkAllocations on n.WorkAllocationID equals wa.WorkAllocationID
																				  join u in DB.Users on wa.UserID equals u.UserID
																				  join a in DB.Activities on wa.ActivityID equals a.ActivityID
																				  join p in DB.Projects on a.ProjectID equals p.ProjectID
																				  where n.WorkAllocationID == workAllocationID
																				  select new NoteDetailModel
																				  {
																					  Note = n,
																					  WorkAllocation = wa,
																					  User = u,
																					  Activity = a,
																					  Project = p
																				  }).SingleOrDefault();

		#endregion

		#region PROJECT

		internal static Project GetProject(int activityID) => (from a in DB.Activities
															   join p in DB.Projects on a.ProjectID equals p.ProjectID
															   where a.ActivityID == activityID
															   select p
				   )
				   .SingleOrDefault();

		#endregion

		#region INVOICE CREDITS

		internal static InvoiceCredit GetInvoiceCredit(int workAllocationID)
		{
			if (workAllocationID <= 0)
				return null;

			return DB.InvoiceCredits
					.SingleOrDefault(ic => ic.WorkAllocationID == workAllocationID);
		}

		internal static void DeleteInvoiceCredit(InvoiceCredit invCredit)
		{
			if (invCredit == null)
				return;

			DB.InvoiceCredits.Remove(invCredit);
			DB.SaveChanges();
		}

		#endregion

		#region

		public static double GetAllocatedHoursByTicketReference(string ticketReference)
		{
			var data = DB.WorkAllocations
					.Where(w => w.TicketReference == ticketReference);

			if (data.Count() > 0)
				return data.Sum(w => w.HoursWorked);
			else
				return 0;
		}

		#endregion

		internal static void Save(WorkAllocation workAllocation)
		{
			if (workAllocation == null)
				return;

			if (workAllocation.WorkAllocationID <= 0)
				DB.WorkAllocations.Add(workAllocation);

			DB.SaveChanges();
		}

		#region JSON LOOKUPS

		public static List<JsonLookup> AutoCompleteTicket(string term, int topCount, int clientID, int projectID) => (from t in DB.Tickets
																													  where t.ClientID == clientID
																													  && t.ProjectID == projectID
																													  && !t.IsDeleted
																													  && t.TicketReference.StartsWith(term)
																													  select new JsonLookup
																													  {
																														  id = t.TicketID,
																														  label = t.TicketReference,
																														  value = t.TicketReference
																													  }).Distinct()
					.OrderBy(p => p.label)
					.Take(topCount)
					.ToList();

		#endregion
	}
}