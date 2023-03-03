
CREATE VIEW vwTicketsAllParked

--select * from vwTicketsAllParked
AS


-- all parked tickets
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
where IsDeleted = 0
AND TicketStatus = 6 -- parked
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwTicketsAllParked] TO PUBLIC
    AS [dbo];

