
/*
	 2016-10-04
	 Gets Non Work Session days which includes weekend days and public holidays.
	 This also excludes any work sessions that are on weekend days and/or public holidays.
*/
        
CREATE PROCEDURE [dbo].[GetOutstandingNonWorkSessionDaysByUserID] 
	 @DateTimeFrom VARCHAR(25),
	 @DateTimeTo VARCHAR(25),
	 @UserID int
AS

/*
	 SELECT @@DATEFIRST -- To get the SQL configuration for first day
	 1 = Sunday
	 2 = Monday
	 3 = Tuesday
	 4 = Wednesday
	 5 = Thursday
	 6 = Friday
	 7 = Saturday

	 Work Session Status
	 1 = Unapproved
	 2 = Approved
	 3 = Exception
*/

DECLARE 
	 @DateFrom Date = CAST(@DateTimeFrom AS date),
	 @DateTo Date = CAST(@DateTimeTo AS date)

-- Create Temporary Leave Table
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
	 @UserID,
	 CAST(WorkDate AS date) WorkDate,
	 'System' LastModifiedBy
FROM
	[dbo].[Workdays]
WHERE 
	 [Isworkable] = 0
	 -- Include statements below to limit to just weekend days
	 --AND DATEPART([dw], [Workdate]) IN
		--  (
		--		1, -- Sunday
		--		7 -- Saturday
		--  )
	 AND CAST(WorkDate AS date) >= @DateFrom AND CAST(WorkDate AS date) <= @DateTo
	 AND CAST(WorkDate AS date) NOT IN
	 (
		  -- Exclude Work Sessions
		  SELECT 
				CAST(StartTime AS date) WorkDate 
		  FROM
				dbo.Worksessions
		  WHERE 
				UserID = @UserID
				AND CAST(StartTime AS date) >= @DateFrom AND CAST(StartTime AS date) <= @DateTo
				-- Include statements below to limit to just weekend days
				--AND DATEPART([dw], [StartTime]) IN
				--(
				--	 1, -- Sunday
				--	 7 -- Saturday
				--)
		  
		  UNION
		  -- Exclude Existing Non Work Sessions
		  SELECT
				[Date] WorkDate
		  FROM dbo.NonWorkSessions
		  WHERE
				UserID = @UserID
				AND ([Date] >= @DateFrom AND [Date] <= @DateTo)
	 )

UNION
-- Include Leave days not already in Non Work Sessions		  
SELECT
	 @UserID,
	 lt.WorkDate,
	 'System' LastModifiedBy
FROM
	 #LeaveTemp lt
	 LEFT JOIN dbo.NonWorkSessions n
	 ON lt.WorkDate = n.[Date]
		  AND n.UserID = @UserID
		  AND n.[Date] IS NULL

UNION
-- Include Flexi days not already included in Non Work Sessions
SELECT
	 @UserID,
	 CAST(f.FlexiDate AS date) WorkDate,
	 'System' LastModifiedBy
FROM
	 FlexiDays f
	 LEFT JOIN dbo.NonWorkSessions n
	 ON (CAST(f.FlexiDate AS date) = n.[Date]) AND (f.UserID = n.UserID) AND n.[Date] IS NULL
	 WHERE
		  f.UserID = @UserID
		  AND f.ApprovalStatusID = 4
		  AND CAST(f.FlexiDate AS date) >= @DateFrom
		  AND CAST(f.FlexiDate AS date) <= @DateTo

ORDER BY WorkDate

/*
EXECUTE GetOutstandingNonWorkSessionDaysByUserID '2016-03-01', '2016-04-30', 89
GO
*/