-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <13/11/2015>
-- Description:	<Get Users and Work Allocations>
-- =============================================
CREATE PROCEDURE [dbo].[GetUsersWorkAllocations] 
	-- Add the parameters for the stored procedure here
	@DateFrom DATETIME,
	@DateTo DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	EXEC GetUsers
	EXEC GetWorkAllocations @DateFrom, @DateTo
	EXEC GetRegions
	EXEC GetBillableIndicators
	EXEC GetDivisions
	EXEC GetEmploymentTypes
	EXEC GetActivities
	EXEC GetDepartments
	EXEC GetProjects
	EXEC GetClients
	EXEC GetTrainingCategories
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetUsersWorkAllocations] TO [Enabill_User]
    AS [dbo];

