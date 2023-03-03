-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <28 November 2016>
-- Description:	<Get Work Sessions>
-- =============================================
CREATE PROCEDURE [dbo].[GetWorkSessionsByUserID]
	-- Add the parameters for the stored procedure here
	@UserID INT,
	@DateFrom DATETIME,
	@DateTo DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT
		W.EndTime,
		W.LastModifiedBy,
		W.LunchTime,
		W.StartTime,
		W.UserID,
		W.WorkSessionID,
		W.WorkSessionStatusID
	FROM
		WorkSessions W
	WHERE
		W.UserID = @UserID
	AND 
		(DATEPART(Year, EndTime) = DATEPART(Year, @DateTo) AND DatePart(Month, EndTime) = DatePart(Month,@DateTo) AND DatePart(Day, EndTime) <= DatePart(Day,@DateTo))
	AND
		(DATEPART(Year, StartTime) = DATEPART(Year, @DateFrom) AND DatePart(Month, StartTime) = DatePart(Month,@DateFrom) AND DatePart(Day, StartTime) >= DatePart(Day,@DateFrom))
END