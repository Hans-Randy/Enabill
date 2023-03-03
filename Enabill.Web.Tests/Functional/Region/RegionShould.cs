using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enabill.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enabill.Web.Controllers;
using System.Collections.Specialized;
using System.Web.Mvc;

namespace Enabill.Web.Tests.Functional.Region
{
	[TestClass]
	public class RegionShould
	{
		private EnabillContext context;
		[TestInitialize]
		public void Init() => EnabillSettings.Setup(
				cntxt => this.context = cntxt as EnabillContext,
				() => this.context,
				() => new EnabillContext()
			);

		[TestMethod]
		public void StoreUniqueRegionNameAndShortCode()
		{
			//Arrange
			var region = new Enabill.Models.Region()
			{
				RegionName = "Gauteng",
				RegionShortCode = "GAU"
			};
			var controller = new RegionController();
			var formCollection = new FormCollection
			{
				new NameValueCollection
				{
					{ "RegionName", $"{region.RegionName}" },
					{ "RegionShortCode",  $"{region.RegionShortCode}" },
				}
			};
			//Act
			var data = controller.Create(formCollection);
			//Assert
			Assert.IsNotNull(data);		
		}

		[TestMethod]
		public void UpdateUniqueRegionNameAndShortCode()
		{
			//Arrange
			var region = new Enabill.Models.Region()
			{
				RegionName = "Gauteng",
				RegionShortCode = "GAU"
			};
			var controller = new RegionController();
			var formCollection = new FormCollection
			{
				new NameValueCollection
				{
					{ "RegionName", $"{region.RegionName}" },
					{ "RegionShortCode",  $"{region.RegionShortCode}" },
				}
			};
			//Act
			var data = controller.Edit(region.RegionID,formCollection);
			//Assert
			Assert.IsNotNull(data);
		}

	}
}
