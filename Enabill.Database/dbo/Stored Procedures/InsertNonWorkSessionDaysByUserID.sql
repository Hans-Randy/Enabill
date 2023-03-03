     
CREATE PROCEDURE [dbo].[InsertNonWorkSessionDaysByUserID] 
	 @DateTimeFrom DateTime,
	 @DateTimeTo DateTime,
	 @UserID int
AS

INSERT INTO dbo.NonWorkSessions
EXECUTE GetOutstandingNonWorkSessionDaysByUserID @DateTimeFrom, @DateTimeTo, @UserID
 
/*
EXECUTE InsertNonWorkSessionDaysByUserID '2016/03/01 12:00:00 AM', '2016/03/31 12:00:00 AM', 89
GO
*/