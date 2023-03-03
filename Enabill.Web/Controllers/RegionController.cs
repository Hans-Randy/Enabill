using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using Enabill.Models;
using Enabill.Properties;
using Enabill.Repos;
using Enabill.Web.Models;
using StoryWriter.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class RegionController : SearchableController
	{
		protected override string GetSearchLabelText() => "regions";
		public override ActionResult Index(string q, bool? isActive)
		{
			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			this.ViewBag.q = InputHistory.Get(q, HistoryItemType.RegionSearchCriteria);
			this.ViewBag.Region_b = InputHistory.Get(isActive, HistoryItemType.RegionFilterBy, true);

			SetViewData(false, this.ViewBag.Region_b);

			List<RegionSearchResult> model = Region.Search(this.CurrentUser, this.ViewBag.q, this.ViewBag.Region_b, false);

			SaveAllInput(HistoryItemType.RegionSearchCriteria, this.ViewBag.q, HistoryItemType.RegionFilterBy, this.ViewBag.Region_b);

			return this.View(model);
		}
		[HttpGet]
		public ActionResult Create()
		{
			var region = RegionRepo.GetNew();
			this.SetViewDataLists();

			return this.View(region);
		}
		[HttpPost]
		public ActionResult Create(FormCollection form)
		{
			try
			{
				var region = RegionRepo.GetNew();
				region = new Region(form["RegionName"].ToString(), form["RegionShortCode"].ToString(), true);
				region.ValidateSave();

				this.UpdateModel(region);
				region.Save(this.CurrentUser);

				this.SetViewDataLists(region);

				return this.Json(new
				{
					IsError = false,
					Description = "Region saved successfully",
					Url = "/Region/Edit/" + region.RegionID
				});
			}
			catch (Exception ex)
			{	
				return this.ReturnJsonException(ex);
			}
			
		}
		[HttpGet]
		public ActionResult Edit(int id)
		{
			var model = RegionRepo.GetByID(id);

			if (model == null)
				return this.ErrorView(new ErrorModel(Resources.ERR_NoRegion_Message));

			if (!this.CurrentUser.CanManage(model))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));
			model.IsNew = false;
			this.SetViewDataLists(model);

			return this.View(model);
		}
		[HttpPost]
		public ActionResult Edit(int regionId, FormCollection form)
		{
			try
			{
				var region = RegionRepo.GetByID(regionId);
				if (region == null)
					throw new EnabillException(Resources.ERR_NoRegionForUpdate_Message);
				if (region.RegionID == 0 || !int.TryParse(form["RegionID"], out int id))
					throw new EnabillException(Resources.ERR_NoRegionName_Message);
				if (string.IsNullOrEmpty(form["RegionName"]))
					throw new EnabillException(Resources.ERR_NoRegionName_Message);
				if (string.IsNullOrEmpty(form["RegionShortCode"]))
					throw new EnabillException(Resources.ERR_NoRegionShortCode_Message);
				if (string.IsNullOrEmpty(form["RegionStatusID"]))
					throw new EnabillException(Resources.ERR_NoRegionStatus_Message);
				int.TryParse(form["RegionStatusID"], out int statusID);

				region.RegionID = id;
				region.RegionName = form["RegionName"];
				region.RegionShortCode = form["RegionShortCode"];
				region.IsActive = statusID == 1;

				this.UpdateModel(region);
				region.Save(this.CurrentUser);

				this.SetViewDataLists(region);

				return this.PartialView("ucRegionDetail", region);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		
		}
		[HttpPost]
		public ActionResult RefreshList(string q, bool isActive, bool showAllRegions)
		{
			this.SetViewData(showAllRegions, isActive);

			this.ViewBag.q = InputHistory.Get(q, HistoryItemType.RegionSearchCriteria);
			this.ViewBag.Region_b = InputHistory.Get(isActive, HistoryItemType.RegionFilterBy, true);
			List<RegionSearchResult> model = Region.Search(this.CurrentUser, this.ViewBag.q, this.ViewBag.Region_b, showAllRegions);

			SaveAllInput(HistoryItemType.RegionSearchCriteria, this.ViewBag.q, HistoryItemType.RegionFilterBy, this.ViewBag.Region_b);

			return this.PartialView("Index", model);
		}
		private void SetViewData(bool? showAllRegions, bool? isRegionActive = true) => this.ViewData["StatusFilter"] = new List<SelectListItem>
			{
				new SelectListItem() { Value = ((int)StatusTypeEnum.Active).ToString(), Text = "Active", Selected = isRegionActive == true },
				new SelectListItem() { Value = ((int)StatusTypeEnum.Inactive).ToString(), Text = "Inactive", Selected = isRegionActive == false },
				new SelectListItem() { Value = ((int)StatusTypeEnum.All).ToString(), Text = "All", Selected = showAllRegions == true }
			};
		private void SetViewDataLists(Region model = null)
		{
			var statusFilters = new List<SelectListItem>
			{
				new SelectListItem() { Value = ((int)StatusTypeEnum.Active).ToString(), Text = StatusTypeEnum.Active.ToString()},
				new SelectListItem() { Value = ((int)StatusTypeEnum.Inactive).ToString(), Text = StatusTypeEnum.Inactive.ToString() },
				new SelectListItem() { Value = ((int)StatusTypeEnum.All).ToString(), Text = StatusTypeEnum.All.ToString()}
			};
			this.ViewData["StatusFilter"] = statusFilters;
			
			if (model == null)
				return;

			var statusList = RegionRepo.GetRegionStatusList().OrderBy(r => r.StatusName).ToList();
			var regionStatusList = new List<SelectListItem>();
			foreach (var item in statusList)
			{
				bool selectedStatus = false;
				if (model.IsActive & item.StatusID == (int)StatusTypeEnum.Active)
				{ selectedStatus = true; }
				if (!model.IsActive & item.StatusID == (int)StatusTypeEnum.Inactive)
				{ selectedStatus = true; }
				regionStatusList.Add(new SelectListItem { Value = $"{item.StatusID}", Text = item.StatusName, Selected = selectedStatus });
			}
			this.ViewData["RegionStatusList"] = regionStatusList;
		}
	}
}