


CREATE VIEW [dbo].[vwInvoiceRuleInactiveButUserAllocationActive]  as
SELECT Distinct R.RegionName,
       D.DepartmentName,
       C.ClientName,
       P.ProjectName,
       A.ActivityName,
       B.BillingMethodName,       
       IR.InvoiceRuleID,
       IR.OrderNo,
       IR.DateFrom as "IR DateFrom",
       IR.DateTo as "IR DateTo",
       IR.InvoiceAmountExclVAT,
       U.USerid,
       A.ActivityID,
       U.Username,
       U.Fullname,
       UA.StartDate,
       UA.ConfirmedEndDate,
       UA.ChargeRate,
       WA.[Period],      
       sum(WA.HoursWorked ) as HoursWorked
FROM  invoicerules IR inner join
vwLatestInvoiceRuleEntry lir on ir.BillingMethodID = lir.BillingMethodID and ir.clientid = lir.clientid and ir.projectid = lir.projectid and ir.DateFrom = lir.Datefrom
inner join clients c on c.clientID = IR.clientID
inner join projects p on p.clientid = c.clientid
inner join Regions r on p.regionid = r.regionid
inner join Departments d on d.departmentid = p.departmentid
inner join BillingMethods B on p.billingmethodid = b.billingmethodid
inner join activities a on p.projectid = a.projectid
inner join workallocations wa on wa.activityid = a.activityid
inner join users u on u.Userid = wa.userid
inner join (Select ua1.userid, ua1.activityid, ua1.startdate, ua1.confirmedenddate, ua1.chargerate
            from Userallocations ua1 inner join
            dbo.vwLatestUserAllocationEntry  lue on ua1.userid = lue.userid and
            ua1.activityid = lue.activityid and
            ua1.startdate = lue.startdate
            )ua on ua.userid = u.userid and ua.activityid = a.activityid

where wa.invoiceid is null and
 ir.dateto is not null and
ua.StartDate >= ir.datefrom and
wa.period = 201302 and
wa.dayworked > IR.dateTo and
wa.dayworked >= ua.startdate and
ir.dateto is not null and
(ua.confirmedenddate is null  or ua.confirmedenddate > '2013-02-28') and b.billingmethodid not in (8,16,32)
GROUP BY R.RegionName,
       D.DepartmentName,
       C.ClientName,
       P.ProjectName,
       A.ActivityName,
       B.BillingMethodName,       
       IR.InvoiceRuleID,
       IR.OrderNo,
       IR.DateFrom,
       IR.DateTo,
       IR.InvoiceAmountExclVAT,
       U.Userid,
       A.ActivityID,
       U.Username,
       U.Fullname,
       UA.StartDate,
       UA.ConfirmedEndDate,
       UA.ChargeRate,
       WA.[Period]