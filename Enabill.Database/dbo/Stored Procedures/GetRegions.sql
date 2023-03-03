

-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <13/11/2015>
-- Description:	<Get the regions>
-- =============================================
CREATE PROCEDURE [dbo].[GetRegions] 
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT
		R.RegionID,
		R.RegionName
	FROM
		 [dbo].[Regions] R
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetRegions] TO [Enabill_User]
    AS [dbo];

