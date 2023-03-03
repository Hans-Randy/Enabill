using System.Configuration;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Alacrity.DataAccess;
using Alacrity.DataAccess.SqlServer;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Enabill.Repository.Interfaces;
using Enabill.Repository.SqlServer;
using GdPicture14.WEB;

namespace Enabill.Web
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode,
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : HttpApplication
	{
		protected void Application_Start()
		{
			var builder = new ContainerBuilder();

			var config = GlobalConfiguration.Configuration;

			builder.RegisterModelBinders(Assembly.GetExecutingAssembly());
			builder.RegisterModelBinderProvider();
			builder.RegisterControllers(Assembly.GetExecutingAssembly());

			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

			builder.RegisterType<UserRepository>().As<IUserRepository>();
			builder.RegisterType<ClientRepository>().As<IClientRepository>();
			builder.RegisterType<DbManager>().As<IDbManager>()
				.WithParameter("connectionStringName", "EnabillContext");

			builder.RegisterModule<AutofacWebTypesModule>();

			var container = builder.Build();
			DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
			config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);

			DocuViewareManager.SetupConfiguration();
			string docuViewareKey = ConfigurationManager.AppSettings["DocuViewareKey"];
			DocuViewareLicensing.RegisterKEY(docuViewareKey); //Unlocking DocuVieware

			EnabillSettings.Setup(
				cntxt => HttpContext.Current.Items["__DB__"] = cntxt,
				() => HttpContext.Current.Items["__DB__"],
				() => new DB.EnabillContext()
			);

			EnabillSettings.StoryWriterProjectID = 1;

			#region ENCRYPTION

			bool toEncrypt = true;

#if DEBUG
			toEncrypt = false;
#endif

			//Connection String
			EncryptConfig("/", "connectionStrings", toEncrypt);

			//SendGrid Password
			EncryptConfig("/", "sendGrid", toEncrypt);
		}

		protected static void EncryptConfig(string path, string configSection, bool toEncrypt)
		{
			var config = WebConfigurationManager.OpenWebConfiguration(path);
			var section = config.GetSection(configSection);

			if (section?.SectionInformation.IsProtected == toEncrypt)
				return;

			if (toEncrypt)
				section.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");
			else
				section.SectionInformation.UnprotectSection();

			config.Save();
		}

		#endregion ENCRYPTION

		protected void Session_Start()
		{
			Settings.Current.Passphrase = string.Empty;
			if (this.Request.Cookies["Enabill.aspxauth"] != null)
			{
				string s = this.Request.Cookies["Enabill.aspxauth"].Value;
				var ticket = FormsAuthentication.Decrypt(s);
				// log the user back on with it's ticket
				Settings.Current.LoginUser(ticket.Name, true);
			}
		}
	}
}