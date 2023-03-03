/*
-- =============================================
-- Author:		<Nico van der Walt>
-- Create date: <31/10/2017>
-- Description:	Get the actual leave days over a specific month.
				--If the leave starts in one month and finishes in another, it will only give the leave days for that specific month.

DROP PROCEDURE IF EXISTS [dbo].[GetActualLeaveDaysForMonth]

Leave Type:
    1 = Annual
    2 = Sick
    4 = Family Responsibility
    8 = Study
    16 = Maternity
    32 = Relocation
    64 = Unpaid
    128 = No Work Day

EXEC [dbo].[GetActualLeaveDaysForMonth]
	@Month = 9
	,@Year = 2017
	,@LeaveType = 1
*/

CREATE PROCEDURE [dbo].[GetActualLeaveDaysForMonth]
	@Month int
	,@Year int
	,@LeaveType nvarchar(10) = ''
AS
BEGIN
	DECLARE
		@UserID int,
		@EmployeeCode varchar(10),
		@LeaveTypeID varchar(64),
		@PayoutRate varchar(5),
		@DateFrom date,
		@DateTo date,
		@UnitsTaken float,
		@LeaveReasonID varchar(200),
		@Comment varchar(200),
		@ReferenceNumber int,
		@NoteReceived varchar(5),
		@TakeOnTransaction varchar(5),
		@StartDayType varchar(10),
		@EndDayType varchar(10),
		@ApplyAllDayType varchar(10),
		@DateStartOfMonth date,
		@DateEndOfMonth date,
		@LeaveCursor AS cursor

		SET @DateStartOfMonth = (DATEFROMPARTS (@Year, @Month, 1))
		SET @DateEndOfMonth = EOMONTH (@DateStartOfMonth)

	DECLARE @Temp TABLE
	(
		EmployeeCode varchar(10),
		LeaveTypeID varchar(64),
		PayoutRate varchar(5),
		FromDate date,
		ToDate date,
		UnitsTaken float,
		LeaveReasonID varchar(200),
		Comment varchar(200),
		ReferenceNumber int,
		NoteReceived varchar(5),
		TakeOnTransaction varchar(5),
		StartDayType varchar(10),
		EndDayType varchar(10),
		ApplyAllDayType varchar(10)
	);
 
	SET @LeaveCursor = CURSOR FOR
		SELECT
			u.UserID,
			l.LeaveID,
			u.PayrollRefNo,
			CAST(l.DateFrom AS DATE) DateFrom,
			CAST(l.DateTo AS DATE) DateTo,
			CASE lt.LeaveTypeName
				WHEN 'Annual' THEN 'ANNUAL'
				WHEN 'Sick' THEN 'SICK'
				WHEN 'Family Responsibility' THEN 'FAMILY'
				WHEN 'Study' THEN 'STUDY'
				WHEN 'Maternity' THEN 'MATERNITY'
				WHEN 'Relocation' THEN 'RELOCATION'
				WHEN 'Unpaid' THEN 'UNPAID'
				WHEN 'No Work Day' THEN 'NO WORK DAY'
				WHEN 'Birthday' THEN 'BIRTHDAY'
			END LeaveTypeName,
			l.Remark
		FROM
			dbo.Leaves l
		INNER JOIN 
			dbo.Users u on u.UserID = l.UserID
		INNER JOIN
			dbo.LeaveTypes lt on lt.LeaveTypeID = l.LeaveType
		WHERE
		(
			(DateFrom >= @DateStartOfMonth AND DateFrom <= @DateEndOfMonth)
		OR
			(DateTo >= @DateStartOfMonth AND DateTo <= @DateEndOfMonth)
		)
		AND l.ApprovalStatus = 4
		AND
			l.LeaveType LIKE
			CASE WHEN IsNumeric(@LeaveType) = 1 THEN  
			@LeaveType
			ELSE
			'%' + @LeaveType
			END
		--AND u.UserID = 208
 
	OPEN @LeaveCursor;
		FETCH NEXT FROM @LeaveCursor INTO
			@UserID,
			@ReferenceNumber, --LeaveID
			@EmployeeCode, --PayrollRefNo
			@DateFrom, --DateFrom
			@DateTo, --DateTo
			@LeaveTypeID, --LeaveTypeName
			@Comment --Remark
			 
		WHILE @@FETCH_STATUS = 0
		BEGIN
			
			SET @DateFrom = IIF(@DateFrom < @DateStartOfMonth, @DateStartOfMonth, @DateFrom)
			SET @DateTo = IIF(@DateTo > @DateEndOfMonth, @DateEndOfMonth, @DateTo)

			PRINT @DateFrom
			PRINT @DateTo
			PRINT @UserID

			SET @LeaveReasonID = ''
			SET @PayoutRate = 'O'
			SET @UnitsTaken = (SELECT COUNT(WorkDate) FROM dbo.WorkDays WHERE WorkDate >= @DateFrom AND WorkDate <= @DateTo AND IsWorkable = 1)
			SET @NoteReceived = 'FALSE'
			SET @TakeOnTransaction = 'FALSE'
			SET @StartDayType = ''
			SET @EndDayType = ''
			SET @ApplyAllDayType = ''

			INSERT INTO @Temp
			(	
				EmployeeCode,
				LeaveTypeID,
				PayoutRate,
				FromDate,
				ToDate,
				UnitsTaken,
				LeaveReasonID,
				Comment,
				ReferenceNumber,
				NoteReceived,
				TakeOnTransaction,
				StartDayType,
				EndDayType,
				ApplyAllDayType
			)
			VALUES
			(
				@EmployeeCode,
				@LeaveTypeID,
				@PayoutRate,
				@DateFrom,
				@DateTo,
				@UnitsTaken,
				@LeaveReasonID,
				@Comment,
				@ReferenceNumber,
				@NoteReceived,
				@TakeOnTransaction,
				@StartDayType,
				@EndDayType,
				@ApplyAllDayType
			)
		
			FETCH NEXT FROM @LeaveCursor INTO
				@UserID,
				@ReferenceNumber, --LeaveID
				@EmployeeCode, --PayrollRefNo
				@DateFrom, --DateFrom
				@DateTo, --DateTo
				@LeaveTypeID, --LeaveTypeName
				@Comment --Remark
		
		END
 
	CLOSE @LeaveCursor;
	DEALLOCATE @LeaveCursor;

	SELECT
		*
	FROM
		@Temp
	ORDER BY
		EmployeeCode, FromDate, LeaveTypeID

	RETURN;
END