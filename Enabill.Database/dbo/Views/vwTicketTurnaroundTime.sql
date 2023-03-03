
CREATE VIEW [dbo].[vwTicketTurnaroundTime]
/*-------------------------------------------
Modified By:	Izaan Mobey
On:				26 Aug 2013
Reason:			Exclude 'Parked' tickets
-------------------------------------------*/

-- select * from vwTicketTurnaroundTime

AS

-- turnaround times per client
select 
	ClientName, ProjectName, TicketPriorityName,  TicketStatusName, TicketTypeName, TicketReference, Datecreated, DateModified,
	LeadTime = datediff(day, Datecreated, datemodified)
from tickets t
join projects p on p.projectid = t.projectid
join clients c on c.clientid = p.clientid
join ticketstatus ts on ts.ticketstatusid = t.ticketstatus
join TicketTypes tt on tickettypeid = t.tickettype
join TicketPriorities tp on tp.ticketpriorityid = t.priority
WHERE IsDeleted = 0
AND T.TicketStatus <> '6' -- parked tickets