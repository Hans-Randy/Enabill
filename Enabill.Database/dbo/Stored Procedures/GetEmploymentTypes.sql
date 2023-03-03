

-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <13/11/2015>
-- Description:	<Get the employment types>
-- =============================================
CREATE PROCEDURE [dbo].[GetEmploymentTypes]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT
		E.EmploymentTypeID,
		E.EmploymentTypeName
	FROM
		[dbo].[EmploymentTypes] E
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetEmploymentTypes] TO [Enabill_User]
    AS [dbo];

