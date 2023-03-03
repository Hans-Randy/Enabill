CREATE PROCEDURE dbo.GetDaysInLeaveCycleByUser
    @UserID int,
    @LeaveType int,
	 @DateWorkDay datetime
AS

/*
	=============================================
	Author:			<Nico van der Walt>
	Create date:	<10/04/2017>
	Description:	<Get number of days in leave cycle by user>
	=============================================

	IF OBJECT_ID('dbo.GetDaysInLeaveCycleByUser') IS NOT NULL
		EXEC ('DROP PROCEDURE dbo.GetDaysInLeaveCycleByUser;')
	GO

	Leave Type:
		1 = Annual
		2 = Sick
		4 = Family Responsibility
		8 = Study
		16 = Maternity
		32 = Relocation
		64 = Unpaid
		128 = No Work Day

	Approval Status:
		1 = Pending
		2 = Declined
		4 = Approved
		8 = Withdrawn 

	EXEC dbo.GetDaysInLeaveCycleByUser
		@UserID = 10,
		@LeaveType = 2,
		@DateWorkDay = '2017-01-01' --Format: '2016-04-13'
*/

	SELECT
		lcb.LeaveCycleBalanceID,
		lcb.UserID,
		lcb.LeaveTypeID,
		lcb.StartDate,
		lcb.EndDate,
		lcb.OpeningBalance,
		sum(l.NumberOfDays) Taken,
		lcb.ClosingBalance,
		lcb.Active,
		lcb.LastUpdatedDate,
		lcb.ManualAdjustment
	FROM
		dbo.Leaves l
	INNER JOIN
		dbo.LeaveCycleBalances lcb
	ON
		l.LeaveType = lcb.LeaveTypeID
	AND
		l.UserID = lcb.UserID
	WHERE
		l.UserID = @UserID
	AND
		l.LeaveType = @LeaveType
	AND
		lcb.Active = 1
	AND
		l.ApprovalStatus = 4
	AND
		l.DateFrom >= lcb.StartDate
	AND
		l.DateFrom <= lcb.EndDate
	AND
		@DateWorkDay BETWEEN lcb.StartDate AND lcb.EndDate
	GROUP BY
		lcb.UserID,
		lcb.LeaveTypeID,
		lcb.StartDate,
		lcb.EndDate,
		lcb.OpeningBalance,
		Taken,
		lcb.ClosingBalance,
		lcb.Active,
		lcb.ManualAdjustment,
		lcb.LeaveCycleBalanceID,
		lcb.LastUpdatedDate