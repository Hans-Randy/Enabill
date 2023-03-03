﻿
CREATE VIEW vwTicketsClosedThisMonthForPreviousMonths

--select * from vwTicketsClosedThisMonthForPreviousMonths
AS


-- tickets closed since beginning of the month (received in previous months)

select ClientName, ProjectName, TicketPriorityName,  TicketStatusName, TicketTypeName, TicketReference, Datecreated, DateModified,
	MonthReceived = datename(month,datecreated), YearReceived = year(datecreated), MonthClosed = datename(month, DateModified),
	YearClosed = year(DateModified)
from tickets t
join projects p on p.projectid = t.projectid
join clients c on c.clientid = p.clientid
join ticketstatus ts on ts.ticketstatusid = t.ticketstatus
join TicketTypes tt on tickettypeid = t.tickettype
join TicketPriorities tp on tp.ticketpriorityid = t.priority
where datecreated < convert(datetime,cast(year(getdate()) as varchar) + '/' + cast(month(getdate()) as varchar) + '/01')
and ticketstatus = 5 -- 5 = Closed 
and DateModified >= convert(datetime,cast(year(getdate()) as varchar) + '/' + cast(month(getdate()) as varchar) + '/01')
AND IsDeleted = 0
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwTicketsClosedThisMonthForPreviousMonths] TO PUBLIC
    AS [dbo];

