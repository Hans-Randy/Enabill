
CREATE VIEW vwTicketsReceivedSinceStartOfMonth

--select * from vwTicketsReceivedSinceStartOfMonth
AS


-- tickets received since beginning of the month

select ClientName, ProjectName, TicketPriorityName,  TicketStatusName, TicketTypeName, TicketReference, Datecreated,
	MonthReceived = datename(month,datecreated), YearReceived = year(datecreated)
from tickets t
join projects p on p.projectid = t.projectid
join clients c on c.clientid = p.clientid
join ticketstatus ts on ts.ticketstatusid = t.ticketstatus
join TicketTypes tt on tickettypeid = t.tickettype
join TicketPriorities tp on tp.ticketpriorityid = t.priority
where datecreated >= convert(datetime,cast(year(getdate()) as varchar) + '/' + cast(month(getdate()) as varchar) + '/01')
AND IsDeleted = 0
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwTicketsReceivedSinceStartOfMonth] TO PUBLIC
    AS [dbo];

