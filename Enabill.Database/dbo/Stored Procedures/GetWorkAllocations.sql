


-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <13/11/2015>
-- Description:	<GetWorkAllocations>
-- =============================================
CREATE PROCEDURE [dbo].[GetWorkAllocations] 
	-- Add the parameters for the stored procedure here
	@DateFrom DATETIME,
	@DateTo DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT
		W.[WorkAllocationID],
		W.[WorkAllocationType],
		W.[UserID],
		U.[UserName],
		U.[FullName],
		W.[ActivityID],
		W.[DayWorked],
		W.[Period],
		W.[HoursWorked],
		W.[HoursBilled],
		W.[Remark],
		W.[UserCreated],
		W.[DateCreated],
		W.[UserModified],
		W.[DateModified],
		W.[ParentWorkAllocationID],
		W.[LastModifiedBy],
		W.[InvoiceID],
		W.[HourlyRate],
		W.[TrainingCategoryID],
		W.[TrainerName],
		W.[TrainingInstitute],
		W.[TicketReference]
	FROM
		[dbo].[WorkAllocations] W JOIN [dbo].[Users] U ON W.UserID = U.UserID
	WHERE
		DayWorked >= @DateFrom AND DayWorked <= @DateTo
	ORDER BY
		DayWorked DESC
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetWorkAllocations] TO [Enabill_User]
    AS [dbo];

