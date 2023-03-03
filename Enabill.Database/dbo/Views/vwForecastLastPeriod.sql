CREATE VIEW [dbo].[vwForecastLastPeriod] AS
SELECT ForecastHeaderID, MAX(Period) AS Period
FROM ForeCastDetails 
GROUP BY ForecastHeaderID
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwForecastLastPeriod] TO [EnabillReportRole]
    AS [dbo];

