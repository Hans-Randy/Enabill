-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <20 November 2015>
-- Description:	<Get Work Days>
-- =============================================
CREATE PROCEDURE [dbo].[GetWorkDays]
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
		W.IsWorkable,
		W.WorkDate
	FROM
		WorkDays W	
	WHERE
		W.WorkDate >= @DateFrom AND W.WorkDate <= @DateTo
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetWorkDays] TO [Enabill_User]
    AS [dbo];

