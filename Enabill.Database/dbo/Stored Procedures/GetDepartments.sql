

-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <13/11/2015>
-- Description:	<Get the departments>
-- =============================================
CREATE PROCEDURE [dbo].[GetDepartments]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT
		D.DepartmentID,
		D.DepartmentName
	FROM
		[dbo].[Departments] D
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetDepartments] TO [Enabill_User]
    AS [dbo];

