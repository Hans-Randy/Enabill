

--select * from vwInvoiceWorkAllocationExceptions where invoiceid = 610
CREATE VIEW [dbo].[vwInvoiceWorkAllocationExceptions]
AS
/* ************************************************
	VIEW: Check for and determine workallocation exceptions per invoice
	
************************************************ */
SELECT I.InvoiceID, IR.InvoiceRuleID, A.ActivityID, ActivityName, U.UserID, U.Username, U.FullName, WD.WorkDate, WD.IsWorkable, WSO.HoursWorked, WSO.HoursDiff,
	   Exception =  CASE WHEN WWL.ActivityID IS NULL THEN 'Missing Work Allocation for Activity'
					WHEN WSO.UserID IS NULL THEN 'No Work Session for this day'
					WHEN WSO.HoursDiff != 0 THEN 'Over or Under allocation of work hours' 
					ELSE NULL END
FROM Invoices I
JOIN InvoiceRules IR						ON I.InvoiceRuleID = IR.InvoiceRuleID
JOIN InvoiceRuleActivities IRA				ON IR.InvoiceRuleID = IRA.InvoiceRuleID
JOIN Activities A							ON IRA.ActivityID = A.ActivityID
JOIN UserAllocations UA						ON A.ActivityID = UA.ActivityID
JOIN Users U								ON UA.UserID = U.UserID
JOIN UserRoles UR							ON U.UserID = UR.UserID
JOIN WorkDays WD							ON WD.WorkDate >= I.DateFrom AND WD.WorkDate <= I.DateTo AND WD.IsWorkable = 1
LEFT JOIN vwWorkAllocationsWithLeave WWL	ON U.UserID = WWL.UserID AND WD.WorkDate = WWL.DayWorked --AND A.ActivityID = WWL.ActivityID
LEFT JOIN vwWorkSessionOverview WSO			ON U.UserID = WSO.UserID AND WD.WorkDate = WSO.DayWorked
WHERE UR.RoleID = 8
  AND WD.WorkDate >= U.EmployStartDate
  AND UA.StartDate <= I.DateTo
  AND (UA.ConfirmedEndDate IS NULL OR UA.ConfirmedEndDate > I.DateTo)
  AND (WWL.ActivityID IS NULL OR (WSO.UserID IS NULL AND WWL.ActivityID IS NULL) OR WSO.HoursDiff != 0)

UNION

SELECT I.InvoiceID, IR.InvoiceRuleID, A.ActivityID, ActivityName, U.UserID, U.Username, U.FullName, WD.WorkDate, WD.IsWorkable, WSO.HoursWorked, WSO.HoursDiff,
	   Exception = CASE WHEN WWL.ActivityID IS NULL THEN 'Missing Work Allocation for Activity'
				   WHEN WSO.UserID IS NULL THEN 'No Work Session for this day'
				   WHEN WSO.HoursDiff != 0 THEN 'Over or Under allocation of work hours' 
				   ELSE NULL END
FROM Invoices I								
JOIN InvoiceRules IR						ON I.InvoiceRuleID = IR.InvoiceRuleID
JOIN Activities A							ON IR.ProjectID = A.ProjectID
JOIN UserAllocations UA						ON A.ActivityID = UA.ActivityID
JOIN Users U								ON UA.UserID = U.UserID
JOIN UserRoles UR							ON U.UserID = UR.UserID
JOIN WorkDays WD							ON WD.WorkDate >= I.DateFrom AND WD.WorkDate <= I.DateTo AND WD.IsWorkable = 1
LEFT JOIN vwWorkAllocationsWithLeave WWL	ON U.UserID = WWL.UserID AND WD.WorkDate = WWL.DayWorked --AND A.ActivityID = WWL.ActivityID
LEFT JOIN vwWorkSessionOverview WSO			ON U.UserID = WSO.UserID AND WD.WorkDate = WSO.DayWorked
WHERE UR.RoleID = 8
  AND WD.WorkDate >= U.EmployStartDate
  AND UA.StartDate <= I.DateTo
  AND (UA.ConfirmedEndDate IS NULL OR UA.ConfirmedEndDate > I.DateTo)
  AND (WWL.ActivityID IS NULL OR (WSO.UserID IS NULL AND WWL.ActivityID IS NULL) OR WSO.HoursDiff != 0)
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwInvoiceWorkAllocationExceptions] TO PUBLIC
    AS [dbo];

