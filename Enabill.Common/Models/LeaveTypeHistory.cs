using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("LeaveTypeHistories")]
	public class LeaveTypeHistory
	{
		/*
		 * Created by Gavin van Gent -- if possible, discuss with him before making this change
		 * Currently, this model is not being used, but, when the annual days given to a user is changed
		 * (currently the SA gov stipulates that 22 annual leave days are minimum per staff)
		 * this table wil have to be created in the database and a record specifying that annual leave was
		 * 22 days between 2011-07-01 and the implementation date that the governament would give to a new value
		*/

		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int LeaveTypeHistoryID { get; private set; }

		[Required, EnumDataType(typeof(LeaveTypeEnum))]
		public int LeaveTypeID { get; internal set; }

		[Required]
		public double DaysGivenAnnually { get; internal set; }

		[Required]
		public DateTime DateFrom { get; internal set; }

		public DateTime? DateTo { get; internal set; }

		#endregion PROPERTIES

		#region LEAVE TYPE HISTORY

		//This method will get the record that saves the annual leave given
		//to a user annually, but a table or record does not exist in the database yet,
		//so when an entry needs to be inserted, this table will have to be built
		//and some small changes made

		/*
		 * When changing over to table:
		 * 1 - Create table in database with the same properties as above
		 * 2 - Create a record in the table that captures the details of the past laws.
		 * 3 - Create a new record with the new law stipulations
		 * 4 - follow instructions below
		 */

		internal static LeaveTypeHistory GetForDate(LeaveTypeEnum leaveType, DateTime refDate)
		{
			//Only annual leave is credited to a user, and therefore, this must
			//break if any other leave type is requested
			if (leaveType != LeaveTypeEnum.Annual)
				throw new EnabillDomainException(string.Format($"{leaveType} does not allow for a balance. Action cancelled."));

			return new LeaveTypeHistory()
			{
				LeaveTypeHistoryID = 1,
				LeaveTypeID = (int)LeaveTypeEnum.Annual,
				DateFrom = EnabillSettings.SiteStartDate,
				DateTo = null,
				DaysGivenAnnually = 22
			};

			//One day when this model moves to a table in the database, remove this line and the above code

			//Only annual leave is credited to a user, and therefore, this must
			//break if any other leave type is requested
			if (leaveType != LeaveTypeEnum.Annual)
				throw new EnabillDomainException(string.Format($"{leaveType} does not allow for a balance. Action cancelled."));

			var model = LeaveTypeHistoryRepo.GetForTypeAndDate((int)leaveType, refDate);
			if (model == null)
				throw new EnabillDomainException("There is an error with the records inserted into the database, or no record has been inserted yet.");

			return model;
		}

		#endregion LEAVE TYPE HISTORY
	}
}