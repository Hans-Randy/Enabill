

-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <13/11/2015>
-- Description:	<Get Training Categories>
-- =============================================
CREATE PROCEDURE [dbo].[GetTrainingCategories] 
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT
		TC.TrainingCategoryID,
		TC.TrainingCategoryName
	FROM
		[dbo].[TrainingCategories] TC
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetTrainingCategories] TO [Enabill_User]
    AS [dbo];

