


/****** Script for SelectTopNRows command from SSMS  ******/
CREATE VIEW [dbo].[vwIndividualLeaveDays] As
SELECT U.userid,
       U.UserName,
	   U.FullName,
       DV.DivisionName,
       LB.BalanceDate,
       LB.Balance as OpeningBalance,
       LB.LeaveTaken as LeaveTaken,
       LB.LeaveCredited,
       LB.ManualAdjustment,
       L.DateFrom as LeavePeriodStartDate,
       L.DateTo as LeavePeriodEndDate,      
       W.WorkDate,  
       L.ApprovalStatus ,
       L.LeaveType as LeaveTypeID,  
       LT.LeaveTypeName, 
       U.WorkHours as NormalHours,
       case when L.NumberOfHours > 0 then L.NumberOfHours/U.WorkHours else U.WorkHours/U.WorkHours end as NumberOfDays  ,   
       case when L.NumberOfHours > 0 then L.NumberOfHours else U.WorkHours end as HoursTaken ,
       L.Remark,
       UM.FirstName + ' ' + UM.LastName as Manager       
FROM [dbo].LeaveBalances LB 
Inner Join [dbo].[Users] U on LB.UserID = U.Userid
Inner Join [dbo].[Leaves] L  on U.UserID = L.UserID
Inner Join [dbo].[WorkDays] W On
W.WorkDate >= L.DateFrom and   W.WorkDate <= L.DateTo 
Inner Join [dbo].[LeaveTypes] LT on L.LeaveType = LT.LeaveTypeID
Inner Join [dbo].[Users] UM on U.ManagerID = UM.UserID INNER JOIN
dbo.Divisions DV ON U.DivisionID = DV.DivisionID
Where W.IsWorkable = 1 and  LB.LeaveType = L.LeaveType
and U.IsActive = 1