
CREATE VIEW [dbo].[vwBillableTimeInvoiced] as
SELECT DISTINCT 
       r.regionName,
       d.departmentName,
       c.clientName,
       p.projectName,
       a.activityName,
       u.username,
       u.fullname,
       b.billingmethodName,
       wa.period,
       wa.dayworked,
       wa.workallocationid,
       wa.hoursworked,
       ua.startdate,
       ua.confirmedenddate,
       ua.chargerate,
       wa.invoiceid,
       i.OrderNo,
       i.Period as InvoicePeriod,
       i.InvoiceAmountExclVat       
FROM workallocations wa
INNER JOIN users u on u.userid = wa.userid
INNER JOIN activities a  on wa.activityid =  a.activityid 
INNER JOIN projects p on p.projectid = a.projectid
INNER JOIN regions r on p.regionid = r.regionid
INNER JOIN departments d on p.departmentid = d.departmentid
INNER JOIN clients c on c.clientid = p.clientid
INNER JOIN billingmethods b  on  p.billingmethodid = b.billingmethodid
INNER JOIN userallocations ua on ua.userid = wa.userid and ua.activityid = wa.activityid
INNER JOIN vwlatestuserallocationEntry uae on uae.userid = ua.userid and uae.activityid = ua.activityid and ua.startdate = uae.startdate
INNER JOIN vwLatestInvoiceRuleEntry ir  on p.projectid =ir.projectid and p.clientid = ir.clientid and p.billingmethodid = ir.billingmethodid
inner join invoices I on I.invoiceID = wa.invoiceid
WHERE wa.invoiceid is not null and 
      billingmethodname not in ('Ad Hoc', 'Non-Billable') and
      ir.projectid is not null 
UNION
SELECT DISTINCT 
       r.regionName,
       d.departmentName,
       c.clientName,
       p.projectName,
       a.activityName,
       u.username,
       u.fullname,
       b.billingmethodName,
       wa.period,
       wa.dayworked,
       wa.workallocationid,
       wa.hoursworked,
       ua.startdate,
       ua.confirmedenddate,
       ua.chargerate,
       wa.invoiceid,
       i.OrderNo,
       i.Period as InvoicePeriod,
       i.InvoiceAmountExclVat
FROM workallocations wa
INNER JOIN users u on u.userid = wa.userid
INNER JOIN activities a  on wa.activityid =  a.activityid 
INNER JOIN projects p on p.projectid = a.projectid
INNER JOIN regions r on p.regionid = r.regionid
INNER JOIN departments d on p.departmentid = d.departmentid
INNER JOIN clients c on c.clientid = p.clientid
INNER JOIN billingmethods b  on  p.billingmethodid = b.billingmethodid
INNER JOIN userallocations ua on ua.userid = wa.userid and ua.activityid = wa.activityid
INNER JOIN vwlatestuserallocationEntry uae on uae.userid = ua.userid and uae.activityid = ua.activityid and ua.startdate = uae.startdate
INNER JOIN vwLatestInvoiceRuleEntry ir  on  p.clientid = ir.clientid and p.billingmethodid = ir.billingmethodid
inner join invoices I on I.invoiceID = wa.invoiceid
WHERE wa.invoiceid is not null and 
      billingmethodname not in ('Ad Hoc', 'Non-Billable') and
      ir.projectid is null