
CREATE VIEW [dbo].[vwForecastWithInvoices] AS

SELECT DISTINCT        
			    H.BillingMethod,
			    H.Client,			    
			    H.Region,
			    H.Division,			  
			    H.RegionID,
			    H.DivisionID,
                I.Period,  
                I.InvoiceID,
                I.OrderNo,        
                Sum(D.Amount) as ForecastAmount, 
                max(I.ProvisionalAccrualAmount) as ProjectedAmount,       
                max(I.AccrualExclVAT) as ActualAmount      
 FROM  [dbo].[ForecastHeaders] H inner join
       [dbo].[vwForecastHeaderMostRecentDetailLines] D on 
        H.ForecastHeaderID = D.ForecastHeaderID inner join 
       [dbo].ForecastInvoiceLinks L on D.ForecastDetailID = L.ForecastDetailID inner join
       [dbo].Invoices I on L.InvoiceID = I.InvoiceID
 WHERE InvoiceAmountExclVAT <> 0
 GROUP BY H.BillingMethod,
			    H.Client,			    
			    H.Region,
			    H.Division,			  
			    H.RegionID,
			    H.DivisionID,
                I.Period,  
                I.InvoiceID,
                I.OrderNo
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwForecastWithInvoices] TO [EnabillReportRole]
    AS [dbo];

