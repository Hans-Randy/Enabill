


CREATE view [dbo].[vwLeonProjectMIS2]
as
/*------------------------------------------------------
Function : Easy view on MIS. Leave it alone!!!
Created by: Leon
----------------------------------------------------*/

select     
	Type = ' 1.Hours Worked',
	R.RegionName,
	D. DepartmentName, 	
	ClientName = (CASE WHEN C.ClientName = 'Gallo ME' THEN 'Gallo Images' else C.ClientName end), 
	ProjectName = (CASE WHEN C.CLientID = 1 THEN P.ProjectName + ' - ' + A.ActivityName
											ELSE P.ProjectName END),
	P.ProjectID, 	
	Name = U.FirstName + ' ' + U.LastName, 
	W.Period, 
	sum(W.HoursWorked) as HoursWorked,
	CTC = '', 
	Turnover = '', 
	CTCClean = ''
from
	vwWorkAllocationsWithLeave W
JOIN Users U on U.UserID = W.UserID
JOIN Activities A on A.ActivityID = W.ActivityID
JOIN Projects P on P.ProjectID = A.ProjectID
JOIN Regions R on R.RegionID = P.RegionID
JOIN Departments D on D.DepartmentID = P.DepartmentID
					--	and D.DepartmentID in(1,2,4,16)
JOIN Clients C on C.ClientID = P.ClientID
			--	and C.ClientID <> 1
GROUP BY
	R.RegionName, 
	D. DepartmentName,	
	C.ClientName, 
    (CASE WHEN C.CLientID = 1 THEN P.ProjectName + ' - ' + A.ActivityName
											ELSE P.ProjectName END),
    P.ProjectID, 
	U.FirstName, 
	U.LastName, 	
	W.Period
UNION ALL
select    
	Type = ' 2. Cost to Company',
	R.RegionName, 
	D. DepartmentName, 	
	ClientName = (CASE WHEN  C.ClientName = 'Gallo ME' THEN 'Gallo Images' else C.ClientName end), 
	ProjectName = (CASE WHEN C.CLientID = 1 THEN P.ProjectName + ' - ' + A.ActivityName
											ELSE P.ProjectName END),
	P.ProjectID, 	
	Name = U.FirstName + ' ' + U.LastName, W.Period, 
	HoursWorked = '', (sum(W.HoursWorked)/TH.TotalHours*
								isnull(dbo.DeCryptDec(PR.PassPhraseName,UC.CostToCompany),0) *1.33) as CTC, Turnover = '',
								CTCClean = (sum(W.HoursWorked)/TH.TotalHours*isnull(dbo.DeCryptDec(PR.PassPhraseName,UC.CostToCompany),0) *1)
from
	vwWorkAllocationsWithLeave W
JOIN Users U on U.UserID = W.UserID
LEFT JOIN dbo.UserCostToCompanies UC on U.UserID = UC.UserID
									and UC.Period = W.Period
JOIN Activities A on A.ActivityID = W.ActivityID
JOIN Projects P on P.ProjectID = A.ProjectID
JOIN Regions R on R.RegionID = P.RegionID
LEFT JOIN PassPhrases PR on 1 = 1
JOIN Departments D on D.DepartmentID = P.DepartmentID
					--and D.DepartmentID in(1,2,4,16) 
JOIN Clients C on C.ClientID = P.ClientID
	--	and C.ClientID <> 1
LEFT JOIN
	(
		Select W.UserID, W.Period, sum(W.HoursWorked) as TotalHours
		from
			vwWorkAllocationsWithLeave W
		GROUP By UserID, Period
			
	) TH on TH.UserID = W.userID
		and TH.Period = W.Period

GROUP BY
	R.RegionName, 
	D. DepartmentName,
	PR.PassPhraseName,	
	C.ClientName,
	(CASE WHEN C.CLientID = 1 THEN P.ProjectName + ' - ' + A.ActivityName
											ELSE P.ProjectName END),
	P.ProjectID,	
	U.FirstName, 
	U.LastName,
	W.Period, 
	TH.UserID, 
	TH.Period, 
	TH.TotalHours, 
	UC.CostToCompany
UNION ALL
Select   
	Type = '3. Turnover',
	R.RegionName, 
	D. DepartmentName,	
	ClientName = (CASE WHEN C.ClientName =  'Gallo ME' THEN 'Gallo Images' else C.ClientName end),
	PM.ProjectName, 
	PM.ProjectID,	
	Name = 'Invoices', 
	I.Period, 
	HoursWorked = '', 
	CTC = '', 
	sum(AccrualExclVat) as Turnover,
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
--where
	--I.InvoiceStatusID in(4,8)
GROUP BY
	R.RegionName, 
	D. DepartmentName,	
	C.ClientName, 
	P.ProjectID,
	PM.ProjectName, 
	PM.ProjectID, 
	I.Period
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwLeonProjectMIS2] TO [EnabillReportRole]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[vwLeonProjectMIS2] TO PUBLIC
    AS [dbo];

