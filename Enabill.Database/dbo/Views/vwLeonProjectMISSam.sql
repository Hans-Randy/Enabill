CREATE view [dbo].[vwLeonProjectMISSam]
as
/*------------------------------------------------------
Function : Easy view on MIS. For Sam stuff
Created by: Leon
----------------------------------------------------*/


select
	CTC.Period,U.UserID, Name = U.FirstName + ' ' + U.Lastname, D.DivisionName, D.DivisionCode, U.PayRollRefNo, 
	CLientName = (CASE WHEN WA.ActivityID IN (-1,-2) THEN 'Alacrity' WHEN WA.ActivityID is null then 'Alacrity' ELSE C.CLientName END), 
	ProjectName = (CASE WHEN WA.ActivityID IN (-1,-2) THEN 'Leave OverHead' WHEN WA.ActivityID is null then 'Alacrity' ELSE P.ProjectName END), 
	ActivityName = (CASE WHEN WA.ActivityID = -1 THEN 'Leave' WHEN WA.ActivityID = -2 THEN 'Flexi' WHEN WA.ActivityID is null then 'Not Logged' ELSE A.ActivityName end), 
	RegionName = (Case WHEN ClientName = 'Alacrity' THEN 'Alacrity' WHEN ClientName is null THEN 'Alacrity' ELSE R.RegionName END), 
	DepartmentName = isnull(De.DepartmentName,'General'),
	sum(WA.HoursWorked) as HoursWorked, OA.TotalHours,
	PercentageAllocated = convert(decimal(18,2),(convert(decimal(18,2),sum(WA.HoursWorked)) / convert(decimal(18,2),OA.TotalHours) * 100.00)),
	TotalCost = OA.CTC, 
	AssignedCost = (CASE WHEN WA.ActivityID is null then OA.CTC ELSE ( convert(decimal(18,2),(convert(decimal(18,2),sum(WA.HoursWorked)) / convert(decimal(18,2),OA.TotalHours) * convert(decimal(18,2),OA.CTC)))  ) END)

from
		UserCostToCompanies CTC
JOIN	Users				U	on U.UserID = CTC.UserID
JOIN Divisions				D	on D.DivisionID = U.DivisionID
LEFT JOIN	vwWorkAllocationsWithLeave WA on WA.UserID = CTC.UserID
										and WA.Period = CTC.Period
LEFT JOIn Activities A on A.ActivityID = WA.ActivityID
LEFT JOIN Projects P on P.ProjectID = A.ProjectID
LEFT JOIN Regions R on R.RegionID = P.Regionid
LEFT JOIN PassPhrases PR on 1 = 1
LEFT JOIN Departments De on De.DepartmentID = P.DepartmentID
LEFT JOIN Clients C on C.ClientID = P.CLientID
LEFT JOIN
	(
		select
			CTC.Period,U.UserID, dbo.DeCryptDec(PR.PassPhraseName,CTC.CostToCompany) as CTC, sum(HoursWorked) as TotalHours
		from
				UserCostToCompanies CTC
		JOIN	Users				U	on U.UserID = CTC.UserID
		LEFT JOIN	vwWorkAllocationsWithLeave WA on WA.UserID = CTC.UserID
												and WA.Period = CTC.Period
		LEFT JOIN PassPhrases PR on 1 = 1
		GROUP BY  CTC.Period, dbo.DeCryptDec(PR.PassPhraseName,CTC.CostToCompany), U.UserID
	) OA on OA.UserID = U.UserID
		and OA.Period = CTC.Period
GROUP BY
	WA.ActivityID, U.FirstName, U.LastName, CTC.Period, U.UserID, D.DivisionName,  D.DivisionCode, U.PayRollRefNo,
	C.CLientName, P.ProjectName, A.ActivityName, OA.TotalHours,OA.CTC,R.RegionName, De.DepartmentName
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwLeonProjectMISSam] TO [EnabillReportRole]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[vwLeonProjectMISSam] TO PUBLIC
    AS [dbo];

