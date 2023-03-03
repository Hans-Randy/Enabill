
CREATE VIEW [dbo].[vwActivityNotLinkedToInvoiceRule] AS
SELECT DISTINCT R.RegionName,
       D.DepartmentName,
       C.ClientName,
       P.ProjectName,
       A.ActivityID,
       A.ActivityName,
       B.BillingMethodName,
       U.UserName,       
       U.FullName,       
       WA.[Period],
       Sum(WA.HoursWorked) as HoursWorked,
       'Activity Not linked to Invoice Rule' as "Exception"      
FROM Workallocations WA 
INNER JOIN Activities A on WA.ActivityID = A.ActivityID
INNER JOIN Projects P on P.ProjectID = A.ProjectID
INNER JOIN Regions R on R.RegionID = P.RegionID 
INNER JOIN Departments D on D.DepartmentID = P.DepartmentID
INNER JOIN Clients C on C.ClientID = P.ClientID
INNER JOIN BillingMethods B on B.BillingMethodID = 
P.BillingMethodID AND P.BillingmethodID NOT IN (8,16,32)
INNER JOIN Users U on WA.UserID = U.UserID
WHERE WA.InvoiceID IS NULL AND WA.Period = 201302 AND
WA.ActivityID NOT IN (SELECT IRA.ActivityID from InvoiceRuleActivities IRA
    INNER JOIN InvoiceRules IR on IRA.InvoiceRuleID = IR.InvoiceRuleID
    WHERE IR.Datefrom < '2013-02-01' and (IR.dateto > '2013-02-28' OR IR.dateto IS NULL))
GROUP BY  R.RegionName,
          D.DepartmentName,
          C.ClientName,
          P.ProjectName,
          A.ActivityID,
          A.ActivityName,
          B.BillingMethodName,
          U.UserName,       
          U.FullName,       
          WA.[Period]