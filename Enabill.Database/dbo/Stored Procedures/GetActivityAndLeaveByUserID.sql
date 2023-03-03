

-- Will return Activities and Leave per employee

CREATE PROCEDURE [dbo].[GetActivityAndLeaveByUserID]
	 @DateTimeFrom DATETIME,
	 @DateTimeTo DATETIME,
	 @UserID INT
AS

DECLARE
	 @UserName varchar(50),
	 @FullName varchar(50),
	 @DateFrom DATE,
	 @DateTo DATE

SET @UserName = (SELECT UserName FROM dbo.Users WHERE UserID = @UserID)
SET @FullName = (SELECT FullName FROM dbo.Users WHERE UserID = @UserID)
SET @DateFrom = CAST(@DateTimeFrom AS DATE);
SET @DateTo = CAST(@DateTimeTo AS DATE);

IF(OBJECT_ID('tempdb..#LeaveTemp') IS NOT NULL)
BEGIN
	 DROP TABLE #LeaveTemp
END

CREATE TABLE #LeaveTemp
(
	 WorkDate DATE
)

INSERT INTO #LeaveTemp
SELECT * FROM dbo.F_GetLeaveDatesBetweenRangesByUserID (@DateFrom, @DateTo, @UserID)

SELECT 
	 u.UserName [User Name],
	 u.FullName [Full Name],
	 CAST(ws.StartTime AS DATE) [Date],
	 wa.[Period],
	 c.ClientName Client,
	 p.ProjectName Project,
	 a.ActivityName Activity,
	 u.WorkHours [Required],
	 wa.HoursWorked [Hours],
	 wa.Remark	  
FROM dbo.WorkSessions ws
INNER JOIN dbo.Users u ON u.UserID = ws.UserID 
INNER JOIN dbo.workallocations wa ON (ws.UserID = wa.UserID)
	 AND (CAST(ws.StartTime AS DATE) = CAST(wa.DayWorked AS DATE))
LEFT JOIN dbo.Activities a ON wa.ActivityID = a.ActivityID
LEFT JOIN dbo.Projects p ON a.ProjectID = p.ProjectID
LEFT JOIN dbo.Clients c ON p.ClientID = c.ClientID
WHERE ws.UserID = @UserID
	 AND CAST(ws.StartTime AS DATE) >= @DateFrom
	 AND CAST(ws.StartTime AS DATE) <= @DateTo

UNION

-- Leave Days
SELECT
	 @UserName [User Name],
	 @FullName [Full Name],
	 WorkDate [Date],
	 NULL,
	 NULL,
	 NULL,
	 'Leave',
	 NULL,
	 NULL,
	 t.LeaveTypeName
FROM #LeaveTemp lt
INNER JOIN dbo.Leaves l ON lt.WorkDate >= CAST(l.DateFrom AS DATE)
	 AND lt.WorkDate <= CAST(l.DateTo AS DATE)
INNER JOIN dbo.LeaveTypes t ON l.LeaveType = t.LeaveTypeID
WHERE l.UserID = @UserID 

UNION

-- Flexi Days
SELECT
	 u.UserName [User Name],
	 u.FullName [Full Name],
	 CAST(f.FlexiDate AS DATE) [Date],
	 NULL,
	 NULL,
	 NULL,
	 'Leave',
	 NULL,
	 NULL,
	 'Flexi Day'
FROM dbo.Users u
INNER JOIN dbo.flexidays f ON u.UserID = f.UserID
WHERE f.UserID = @UserID
AND CAST(f.FlexiDate As DATE) >= @DateFrom
AND CAST(f.FlexiDate as DATE) <= @DateTo

/*
EXEC GetActivityAndLeaveByUserID '2015-07-01 09:00:00.000', '2016-07-31 09:00:00.000', 89
*/