


CREATE VIEW [dbo].[vwForecastWithInvoicesSummaries] AS
SELECT DISTINCT W.BillingMethod,
			    W.Client,			   
			    W.Region,
			    W.Division,
			    W.RegionID,
			    W.DivisionID,
			    F.Period,           
                Max(F.ForecastAmount) as ForecastAmount, 
                Sum(W.ProjectedAmount) as ProjectedAmount,       
                Sum(W.ActualAmount) as ActualAmount       
 FROM  [dbo].[vwForecastWithInvoices] W inner join
       [dbo].[vwForecastAmountByClientBillingMethodTotals] F on 
       W.Client = F.Client  and W.BillingMethod = F.BillingMethod and W.Period = F.Period
GROUP BY W.BillingMethod,
	     W.Client,			   
		 W.Region,
		 W.Division,
		 W.RegionID,
		 W.DivisionID,
		 F.Period
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwForecastWithInvoicesSummaries] TO [EnabillReportRole]
    AS [dbo];

