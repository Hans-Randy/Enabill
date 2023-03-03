
-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <11/12/2015>
-- Description:	<GetWorkAllocations By User>
-- =============================================
CREATE PROCEDURE [dbo].[GetWorkAllocationsByUserID] 
	-- Add the parameters for the stored procedure here
	@UserId INT,
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
		[dbo].[WorkAllocations] W
	WHERE
		UserID = @UserId 
		AND
		--AND DayWorked >= @DateFrom AND DayWorked <= @DateTo
		(DATEPART(Year, DayWorked) = DATEPART(Year, @DateFrom) AND DatePart(Month, DayWorked) = DatePart(Month,@DateFrom) AND DatePart(Day, DayWorked) >= DatePart(Day,@DateFrom))
		AND
		(DATEPART(Year, DayWorked) = DATEPART(Year, @DateTo) AND DatePart(Month, DayWorked) = DatePart(Month,@DateTo) AND DatePart(Day, DayWorked) <= DatePart(Day,@DateTo))
	ORDER BY
		DayWorked DESC
END