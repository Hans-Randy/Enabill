

-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <13/11/2015>
-- Description:	<Get a user by ID>
-- =============================================
CREATE PROCEDURE [dbo].[GetUserByUserID] 
	-- Add the parameters for the stored procedure here
	@UserId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	DECLARE @ManagerID INT
	SET @ManagerID = (SELECT [ManagerID] FROM [dbo].[Users] WHERE UserID = @UserId)
	SELECT 
		[UserID],
		[UserName],
		[FullName],
		[Password],
		[IsActive],
		[FirstName],
		[LastName],
		[Email],
		[WorkHours],
		[ManagerID],
		[BillableIndicatorID],
		[DivisionID],
		[EmploymentTypeID],
		[EmployStartDate],
		[RegionID],
		[Phone],
		[CanLogin],
		[MustResetPwd],
		[ExternalRef],
		[ForgottenPasswordToken],
		[LastModifiedBy],
		[FlexiBalanceTakeOn],
		[AnnualLeaveTakeOn],
		[PayrollRefNo],
		[IsSystemUser] 
	FROM 
		[dbo].[Users] 
	WHERE 
		UserID = @UserId

	SELECT 
		[UserID],
		[UserName],
		[FullName],
		[Password],
		[IsActive],
		[FirstName],
		[LastName],
		[Email],
		[WorkHours],
		[ManagerID],
		[BillableIndicatorID],
		[DivisionID],
		[EmploymentTypeID],
		[EmployStartDate],
		[RegionID],
		[Phone],
		[CanLogin],
		[MustResetPwd],
		[ExternalRef],
		[ForgottenPasswordToken],
		[LastModifiedBy],
		[FlexiBalanceTakeOn],
		[AnnualLeaveTakeOn],
		[PayrollRefNo],
		[IsSystemUser] 
	FROM 
		[dbo].[Users] 
	WHERE 
		UserID = @ManagerID	
END