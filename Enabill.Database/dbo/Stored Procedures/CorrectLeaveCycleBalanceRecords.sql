CREATE PROCEDURE dbo.CorrectLeaveCycleBalanceRecords
	 @UserID INT,
	 @LeaveType INT 
AS

/*
	 =============================================
	 Author:			<Nico van der Walt>
	 Create date:	<10/04/2017>
	 Description:	<Correct records where there is a discrepancy between the leave taken and the leave cycle balance>
	 =============================================

	 IF OBJECT_ID('dbo.CorrectLeaveCycleBalanceRecords') IS NOT NULL
		  EXEC ('DROP PROCEDURE dbo.CorrectLeaveCycleBalanceRecords;')
	 GO

	 EXEC dbo.CorrectLeaveCycleBalanceRecords
		  @UserID = 10,
		  @LeaveType = 2
*/

	 DECLARE		  
		  @LeaveCycleBalanceID INT,
		  @FutureLeaveCycleBalanceID INT,
		  @Taken FLOAT = 0,
		  @FutureTaken FLOAT = 0,
		  @DateFrom DATETIME = NULL,
		  @FutureDateTo DATETIME,
		  @EndDate DATETIME,
		  @FutureEndDate DATETIME,
		  @FutureStartDate DATETIME,
		  @NoOfDaysBefore FLOAT = 0,
		  @NoOfDaysAfter FLOAT = 0

	 -- Active leave cycle
	 SELECT 
		  @LeaveCycleBalanceID = lcb.LeaveCycleBalanceID,
		  @Taken = SUM(l.NumberOfDays)
	 FROM [dbo].[Leaves] l 
		  JOIN [dbo].[LeaveCycleBalances] lcb ON lcb.UserID = l.UserID
		  AND lcb.LeaveTypeID = l.LeaveType
		  JOIN dbo.Users u ON u.UserID = l.UserID
	 WHERE 
		  l.DateFrom >= lcb.StartDate
		  AND l.DateTo <= lcb.EndDate
		  AND l.ApprovalStatus = 4 -- Approved
		  AND lcb.Active = 1
		  AND u.IsActive = 1
		  AND u.UserID = @UserID
		  AND l.LeaveType = @LeaveType
	 GROUP BY
		  lcb.LeaveCycleBalanceID,
		  lcb.Taken
	 HAVING
		  SUM(l.NumberOfDays) <> lcb.Taken

	 -- Get end date of active leave cycle
	 SELECT
		  @EndDate = EndDate
	 FROM
		  dbo.LeaveCycleBalances
	 WHERE
		  LeaveCycleBalanceID = @LeaveCycleBalanceID

	 -- Set start date of new leave cycle
	 SET @FutureStartDate = @EndDate + 1

	 -- Find any records in a future leave cycle period	 
	 SELECT
		  @DateFrom = DateFrom,
		  @FutureDateTo = DateTo
	 FROM
		  dbo.Leaves
	 WHERE
		  UserID = @UserID
		  AND LeaveType = @LeaveType
		  AND DateFrom <= @EndDate
		  AND DateTo > @EndDate

	 -- If any records, update active cycle as well as future cycle
	 IF(@DateFrom IS NOT NULL)
		  BEGIN
				EXEC @NoOfDaysBefore = dbo.GetNoOfWorkDays @DateFrom, @EndDate, '1', 1
				EXEC @NoOfDaysAfter = dbo.GetNoOfWorkDays @FutureStartDate, @FutureDateTo, '1', 1

				-- Leave Cycle
				SELECT
					 @FutureLeaveCycleBalanceID = LeaveCycleBalanceID,
					 @FutureEndDate = EndDate
				FROM
					 dbo.LeaveCycleBalances
				WHERE
					 UserID = @UserID
					 AND LeaveTypeID = @LeaveType
					 AND StartDate = @FutureStartDate

				-- Leave
				SELECT 
					 @FutureTaken = SUM(NumberOfDays)
				FROM
					 dbo.Leaves
				WHERE
					 UserID = @UserID
					 AND LeaveType = @LeaveType
					 AND DateFrom >= @FutureStartDate
					 AND DateTo <= @FutureEndDate

				-- If value is null then set to zero
				SET @FutureTaken = ISNULL(@FutureTaken, 0)

				-- Update future leave cycle
				UPDATE
					 dbo.LeaveCycleBalances
				SET
					 Taken = @FutureTaken + @NoOfDaysAfter,
					 ClosingBalance = OpeningBalance - (@FutureTaken + @NoOfDaysAfter)
				WHERE
					 LeaveCycleBalanceID = @FutureLeaveCycleBalanceID
		  END

	 -- Update active leave cycle
	 UPDATE
		  dbo.LeaveCycleBalances
	 SET
		  Taken = @Taken + @NoOfDaysBefore,
		  ClosingBalance = OpeningBalance - (@Taken + @NoOfDaysBefore)
	 WHERE
		  LeaveCycleBalanceID = @LeaveCycleBalanceID