using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public class NoteRepo : BaseRepo
	{
		#region NOTE SPECIFIC

		internal static void Save(Note note)
		{
			if (note.NoteID == 0)
				DB.Notes.Add(note);

			DB.SaveChanges();
		}

		internal static void Delete(Note note)
		{
			DB.Notes.Remove(note);
			DB.SaveChanges();
		}

		internal static List<NoteDetailModel> GetForSearchCriteria(List<int> activityList, List<int> userList, DateTime dateFrom, DateTime dateTo, string keyWord)
		{
			//Returns a detailed list of notes for a list of users and activities
			if (userList == null)
				userList = new List<int>();

			if (activityList == null)
				activityList = new List<int>();

			var model = new List<NoteDetailModel>();

			if (activityList.Count > 0 && userList.Count > 0)
			{
				foreach (int userID in userList)
				{
					foreach (int actID in activityList)
					{
						var items = (from n in DB.Notes
									 join wa in DB.WorkAllocations on n.WorkAllocationID equals wa.WorkAllocationID
									 join u in DB.Users on wa.UserID equals u.UserID
									 join a in DB.Activities on wa.ActivityID equals a.ActivityID
									 join p in DB.Projects on a.ProjectID equals p.ProjectID
									 where wa.ActivityID == actID
										  && wa.UserID == userID
										  && wa.DayWorked >= dateFrom
										  && wa.DayWorked <= dateTo
									 select new NoteDetailModel
									 {
										 Note = n,
										 WorkAllocation = wa,
										 User = u,
										 Activity = a,
										 Project = p
									 })
													   .Where(m => m.Note.NoteText.Contains(keyWord))
													   .ToList();

						model.AddRange(items);
					}
				}

				return model;
			}

			if (activityList.Count > 0)
			{
				foreach (int actID in activityList)
				{
					var items = (from n in DB.Notes
								 join wa in DB.WorkAllocations on n.WorkAllocationID equals wa.WorkAllocationID
								 join u in DB.Users on wa.UserID equals u.UserID
								 join a in DB.Activities on wa.ActivityID equals a.ActivityID
								 join p in DB.Projects on a.ProjectID equals p.ProjectID
								 where wa.ActivityID == actID
										  && wa.DayWorked >= dateFrom
										  && wa.DayWorked <= dateTo
								 select new NoteDetailModel
								 {
									 Note = n,
									 WorkAllocation = wa,
									 User = u,
									 Activity = a,
									 Project = p
								 })
												   .Where(m => m.Note.NoteText.Contains(keyWord))
												   .ToList();

					model.AddRange(items);
				}

				return model;
			}

			if (userList.Count > 0)
			{
				foreach (int userID in userList)
				{
					var items = (from n in DB.Notes
								 join wa in DB.WorkAllocations on n.WorkAllocationID equals wa.WorkAllocationID
								 join u in DB.Users on wa.UserID equals u.UserID
								 join a in DB.Activities on wa.ActivityID equals a.ActivityID
								 join p in DB.Projects on a.ProjectID equals p.ProjectID
								 where wa.UserID == userID
										  && wa.DayWorked >= dateFrom
										  && wa.DayWorked <= dateTo
								 select new NoteDetailModel
								 {
									 Note = n,
									 WorkAllocation = wa,
									 User = u,
									 Activity = a,
									 Project = p
								 })
												   .Where(m => m.Note.NoteText.Contains(keyWord))
												   .ToList();

					model.AddRange(items);
				}

				return model;
			}

			//if we get this far, then there is a problem with the search criteria, return new list
			return new List<NoteDetailModel>();
		}

		#endregion NOTE SPECIFIC
	}
}