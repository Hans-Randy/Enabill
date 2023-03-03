CREATE VIEW vwForecastHeaderMostRecentResourceAssignments AS
SELECT * FROM ForeCastResourceAssignments
WHERE  ForecastDetailID  in
(
SELECT MAX(ForecastDetailID) AS ForecastDetailID
FROM ForeCastDetails 
GROUP BY ForecastHeaderID, Period
)
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwForecastHeaderMostRecentResourceAssignments] TO [EnabillReportRole]
    AS [dbo];

