
CREATE VIEW [dbo].[vwForecastHeaderMostRecentDetailLines] 
AS 
SELECT * FROM ForeCastDetails 
WHERE  ForecastDetailID  in
(
SELECT MAX(ForecastDetailID) AS ForecastDetailID
FROM ForeCastDetails 
GROUP BY ForecastHeaderID, Period
)
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwForecastHeaderMostRecentDetailLines] TO [EnabillReportRole]
    AS [dbo];

