
-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <20 November 2015>
-- Description:	<Get Work Sessions>
-- =============================================
CREATE PROCEDURE [dbo].[GetWorkSessions]
	-- Add the parameters for the stored procedure here
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
		StartTime >= @DateFrom AND StartTime <= DATEADD(day, 1, @DateTo)
END