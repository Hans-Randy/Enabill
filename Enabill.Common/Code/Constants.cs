using System.Configuration;

namespace Enabill.Code
{
	public static class Constants
	{
		static Constants()
		{
			//Company
			COMPANYNAME = ConfigurationManager.AppSettings["CompanyName"];
			COMPANYREGISTRATIONNO = ConfigurationManager.AppSettings["CompanyRegistrationNo"];
			COMPANYVATNO = ConfigurationManager.AppSettings["CompanyVatNo"];

			//Banking
			BANK = ConfigurationManager.AppSettings["Bank"];
			BANKBRANCH = ConfigurationManager.AppSettings["BankBranch"];
			BANKBRANCHCODE = ConfigurationManager.AppSettings["BankBranchCode"];
			BANKACCOUNTNO = ConfigurationManager.AppSettings["BankAccountNo"];

			//Head Office
			HEADOFFICETELNO = ConfigurationManager.AppSettings["HeadOfficeTelNo"];
			HEADOFFICEFAXNO = ConfigurationManager.AppSettings["HeadOfficeFaxNo"];
			HEADOFFICEADDRESS1 = ConfigurationManager.AppSettings["HeadOfficeAddress1"];
			HEADOFFICEADDRESS2 = ConfigurationManager.AppSettings["HeadOfficeAddress2"];
			HEADOFFICEADDRESS3 = ConfigurationManager.AppSettings["HeadOfficeAddress3"];
			HEADOFFICEADDRESS4 = ConfigurationManager.AppSettings["HeadOfficeAddress4"];
			HEADOFFICEADDRESS5 = ConfigurationManager.AppSettings["HeadOfficeAddress5"];
			HEADOFFICEPOSTCODE = ConfigurationManager.AppSettings["HeadOfficePostCode"];

			//Branch 1
			BRANCH1TELNO = ConfigurationManager.AppSettings["Branch1TelNo"];
			BRANCH1FAXNO = ConfigurationManager.AppSettings["Branch1FaxNo"];
			BRANCH1ADDRESS1 = ConfigurationManager.AppSettings["Branch1Address1"];
			BRANCH1ADDRESS2 = ConfigurationManager.AppSettings["Branch1Address2"];
			BRANCH1ADDRESS3 = ConfigurationManager.AppSettings["Branch1Address3"];
			BRANCH1ADDRESS4 = ConfigurationManager.AppSettings["Branch1Address4"];
			BRANCH1ADDRESS5 = ConfigurationManager.AppSettings["Branch1Address5"];
			BRANCH1POSTCODE = ConfigurationManager.AppSettings["Branch1PostCode"];

			//Email Address
			EMAILADDRESSDEFAULT = ConfigurationManager.AppSettings["EmailAddressDefault"];
			EMAILADDRESSINFO = ConfigurationManager.AppSettings["EmailAddressInfo"];
			EMAILADDRESSACCOUNTS = ConfigurationManager.AppSettings["EmailAddressAccounts"];
			EMAILADDRESSSUPPORT = ConfigurationManager.AppSettings["EmailAddressSupport"];
			EMAILADDRESSHR = ConfigurationManager.AppSettings["EmailAddressHR"];
			EMAILADDRESSINVOICES = ConfigurationManager.AppSettings["EmailAddressInvoices"];

			//Email
			EMAILDOMAIN = ConfigurationManager.AppSettings["EmailDomain"];
			EMAILADDRESSDEFAULTFROM = ConfigurationManager.AppSettings["EmailAddressDefaultFrom"];
			EMAILADDRESSDEFAULTREPLYTO = ConfigurationManager.AppSettings["EmailAddressDefaultReplyTo"];
			EMAILADDRESSOVERRIDETO = ConfigurationManager.AppSettings["EmailAddressOverrideTo"];

			//Email Server Details
			EMAILSERVERSEND = ConfigurationManager.AppSettings["SendEmails"];
			EMAILSERVERSMTP = ConfigurationManager.AppSettings["SMTPServer"];
			EMAILSERVERMAILINGMETHOD = ConfigurationManager.AppSettings["MailingMethod"];

			//Site Details
			SITEADDRESS = ConfigurationManager.AppSettings["SiteAddress"];
			SITESTARTDATE = ConfigurationManager.AppSettings["SiteStartDate"];

			//Path Details
			PATHTEMP = ConfigurationManager.AppSettings["PathTemp"];
			PATHTEMPLATE = ConfigurationManager.AppSettings["PathTemplate"];
			PATHEXPENSE = ConfigurationManager.AppSettings["PathExpense"];
			PATHCONTRACT = ConfigurationManager.AppSettings["PathContract"];
			PATHTIMEREMINDERFILE = ConfigurationManager.AppSettings["PathTimeReminderFile"];
			PATHLOG = ConfigurationManager.AppSettings["PathLog"];

			//BuiltIn Details
			CONFIG = ConfigurationManager.AppSettings["Config"];

			//Encryption Details
			PASSPHRASE = "XyQEz%gyo*%o";
		}

