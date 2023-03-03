
CREATE VIEW dbo.vwForecastHeaderLastPeriodDetails AS
SELECT D.* FROM ForeCastDetails D INNER JOIN
dbo.vwForecastLastPeriod P on D.ForecastHeaderID = P.ForecastHeaderID and
D.Period = P.Period 
WHERE  ForecastDetailID  in
(
SELECT MAX(ForecastDetailID) AS ForecastDetailID
FROM ForeCastDetails 
GROUP BY ForecastHeaderID, Period
)
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwForecastHeaderLastPeriodDetails] TO [EnabillReportRole]
    AS [dbo];

