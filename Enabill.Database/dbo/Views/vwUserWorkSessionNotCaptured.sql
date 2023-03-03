

CREATE VIEW [dbo].[vwUserWorkSessionNotCaptured] AS
SELECT DISTINCT U.UserID ,
       U.UserName,
	   U.FullName,
       D.DivisionName,
       R.RegionName,
       'Department' AS DepartmentName,
       'Alacrity' AS ClientName,
       'OverHeads' AS ProjectName,
       'Exceptions' AS ActivityName,
       convert(char(4),Year(WD.WorkDate)) + case when len(convert(char(2),Month(WD.WorkDate))) < 2 then '0' + convert(char(2),Month(WD.WorkDate)) else convert(char(2),Month(WD.WorkDate))end as Period,
       WD.WorkDate AS DayWorked,      
       U.WorkHours * -1 as HoursWorked 
FROM WorkDays WD
CROSS JOIN Users U
INNER JOIN Divisions D ON U.DivisionID = D.DivisionID
INNER JOIN Regions R ON U.RegionID  = R.RegionID
WHERE WD.Workdate >= U.EmployStartDate AND
      WD.WorkDate < getdate() AND
      isWorkable = 1 AND
      U.IsActive = 1 AND
      WD.Workdate NOT IN
(SELECT Dayworked FROM vwWorkAllocationsWithLeave)