

create view [dbo].[vwTicketExceptionReport]
/*------------------------------------------------------------------------------------
Modified By:	Izaan Mobey
ON:				12 April 2013
Reason:			Added the parked status to the exclusions list.
				And excluded deleted tickets
				
Modified By:	Izaan Mobey
On:				08 Oct 2013
Reason:			remove filter for resolved tickets
--------------------------------------------------------------------------------------*/

as

select TOP 100 PERCENT 
	ClientName,  ProjectName, TicketReference, TicketSubject, TicketPriorityName, TicketStatusName, FirstName, LastName, DateModified, LeadTime = datediff(day, datemodified, getdate())
from tickets t
join ticketstatus ts on ts.ticketstatusid = t.ticketstatus
join clients c on c.clientid = t.clientid
join projects p on p.projectid = t.projectid
join ticketpriorities tp on tp.ticketpriorityid = t.priority
left outer join users u on u.userid = t.userassigned
where t.ticketstatus not in ( 5, 6) --4: Resolved; 5: Closed; 6: Parked
and IsDeleted = 0
order by LeadTime desc