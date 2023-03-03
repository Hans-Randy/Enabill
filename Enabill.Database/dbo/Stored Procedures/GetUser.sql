

-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <28 April 2016>
-- Description:	<Gets a user by UserID>
-- =============================================
CREATE PROCEDURE [dbo].[GetUser]
	-- Add the parameters for the stored procedure here
	@UserID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		U.[UserID],
		U.[UserName],
		U.[FullName],
		U.[Password],
		U.[IsActive],
		U.[FirstName],
		U.[LastName],
		U.[Email],
		U.[WorkHours],
		U.[ManagerID],
		U.[BillableIndicatorID],
		U.[DivisionID],
		U.[EmploymentTypeID],
		U.[EmployStartDate],
		U.[RegionID],
		U.[Phone],
		U.[CanLogin],
		U.[MustResetPwd],
		U.[ExternalRef],
		U.[ForgottenPasswordToken],
		U.[LastModifiedBy],
		U.[FlexiBalanceTakeOn],
		U.[AnnualLeaveTakeOn],
		U.[PayrollRefNo],
		U.[IsSystemUser],
		U.FirstName + ' ' + U.LastName As FriendlyName,
		U.[EmployEndDate]
	FROM 
		[Users] U 
	WHERE
		U.UserID = @UserID
END