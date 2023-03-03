

-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <13/11/2015>
-- Description:	<Get billable indicators>
-- =============================================
CREATE PROCEDURE [dbo].[GetBillableIndicators] 
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT
		B.BillableIndicatorID,
		B.BillableIndicatorName
	FROM
		 [dbo].[BillableIndicators] B
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetBillableIndicators] TO [Enabill_User]
    AS [dbo];

