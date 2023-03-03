

CREATE VIEW [dbo].[vwInvoiceRulesWithAllocatedTimeButNoInvoice] AS
SELECT C.ClientName,
       'All' as ProjectName,
        B.BillingMethodName ,
        IR.InvoiceRuleID,
        IR.DateFrom,
        IR.DateTo,
        IR.ConfirmedEndDate,
        WA.WA_ProjectName,
        WA.WA_ActivityName,
        WA.Period,
        sum(WA.HoursWorked) as Hours
FROM InvoiceRules  ir
INNER JOIN clients c on c.clientid = ir.clientid
INNER JOIN billingmethods b on b.billingmethodid = ir.billingmethodid
LEFT OUTER JOIN
(SELECT c.clientid,
        p.projectID, 
        p.projectName as WA_ProjectName,
        p.billingmethodid, 
        workallocationID, 
        a.ActivityName as WA_ActivityName, 
        wa1.userid,
        wa1.dayworked,
        wa1.period, 
        hoursworked 
  FROM Workallocations  wa1
  INNER JOIN Activities a on a.activityid = wa1.activityid
  INNER JOIN Projects p on p.projectid = a.projectid
  INNER JOIN Clients c on c.clientid = p.clientid
  WHERE dayworked between '2013-03-01' and '2013-03-31' and invoiceID is null) WA 
  on c.clientid = wa.clientid  and b.billingmethodid = wa.billingmethodid 
WHERE WA.HoursWorked is not null and datefrom <= '2013-03-31' and (dateto is null or dateto > '2013-03-31') and
ir.projectID is null and
invoiceruleid not in
(select invoiceruleid from invoices where period in (201303))
Group by C.ClientName,
             B.BillingMethodName ,
       IR.InvoiceRuleID,
       IR.DateFrom,
       IR.DateTo,
       IR.ConfirmedEndDate,
       WA_ProjectName,
       WA_ActivityName,
       Period
UNION
SELECT C.ClientName,
       P.ProjectName,
       B.BillingMethodName ,
       IR.InvoiceRuleID,
       IR.DateFrom,
       IR.DateTo,
       IR.ConfirmedEndDate,  
       WA_ProjectName,
       WA_ActivityName,    
       WA.Period,
       sum(WA.HoursWorked) as Hours
FROM invoicerules  ir
INNER JOIN clients c on c.clientid = ir.clientid
INNER JOIN projects p on p.projectid = ir.projectid
INNER JOIN billingmethods b on b.billingmethodid = ir.billingmethodid
LEFT OUTER JOIN 
(SELECT  c.clientid, p.projectID, p.projectName as WA_ProjectName, p.billingmethodid, workallocationID, a.ActivityName as WA_ActivityName, wa1.userid, wa1.dayworked, wa1.period, hoursworked from workallocations  wa1
 INNER join activities a on a.activityid = wa1.activityid
  inner join projects p on p.projectid = a.projectid
  inner join clients c on c.clientid = p.clientid
     where dayworked between '2013-03-01' and '2013-03-31' and invoiceID is null) WA on c.clientid = wa.clientid and p.projectid = wa.projectid and p.billingmethodid = wa.billingmethodid
where WA.HoursWorked is not null and datefrom <= '2013-03-31' and (dateto is null or dateto > '2013-03-31') and
p.projectID is not null and
invoiceruleid not in
(select invoiceruleid from invoices where period in (201303))
Group by C.ClientName,
       ProjectName,
       B.BillingMethodName ,
       IR.InvoiceRuleID,
       IR.DateFrom,
       IR.DateTo,
       IR.ConfirmedEnddate,
       WA_ProjectName,
       WA_ActivityName,
       Period