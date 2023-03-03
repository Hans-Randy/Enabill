

-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <13/11/2015>
-- Description:	<Get Activities>
-- =============================================
CREATE PROCEDURE [dbo].[GetActivities] 
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT
		A.ActivityID,
		A.ActivityName,
		A.CanHaveNotes,
		A.DepartmentID,
		A.IsActive,
		A.LastModifiedBy,
		A.MustHaveRemarks,
		A.ProjectID,
		A.RegionID
	FROM
		[dbo].[Activities] A
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetActivities] TO [Enabill_User]
    AS [dbo];

