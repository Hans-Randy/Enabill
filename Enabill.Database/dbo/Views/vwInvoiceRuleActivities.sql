CREATE VIEW [dbo].[vwInvoiceRuleActivities] AS
SELECT C.ClientName,
       P.ProjectName,
       A.ActivityID,
       A.ActivityName,       
       R.InvoiceRuleID,
       R.DateFrom,
       R.DateTo,
       R.ConfirmedEndDate
 FROM dbo.InvoiceRuleActivities RA
  inner join dbo.InvoiceRules R on
  R.InvoiceRuleID = RA.InvoiceRuleID
  inner join dbo.Activities A
  on A.ActivityID = RA.ActivityID
  inner join dbo.Projects P on P.ProjectID = A.ProjectID
  inner join dbo.Clients C on C.CLientID = P.ClientID