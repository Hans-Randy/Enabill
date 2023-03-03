create view dbo.vwLeonProjectMISProv
as
/*-------------------------------
Function : Provisional Invoice Amounts
Created by : Leon
------------------------------*/


Select
	R.RegionName, D. DepartmentName, C.ClientName, PM.ProjectName, PM.ProjectID,
	 I.Period, sum(ProvisionalAccrualAmount) as AccrualAmount,
	sum(ProvisionalIncomeAmount)as CashAmount,
	CTCClean  = ''
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
    ON OBJECT::[dbo].[vwLeonProjectMISProv] TO [EnabillReportRole]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[vwLeonProjectMISProv] TO PUBLIC
    AS [dbo];

