
CREATE VIEW [dbo].[vwLatestInvoiceRuleEntry] AS
SELECT IR.BillingMethodID, IR.ClientID, IR.ProjectID, max(IR.DateFrom) AS DateFrom                   
FROM InvoiceRules IR 
WHERE IR.DateFrom < getdate()
GROUP BY IR.InvoiceRuleID, IR.BillingMethodID, IR.ClientID, IR.ProjectID