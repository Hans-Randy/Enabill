-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <20 November 2015>
-- Description:	<Get Users Work Sessions>
-- =============================================
CREATE PROCEDURE [dbo].[GetTimesheets] 
	-- Add the parameters for the stored procedure here
	@DateFrom DATETIME,
	@DateTo DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	EXEC GetUsers
	EXEC GetWorkAllocations @DateFrom, @DateTo
	EXEC GetWorkSessions @DateFrom, @DateTo
	EXEC GetWorkDays @DateFrom, @DateTo
	EXEC GetLeaves @DateFrom, @DateTo
	EXEC GetFlexiDays @DateFrom, @DateTo
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetTimesheets] TO [Enabill_User]
    AS [dbo];

