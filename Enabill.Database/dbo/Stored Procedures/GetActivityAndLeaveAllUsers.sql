
/*
IsActive:
	 0 = No
	 1 = Yes
	 '' = All
*/

CREATE PROCEDURE [dbo].[GetActivityAndLeaveAllUsers]
	 @DateTimeFrom DATETIME,
	 @DateTimeTo DATETIME,
	 @IsActive VARCHAR(10) = ''
AS

DECLARE
	 @UserID INT,
	 @DateFrom DATE,
	 @DateTo DATE,
	 @UserCursor as CURSOR;
DECLARE @Temp TABLE
(
	 [User Name] VARCHAR(50) NOT NULL,
	 [Date] DATE NOT NULL,
	 [Period] INT NULL,
	 Client VARCHAR(100) NULL,
	 Project VARCHAR(50) NULL,
	 Activity VARCHAR(50) NULL,
	 [Required] FLOAT NULL,
	 [Hours] FLOAT NULL,
	 Remark VARCHAR(200)
);

SET @DateFrom = cast(@DateTimeFrom AS DATE);
SET @DateTo = cast(@DateTimeTo AS DATE); 
SET @UserCursor = CURSOR FOR
	 SELECT 
		  UserID
	 FROM dbo.Users
	 WHERE 
		IsActive LIKE
		CASE WHEN IsNumeric(@IsActive) = 1 THEN  
		  @IsActive
		ELSE
		  '%' + @IsActive
		END
 
OPEN @UserCursor;
	 FETCH NEXT FROM @UserCursor INTO @UserID;
 
	 WHILE @@FETCH_STATUS = 0
	 BEGIN
		  INSERT INTO @Temp
				EXEC GetActivityAndLeaveByUserID @DateTimeFrom, @DateTimeTo, @UserID
		  FETCH NEXT FROM @UserCursor INTO @UserID;
	 END
 
CLOSE @UserCursor;
DEALLOCATE @UserCursor;

SELECT * FROM @Temp
ORDER BY [User Name];

/*
All Users:
EXEC GetActivityAndLeaveAllUsers '2016-09-01 00:00:00.000', '2016-09-30 00:00:00.000'

All Inactive users:
EXEC GetActivityAndLeaveAllUsers '2016-09-01 00:00:00.000', '2016-09-30 00:00:00.000', 0

All Active users:
EXEC GetActivityAndLeaveAllUsers '2016-09-01 00:00:00.000', '2016-09-30 00:00:00.000', 1
*/