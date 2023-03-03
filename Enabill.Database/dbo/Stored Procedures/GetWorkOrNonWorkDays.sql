
/*
2016-10-04
Get Workable or Non-Workable days with option to include or exclude weekends
----------------------------------------------------------------------------------
IF OBJECT_ID('dbo.GetWorkOrNonWorkDays') IS NOT NULL
  EXEC ('DROP PROCEDURE dbo.GetWorkOrNonWorkDays;')
GO
----------------------------------------------------------------------------------
IsWorkable:
    '' = All
	 0 = No
    1 = Yes

IncludeWeekends:
    '' = All
	 0 = No
    1 = Yes

Days of the Week:
	 1 = Sunday
	 2 = Monday
	 3 = Tuesday
	 4 = Wednesday
	 5 = Thursday
	 6 = Friday
	 7 = Saturday
----------------------------------------------------------------------------------
EXEC [dbo].[GetWorkOrNonWorkDays]
	 '', -- IsWorkable
	 '', -- IncludeWeekends
	 '2017-01-01 00:00:00.000', -- DateTimeFrom
	 '2017-12-31 00:00:00.000' -- DateTimeTo
*/
        
CREATE PROCEDURE [dbo].[GetWorkOrNonWorkDays] 
	 @IsWorkable VARCHAR(10) = '',
	 @IncludeWeekends VARCHAR(10) = '',
	 @DateTimeFrom VARCHAR(25) = NULL, -- '2017-01-01 00:00:00.000'
	 @DateTimeTo VARCHAR(25) = NULL -- '2017-12-31 00:00:00.000'
AS

IF IsNumeric(@IncludeWeekends) = 1  
	 IF @IncludeWeekends = '0'
		  SELECT
				[WorkDate]
				,[IsWorkable]
		  FROM 
				[dbo].[WorkDays]
		  WHERE
				[IsWorkable] LIKE
					 CASE WHEN IsNumeric(@IsWorkable) = 1 THEN  
						  @IsWorkable
					 ELSE
						  '%' + @IsWorkable
					 END
				AND
					 (([Workdate] >= @DateTimeFrom OR @DateTimeFrom IS NULL) AND ([Workdate] <= @DateTimeTo OR @DateTimeTo IS NULL))
				AND
				DATEPART([dw], [Workdate])
					 NOT IN
						  (
								1, -- Sunday
								7 -- Saturday
						  )
		  ORDER BY
				[WorkDate]
	 ELSE
		  SELECT
				[WorkDate]
				,[IsWorkable]
		  FROM 
				[dbo].[WorkDays]
		  WHERE
				[IsWorkable] LIKE
					 CASE WHEN IsNumeric(@IsWorkable) = 1 THEN  
						  @IsWorkable
					 ELSE
						  '%' + @IsWorkable
					 END
				AND
					 (([Workdate] >= @DateTimeFrom OR @DateTimeFrom IS NULL) AND ([Workdate] <= @DateTimeTo OR @DateTimeTo IS NULL))
				AND
				DATEPART([dw], [Workdate])
					 IN
						  (
								1, -- Sunday
								7 -- Saturday
						  )
		  ORDER BY
				[WorkDate]
ELSE
	 SELECT
		  [WorkDate]
		  ,[IsWorkable]
	 FROM 
		  [dbo].[WorkDays]
	 WHERE
		  [IsWorkable] LIKE
				CASE WHEN IsNumeric(@IsWorkable) = 1 THEN  
					 @IsWorkable
				ELSE
					 '%' + @IsWorkable
				END
		  AND
				(([Workdate] >= @DateTimeFrom OR @DateTimeFrom IS NULL) AND ([Workdate] <= @DateTimeTo OR @DateTimeTo IS NULL))
	 ORDER BY
		  [WorkDate];