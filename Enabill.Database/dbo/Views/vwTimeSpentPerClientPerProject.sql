
create view [dbo].[vwTimeSpentPerClientPerProject]
/*------------------------------------------------------------------------------------
Modified By:	Izaan Mobey
ON:				12 April 2013
Reason:			Changed the where clause to be more dynamic. Was hardcoded to 201302.
--------------------------------------------------------------------------------------*/

as



--- get all projects

SELECT TOP 100 PERCENT
	ClientName,
	Period,
--	AccountCode,
--	SupportEmailAddress,
	ProjectName,
--	ProjectCode,
	ProjectDesc,
--	BillingMethodName,
	Hours = SUM(Hours)
FROM (
	SELECT 
		ClientName,
		FinPeriodID as Period,
		AccountCode,
		SupportEmailAddress = ISNULL( P.SupportEmailAddress,C.SupportEmailAddress),
		ProjectName,
		ProjectCode,
		ProjectDesc,
		BillingMethodName,
		Hours = CASE WHEN W.Period IS NULL THEN 0 ELSE SUM(HoursWorked) END
	FROM 
		Clients C
	JOIN Projects P on P.ClientID = C.ClientID
	JOIN BillingMethods B ON B.BillingMethodID = P.BillingMethodID
	JOIN Activities A ON A.ProjectID = P.ProjectID
	CROSS JOIN FinPeriods F 								
	LEFT OUTER JOIN WorkAllocations W ON W.ActivityID = A.ActivityID
										AND W.Period = F.FinPeriodID
	WHERE 
		P.RegionID = 1 --cpt
	AND C.IsActive = 1
	AND (P.DeactivatedDate is null OR P.DeactivatedDate >= getdate())
	AND FinPeriodID >= 201301 
	and FinPeriodID <= (SELECT FinPeriodID + 1 FROM FinPeriods WHERE IsCurrent = 1)
	GROUP BY
		ClientName,
		FinPeriodID, 
		AccountCode,
		ISNULL( P.SupportEmailAddress,C.SupportEmailAddress),
		ProjectName,
		ProjectCode,
		ProjectDesc,
		BillingMethodName,
		W.Period
	)BASE 
GROUP BY
	ClientName,
	Period,
--	AccountCode,
--	SupportEmailAddress,
	ProjectName,
--	ProjectCode,
	ProjectDesc--,
--	BillingMethodName

ORDER BY
	ClientName,
	Period,
	ProjectName
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwTimeSpentPerClientPerProject] TO [EnabillReportRole2]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[vwTimeSpentPerClientPerProject] TO PUBLIC
    AS [dbo];

