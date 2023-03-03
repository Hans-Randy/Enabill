CREATE PROCEDURE dbo.GetNoOfWorkDays 
		  @DateFrom DATE,
		  @DateTo DATE,
		  @IsWorkday VARCHAR(1) = '',
		  @FromSQL INT = 1 -- True if called from SQL. False if called from C#
AS

/*
	 =============================================
	 Author:			  <Nico van der Walt>
	 Create date:	  <10/04/2017>
	 Description:	  <Get number of workdays in a period>
	 =============================================

	 IF OBJECT_ID('dbo.GetNoOfWorkDays') IS NOT NULL
		  EXEC ('DROP PROCEDURE dbo.GetNoOfWorkDays;')
	 GO
	
	 EXEC dbo.GetNoOfWorkDays
		  @DateFrom = '2017-01-01'
		  ,@DateTo = '2017-01-31'
		  ,@IsWorkday = 1
*/

	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	 SET NOCOUNT ON

	 IF @FromSQL = 1
		  BEGIN
				DECLARE @Total FLOAT
				
				SELECT
					 @Total = COUNT(IsWorkable)
				FROM
					 dbo.WorkDays
				WHERE
					 WorkDate >= @DateFrom
					 AND
						  WorkDate <= @DateTo
					 AND
						  IsWorkable LIKE
						  CASE WHEN IsNumeric(@IsWorkday) = 1 THEN  
								@IsWorkday
						  ELSE
								'%' + @IsWorkday
						  END

				RETURN @Total
		  END
	 ELSE
		  BEGIN
				SELECT
					 COUNT(IsWorkable)
				FROM
					 dbo.WorkDays
				WHERE
					 WorkDate >= @DateFrom
					 AND
						  WorkDate <= @DateTo
					 AND
						  IsWorkable LIKE
						  CASE WHEN IsNumeric(@IsWorkday) = 1 THEN  
								@IsWorkday
						  ELSE
								'%' + @IsWorkday
						  END
		  END