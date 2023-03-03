
/*
	=============================================
	Author:				<Nico van der Walt>
	Create date:		<10/04/2017>
	Description:		<To duplicate the current workday to the previous or next workday. Workday excludes any leave days in the period prior to or following the current day>
	=============================================
--------------------------------------------------
	IF OBJECT_ID('dbo.DuplicateWorkDay') IS NOT NULL
		EXEC ('DROP PROCEDURE dbo.DuplicateWorkDay;')
	GO
--------------------------------------------------
	EXEC dbo.DuplicateWorkDay
		@UserID = 36
		, @CurrentDay = '2018-02-16'
		, @IsPrevious = 0
--------------------------------------------------
*/

CREATE PROCEDURE dbo.DuplicateWorkDay 
    @UserID INT
	, @CurrentDay DATE -- Day that user is on
	, @IsPrevious BIT -- Is it the day before or the day after that must be copied
AS	
	DECLARE
		@WorkDay DATE -- The workday either before or after the current day
		, @LeaveDay DATE -- The leave day in the period either before or after the workday
		, @Count INT

	SET @LeaveDay = ''
	SET @Count = 0

	-- Find the next workday before or after the current day
	SELECT TOP 1
		@WorkDay = CONVERT(CHAR(10), WorkDate, 126)
	FROM
		dbo.workdays
	WHERE
		IsWorkable = 1
		AND
		(
			-- Previous work day
			CASE WHEN @IsPrevious = 1 THEN CONVERT(CHAR(10), WorkDate, 126) END < @CurrentDay
			OR
			-- Following work day	
			CASE WHEN @IsPrevious = 0 THEN CONVERT(CHAR(10), WorkDate, 126) END > @CurrentDay
		)
	ORDER BY
		CASE WHEN @IsPrevious = 1 THEN Workdate END DESC,
		CASE WHEN @IsPrevious = 0 THEN Workdate END ASC

	-- Find any leave period that falls into the workday before or after the current workday and get the end of that period
	SELECT
		@LeaveDay =
			CASE WHEN @IsPrevious = 1
				-- Previous workday falls into the end of the leave period
				THEN CONVERT(CHAR(10), DateFrom, 126)
				-- Following workday falls into the beginning of the leave period	
				ELSE CONVERT(CHAR(10), DateTo, 126) END 
	FROM
		dbo.Leaves
	WHERE
		UserID = @UserID
		AND
		(
			-- Previous work day
			CASE WHEN @IsPrevious = 1 THEN CONVERT(CHAR(10), DateTo, 126) END = @WorkDay
			OR
			-- Following work day	
			CASE WHEN @IsPrevious = 0 THEN CONVERT(CHAR(10), DateFrom, 126) END = @WorkDay
		)

	-- If there is a leave period then take the workday before or after the leave period
	IF ISNULL(@LeaveDay, '') <> ''
		BEGIN
			SELECT TOP 1
				@WorkDay = CONVERT(CHAR(10), WorkDate, 126)
			FROM
				dbo.workdays
			WHERE
				IsWorkable = 1
				AND
				(
					-- Previous work day
					CASE WHEN @IsPrevious = 1 THEN CONVERT(CHAR(10), WorkDate, 126) END < @LeaveDay
					OR
					-- Following work day	
					CASE WHEN @IsPrevious = 0 THEN CONVERT(CHAR(10), WorkDate, 126) END > @LeaveDay
				)
			ORDER BY
				CASE WHEN @IsPrevious = 1 THEN Workdate END DESC,
				CASE WHEN @IsPrevious = 0 THEN Workdate END ASC
		END

	-- Check that there are Work Sessions and Work Allocations on the day that need to be copied from
	-- Check for any Work Sessions first
	SELECT
		@Count = COUNT(UserID)
	FROM 
		dbo.WorkSessions
	WHERE
		UserID = @UserID
		AND
		CONVERT(CHAR(10), StartTime, 126) =
			CASE WHEN @IsPrevious = 1
				-- Current day
				THEN @WorkDay
				-- Following work day	
				ELSE @CurrentDay END 

	IF @Count > 0
		BEGIN
			-- Now check for any Work Allocations
			SELECT
				@Count = COUNT(UserID)
			FROM 
				dbo.WorkAllocations
			WHERE
				UserID = @UserID
				AND
				CONVERT(CHAR(10), DayWorked, 126) =
					CASE WHEN @IsPrevious = 1
						-- Current day
						THEN @WorkDay
						-- Following work day	
						ELSE @CurrentDay END 
		END

	IF @Count > 0
		BEGIN
			SET @Count = 0

			-- Delete any entries in the Work Sessions table for current day or the the following workday
			-- Check first if any entries exist
			SELECT
				@Count = COUNT(UserID)
			FROM 
				dbo.WorkSessions
			WHERE
				UserID = @UserID
				AND
				CONVERT(CHAR(10), StartTime, 126) =
					CASE WHEN @IsPrevious = 1
						-- Current day
						THEN @CurrentDay
						-- Following work day	
						ELSE @WorkDay END 

			IF @Count > 0
				BEGIN
					DELETE FROM dbo.WorkSessions
					WHERE
						UserID = @UserID
						AND
						CONVERT(CHAR(10), StartTime, 126) =
							CASE WHEN @IsPrevious = 1
								-- Current day
								THEN @CurrentDay
								-- Following work day	
								ELSE @WorkDay END 

					SET @Count = 0
				END

			-- Duplicate the entry in the Work Sessions table
			INSERT INTO dbo.WorkSessions
				(
					UserID
					, StartTime
					, EndTime
					, LunchTime
					, LastModifiedBy
					, WorkSessionStatusID
				)
			SELECT
				@UserID UserID
				, CASE WHEN @IsPrevious = 1
					THEN 
						STUFF(CONVERT(VARCHAR(50),StartTime, 121) ,1, 10, @CurrentDay)
					ELSE 
						STUFF(CONVERT(VARCHAR(50),StartTime, 121) ,1, 10, @WorkDay) END StartTime 
				, CASE WHEN @IsPrevious = 1
					THEN 
						STUFF(CONVERT(VARCHAR(50),EndTime, 121) ,1, 10, @CurrentDay)
					ELSE 
						STUFF(CONVERT(VARCHAR(50),EndTime, 121) ,1, 10, @WorkDay) END EndTime 
				, LunchTime
				, LastModifiedBy
				, 1 WorkSessionStatusID -- Unapproved
			FROM
				dbo.WorkSessions
			WHERE
				UserID = @UserID
				AND
				CONVERT(CHAR(10), StartTime, 126) =
					CASE WHEN @IsPrevious = 1
						-- Previous workday
						THEN @WorkDay
						-- Current day	
						ELSE @CurrentDay END 

			-- Delete any entries in the Work Allocations table for current day or the following work day
			-- Check first if any entries exist
			SELECT
				@Count = COUNT(UserID)
			FROM 
				dbo.WorkAllocations
			WHERE
				UserID = @UserID
				AND
				CONVERT(CHAR(10), DayWorked, 126) =
					CASE WHEN @IsPrevious = 1
						-- Current day
						THEN @CurrentDay
						-- Following work day	
						ELSE @WorkDay END 

			IF @Count > 0
				BEGIN
					DELETE FROM dbo.WorkAllocations
					WHERE
						UserID = @UserID
						AND
						-- Current day
						CONVERT(CHAR(10), DayWorked, 126) =
							CASE WHEN @IsPrevious = 1
								THEN @CurrentDay
								-- Following work day	
								ELSE @WorkDay END 
				END

			-- Duplicate the entries in the Work Allocations table
			INSERT INTO dbo.WorkAllocations
			(
				WorkAllocationType
				, UserID
				, ActivityID
				, DayWorked
				, [Period]
				, HoursWorked
				, Remark
				, UserCreated
				, DateCreated
				, ParentWorkAllocationID
				, LastModifiedBy
				, InvoiceID
				, HourlyRate
				, TrainingCategoryID
				, TrainerName
				, TrainingInstitute
				, TicketReference
			)
			SELECT
				WorkAllocationType
				, @UserID UserID
				, ActivityID
				, CASE WHEN @IsPrevious = 1
					THEN 
						STUFF(CONVERT(VARCHAR(50),DayWorked, 121) ,1, 10, @CurrentDay)
					ELSE 
						STUFF(CONVERT(VARCHAR(50),DayWorked, 121) ,1, 10, @WorkDay) END DayWorked 
				, CAST(LEFT(@WorkDay, 4) + RIGHT(LEFT(@WorkDay, 7), 2) AS INT) [Period]
				, HoursWorked
				, Remark
				, UserCreated
				, GetDate() DateCreated
				, ParentWorkAllocationID
				, LastModifiedBy
				, InvoiceID
				, HourlyRate
				, TrainingCategoryID
				, TrainerName
				, TrainingInstitute
				, TicketReference
			FROM
				dbo.WorkAllocations
			WHERE
				UserID = @UserID
				AND
				CONVERT(CHAR(10), DayWorked, 126) =
					CASE WHEN @IsPrevious = 1
						-- Current day
						THEN @WorkDay
						-- Following work day	
						ELSE @CurrentDay END
	END