

CREATE VIEW [dbo].[vwWorkableDaysPerPeriod] AS
SELECT CAST(CAST(year([WorkDate]) as varchar(4)) + CASE LEN(CAST(MONTH(workdate) as varchar(2))) WHEN 1 THEN '0' + CAST(MONTH(workdate) as varchar(2)) ELSE CAST(MONTH(workdate) as varchar(2)) END as int) as Period ,
       count([IsWorkable]) AS NumberOfDays
FROM [dbo].[WorkDays]
WHERE IsWorkable = 1
GROUP BY year([WorkDate]), MONTH(workdate)