
CREATE VIEW vwTicketsAllDeleted

--select * from vwTicketsAllDeleted
AS


-- all deleted tickets
select 
	ClientName, ProjectName, TicketPriorityName,  TicketStatusName, TicketTypeName, TicketReference, Datecreated,
	MonthReceived = datename(month,datecreated), YearReceived = year(datecreated), MonthClosed = datename(month, DateModified),
	YearClosed = year(DateModified)	
from tickets t
left outer join projects p on p.projectid = t.projectid
left outer join clients c on c.clientid = p.clientid
join ticketstatus ts on ts.ticketstatusid = t.ticketstatus
join TicketTypes tt on tickettypeid = t.tickettype
join TicketPriorities tp on tp.ticketpriorityid = t.priority
where IsDeleted = 1
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwTicketsAllDeleted] TO PUBLIC
    AS [dbo];

