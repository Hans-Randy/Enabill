

CREATE VIEW [dbo].[vwForecastWithOutInvoices] AS
SELECT DISTINCT H.ForecastHeaderID,           
			    H.BillingMethod,
			    H.Client,
			    H.Project,           
			    H.Resource,
			    H.Region,
			    H.Division,
			    H.InvoiceCategory,
			    H.RegionID,
			    H.DivisionID,
			    H.InvoiceCategoryID,
			    H.Remark,
			    H.Probability,  
			    D.ForecastDetailID, 
                D.Period,           
                Max(D.Amount) as ForecastAmount
 FROM  [dbo].[ForecastHeaders] H inner join
       [dbo].[vwForecastHeaderMostRecentDetailLines] D on 
        H.ForecastHeaderID = D.ForecastHeaderID 
 WHERE D.ForecastDetailID NOT IN
 (SELECT ForecastDetailID from dbo.vwForecastWithInvoicesDetails)
 GROUP BY H.ForecastHeaderID,           
          H.BillingMethod,
          H.Client,
          H.Project,        
          H.Resource,
          H.Region,
          H.Division,
          H.InvoiceCategory,
          H.RegionID,
          H.DivisionID,
          H.InvoiceCategoryID,
          H.Remark,
          H.Probability,
          D.ForecastDetailID, 
          D.Period
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwForecastWithOutInvoices] TO [EnabillReportRole]
    AS [dbo];

