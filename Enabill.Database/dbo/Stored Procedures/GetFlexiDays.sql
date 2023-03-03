-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <20 November 2015>
-- Description:	<Get Flexi Days>
-- =============================================
CREATE PROCEDURE [dbo].[GetFlexiDays]
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
		F.ApprovalStatusID,
		F.DateSubmitted,
		F.FlexiDate,
		F.FlexiDayID,
		F.LastModifiedBy,
		F.Remark,
		F.UserID
	FROM
		FlexiDays F
	WHERE
		F.FlexiDate >= @DateFrom AND F.FlexiDate <= @DateTo
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetFlexiDays] TO [Enabill_User]
    AS [dbo];

