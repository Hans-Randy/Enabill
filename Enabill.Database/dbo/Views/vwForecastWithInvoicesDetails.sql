CREATE VIEW [dbo].[vwForecastWithInvoicesDetails] AS
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
                Max(D.Amount) as ForecastAmount, 
                Sum(I.ProvisionalAccrualAmount) as ProjectedAmount,       
                Sum(I.AccrualExclVAT) as ActualAmount                      
 FROM  [dbo].[ForecastHeaders] H inner join
       [dbo].[vwForecastHeaderMostRecentDetailLines] D on 
        H.ForecastHeaderID = D.ForecastHeaderID inner join 
       [dbo].ForecastInvoiceLinks L on D.ForecastDetailID = L.ForecastDetailID inner join
       [dbo].Invoices I on L.InvoiceID = I.InvoiceID
 WHERE InvoiceAmountExclVAT <> 0
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
    ON OBJECT::[dbo].[vwForecastWithInvoicesDetails] TO [EnabillReportRole]
    AS [dbo];

