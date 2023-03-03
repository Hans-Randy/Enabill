using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Web.Models
{
	public class ActivityGroup
	{
		public ActivityDetail Activity { get; internal set; }

		public List<WorkAllocationExtendedModel> WorkAllocations { get; internal set; }
	}

	public class InvoiceWorkAllocationModel
	{
		#region INITIALIZATION

		public InvoiceWorkAllocationModel(Invoice invoice)
		{
			this.Invoice = invoice;
			this.Client = invoice.GetClient(Settings.Current.SystemUser);
			this.Activities = this.LoadAllocations(invoice);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public Client Client { get; set; }
		public Invoice Invoice { get; set; }

		public List<ActivityGroup> Activities { get; set; }
		//public List<WorkAllocationExtendedModel> WorkAllocations { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<ActivityGroup> LoadAllocations(Invoice invoice)
		{
			var model = new List<ActivityGroup>();
			ActivityGroup group;

			foreach (var allocation in invoice.GetWorkAllocationsExtendedModel())
			{
				group = model.SingleOrDefault(m => m.Activity.ActivityID == allocation.Activity.ActivityID);

				if (group == null)
				{
					group = new ActivityGroup
					{
						Activity = allocation.Activity,
						WorkAllocations = new List<WorkAllocationExtendedModel>()
					};
					model.Add(group);
				}

				group.WorkAllocations.Add(allocation);
			}

			return model;
		}

		#endregion FUNCTIONS
	}
}