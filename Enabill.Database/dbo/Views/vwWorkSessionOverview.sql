--select * from vwWorkSessionOverview where UserID = 49 ORDER BY DayWorked
CREATE view [dbo].[vwWorkSessionOverview]
as
/*---------------------------------
Function : Work Session Overview
Created by : Henk
----------------------------------------*/

SELECT WS.UserID, WS.DayWorked, WS.Duration, WA.HoursWorked, HoursDiff = WA.HoursWorked - WS.Duration, IsWorkable = ISNULL(WD.IsWorkable, 0)
FROM (
		SELECT WS.UserID,
			   DayWorked = CONVERT(datetime, CONVERT(varchar, YEAR(WS.StartTime)) + '-' + CONVERT(varchar, MONTH(WS.StartTime)) + '-' + CONVERT(varchar, DAY(WS.StartTime))), 
			   Duration = SUM(CONVERT(float, DATEDIFF(MI, StartTime, EndTime)) / 60.0 - LunchTime)
		FROM WorkSessions WS
		
		GROUP BY WS.UserID, 
				 CONVERT(datetime, CONVERT(varchar, YEAR(WS.StartTime)) + '-' + CONVERT(varchar, MONTH(WS.StartTime)) + '-' + CONVERT(varchar, DAY(WS.StartTime)))
	) WS
JOIN (
		SELECT WA.UserID, 
			   WA.DayWorked, 
			   HoursWorked = SUM(WA.HoursWorked)
		FROM WorkAllocations WA
		GROUP BY WA.UserID, WA.DayWorked
	 ) WA ON WS.UserID = WA.UserID AND WS.DayWorked = WA.DayWorked
LEFT JOIN WorkDays WD ON WS.DayWorked = WD.WorkDate
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwWorkSessionOverview] TO PUBLIC
    AS [dbo];

