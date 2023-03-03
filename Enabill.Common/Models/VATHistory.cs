using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("VATHistories")]
	public class VATHistory
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int VATRateID { get; private set; }

		[Required]
		public double VATRate { get; private set; }

		[Required]
		public DateTime ImplementationDate { get; private set; }

		/*
		 * For this model, only the implimentation date will be saved, so from 2011-07-01 there will be a record for 14%
		 * If this rate was to change on 2015-01-01, the next record would set the implimentation date to 2015-01-01
		 * Dates between 2011-07-01 and 2014-12-31 will use the 14% VAT rate, and thereafter, use the new rate
		 */

		/*
		 * Created by Gavin van Gent -- if possible, discuss with him before making this change
		 * This Table has not yet been created in the database. When it is required that a new record be inserted,
		 * create the table in the database with the same properties as above, and then alter the code below as instructed
		 * and the site should continue as normal.
		 */

		#endregion PROPERTIES

		#region VAT HISTORY

		internal static double GetCurrentVATRate() => GetVATRateOnDate(DateTime.Today);

		private static double GetVATRateOnDate(DateTime refDate) => 15;

		/*
		 * We do not have this table in the database yet, because we know the current VAT rate is 14%, if this were to change:
		 * Create the table in the database with the same properties as above.
		 * Add a record that would have a VAT rate value of 0.14 and an impementation date that is the same as EnabillSettings.SiteStartDate
		 * Then add a new record with the new VAT rate and implementation date
		 * Then remove this code and the code above this, inside this method
		 *//*
		VATHistory vatModel = VATHistoryRepo.GetForDate(refDate);

		if (vatModel == null)
			return 0;

		return vatModel.VATRate;
		 */

		#endregion VAT HISTORY
	}
}