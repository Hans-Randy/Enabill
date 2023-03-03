using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Enabill.Models;
using Enabill.Repos;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class ForecastController : BaseController
	{
		[HttpGet]
		public ActionResult Index()
		{
			var activePeriod = FinPeriodRepo.GetCurrentFinPeriod();
			//Only set history if 1st time entering the forecast page else you loose the selected
			///search criteria when relisting after edit, copy etc

			if (InputHistory.Get(HistoryItemType.ForecastRegion, 0) == 0)
				InputHistory.Set(HistoryItemType.ForecastRegion, 0);

			if (InputHistory.Get(HistoryItemType.ForecastDivision, 0) == 0)
				InputHistory.Set(HistoryItemType.ForecastDivision, 0);

			if (InputHistory.Get(HistoryItemType.ForecastPeriodFrom, 0) == 0)
				InputHistory.Set(HistoryItemType.ForecastPeriodFrom, activePeriod.DateFrom.ToPeriod());

			if (InputHistory.Get(HistoryItemType.ForecastPeriodTo, 0) == 0)
				InputHistory.Set(HistoryItemType.ForecastPeriodTo, activePeriod.DateFrom.AddMonths(2).ToPeriod());

			if (InputHistory.Get(HistoryItemType.ForecastProbability, 0) == 0)
				InputHistory.Set(HistoryItemType.ForecastProbability, 0);

			if (InputHistory.Get(HistoryItemType.ForecastReferences, 0) == 0)
				InputHistory.Set(HistoryItemType.ForecastReferences, "All");

			this.SetViewDataLists(InputHistory.Get(HistoryItemType.ForecastReferences, 0).ToString());
			//ForecastIndexModel model = new ForecastIndexModel();
			var model = new ForecastIndexModel(InputHistory.Get(HistoryItemType.ForecastRegion, 0), InputHistory.Get(HistoryItemType.ForecastDivision, 0), InputHistory.Get(HistoryItemType.ForecastPeriodFrom, 0), InputHistory.Get(HistoryItemType.ForecastPeriodTo, 0), InputHistory.Get(HistoryItemType.ForecastProbability, 0), DateTime.Today, InputHistory.Get(HistoryItemType.ForecastReferences, "All"), "All");

			return this.View(model);
		}

		[HttpPost]
		public ActionResult Index(FormCollection form)
		{
			try
			{
				int? regionID = form["Region"].ToInt();
				int? divisionID = form["Division"].ToInt();
				var dateFrom = form["PeriodFrom"].ToDate();
				var dateTo = form["PeriodTo"].ToDate();
				double? probability = form["Probability"].Replace(",", "").Replace(" ", "").ToDouble();
				probability = probability ?? 0;
				var snapShotDate = form["SnapShotDate"].ToDate();
				snapShotDate = snapShotDate ?? DateTime.Today;
				int periodFrom = dateFrom.Value.ToPeriod();
				int periodTo = dateTo.Value.ToPeriod();
				string references = form["References"];
				string client = form["Clients"];
				client = client ?? "All";

				InputHistory.Set(HistoryItemType.ForecastRegion, regionID.Value);
				InputHistory.Set(HistoryItemType.ForecastDivision, divisionID.Value);
				InputHistory.Set(HistoryItemType.ForecastPeriodFrom, periodFrom);
				InputHistory.Set(HistoryItemType.ForecastPeriodTo, periodTo);
				InputHistory.Set(HistoryItemType.ForecastProbability, probability.ToString());
				InputHistory.Set(HistoryItemType.ForecastReferences, references);
				InputHistory.Set(HistoryItemType.ForecastClient, client);

				this.SetViewDataLists(references);
				var model = new ForecastIndexModel(regionID.Value, divisionID.Value, periodFrom, periodTo, probability.Value, snapShotDate.Value, references, client);

				return this.View(model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult CreateForecast(DateTime? yearMonth, int? region, int? division, int? billingMethod, int? invoiceCategory, int? probability, string client = "", string project = "", string remark = "")
		{
			//on change of period of billingMethod the createform will be reloaded as it has to retrieve\refresh the workdays
			// and required fields. Keep the values of data already entered instead of rendering a blank form.

			int period = DateTime.Today.ToPeriod();

			if (yearMonth.HasValue)
				period = yearMonth.Value.ToPeriod();

			var forecastHeader = new ForecastHeader
			{
				RegionID = region ?? 0,
				DivisionID = division ?? 0,
				BillingMethod = billingMethod ?? 0,
				InvoiceCategoryID = invoiceCategory ?? 0,
				Client = client,
				Project = project,
				Probability = probability ?? 0,
				Remark = remark
			};

			InputHistory.Set(HistoryItemType.ForecastCreateMonthList, period);
			InputHistory.Set(HistoryItemType.ForecastCreateRegion, forecastHeader.RegionID.ToString());
			InputHistory.Set(HistoryItemType.ForecastCreateDivision, forecastHeader.DivisionID.ToString());
			InputHistory.Set(HistoryItemType.ForecastCreateBillingMethod, forecastHeader.BillingMethod.ToString());
			InputHistory.Set(HistoryItemType.ForecastCreateInvoiceCategory, forecastHeader.InvoiceCategoryID.ToString());
			InputHistory.Set(HistoryItemType.ForecastCreateClient, forecastHeader.Client);
			InputHistory.Set(HistoryItemType.ForecastCreateProject, forecastHeader.Project);
			InputHistory.Set(HistoryItemType.ForecastCreateProbability, forecastHeader.Probability.ToString());
			InputHistory.Set(HistoryItemType.ForecastCreateRemark, forecastHeader.Remark);

			this.SetCreateViewDataLists();
			var model = new ForecastCreateModel(forecastHeader, period);

			return this.View(model);
		}

		[HttpGet]
		public ActionResult EditForecast(int forecastHeaderID, int mostRecentDetailID, DateTime yearMonth)
		{
			var forecastHeader = ForecastHeaderRepo.GetByID(forecastHeaderID);
			var mostRecentDetail = mostRecentDetailID != 0 ? ForecastDetailRepo.GetByID(mostRecentDetailID) : ForecastDetailRepo.GetLastDetailEntryForHeader(forecastHeaderID, yearMonth.ToPeriod());
			mostRecentDetail = mostRecentDetail ?? new ForecastDetail { Period = yearMonth.ToPeriod(), Reference = ForecastReferenceDefaultRepo.GetByDetailEntryDate(DateTime.Today).Reference };

			this.SetViewDataLists(forecastHeader, mostRecentDetail.Period);
			var model = new ForecastEditModel(forecastHeader, mostRecentDetail);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult ShowCopyPeriodPartialView(int forecastDetailID)
		{
			try
			{
				var model = new ForecastCopyModel(forecastDetailID);

				return this.PartialView("ucCopyPeriod", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult ShowEditDetailPartialView(int forecastDetailID)
		{
			try
			{
				var forecastDetail = ForecastDetailRepo.GetByID(forecastDetailID);
				var forecastHeader = ForecastHeaderRepo.GetByID(forecastDetail.ForecastHeaderID);
				this.SetViewDataLists(forecastHeader, forecastDetail.Period);
				var model = new ForecastEditModel(forecastHeader, forecastDetail);

				return this.PartialView("ucEditDetail", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult ShowLinkInvoicesPartialView(int forecastHeaderID, int? period)
		{
			try
			{
				period = period.HasValue && period.Value != 0 ? period.Value : DateTime.Today.AddMonths(-1).ToPeriod();
				var forecastHeader = ForecastHeaderRepo.GetByID(forecastHeaderID);
				this.SetLinkInvoiceViewDataLists(forecastHeaderID, period.Value);
				var model = new ForecastLinkModel(forecastHeader, period.Value);

				return this.PartialView("ucLinkInvoices", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult CopyForecast(FormCollection form)
		{
			int? forecastDetailID = form["ForecastDetailID"].ToInt();
			int? periodFrom = form["PeriodFrom"].ToInt();

			//By default the copy will copy from the last available period for the header.
			//Get the correct detail entry if a different periodfrom has been entered
			var lastForecastPeriodDetail = ForecastDetailRepo.GetByID(forecastDetailID.Value);

			if (periodFrom.HasValue && periodFrom.Value < lastForecastPeriodDetail.Period)
				lastForecastPeriodDetail = ForecastDetailRepo.GetLastDetailEntryForHeader(lastForecastPeriodDetail.ForecastHeaderID, periodFrom.Value);

			int? periodTo = form["PeriodTo"].ToInt();
			periodTo = periodTo ?? lastForecastPeriodDetail.Period;
			int currentPeriod = lastForecastPeriodDetail.Period;

			while (currentPeriod < periodTo)
			{
				var newForecastDetail = new ForecastDetail
				{
					ForecastHeaderID = lastForecastPeriodDetail.ForecastHeaderID,
					Period = currentPeriod.GetMonth() < 12 ? currentPeriod + 01 : ((currentPeriod.GetYear() + 1) * 100) + 1
				};
				int NrOfWorkableDays = WorkDayRepo.GetNumberOfWorkableDays(true, new DateTime(newForecastDetail.Period.GetYear(), newForecastDetail.Period.GetMonth(), 01).ToFirstDayOfMonth(), new DateTime(newForecastDetail.Period.GetYear(), newForecastDetail.Period.GetMonth(), 01).ToLastDayOfMonth());
				newForecastDetail.EntryDate = DateTime.Today;
				var forecastHeader = ForecastHeaderRepo.GetByID(lastForecastPeriodDetail.ForecastHeaderID);
				newForecastDetail.Amount = (BillingMethodType)forecastHeader.BillingMethod == BillingMethodType.TimeMaterial ? lastForecastPeriodDetail.HourlyRate * lastForecastPeriodDetail.AllocationPercentage * NrOfWorkableDays * 7 : lastForecastPeriodDetail.Amount;

				//calculate the adjustment if the existing periods will be overriden by the copy
				var newPeriodLastForecastDetail = ForecastDetailRepo.GetLastDetailEntryForHeader(lastForecastPeriodDetail.ForecastHeaderID, newForecastDetail.Period);
				newForecastDetail.Adjustment = newPeriodLastForecastDetail == null ? newForecastDetail.Amount : newForecastDetail.Amount - newPeriodLastForecastDetail.Amount;

				newForecastDetail.HourlyRate = lastForecastPeriodDetail.HourlyRate;
				newForecastDetail.AllocationPercentage = lastForecastPeriodDetail.AllocationPercentage;
				newForecastDetail.Remark = "Copied from " + lastForecastPeriodDetail.Period.ToString();
				newForecastDetail.Reference = lastForecastPeriodDetail.Reference;
				newForecastDetail.ModifiedByUserID = this.CurrentUserID;
				ForecastDetailRepo.Save(newForecastDetail);

				if (currentPeriod.GetMonth() < 12)
					currentPeriod++;
				else
					currentPeriod = ((currentPeriod.GetYear() + 01) * 100) + 01;
			}

			InputHistory.Set(HistoryItemType.ForecastPeriodTo, periodTo.Value);
			this.SetViewDataLists(InputHistory.Get(HistoryItemType.ForecastReferences, 0).ToString());
			var model = new ForecastIndexModel(InputHistory.Get(HistoryItemType.ForecastRegion, 0), InputHistory.Get(HistoryItemType.ForecastDivision, 0), InputHistory.Get(HistoryItemType.ForecastPeriodFrom, 0), InputHistory.Get(HistoryItemType.ForecastPeriodTo, 0), InputHistory.Get(HistoryItemType.ForecastProbability, 0), DateTime.Today, InputHistory.Get(HistoryItemType.ForecastReferences, "All"), InputHistory.Get(HistoryItemType.ForecastClient, "All"));

			return this.PartialView("ucIndex", model);
		}

		[HttpPost]
		public ActionResult LinkInvoices(FormCollection form, string selectedInvoices)
		{
			int? period = form["MonthList"].ToInt();
			int? forecastHeaderID = form["ForecastHeaderID"].ToInt();

			var lastForecastDetail = new ForecastDetail();
			lastForecastDetail = ForecastDetailRepo.GetLastDetailEntryForHeader(forecastHeaderID.Value, period.Value);

			foreach (string invoice in selectedInvoices.Split(','))
			{
				var forecastInvoiceLink = new ForecastInvoiceLink
				{
					ForecastHeaderID = lastForecastDetail.ForecastHeaderID,
					ForecastDetailID = lastForecastDetail.ForecastDetailID,
					InvoiceID = invoice.ToInt().Value
				};
				ForecastInvoiceLinkRepo.Save(forecastInvoiceLink);
			}

			this.SetViewDataLists(InputHistory.Get(HistoryItemType.ForecastReferences, 0).ToString());
			var model = new ForecastIndexModel(InputHistory.Get(HistoryItemType.ForecastRegion, 0), InputHistory.Get(HistoryItemType.ForecastDivision, 0), InputHistory.Get(HistoryItemType.ForecastPeriodFrom, 0), InputHistory.Get(HistoryItemType.ForecastPeriodTo, 0), InputHistory.Get(HistoryItemType.ForecastProbability, 0), DateTime.Today, InputHistory.Get(HistoryItemType.ForecastReferences, "All"), InputHistory.Get(HistoryItemType.ForecastClient, "All"));

			return this.PartialView("ucIndex", model);
		}

		[HttpPost]
		public ActionResult SaveForeCastHeader(int forecastHeaderID, FormCollection form)
		{
			try
			{
				var period = form["MonthList"].ToDate();
				int? regionID = form["Region"].ToInt();
				string regionName = regionID.Value != 0 ? RegionRepo.GetByID(regionID.Value).RegionName : "";
				int? divisionID = form["Division"].ToInt();
				string divisionName = divisionID.Value != 0 ? DivisionRepo.GetByID(divisionID.Value).DivisionName : "";
				int? billingMethodID = form["BillingMethod"].ToInt();
				string billingMethodName = ((BillingMethodType)billingMethodID.Value).ToString();
				int? invoiceCategoryID = form["InvoiceCategory"].ToInt();
				string invoiceCategoryName = ((InvoiceCategoryType)invoiceCategoryID.Value).ToString();//Annuity Type
				string clientName = form["Client"];
				string projectName = form["Project"];
				string headerResource = form["HeaderResource"] ?? "";
				double? probability = form["Probability"].Replace(",", "").Replace(" ", "").ToDouble();
				string remark = form["Remark"];

				//---------------------------------------------------------------------
				//Update the header if it already exist, else create new header
				//----------------------------------------------------------------------
				var existingForecastHeader = forecastHeaderID == 0 ? ForecastHeaderRepo.GetHeaderByUniqueKey(regionID.Value, divisionID.Value, billingMethodID.Value, invoiceCategoryID.Value, clientName, projectName, headerResource) : ForecastHeaderRepo.GetByID(forecastHeaderID);

				var forecastHeader = new ForecastHeader();
				forecastHeader = existingForecastHeader ?? forecastHeader;

				//insert or update forecastHeader
				forecastHeader.RegionID = regionID;
				forecastHeader.Region = regionName;
				forecastHeader.DivisionID = divisionID;
				forecastHeader.Division = divisionName;
				forecastHeader.BillingMethod = billingMethodID.Value;
				forecastHeader.InvoiceCategoryID = invoiceCategoryID;
				forecastHeader.InvoiceCategory = invoiceCategoryName;
				forecastHeader.Client = clientName;
				forecastHeader.ClientID = ClientRepo.GetByName(clientName)?.ClientID ?? 0;
				forecastHeader.Project = projectName;
				forecastHeader.ProjectID = ProjectRepo.GetByClientIDProjectName(forecastHeader.ClientID.Value, projectName)?.ProjectID ?? 0;
				forecastHeader.Resource = headerResource;
				forecastHeader.UserID = UserRepo.GetByUserName(headerResource.ToLower().Replace(' ', '.'))?.UserID ?? 0;
				forecastHeader.Remark = remark;
				forecastHeader.Probability = probability ?? 0;
				ForecastHeaderRepo.Save(forecastHeader);

				//Save the details  ONLY If new forecast header
				string action = form["Action"];

				if (action == "Create")
				{
					double? forecastHourlyRate = form["ForecastHourlyRate"].Replace(",", "").Replace(" ", "").ToDouble();
					double? forecastAllocationPercentage = form["ForecastAllocationPercentage"].Replace(",", "").Replace(" ", "").ToDouble();
					double? forecastAmount = (BillingMethodType)forecastHeader.BillingMethod == BillingMethodType.TimeMaterial ? form["TMForecastAmount"].ToDouble() : form["NonTMForecastAmount"].ToDouble();
					string reasonForChange = form["ReasonForChange"];
					string resources = form["Resources"];
					string reference = form["Reference"];
					int? forecastWorkableDays = form["ForecastWorkableDays"].Replace(",", "").Replace(" ", "").ToInt();
					forecastWorkableDays = forecastWorkableDays ?? WorkDayRepo.GetNumberOfWorkableDays(true, period.Value.ToFirstDayOfMonth(), period.Value.ToLastDayOfMonth());

					//calculate some of totals to determine where a new detail line is required or whether to update the last detail
					double hourlyRate = forecastHourlyRate ?? 0;
					double allocationPercentage = forecastAllocationPercentage ?? 0;
					double amount = forecastHeader.BillingMethod == (int)BillingMethodType.TimeMaterial ? hourlyRate * allocationPercentage * forecastWorkableDays.Value * 7 : forecastAmount.Value;
					amount = amount != forecastAmount.Value ? forecastAmount.Value : amount;
					double adjustment = amount;

					var forecastDetail = new ForecastDetail
					{
						Period = period.Value.ToPeriod(),
						ForecastHeaderID = forecastHeader.ForecastHeaderID,
						EntryDate = DateTime.Now,
						HourlyRate = hourlyRate,
						AllocationPercentage = allocationPercentage,
						Amount = amount
					};

					if (adjustment != 0)
						forecastDetail.Adjustment = adjustment;

					forecastDetail.Remark = reasonForChange;
					forecastDetail.Reference = reference;
					forecastDetail.ModifiedByUserID = this.CurrentUserID;
					ForecastDetailRepo.Save(forecastDetail);

					//Create Resource assignments
					string[] resourceList = resources.Split(',');

					if (resourceList.Length > 0)
					{
						foreach (string resource in resourceList)
						{
							var currentResource = ForecastResourceAssignmentRepo.GetByDetailIDResource(forecastDetail.ForecastDetailID, resource);

							if (currentResource != null)
								ForecastResourceAssignmentRepo.Delete(currentResource);

							var forecastResourcesAssignment = new ForecastResourceAssignment
							{
								ForecastDetailID = forecastDetail.ForecastDetailID,
								Resource = resource.Replace("\r\n", "")
							};
							ForecastResourceAssignmentRepo.Save(forecastResourcesAssignment);
						}
					}
				}

				this.SetViewDataLists(InputHistory.Get(HistoryItemType.ForecastReferences, 0).ToString());
				var model = new ForecastIndexModel();

				return this.PartialView("ucIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult SaveForeCastDetail(FormCollection form)
		{
			try
			{
				int? forecastDetailID = form["ForecastDetailID"].ToInt();
				int? period = form["DetailPeriod"].Replace(",", "").Replace(" ", "").ToInt();
				double? forecastHourlyRate = form["ForecastHourlyRate"].Replace(",", "").Replace(" ", "").ToDouble();
				double? forecastAllocationPercentage = form["ForecastAllocationPercentage"].Replace(",", "").Replace(" ", "").ToDouble();
				var forecastDetail = ForecastDetailRepo.GetByID(forecastDetailID.Value);
				var forecastHeader = ForecastHeaderRepo.GetByID(forecastDetail.ForecastHeaderID);
				double? forecastAmount = (BillingMethodType)forecastHeader.BillingMethod == BillingMethodType.TimeMaterial ? form["TMForecastAmount"].Replace(",", "").Replace(" ", "").ToDouble() : form["NonTMForecastAmount"].Replace(",", "").Replace(" ", "").ToDouble();

				string reasonForChange = form["ReasonForChange"];
				string resources = form["Resources"];
				string reference = form["Reference"];
				int? forecastWorkableDays = form["ForecastWorkableDays"].ToInt();
				int? periodYear = period.Value.ToString().Substring(0, 4).ToInt();
				int? periodMonth = period.Value.ToString().Substring(4, 2).ToInt();
				var periodDate = new DateTime(periodYear.Value, periodMonth.Value, 1);
				forecastWorkableDays = forecastWorkableDays ?? WorkDayRepo.GetNumberOfWorkableDays(true, periodDate.ToFirstDayOfMonth(), periodDate.ToLastDayOfMonth());

				//Get the last detail entry for the header in order to cal adjustment amount
				var lastForecastDetail = new ForecastDetail();
				lastForecastDetail = forecastDetail;

				//calculate some of totals to determine where a new detail line is required or whether to update the last detail
				double hourlyRate = forecastHourlyRate ?? 0;
				double allocationPercentage = forecastAllocationPercentage ?? 0;
				double amount = forecastHeader.BillingMethod == (int)BillingMethodType.TimeMaterial ? hourlyRate * allocationPercentage * forecastWorkableDays.Value * 7 : forecastAmount.Value;
				amount = amount != forecastAmount.Value ? forecastAmount.Value : amount;
				double adjustment = lastForecastDetail == null ? amount : amount - lastForecastDetail.Amount;

				//Create detail entry using prev forecast amount to calculate adjustment if applicable
				forecastDetail = lastForecastDetail == null || adjustment != 0 ? new ForecastDetail() : lastForecastDetail;
				forecastDetail.Period = period.Value;
				forecastDetail.ForecastHeaderID = forecastHeader.ForecastHeaderID;
				forecastDetail.EntryDate = DateTime.Now;
				forecastDetail.HourlyRate = hourlyRate;
				forecastDetail.AllocationPercentage = allocationPercentage;
				forecastDetail.Amount = amount;

				if (adjustment != 0)
					forecastDetail.Adjustment = adjustment;

				forecastDetail.Remark = reasonForChange;
				forecastDetail.Reference = reference;
				forecastDetail.ModifiedByUserID = this.CurrentUserID;
				ForecastDetailRepo.Save(forecastDetail);

				//Create Resource assignments
				string[] resourceList = resources.Split(',');

				if (resourceList.Length > 0)
				{
					foreach (string resource in resourceList)
					{
						var currentResource = ForecastResourceAssignmentRepo.GetByDetailIDResource(forecastDetail.ForecastDetailID, resource);

						if (currentResource != null)
							ForecastResourceAssignmentRepo.Delete(currentResource);

						var forecastResourcesAssignment = new ForecastResourceAssignment
						{
							ForecastDetailID = forecastDetail.ForecastDetailID,
							//forecastResourcesAssignment.PercentageAllocation = ;
							Resource = resource.Replace("\r\n", "")
						};
						ForecastResourceAssignmentRepo.Save(forecastResourcesAssignment);
					}
				}

				this.SetViewDataLists(forecastHeader, forecastDetail.Period);
				var model = new ForecastEditModel(forecastHeader, forecastDetail);

				return this.PartialView("ucEditForecast", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult SelectReferences(string references)
		{
			try
			{
				references = references ?? "All";
				this.SetViewDataLists(references);

				return this.PartialView("ucSelectReferences");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult SetHistoryItemReferences(string references)
		{
			try
			{
				InputHistory.Set(HistoryItemType.ForecastReferences, references);
				this.SetViewDataLists(references);
				var model = new ForecastIndexModel(InputHistory.Get(HistoryItemType.ForecastRegion, 0), InputHistory.Get(HistoryItemType.ForecastDivision, 0), InputHistory.Get(HistoryItemType.ForecastPeriodFrom, 0), InputHistory.Get(HistoryItemType.ForecastPeriodTo, 0), InputHistory.Get(HistoryItemType.ForecastProbability, 0), DateTime.Today, InputHistory.Get(HistoryItemType.ForecastReferences, "All"), InputHistory.Get(HistoryItemType.ForecastClient, "All"));

				return this.PartialView("ucIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult SelectClients(string clients)
		{
			try
			{
				clients = clients ?? "All";
				this.SetViewDataLists(clients);

				return this.PartialView("ucSelectClients");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult SetHistoryItemClients(string clients)
		{
			try
			{
				InputHistory.Set(HistoryItemType.ForecastClient, clients);
				this.SetViewDataLists(clients);
				var model = new ForecastIndexModel(InputHistory.Get(HistoryItemType.ForecastRegion, 0), InputHistory.Get(HistoryItemType.ForecastDivision, 0), InputHistory.Get(HistoryItemType.ForecastPeriodFrom, 0), InputHistory.Get(HistoryItemType.ForecastPeriodTo, 0), InputHistory.Get(HistoryItemType.ForecastProbability, 0), DateTime.Today, InputHistory.Get(HistoryItemType.ForecastReferences, "All"), InputHistory.Get(HistoryItemType.ForecastClient, "All"));

				return this.PartialView("ucIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult ShowViewDefaultReferencesPartialView()
		{
			try
			{
				var model = new ForecastDefaultReferenceModel();

				return this.PartialView("ucViewReferenceDefaults", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult ShowCreateDefaultReferencePartialView()
		{
			try
			{
				return this.PartialView("ucCreateReferenceDefault");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult CreateDefaultReference(FormCollection form)
		{
			string defaultReference = form["DefaultReference"];
			var effectiveDate = form["EffectiveDate"].ToDate();
			var forecastReferenceDefault = new ForecastReferenceDefault
			{
				Reference = defaultReference,
				EffectiveDate = effectiveDate ?? DateTime.Today,
				ModifiedByUserID = CurrentUserID
			};
			ForecastReferenceDefaultRepo.Save(forecastReferenceDefault);

			this.SetViewDataLists("All");
			var model = new ForecastIndexModel(InputHistory.Get(HistoryItemType.ForecastRegion, 0), InputHistory.Get(HistoryItemType.ForecastDivision, 0), InputHistory.Get(HistoryItemType.ForecastPeriodFrom, 0), InputHistory.Get(HistoryItemType.ForecastPeriodTo, 0), InputHistory.Get(HistoryItemType.ForecastProbability, 0), DateTime.Today, InputHistory.Get(HistoryItemType.ForecastReferences, "All"), InputHistory.Get(HistoryItemType.ForecastClient, "All"));

			return this.PartialView("ucIndex", model);
		}

		#region FORECAST SETVIEWDATA

		private void SetViewDataLists(string references)
		{
			var regionItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All Regions" }
			};
			RegionRepo.GetAll().OrderBy(r => r.RegionName).ToList().ForEach(r => regionItems.Add(new SelectListItem { Value = r.RegionID.ToString(), Text = r.RegionName, Selected = r.RegionID == InputHistory.Get(HistoryItemType.ForecastRegion, 0) }));
			this.ViewData["Region"] = regionItems;

			var divisionItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All Divisions" }
			};
			DivisionRepo.GetAll().OrderBy(d => d.DivisionName).ToList().ForEach(d => divisionItems.Add(new SelectListItem { Value = d.DivisionID.ToString(), Text = d.DivisionName, Selected = d.DivisionID == InputHistory.Get(HistoryItemType.ForecastDivision, 0) }));
			this.ViewData["Division"] = divisionItems;

			this.ViewData["BillingMethod"] = BillingMethodRepo.GetAll().OrderBy(b => b.BillingMethodName).Select(b => new SelectListItem { Value = b.BillingMethodID.ToString(), Text = b.BillingMethodName });
			this.ViewData["InvoiceCategory"] = InvoiceCategoryRepo.GetAll().OrderBy(c => c.CategoryName).Select(c => new SelectListItem { Value = c.InvoiceCategoryID.ToString(), Text = c.CategoryName });

			this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(12)).OrderBy(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.ToPeriod().ToString() == InputHistory.Get(HistoryItemType.ForecastPeriodFrom, 0).ToString() });
			this.ViewData["PeriodFrom"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(12)).OrderBy(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.ToPeriod().ToString() == InputHistory.Get(HistoryItemType.ForecastPeriodFrom, 0).ToString() });
			this.ViewData["PeriodTo"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(12)).OrderBy(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.ToPeriod().ToString() == InputHistory.Get(HistoryItemType.ForecastPeriodTo, 0).ToString() });

			var referenceList = new List<string>();
			references = references ?? "All";
			referenceList.Add("All");
			referenceList.AddRange(references.Split(','));
			this.ViewData["References"] = ForecastDetailRepo.GetDistinctReferences().OrderBy(r => r).Select(r => new SelectListItem { Value = r, Text = r, Selected = referenceList.Contains(r) });

			var clientItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "All", Text = "All Clients" }
			};
			string clients = InputHistory.Get(HistoryItemType.ForecastClient, "All");
			ForecastHeaderRepo.GetDistinctClients().OrderBy(c => c).ToList().ForEach(c => clientItems.Add(new SelectListItem { Value = c, Text = c, Selected = clients.Contains(c) }));
			this.ViewData["Clients"] = clientItems.ToArray();
		}

		private void SetViewDataLists(ForecastHeader forecastHeader, int period)
		{
			var regionItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All Regions" }
			};
			RegionRepo.GetAll().OrderBy(r => r.RegionName).ToList().ForEach(r => regionItems.Add(new SelectListItem { Value = r.RegionID.ToString(), Text = r.RegionName, Selected = r.RegionID == forecastHeader.RegionID }));
			this.ViewData["Region"] = regionItems;

			var divisionItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All Divisions" }
			};
			DivisionRepo.GetAll().OrderBy(d => d.DivisionName).ToList().ForEach(d => divisionItems.Add(new SelectListItem { Value = d.DivisionID.ToString(), Text = d.DivisionName, Selected = d.DivisionID == forecastHeader.DivisionID }));
			this.ViewData["Division"] = divisionItems;

			this.ViewData["BillingMethod"] = BillingMethodRepo.GetAll().OrderBy(b => b.BillingMethodName).Select(b => new SelectListItem { Value = b.BillingMethodID.ToString(), Text = b.BillingMethodName, Selected = b.BillingMethodID == forecastHeader.BillingMethod });
			this.ViewData["InvoiceCategory"] = InvoiceCategoryRepo.GetAll().OrderBy(c => c.CategoryName).Select(c => new SelectListItem { Value = c.InvoiceCategoryID.ToString(), Text = c.CategoryName, Selected = c.InvoiceCategoryID == forecastHeader.InvoiceCategoryID });

			this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(12)).OrderBy(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.ToPeriod().ToString() == period.ToString() });
			this.ViewData["References"] = ForecastDetailRepo.GetDistinctReferences().OrderBy(r => r).Select(r => new SelectListItem { Value = r, Text = r });

			var clientItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "All", Text = "All Clients" }
			};
			string clients = InputHistory.Get(HistoryItemType.ForecastClient, "All");
			ForecastHeaderRepo.GetDistinctClients().OrderBy(c => c).ToList().ForEach(c => clientItems.Add(new SelectListItem { Value = c, Text = c, Selected = clients.Contains(c) }));
			this.ViewData["Clients"] = clientItems.ToArray();
		}

		private void SetCreateViewDataLists()
		{
			var regionItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All Regions" }
			};
			RegionRepo.GetAll().OrderBy(r => r.RegionName).ToList().ForEach(r => regionItems.Add(new SelectListItem { Value = r.RegionID.ToString(), Text = r.RegionName, Selected = r.RegionID == InputHistory.Get(HistoryItemType.ForecastCreateRegion, 0) }));
			this.ViewData["Region"] = regionItems;

			var divisionItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All Divisions" }
			};
			DivisionRepo.GetAll().OrderBy(d => d.DivisionName).ToList().ForEach(d => divisionItems.Add(new SelectListItem { Value = d.DivisionID.ToString(), Text = d.DivisionName, Selected = d.DivisionID == InputHistory.Get(HistoryItemType.ForecastCreateDivision, 0) }));
			this.ViewData["Division"] = divisionItems;

			this.ViewData["BillingMethod"] = BillingMethodRepo.GetAll().OrderBy(b => b.BillingMethodName).Select(b => new SelectListItem { Value = b.BillingMethodID.ToString(), Text = b.BillingMethodName, Selected = b.BillingMethodID == InputHistory.Get(HistoryItemType.ForecastCreateBillingMethod, 0) });
			this.ViewData["InvoiceCategory"] = InvoiceCategoryRepo.GetAll().OrderBy(c => c.CategoryName).Select(c => new SelectListItem { Value = c.InvoiceCategoryID.ToString(), Text = c.CategoryName, Selected = c.InvoiceCategoryID == InputHistory.Get(HistoryItemType.ForecastCreateInvoiceCategory, 0) });
			this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(12)).OrderBy(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.ToPeriod().ToString() == InputHistory.Get(HistoryItemType.ForecastCreateMonthList, 0).ToString() });
		}

		private void SetLinkInvoiceViewDataLists(int forecastHeaderID, int period) => this.ViewData["MonthList"] = ForecastHeaderRepo.GetDistinctPeriodsByHeaderID(forecastHeaderID).OrderBy(p => p).Select(p => new SelectListItem { Value = p.ToString(), Text = p.GetYear().ToString() + " " + p.GetMonth().ToMonthName(), Selected = p == period });

		#endregion FORECAST SETVIEWDATA

		#region FORECAST REPORT

		[HttpGet]
		public ActionResult ForecastReportIndex()
		{
			InputHistory.Set(HistoryItemType.ForecastReportPeriod, DateTime.Today.ToPeriod());
			InputHistory.Set(HistoryItemType.ForecastReportRegion, 0);
			InputHistory.Set(HistoryItemType.ForecastReportDivision, 0);

			this.SetForecastReportViewData();
			var model = new ForecastReportModel(DateTime.Today.ToPeriod(), 0, 0);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult ForecastReportIndex(FormCollection form)
		{
			try
			{
				var monthYear = form["ForecastReportPeriod"].ToDate();
				int period = monthYear.Value.ToPeriod();
				InputHistory.Set(HistoryItemType.ForecastReportPeriod, period);

				int? regionID = form["Region"].ToInt();
				regionID = regionID ?? 0;
				InputHistory.Set(HistoryItemType.ForecastReportRegion, regionID.Value);

				int? divisionID = form["Division"].ToInt();
				divisionID = divisionID ?? 0;
				InputHistory.Set(HistoryItemType.ForecastReportDivision, divisionID.Value);

				this.SetForecastReportViewData();
				var model = new ForecastReportModel(period, regionID.Value, divisionID.Value);

				return this.View(model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		private void SetForecastReportViewData()
		{
			var regionItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All Regions" }
			};
			RegionRepo.GetAll().OrderBy(r => r.RegionName).ToList().ForEach(r => regionItems.Add(new SelectListItem { Value = r.RegionID.ToString(), Text = r.RegionName, Selected = r.RegionID == InputHistory.Get(HistoryItemType.ForecastReportRegion, 0) }));
			this.ViewData["Region"] = regionItems;

			var divisionItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All Divisions" }
			};
			DivisionRepo.GetAll().OrderBy(d => d.DivisionName).ToList().ForEach(d => divisionItems.Add(new SelectListItem { Value = d.DivisionID.ToString(), Text = d.DivisionName, Selected = d.DivisionID == InputHistory.Get(HistoryItemType.ForecastReportDivision, 0) }));
			this.ViewData["Division"] = divisionItems;

			this.ViewData["ForecastReportPeriod"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(12)).OrderBy(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.ToPeriod().ToString() == InputHistory.Get(HistoryItemType.ForecastReportPeriod, 0).ToString() });
		}

		#endregion FORECAST REPORT

		#region JSON LOOKUPS

		public ActionResult Lookup(string term)
		{
			var list = ClientRepo.AutoComplete(term, 20);

			return this.Json(list, JsonRequestBehavior.AllowGet);
		}

		public ActionResult LookupProject(string client, string term)
		{
			var list = ProjectRepo.AutoComplete(client, term, 20);

			return this.Json(list, JsonRequestBehavior.AllowGet);
		}

		public ActionResult GetNumberOfWorkDays(int period)
		{
			var nrOfWorkDays = WorkDayRepo.GetNumberOfWorkDays(new DateTime(period.GetYear(), period.GetMonth(), 01).ToFirstDayOfMonth(), new DateTime(period.GetYear(), period.GetMonth(), 01).ToLastDayOfMonth());

			return this.Json(nrOfWorkDays, JsonRequestBehavior.AllowGet);
		}

		public ActionResult LookupUser(string term)
		{
			var list = UserRepo.AutoCompleteUser(term, 20);

			return this.Json(list, JsonRequestBehavior.AllowGet);
		}

		#endregion JSON LOOKUPS
	}
}