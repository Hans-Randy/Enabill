

-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <13/11/2015>
-- Description:	<Get the divisions>
-- =============================================
CREATE PROCEDURE [dbo].[GetDivisions]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT
		D.DivisionCode,
		D.DivisionID,
		D.DivisionName
	FROM
		[dbo].[Divisions] D
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetDivisions] TO [Enabill_User]
    AS [dbo];

