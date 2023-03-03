CREATE VIEW dbo.vwPriorPeriodInvoiceOpen AS
SELECT C.ClientName,
       P.ProjectName,
       A.ActivityName,
       IR.InvoiceRuleID
FROM WorkAllocations WA
INNER JOIN Activities A on WA. ActivityID = A.ActivityID
INNER JOIN Projects P on A.ProjectID = A.ProjectID
INNER JOIN Clients C on P.ClientID = C.ClientID
LEFT JOIN InvoiceRules IR on C.ClientID = IR.ClientID and P.BillingMethodID = IR.BillingMethodID
WHERE WA.Period = 201302 and IR.ProjectID is null and IR.Datefrom < '2013-02-01' and (IR.dateto > '2013-02-28' OR IR.dateto IS NULL) AND
WA.InvoiceID is null and
P.BillingmethodID NOT IN (8,16,32) and
IR.invoiceruleid NOT IN
(SELECT InvoiceRuleID FROM Invoices WHERE period <> 201302 )
UNION
SELECT C.ClientName,
       P.ProjectName,
       A.ActivityName,
       IR.InvoiceRuleID
FROM WorkAllocations WA
INNER JOIN Activities A on WA. ActivityID = A.ActivityID
INNER JOIN Projects P on A.ProjectID = A.ProjectID
INNER JOIN Clients C on P.ClientID = C.ClientID
LEFT JOIN InvoiceRules IR on C.ClientID = IR.ClientID and P.BillingMethodID = IR.BillingMethodID and P.ProjectID = IR.ProjectID
WHERE WA.Period = 201302 and IR.ProjectID is not null and IR.Datefrom < '2013-02-01' and (IR.dateto > '2013-02-28' OR IR.dateto IS NULL) AND
WA.InvoiceID is null and
P.BillingmethodID NOT IN (8,16,32) and
IR.invoiceruleid NOT IN
(SELECT InvoiceRuleID FROM Invoices WHERE period <> 201302 )