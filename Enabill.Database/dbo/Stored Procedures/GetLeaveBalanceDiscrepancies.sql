
/*
	 =============================================
	 Author:		<Nico van der Walt>
	 Create date:	<10/04/2017>
	 Description:	<Get leave balance discrepancies>
	 =============================================
 	 
	 IF OBJECT_ID('dbo.GetLeaveBalanceDiscrepancies') IS NOT NULL
		  EXEC ('DROP PROCEDURE dbo.GetLeaveBalanceDiscrepancies;')
	 GO

	 IF OBJECT_ID('tempdb..#LeaveBalanceDiscrepancy') IS NOT NULL
		  EXEC ('DROP TABLE dbo.#LeaveBalanceDiscrepancy;')
	 GO
	
	 -- Find records where there is a discrepancy between the leave taken and the leave cycle balance
	 EXEC dbo.GetLeaveBalanceDiscrepancies

*/

CREATE PROCEDURE [dbo].[GetLeaveBalanceDiscrepancies]
AS
	 -- Delete temp table if it exists
	 IF OBJECT_ID('tempdb..#LeaveBalanceDiscrepancy') IS NOT NULL
		  EXEC ('DROP TABLE dbo.#LeaveBalanceDiscrepancy;')

	 -- Create Temp Table
	 CREATE TABLE #LeaveBalanceDiscrepancy
	 (
		  UserName VARCHAR(50),
		  FullName VARCHAR(50),
		  UserID FLOAT,
		  LeaveTypeID FLOAT,
		  Taken FLOAT,
		  Number FLOAT
	 )

	-- Insert into Temp Table
	INSERT INTO #LeaveBalanceDiscrepancy
	(
		  UserName,
		  FullName,
		  UserID,
		  LeaveTypeID,
		  Taken,
		  Number
	)
	 SELECT 
		  u.UserName UserName,
		  u.FullName FullName,
		  u.UserID UserID,
		  lcb.LeaveTypeID LeaveTypeID,
		  lcb.Taken Taken,
		  SUM(l.NumberOfDays) Number
	 FROM [dbo].[Leaves] l 
		  JOIN [dbo].[LeaveCycleBalances] lcb ON lcb.UserID = l.UserID
		  AND lcb.LeaveTypeID = l.LeaveType
		  JOIN dbo.Users u ON u.UserID = lcb.UserID
	 WHERE 
		  l.DateFrom >= lcb.StartDate
				AND l.DateTo <= lcb.EndDate
				AND l.ApprovalStatus = 4 -- Approved
				AND lcb.Active = 1
				AND u.IsActive = 1
	 GROUP BY
		  u.UserID,
		  u.UserName,
		  u.FullName,
		  lcb.LeaveTypeID,
		  lcb.Taken
	 HAVING
		  SUM(l.NumberOfDays) <> lcb.Taken
	 ORDER BY
		  u.FullName

	 -- Check if any users have leave into a new cycle
	 DECLARE
		  @UserID INT,
		  @LeaveTypeID INT,
		  @DateFrom DATE,
		  @EndDate DATE,
		  @Taken FLOAT,
		  @Number FLOAT,
		  @NoOfDays FLOAT
	 
	 DECLARE @MyCursor CURSOR
	 SET @MyCursor = CURSOR FAST_FORWARD
	 FOR
	 SELECT 
		  u.UserID,
		  lcb.LeaveTypeID,
		  l.DateFrom,
		  lcb.EndDate
	 FROM [dbo].[Leaves] l 
		  JOIN [dbo].[LeaveCycleBalances] lcb ON lcb.UserID = l.UserID
		  AND lcb.LeaveTypeID = l.LeaveType
		  JOIN dbo.Users u ON u.UserID = lcb.UserID
	 WHERE 
		  l.DateFrom >= lcb.StartDate
				AND l.DateTo > lcb.EndDate
				AND l.ApprovalStatus = 4 -- Approved
				AND lcb.Active = 1
				AND u.IsActive = 1
	 GROUP BY
		  u.UserID,
		  lcb.LeaveTypeID,
		  lcb.Taken,
		  l.DateFrom,
		  lcb.EndDate
	 HAVING
		  SUM(l.NumberOfDays) <> lcb.Taken
	 ORDER BY
		  u.UserID
	 
	 -- Start of cursor
	 OPEN @MyCursor
	 FETCH NEXT FROM @MyCursor
	 INTO @UserID, @LeaveTypeID, @DateFrom, @EndDate
	 WHILE @@FETCH_STATUS = 0
	 BEGIN 
		  -- Get number of workdays
		  SELECT
				@NoOfDays =COUNT(IsWorkable)
		  FROM
				dbo.WorkDays
		  WHERE
				WorkDate >= @DateFrom
				AND
					 WorkDate <= @EndDate
				AND
					 IsWorkable = 1

		  -- Get values from temp table
		  SELECT
				@Taken = Taken,
				@Number = Number
		  FROM
				#LeaveBalanceDiscrepancy
		  WHERE
				UserID = @UserID
				AND LeaveTypeID = @LeaveTypeID

		  -- If Leave taken equals number of days then remove record from temp table
		  IF(@Taken = @Number + @NoOfDays)
				BEGIN
					 DELETE FROM
						  #LeaveBalanceDiscrepancy
					 WHERE
						  UserID = @UserID
						  AND LeaveTypeID = @LeaveTypeID 
				END
 
	 FETCH NEXT FROM @MyCursor
	 INTO @UserID, @LeaveTypeID, @DateFrom, @EndDate
	 END
	 -- Clear cursor
	 CLOSE @MyCursor
	 DEALLOCATE @MyCursor

	 SELECT
		  UserName,
		  FullName,
		  UserID,
		  LeaveTypeID,
		  Taken,
		  Number
	 FROM
		  #LeaveBalanceDiscrepancy