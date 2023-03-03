-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <28 April 2016>
-- Description:	<Get Leave By User>
-- =============================================
CREATE PROCEDURE [dbo].[GetLeavesByUserID] 
	-- Add the parameters for the stored procedure here
	@UserID INT,
	@DateFrom DATETIME,
	@DateTo DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT
		L.ApprovalStatus,
		L.DateFrom,
		L.DateManaged,
		L.DateRequested,
		L.DateTo,
		L.LastModifiedBy,
		L.LeaveID,
		L.LeaveType,
		L.ManagedBy,
		L.NumberOfDays,
		L.NumberOfHours,
		L.Remark,
		L.UserID
	FROM
		Leaves L
	WHERE
		L.DateFrom >= @DateFrom AND L.DateTo <= @DateTo OR
		L.DateFrom >= @DateFrom AND L.DateFrom <= @DateTo OR
		L.DateTo >= @DateFrom AND L.DateTo <= @DateTo AND 
		L.UserID = @UserID
END