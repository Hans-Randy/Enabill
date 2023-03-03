


CREATE VIEW [dbo].[vwUserTimeSplitTotalHoursPerPeriod] AS
SELECT DepartmentName + '_' + convert(varchar(5),UserID) + '_' + convert(varchar(4),[Period]) as UKey,
       DepartmentName,
       UserID,
       UserName,
	   FullName,
       [Period],
       Sum(HoursWorked) as TotalHours
FROM [dbo].[vwUserTimeSplit]
GROUP BY DepartmentName, UserID, FullName, UserName, [Period]