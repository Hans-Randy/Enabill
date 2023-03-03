CREATE VIEW [dbo].[vwInvoicesWithoutForecasts] AS
SELECT DISTINCT I.InvoiceID,
                I.InvoiceRuleID,
                I.Period,                
                B.BillingMethodName,
			    I.ClientName,
			    R.RegionID,
			    R.RegionName,
			    I.OrderNo,
			    I.ProvisionalAccrualAmount,       
                I.AccrualExclVAT
FROM dbo.Invoices I INNER JOIN dbo.BillingMethods B
ON I.BillingMethodID = B.BillingMethodID 
LEFT JOIN InvoiceRuleactivities IR on IR.InvoiceRUleActivityID = (select top 1 INvoiceRUleActivityID
																	from InvoiceRuleactivities
																	where InvoiceRuleID = I.InvoiceRUleID)
LEFT JOIN Activities A on A.ActivityID = IR. ActivityID
LEFT JOIN Projects P2 on P2.ProjectID = A.ProjectID
LEFT JOIN Projects P3 on P3.ProjectID = (select top 1 ProjectID from Projects where clientID = I.CLientID)
JOIN Projects PM on PM.ProjectID = isnull(isnull(P2.ProjectID,P2.ProjectID),P3.ProjectID)
LEFT OUTER JOIN Regions R on PM.RegionID = R.RegionID																	
WHERE I.InvoiceID NOT IN 
(SELECT InvoiceID FROM dbo.ForecastInvoiceLinks)