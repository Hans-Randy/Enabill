
/*
	 2016-10-18
	 Will return all leave dates between multiple date ranges.
*/
        
CREATE PROCEDURE [dbo].[GetLeaveDatesBetweenRangesByUserID] 
	 @DateFrom DATE,
	 @DateTo DATE,
	 @UserID INT
AS
	 IF(OBJECT_ID('tempdb..#DateTemp') IS NOT NULL)
	 BEGIN
		  DROP TABLE #DateTemp
	 END

	 CREATE TABLE #DateTemp
	 (
		  DateValue DATE
	 )

	 DECLARE
		  @DateFromCursor DATE,
		  @DateToCursor DATE,
		  @DateCursor as CURSOR;
 
	 SET @DateCursor = CURSOR FOR
		  SELECT 
				CAST(DateFrom AS date) DateFrom,
				CAST(DateTo AS date) DateTo
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
				INSERT INTO #DateTemp
				SELECT ISODate
				FROM dbo.Calendar
				WHERE ISODate >= @DateFromCursor
				AND ISODate <= @DateToCursor
			FETCH NEXT FROM @DateCursor INTO @DateFromCursor, @DateToCursor;
		  END
 
	 CLOSE @DateCursor;
	 DEALLOCATE @DateCursor;

	 SELECT * FROM #DateTemp
	 WHERE DateValue >= @DateFrom
	 AND DateValue <= @DateTo

	 If(OBJECT_ID('tempdb..#DateTemp') IS NOT NULL)
	 BEGIN
		  DROP TABLE #DateTemp
	 END

/*
EXECUTE GetLeaveDatesBetweenRangesByUserID '2016-03-01', '2016-04-30', 89
GO
*/