		// Use a ReadOnly field insted of a constant as all assemblies using the constant need to be recompiled before a new value can be used. Not needed with ReadOnly.
		//Company
		public static readonly string COMPANYNAME;

		public static readonly string COMPANYREGISTRATIONNO;
		public static readonly string COMPANYVATNO;

		//Banking
		public static readonly string BANK;

		public static readonly string BANKBRANCH;
		public static readonly string BANKBRANCHCODE;
		public static readonly string BANKACCOUNTNO;

		//Head Office
		public static readonly string HEADOFFICETELNO;

		public static readonly string HEADOFFICEFAXNO;
		public static readonly string HEADOFFICEADDRESS1;
		public static readonly string HEADOFFICEADDRESS2;
		public static readonly string HEADOFFICEADDRESS3;
		public static readonly string HEADOFFICEADDRESS4;
		public static readonly string HEADOFFICEADDRESS5;
		public static readonly string HEADOFFICEPOSTCODE;

		//Branch 1
		public static readonly string BRANCH1TELNO;

		public static readonly string BRANCH1FAXNO;
		public static readonly string BRANCH1ADDRESS1;
		public static readonly string BRANCH1ADDRESS2;
		public static readonly string BRANCH1ADDRESS3;
		public static readonly string BRANCH1ADDRESS4;
		public static readonly string BRANCH1ADDRESS5;
		public static readonly string BRANCH1POSTCODE;

		//Email Address
		public static readonly string EMAILADDRESSDEFAULT;

		public static readonly string EMAILADDRESSINFO;
		public static readonly string EMAILADDRESSACCOUNTS;
		public static readonly string EMAILADDRESSSUPPORT;
		public static readonly string EMAILADDRESSHR;
		public static readonly string EMAILADDRESSINVOICES;

		//Email
		public static readonly string EMAILDOMAIN;

		public static readonly string EMAILADDRESSDEFAULTREPLYTO;
		public static readonly string EMAILADDRESSDEFAULTFROM;
		public static readonly string EMAILADDRESSOVERRIDETO;

		//Email
		public static readonly string EMAILSERVERSEND;

		public static readonly string EMAILSERVERSMTP;
		public static readonly string EMAILSERVERMAILINGMETHOD;

		//Site
		public static readonly string SITESTARTDATE;

		public static readonly string SITEADDRESS;

		//Path
		public static readonly string PATHTEMPLATE;

		public static readonly string PATHTEMP;
		public static readonly string PATHEXPENSE;
		public static readonly string PATHCONTRACT;
		public static readonly string PATHLOG;
		public static readonly string PATHTIMEREMINDERFILE;

		//BuiltIn
		public static readonly string CONFIG;

		//Encryption
		public static readonly string PASSPHRASE;
	}
}