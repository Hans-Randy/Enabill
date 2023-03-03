


-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <13 November 2015>
-- Description:	<Gets a list of users>
-- =============================================
CREATE PROCEDURE [dbo].[GetUsers]
	-- Add the parameters for the stored procedure here
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
		U.FirstName + ' ' + U.LastName As FriendlyName
	FROM 
		[dbo].[Users] U 
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetUsers] TO [Enabill_User]
    AS [dbo];

