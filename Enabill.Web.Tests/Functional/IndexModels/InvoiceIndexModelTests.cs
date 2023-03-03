using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.DB;
using Enabill.Models;
using Enabill.Repos;
using Enabill.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enabill.Web.Tests
{
	[TestClass]
	public class InvoiceIndexModelTests
	{
		private EnabillContext context;

		[TestInitialize]
		public void Init() => EnabillSettings.Setup(
				cntxt => this.context = cntxt as EnabillContext,
				() => this.context,
				() => new EnabillContext()
			);

		[TestMethod]
		public void InvoiceIndexModel_Account_Manager_View_Open_Invoices_Only_Test()
		{
			var users = UserRepo.GetAll();
			var user = users.Where(u => u.HasRole(UserRoleType.InvoiceAdministrator)).Where(u => !u.HasRole(UserRoleType.Accountant)).FirstOrDefault();

			Assert.IsNotNull(user);
			Assert.IsTrue(user.HasRole(UserRoleType.InvoiceAdministrator));
			Assert.IsFalse(user.HasRole(UserRoleType.Accountant));

			var invoiceIndexFilterModel = new InvoiceIndexFilterModel {
				MyClients = true,
				ShowMyClientsCheckBox = true,
				TimeMaterial = true,
				FixedCost = true,
				SLA = true,
				Travel = true,
				AdHoc = true,
				MonthlyFixedCost = true,
				ActivityFixedCost = true,
				Open = true,
				InProgress = false,
				Ready = false,
				Complete = false,
				UserRequesting = user,
				DateFrom = new DateTime(2019, 03, 01),
				DateTo = new DateTime(2019, 03, 31)
			};

			var invoiceIndexModel = new InvoiceIndexModel(invoiceIndexFilterModel);			

			Assert.IsTrue(invoiceIndexModel.Open);
			Assert.IsFalse(invoiceIndexModel.InProgress);
			Assert.IsFalse(invoiceIndexModel.Ready);
			Assert.IsFalse(invoiceIndexModel.Complete);
		}

		[TestMethod]
		public void InvoiceIndexModel_Accountant_View_Ready_Invoices_Only_Test()
		{
			var users = UserRepo.GetAll();
			var user = users.Where(u => u.HasRole(UserRoleType.InvoiceAdministrator)).Where(u => u.HasRole(UserRoleType.Accountant)).FirstOrDefault();

			Assert.IsNotNull(user);
			Assert.IsTrue(user.HasRole(UserRoleType.InvoiceAdministrator));
			Assert.IsTrue(user.HasRole(UserRoleType.Accountant));

			var invoiceIndexFilterModel = new InvoiceIndexFilterModel
			{
				MyClients = true,
				ShowMyClientsCheckBox = true,
				TimeMaterial = true,
				FixedCost = true,
				SLA = true,
				Travel = true,
				AdHoc = true,
				MonthlyFixedCost = true,
				ActivityFixedCost = true,
				Open = false,
				InProgress = false,
				Ready = true,
				Complete = false,
				UserRequesting = user,
				DateFrom = new DateTime(2019, 03, 01),
				DateTo = new DateTime(2019, 03, 31)
			};

			var invoiceIndexModel = new InvoiceIndexModel(invoiceIndexFilterModel);

			Assert.IsFalse(invoiceIndexModel.Open);
			Assert.IsFalse(invoiceIndexModel.InProgress);
			Assert.IsTrue(invoiceIndexModel.Ready);
			Assert.IsFalse(invoiceIndexModel.Complete);
		}

		[TestMethod]
		public void InvoiceIndexModel_Accountant_No_Allowable_Invoice_Statuses_Test()
		{
			var users = UserRepo.GetAll();
			var user = users.Where(u => u.HasRole(UserRoleType.InvoiceAdministrator)).Where(u => u.HasRole(UserRoleType.Accountant)).FirstOrDefault();

			Assert.IsNotNull(user);
			Assert.IsTrue(user.HasRole(UserRoleType.InvoiceAdministrator));
			Assert.IsTrue(user.HasRole(UserRoleType.Accountant));

			var invoiceIndexFilterModel = new InvoiceIndexFilterModel
			{
				MyClients = true,
				ShowMyClientsCheckBox = true,
				TimeMaterial = true,
				FixedCost = true,
				SLA = true,
				Travel = true,
				AdHoc = true,
				MonthlyFixedCost = true,
				ActivityFixedCost = true,
				Open = false,
				InProgress = false,
				Ready = true,
				Complete = false,
				UserRequesting = user,
				DateFrom = new DateTime(2019, 03, 01),
				DateTo = new DateTime(2019, 03, 31)
			};

			var invoiceIndexModel = new InvoiceIndexModel(invoiceIndexFilterModel);

			Assert.IsTrue(invoiceIndexModel.AllowedInvoiceStatusTypes.Count() == 0);
		}

		[TestMethod]
		public void InvoiceIndexModel_Accountant_Ready_Allowable_Invoice_Statuses_Test()
		{
			var users = UserRepo.GetAll();
			var user = users.Where(u => u.HasRole(UserRoleType.InvoiceAdministrator)).Where(u => !u.HasRole(UserRoleType.Accountant)).FirstOrDefault();

			Assert.IsNotNull(user);
			Assert.IsTrue(user.HasRole(UserRoleType.InvoiceAdministrator));
			Assert.IsFalse(user.HasRole(UserRoleType.Accountant));

			var invoiceIndexFilterModel = new InvoiceIndexFilterModel
			{
				MyClients = true,
				ShowMyClientsCheckBox = true,
				TimeMaterial = true,
				FixedCost = true,
				SLA = true,
				Travel = true,
				AdHoc = true,
				MonthlyFixedCost = true,
				ActivityFixedCost = true,
				Open = true,
				InProgress = false,
				Ready = false,
				Complete = false,
				UserRequesting = user,
				DateFrom = new DateTime(2019, 03, 01),
				DateTo = new DateTime(2019, 03, 31)
			};

			var invoiceIndexModel = new InvoiceIndexModel(invoiceIndexFilterModel);

			Assert.IsTrue(invoiceIndexModel.AllowedInvoiceStatusTypes.Where(a => a == InvoiceStatusType.Ready).Any());
		}

		[TestMethod]
		public void InvoiceModel_BitWiseTotal_Test()
		{
			bool open = true;
			//1
			bool inProgress = true;
			//2
			bool ready = true;
			//4
			bool complete = true;
			//8

			int statusBWTotal = 
				(open ? (int)InvoiceStatusType.Open : 0) + 
				(inProgress ? (int)InvoiceStatusType.InProgress : 0) + 
				(ready ? (int)InvoiceStatusType.Ready : 0) + 
				(complete ? (int)InvoiceStatusType.Complete : 0);

			//1 + 2 + 4 + 8 = 15
			Assert.AreEqual(statusBWTotal, 15);

			int invoiceStatusID = 5;

			//5 & 15
			//5  = 0101
			//15 = 1111
			//5  = 0101
			int bw = invoiceStatusID & statusBWTotal;

			Assert.AreEqual(bw, 5);
		}
	}
}
