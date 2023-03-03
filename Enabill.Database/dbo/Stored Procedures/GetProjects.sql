

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetProjects]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT
		P.BillingMethodID,
		P.BookInAdvance,
		P.CanHaveNotes,
		P.ClientID,
		P.ConfirmedEndDate,
		P.DeactivatedBy,
		P.DeactivatedDate,
		P.DepartmentID,
		P.GroupCode,
		P.InvoiceAdminID,
		P.LastModifiedBy,
		P.MustHaveRemarks,
		P.ProjectCode,
		P.ProjectDesc,
		P.ProjectID,
		P.ProjectName,
		P.ProjectOwnerID,
		P.ProjectValue,
		P.RegionID,
		P.ScheduledEndDate,
		P.StartDate,
		P.SupportEmailAddress
	FROM
		[dbo].[Projects] P
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetProjects] TO [Enabill_User]
    AS [dbo];

