
-- Function returns all the workable dates between a range of dates per user

CREATE FUNCTION [dbo].[F_GetLeaveDatesBetweenRangesByUserID](@DateFrom DATE, @DateTo DATE, @UserID INT)
RETURNS @Dates TABLE (MyDates DATE)
AS
BEGIN
	 DECLARE
		  @DateFromCursor DATE,
		  @DateToCursor DATE,
		  @DateCursor as CURSOR;

	 DECLARE @Temp TABLE (TempDate DATE NOT NULL);
 
	 SET @DateCursor = CURSOR FOR
		  SELECT 
				CAST(DateFrom AS DATE) DateFrom,
				CAST(DateTo AS DATE) DateTo
		  FROM dbo.Leaves
		  WHERE 
		  UserID = @UserID
		  AND ApprovalStatus = 4
		  AND DateFrom  >= @DateFrom
		  AND DateTo <= DATEADD(mm, 1, @DateTo) -- To make sure a leave end date is included in the range
 
	 OPEN @DateCursor;
		  FETCH NEXT FROM @DateCursor INTO @DateFromCursor, @DateToCursor;
 
		  WHILE @@FETCH_STATUS = 0
		  BEGIN
				INSERT INTO @Temp
				SELECT ISODate
				FROM dbo.Calendar
				WHERE ISODate >= @DateFromCursor
				AND ISODate <= @DateToCursor
			FETCH NEXT FROM @DateCursor INTO @DateFromCursor, @DateToCursor;
		  END
 
	 CLOSE @DateCursor;
	 DEALLOCATE @DateCursor;

	 INSERT INTO @Dates
	 SELECT t.TempDate FROM @Temp t
	 INNER JOIN dbo.WorkDays w ON t.TempDate = CAST(w.WorkDate AS DATE)
	 WHERE w.IsWorkAble = 1
		  AND TempDate >= @DateFrom
		  AND TempDate <= @DateTo;

	 RETURN;
END

/*
SELECT * FROM dbo.F_GetLeaveDatesBetweenRangesByUserID ('2016-09-01', '2016-09-30', 83)
*/