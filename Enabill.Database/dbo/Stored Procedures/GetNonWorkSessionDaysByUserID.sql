       
CREATE PROCEDURE [dbo].[GetNonWorkSessionDaysByUserID] 
	 @DateTimeFrom VARCHAR(25),
	 @DateTimeTo VARCHAR(25),
	 @UserID int
AS

DECLARE 
	 @DateFrom Date = CAST(@DateTimeFrom AS date),
	 @DateTo Date = CAST(@DateTimeTo AS date)

SELECT
	 [NonWorkSessionID],
	 [UserID],
	 [Date],
	 [LastModifiedBy]
FROM
	[dbo].[NonWorkSessions]
WHERE 
	UserID = @UserID
	AND [Date] >= @DateFrom AND [Date] <= @DateTo
ORDER BY [Date]

/*
EXECUTE GetNonWorkSessionDaysByUserID '2016/03/01 12:00:00 AM', '2016/03/31 12:00:00 AM', 89
GO
*/