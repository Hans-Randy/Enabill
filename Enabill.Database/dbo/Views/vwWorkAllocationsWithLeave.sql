--select * from vwWorkAllocationsWithLeave where UserID = 49 ORDER BY DayWorked
CREATE view [dbo].[vwWorkAllocationsWithLeave]
as
/*---------------------------------
Function : Work Allocations with Leave Records
Created by : Leon
----------------------------------------*/


SELECT UserID, ActivityID, DayWorked, Period, HoursWorked, Remark
FROM WorkAllocations WA

UNION ALL

SELECT 
		U.UserID,
		-1 as ActivityID,
     
       W.WorkDate as DayWorked,     
       Period = CONVERT(varchar,YEAR(W.Workdate)) +  
				CASE WHEN LEN(CONVERT(varchar,MONTH(W.Workdate))) = 1 
					 THEN ('0' + (CONVERT(varchar,MONTH(W.Workdate))))
					 ELSE (CONVERT(varchar,MONTH(W.Workdate))) END,        
      CASE WHEN L.NumberOfHours > 0 THEN L.NumberOfHours ELSE U.WorkHours END AS HoursWorked ,
      L.Remark        
  FROM Leaves L 
  INNER JOIN WorkDays W		ON W.WorkDate >= L.DateFrom AND W.WorkDate <= L.DateTo
  INNER JOIN Users U		ON L.UserID = U.Userid
  --INNER JOIN LeaveTypes	LT	ON L.LeaveType = LT.LeaveTypeID
  WHERE W.IsWorkable = 1 
	AND L.ApprovalStatus = 4

UNION ALL

SELECT
		U.UserID,
		-2 as ActivityID,
		
		W.WorkDate AS DayWorked,
       Period = CONVERT(varchar,YEAR(W.Workdate)) +  
				CASE WHEN LEN(CONVERT(varchar,MONTH(W.Workdate))) = 1 
					 THEN ('0' + (CONVERT(varchar,MONTH(W.Workdate))))
					 ELSE (CONVERT(varchar,MONTH(W.Workdate))) END,        
      8 AS HoursWorked ,
      F.Remark        
				
	FROM FlexiDays F
	INNER JOIN WorkDays W ON W.WorkDate >= F.FlexiDate AND W.WorkDate <= F.FlexiDate
	INNER JOIN Users U ON F.UserID = U.UserID
	WHERE W.IsWorkAble = 1
	  AND F.ApprovalStatusID = 4
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwWorkAllocationsWithLeave] TO PUBLIC
    AS [dbo];

