-- =============================================
-- Author:		Gavin van Gent
-- Create date: 2011-04-08
-- Description:	Script which calculates which days are workable and which are not, ie, weekends and public holidays
-- =============================================
CREATE PROCEDURE [dbo].[Calendar_USP_WorkableDays]
AS
BEGIN
	SET NOCOUNT ON;
	
	/*------**********************************************************************************------

	This script calculates the days that are workable from the beginning of 2011 to the end of 2019	

	------**********************************************************************************------*/


	DECLARE @Date DATETIME

	------**********************************************************************************------
	------GET ALL THE DATES FROM 2011-01-01 and 2020-01-01

	SELECT @Date = '2011-01-01'

	WHILE (@Date < CONVERT(DATETIME,'2020-01-01'))
	BEGIN
		INSERT INTO WorkDays (WorkDate, IsWorkable)
		SELECT @Date, CASE WHEN (DATEPART(DW, @DATE) = 1 OR DATEPART(DW,@Date) = 7)THEN 0 ELSE 1 END

		SELECT @Date = DATEADD(D, 1, @Date)
	END

	------**********************************************************************************------
	--GET THE Holidays INTO A TEMPTABLE

	DECLARE @Holidays TABLE (WDay DATETIME)

	--NEW YEARS
	INSERT INTO @Holidays (WDay)
	SELECT WorkDate
	FROM WorkDays
	WHERE MONTH(WorkDate) = 1 AND DAY(WorkDate) = 1

	--HUMAN RIGHTS DAY
	INSERT INTO @Holidays (WDay)
	SELECT WorkDate
	FROM WorkDays
	WHERE MONTH(WorkDate) = 3 AND DAY(WorkDate) = 21

	--FREEDOM DAY
	INSERT INTO @Holidays (WDay)
	SELECT WorkDate
	FROM WorkDays
	WHERE MONTH(WorkDate) = 4 AND DAY(WorkDate) = 27

	--WORKER'S DAY
	INSERT INTO @Holidays (WDay)
	SELECT WorkDate
	FROM WorkDays
	WHERE MONTH(WorkDate) = 5 AND DAY(WorkDate) = 1

	--YOUTH DAY
	INSERT INTO @Holidays (WDay)
	SELECT WorkDate
	FROM WorkDays
	WHERE MONTH(WorkDate) = 6 AND DAY(WorkDate) = 16

	--WOMAN'S DAY
	INSERT INTO @Holidays (WDay)
	SELECT WorkDate
	FROM WorkDays
	WHERE MONTH(WorkDate) = 8 AND DAY(WorkDate) = 9

	--HERITAGE DAY
	INSERT INTO @Holidays (WDay)
	SELECT WorkDate
	FROM WorkDays
	WHERE MONTH(WorkDate) = 9 AND DAY(WorkDate) = 24

	--DAY OF RECONCILIATION
	INSERT INTO @Holidays (WDay)
	SELECT WorkDate
	FROM WorkDays
	WHERE MONTH(WorkDate) = 12 AND DAY(WorkDate) = 16

	--CHRISTMAS DAY
	INSERT INTO @Holidays (WDay)
	SELECT WorkDate
	FROM WorkDays
	WHERE MONTH(WorkDate) = 12 AND DAY(WorkDate) = 25

	--DAY OF GOODWILL
	INSERT INTO @Holidays (WDay)
	SELECT WorkDate
	FROM WorkDays
	WHERE MONTH(WorkDate) = 12 AND DAY(WorkDate) = 26

	------**********************************************************************************------

	----SETTING EASTER HOLIDAYS

	--GOOD FRIDAY
	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2011-04-22')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2012-04-06')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2013-03-29')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2014-04-18')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2015-04-03')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2016-03-25')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2017-04-14')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2018-03-30')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2019-04-19')



	--EASTER MONDAY
	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2011-04-25')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2012-04-09')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2013-04-01')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2014-04-21')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2015-04-06')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2016-03-28')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2017-04-14')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2018-04-02')

	INSERT INTO @Holidays (WDay)
	SELECT CONVERT(DATETIME, '2019-04-22')

	------**********************************************************************************------

	DECLARE @HDate DATETIME

	WHILE ((SELECT COUNT(*) FROM @Holidays) > 0)
	BEGIN
		SELECT @HDate = (SELECT TOP 1 WDay FROM @Holidays)

		IF (DATEPART(DW, @HDate) = 1)
		BEGIN
			UPDATE WorkDays
			SET IsWorkable = 0
			WHERE WorkDate = DATEADD(D, 1, @HDate)
		END
		ELSE
		BEGIN
			UPDATE WorkDays
			SET IsWorkable = 0
			WHERE WorkDate = @HDate
		END

		DELETE FROM @Holidays
		WHERE WDay = @HDate
	END
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Calendar_USP_WorkableDays] TO PUBLIC
    AS [dbo];

