

/*
	=============================================
	Author:			<Nico van der Walt>
	Create date:	<10/04/2017>
	Description:	<Get Leave and include Username in the results>
	=============================================

	IF OBJECT_ID('dbo.GetLeavesWithUserName') IS NOT NULL
		EXEC ('DROP PROCEDURE dbo.GetLeavesWithUserName;')
	GO

	EXEC dbo.GetLeavesWithUserName
		@DateFrom = '2017-02-01 00:00:00.000',
		@DateTo = '2017-02-28 00:00:00.000'
*/

CREATE PROCEDURE [dbo].[GetLeavesWithUserName] 
	@DateFrom DATETIME,
	@DateTo DATETIME
AS
	BEGIN
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;

		-- Insert statements for procedure here
		SELECT
			u.UserName,
			u.FullName,
			l.UserID,
			w.WorkDate,
			l.LeaveID,
			l.ApprovalStatus,
			CASE l.ApprovalStatus
			   WHEN 1 THEN 'Pending'
			   WHEN 2 THEN 'Declined'
			   WHEN 4 THEN 'Approved'
			   WHEN 8 THEN 'Withdrawn'
			END 'Approval Status',   
			l.LeaveType,
			CASE l.LeaveType
				WHEN 1 THEN 'Annual'
				WHEN 2 THEN 'Sick'
				WHEN 4 THEN 'Family Responsibility'
				WHEN 8 THEN 'Study'
				WHEN 16 THEN 'Maternity'
				WHEN 32 THEN 'Relocation'
				WHEN 64 THEN 'Unpaid'
				WHEN 128 THEN 'No Work Day'
			END 'Leave Type',   
			l.DateFrom,
			l.DateTo,
			l.NumberOfDays,
			l.NumberOfHours,
			l.DateRequested,
			l.DateManaged,
			l.LastModifiedBy,
			l.ManagedBy,
			l.Remark
		FROM
			dbo.Leaves l
			INNER JOIN dbo.Users u ON u.UserID = l.UserID
			INNER JOIN dbo.WorkDays w on w.WorkDate >= l.DateFrom AND w.WorkDate <= l.DateTo
		WHERE
			w.IsWorkable = 1
			AND
				l.ApprovalStatus = 4
			AND
				w.WorkDate >= @DateFrom
			AND
				w.WorkDate <= @DateTo
		ORDER BY
			u.FullName,
			w.WorkDate
	END