

CREATE VIEW [dbo].[vwUserWorkAllocationExceptions] AS
SELECT DISTINCT U.UserID ,
                U.UserName,
				U.FullName,
                D.DivisionName,
                R.RegionName,
               'Department' AS DepartmentName,
               'Alacrity' AS ClientName,
               'OverHeads' AS ProjectName,
               'Exceptions' AS ActivityName,
               convert(char(4),Year(wd.WorkDate)) + case when len(convert(char(2),Month(wd.WorkDate))) < 2 then '0' + convert(char(2),Month(wd.WorkDate)) else convert(char(2),Month(wd.WorkDate))end as Period,
               wd.WorkDate AS DayWorked,      
               abs(ws.HoursDiff) AS HoursWorked 
FROM Users U
INNER JOIN Divisions D ON U.DivisionID = D.DivisionID
INNER JOIN Regions R ON U.RegionID  = R.RegionID
INNER JOIN UserRoles ur ON u.UserID = ur.UserID
INNER JOIN vwWorkAllocationsWithLeave wa ON wa.UserID= u.UserID
INNER JOIN WorkDays wd ON wa.DayWorked = wd.WorkDate
INNER JOIN vwWorkSessionOverview ws ON ws.DayWorked = wd.WorkDate AND ws.UserID = u.Userid
WHERE wd.IsWorkable = 1 AND
      ur.RoleID = 8 AND
      u.IsActive = 1 AND
      ws.HoursDiff <> 0 
UNION
SELECT DISTINCT UserID ,
       UserName,
	   FullName,
       DivisionName,
       RegionName,
       DepartmentName,
       ClientName,
       ProjectName,
       ActivityName,
       Period,
       DayWorked,      
       abs(HoursWorked)
FROM   vwUserWorkSessionNotCaptured