


CREATE VIEW vwTicketsOutstandingPerPriority

AS
--select * from vwTicketsOutstandingPerPriority


--weekly report:  number of tickets outstanding per priority. Izaan. 20 March 2013
select ClientName, ProjectName, TicketPriorityName,  TicketStatusName, TicketTypeName, TicketReference, DateCreated
from tickets t
join projects p on p.projectid = t.projectid
join clients c on c.clientid = p.clientid
join ticketstatus ts on ts.ticketstatusid = t.ticketstatus
join TicketTypes tt on tickettypeid = t.tickettype
join TicketPriorities tp on tp.ticketpriorityid = t.priority
where ticketstatus not in (4, 5) -- 4 = Resolved; 5 = Closed
AND IsDeleted = 0
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwTicketsOutstandingPerPriority] TO [EnabillReportRole2]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[vwTicketsOutstandingPerPriority] TO PUBLIC
    AS [dbo];

