namespace Enabill.Models.Dto
{
	public class InvoiceRuleModel
	{
		#region INITIALIZATION

		//public InvoiceRuleModel(User userRequesting, InvoiceRule ir, Client client)
		//{
		//    InvoiceRule = LoadInvoiceRule(ir, client);
		//    Client = client;
		//    Contacts = LoadContacts(userRequesting, InvoiceRule, client);
		//    TM = LoadActivities(BillingMethodType.TimeMaterial, InvoiceRule, client);//client.GetActivities(BillingMethodType.TimeMaterial, false);
		//    FixedCost = LoadProjects(InvoiceRule, client);
		//    SLA = LoadActivities(BillingMethodType.SLA, InvoiceRule, client);//client.GetActivities(BillingMethodType.SLA, false);
		//    Travel = client.GetActivities(BillingMethodType.Travel, false);
		//    AdHoc = client.GetActivities(BillingMethodType.AdHoc, false);
		//    InvoiceRuleLines = InvoiceRule.GetInvoiceRuleLines();
		//}

		#endregion INITIALIZATION

		#region PROPERTIES

		//public InvoiceRule InvoiceRule { get; internal set; }
		//public Client Client { get; internal set; }
		//public List<ContactSelectModel> Contacts { get; internal set; }
		//public List<ActivityDetail> TM { get; internal set; }
		//public List<ProjectSelectModel> FixedCost { get; internal set; }
		//public List<InvoiceRuleLine> FixedCostLines { get; internal set; }
		//public List<ActivityDetail> SLA { get; internal set; }
		//public List<ActivityDetail> Travel { get; internal set; }
		//public List<ActivityDetail> AdHoc { get; internal set; }
		//public List<InvoiceRuleLine> InvoiceRuleLines { get; internal set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		//private InvoiceRule LoadInvoiceRule(InvoiceRule ir, Client client)
		//{
		//    if (ir != null)
		//    {
		//        return ir;
		//    }

		//    ir = new InvoiceRule()
		//    {
		//        ClientID = client.ClientID,
		//        DateFrom = DateTime.Today.ToFirstDayOfMonth(),
		//        InvoiceAmountExclVAT = 0,
		//        InvoiceSubCategoryID = 1
		//    };

		//    return ir;
		//}

		//private List<ContactSelectModel> LoadContacts(User userRequesting, InvoiceRule ir, Client client)
		//{
		//    List<ContactSelectModel> model = new List<ContactSelectModel>();
		//    foreach (var contact in client.GetContacts())
		//    {
		//        model.Add(new ContactSelectModel { Contact = contact, IsSelected = false });
		//    }

		//    foreach (Contact contact in ir.Contacts)
		//    {
		//        model.Single(c => c.Contact == contact).IsSelected = true;
		//    }

		//    return model;
		//}

		//private List<ActivityDetail> LoadActivities(BillingMethodType billingMethodType, InvoiceRule invoiceRule, Client client)
		//{
		//    List<ActivityDetail> model = new List<ActivityDetail>();

		//    client.GetActivities(billingMethodType, false)
		//            .ForEach(a => model.Add(a));

		//    invoiceRule.GetDetailedActivities()
		//            .ForEach(a => model.Add(a));

		//    return model;
		//}

		//private List<ProjectSelectModel> LoadProjects(InvoiceRule invoiceRule, Client client)
		//{
		//    List<ProjectSelectModel> model = new List<ProjectSelectModel>();
		//    client.GetProjects(BillingMethodType.FixedCost, false)
		//        .ForEach(p => model.Add(p));

		//    invoiceRule.GetProjects()
		//        .ForEach(p => model.Add(p));

		//    return model;
		//}

		#endregion FUNCTIONS
	}
}