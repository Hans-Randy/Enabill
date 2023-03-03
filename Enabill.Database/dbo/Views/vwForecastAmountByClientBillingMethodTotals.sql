CREATE VIEW [dbo].[vwForecastAmountByClientBillingMethodTotals]
AS
SELECT H.Client,
       H.BillingMethod,
       D.[Period],
       Sum(D.Amount) as ForecastAmount
  FROM [dbo].[vwForecastHeaderMostRecentDetailLines] D inner join
  dbo.ForecastHeaders H on H.ForecastHeaderID = D.ForecastHeaderID
  GROUP BY H.Client, H.BillingMethod, D.Period
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwForecastAmountByClientBillingMethodTotals] TO [EnabillReportRole]
    AS [dbo];

