CREATE view dbo.vwLeonProjectMIS
as
/*------------------------------------------------------
Function : Easy view on MIS. Leave it alone!!!
Created by: Leon
----------------------------------------------------*/

select 
	Type = ' 1.Hours Worked', R.RegionName, D. DepartmentName, C.ClientName, P.ProjectName, P.ProjectID, 
	Name = U.FirstName + ' ' + U.LastName, 
	W.Period, 
	sum(W.HoursWorked) as HoursCOSTTurn
from
	WorkAllocations W
JOIN Users U on U.UserID = W.UserID
JOIN Activities A on A.ActivityID = W.ActivityID
JOIN Projects P on P.ProjectID = A.ProjectID
JOIN Regions R on R.RegionID = P.RegionID
JOIN Departments D on D.DepartmentID = P.DepartmentID
JOIN Clients C on C.ClientID = P.ClientID
GROUP BY
	R.RegionName, D. DepartmentName, C.ClientName, P.ProjectName, P.ProjectID, 
	U.FirstName, U.LastName, U.UserID, W.Period
UNION ALL
select 
	Type = ' 2. Cost to Company', R.RegionName, D. DepartmentName, C.ClientName, P.ProjectName, P.ProjectID, 
	Name = U.FirstName + ' ' + U.LastName, W.Period, 
	(sum(W.HoursWorked)/145.55*50000.00*1.33) as HoursCOSTTurn
from
	WorkAllocations W
JOIN Users U on U.UserID = W.UserID
LEFT JOIN dbo.UserCostToCompanies UC on U.UserID = UC.UserID
									and UC.Period = W.Period
JOIN Activities A on A.ActivityID = W.ActivityID
JOIN Projects P on P.ProjectID = A.ProjectID
JOIN Regions R on R.RegionID = P.RegionID
JOIN Departments D on D.DepartmentID = P.DepartmentID
JOIN Clients C on C.ClientID = P.ClientID
GROUP BY
	R.RegionName, D. DepartmentName, C.ClientName, P.ProjectName, P.ProjectID, U.FirstName, 
	U.LastName, U.UserID, W.Period
UNION ALL
Select
	Type = '3. Turnover', R.RegionName, D. DepartmentName, C.ClientName, PM.ProjectName, PM.ProjectID, 
	Name = 'Invoices', I.Period, sum(AccrualExclVat) as HoursCOSTTurn
from
	 Invoices I
JOIN Clients C on C.CLientID = I.ClientID
left JOIN Projects P on P.ProjectID = I.ProjectID
LEFT JOIN InvoiceRuleactivities IR on IR.InvoiceRUleActivityID = (select top 1 INvoiceRUleActivityID
																	from InvoiceRuleactivities
																	where InvoiceRuleID = I.InvoiceRUleID)
LEFT JOIN Activities A on A.ActivityID = IR. ActivityID
LEFT JOIN Projects P2 on P2.ProjectID = A.ProjectID
LEFT JOIN Projects P3 on P3.ProjectID = (select top 1 ProjectID from Projects where clientID = C.CLientID)
JOIN Projects PM on PM.ProjectID = isnull(isnull(P.ProjectID,P2.ProjectID),P3.ProjectID)
JOIN Regions R on R.RegionID = PM.RegionID
JOIN Departments D on D.DepartmentID = PM.DepartmentID
GROUP BY
	R.RegionName, D. DepartmentName, C.ClientName, PM.ProjectName, PM.ProjectID, I.Period
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwLeonProjectMIS] TO PUBLIC
    AS [dbo];

