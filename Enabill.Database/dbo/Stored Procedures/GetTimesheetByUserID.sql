
-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <28 April 2016>
-- Description:	<Get User Time Sheet>
-- =============================================
CREATE PROCEDURE [dbo].[GetTimesheetByUserID] 
	-- Add the parameters for the stored procedure here
	@UserID INT,
	@DateFrom DATETIME,
	@DateTo DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @UserID = 0
		SET @UserID = (SELECT TOP 1 UserID FROM Users ORDER BY FullName)
	-- Insert statements for procedure here
	EXEC GetUsers 
	EXEC GetUser @UserID
	EXEC GetWorkAllocationsByUserID @UserID, @DateFrom, @DateTo
	EXEC GetWorkSessionsByUserID @UserID, @DateFrom, @DateTo
	EXEC GetWorkDays @DateFrom, @DateTo
	EXEC GetLeavesByUserID @UserID, @DateFrom, @DateTo
	EXEC GetFlexiDaysByUserID @UserID, @DateFrom, @DateTo
END