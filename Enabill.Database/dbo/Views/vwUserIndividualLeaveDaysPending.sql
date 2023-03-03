
CREATE VIEW [dbo].[vwUserIndividualLeaveDaysPending] As
SELECT U.userid,
       U.UserName,
       U.FullName,
       L.DateFrom as LeavePeriod_StartDate,
       L.DateTo as LeavePeriod_EndDate,      
       W.WorkDate,     
       LT.LeaveTypeName, 
       U.WorkHours as NormalWorkHours,
       case when L.NumberOfHours > 0 then L.NumberOfHours/U.WorkHours else U.WorkHours/U.WorkHours end as NrOfDays  ,   
       case when L.NumberOfHours > 0 then L.NumberOfHours else U.WorkHours end as HoursTaken ,
       L.Remark        
  FROM [dbo].[Leaves] L inner join 
       [dbo].[WorkDays] W On
  W.WorkDate >= L.DateFrom and   W.WorkDate <= L.DateTo
  Inner Join [dbo].[Users] U on L.UserID = U.Userid
  Inner join [dbo].[LeaveTypes] LT on L.LeaveType = LT.LeaveTypeID
  Where   W.IsWorkable = 1 and [ApprovalStatus] = 1 and L.leaveType = 1
  and W.WorkDate >= '2012-04-01' and W.WorkDate <= '2012-04-30'