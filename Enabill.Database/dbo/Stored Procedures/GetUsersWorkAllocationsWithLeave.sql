
/*
	=============================================
	Author:			<Nico van der Walt>
	Create date:	<08/05/2017>
	Description:	<Get Users, Work Allocations and Leave>
	=============================================

	IF OBJECT_ID('dbo.GetUsersWorkAllocationsWithLeave') IS NOT NULL
		EXEC ('DROP PROCEDURE dbo.GetUsersWorkAllocationsWithLeave;')
	GO

	EXEC dbo.GetUsersWorkAllocationsWithLeave
		@DateFrom = '2017-02-01 00:00:00.000',
		@DateTo = '2017-02-28 00:00:00.000'
*/

CREATE PROCEDURE [dbo].[GetUsersWorkAllocationsWithLeave] 
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
		EXEC GetLeavesWithUserName @DateFrom, @DateTo
	END