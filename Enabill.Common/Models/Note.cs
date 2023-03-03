using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("Notes")]
	public class Note
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int NoteID { get; set; }

		[Required]
		public int WorkAllocationID { get; internal set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		[Required, MaxLength]
		public string NoteText { get; set; }

		[MaxLength(128)]
		public string UserModified { get; set; }

		[Required]
		public DateTime DateModified { get; set; }

		#endregion PROPERTIES

		#region INITIALIZATION

		public bool CanEdit => this.GetWorkAllocation().CanEdit;

		public bool CanDelete => this.CanEdit;

		#endregion INITIALIZATION

		#region NOTE SPECIFIC

		internal void Delete(User userDeleting)
		{
			if (this.GetUserID() != userDeleting.UserID && !userDeleting.CanManage(this.GetUser()))
				throw new UserRoleException("You do not have the required permissions to delete this note.");

			if (!this.CanDelete)
				throw new EnabillDomainException("This note is locked and can not be deleted.");

			NoteRepo.Delete(this);
		}

		#endregion NOTE SPECIFIC

		#region WORKALLOCATION

		public WorkAllocation GetWorkAllocation() => WorkAllocationRepo.GetByID(this.WorkAllocationID);

		#endregion WORKALLOCATION

		#region USER

		public int GetUserID() => this.GetWorkAllocation().UserID;

		public User GetUser() => UserRepo.GetByID(this.GetUserID());

		#endregion USER

		#region ACTIVITY

		public Activity GetActivity() => this.GetWorkAllocation().GetActivity();

		#endregion ACTIVITY

		#region PROJECT

		public Project GetProject() => this.GetActivity().GetProject();

		#endregion PROJECT

		#region DETAILED NOTE

		public static List<NoteDetailModel> GetForSearchCriteria(User userRequesting, string activityIDs, string userIDs, DateTime dateFrom, DateTime dateTo, string keyWord, int noteSearchAmt)
		{
			dateFrom = dateFrom.Date;
			dateTo = dateTo.Date;

			if (string.IsNullOrEmpty(keyWord))
				keyWord = string.Empty;

			if (noteSearchAmt <= 0)
				noteSearchAmt = 10000;

			if (dateFrom > dateTo)
			{
				var tempDate = dateTo;
				dateTo = dateFrom;
				dateFrom = tempDate;
			}

			var activityList = new List<int>();
			foreach (int id in activityIDs.ToIntArray())
			{
				activityList.Add(id);
			}

			var userList = new List<int>();
			foreach (int id in userIDs.ToIntArray())
			{
				userList.Add(id);
			}

			if (userRequesting.HasRole(UserRoleType.ProjectOwner) && userRequesting.HasRole(UserRoleType.Manager))
				return NoteRepo.GetForSearchCriteria(activityList, userList, dateFrom, dateTo, keyWord);
			if (userRequesting.HasRole(UserRoleType.ProjectOwner))
				return NoteRepo.GetForSearchCriteria(activityList, null, dateFrom, dateTo, keyWord);
			if (userRequesting.HasRole(UserRoleType.Manager))
				return NoteRepo.GetForSearchCriteria(null, userList, dateFrom, dateTo, keyWord);
			if (userRequesting.HasRole(UserRoleType.TimeCapturing))
				return NoteRepo.GetForSearchCriteria(activityList, null, dateFrom, dateTo, keyWord);

			throw new UserRoleException("You do not have the required permissions to see notes.");
		}

		public NoteDetailModel GetDetailed()
		{
			var wa = this.GetWorkAllocation();
			var act = wa.GetActivity();
			var project = act.GetProject();
			var user = wa.GetUser();

			return new NoteDetailModel()
			{
				Note = this,
				WorkAllocation = wa,
				Activity = act,
				Project = project,
				User = user
			};
		}

		#endregion DETAILED NOTE
	}
}