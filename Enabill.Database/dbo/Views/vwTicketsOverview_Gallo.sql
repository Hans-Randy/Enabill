
CREATE VIEW [dbo].[vwTicketsOverview_Gallo]
/*------------------------------------------------------------------------
Create by:	Izaan Mobey
ON:			04 Oct 2013
Reason:		Ticket overview for Gallo tickets for Nasiefa

Modified By:	Izaan Mobey
On:				04 April 2014
Reason:			Left outer join to tables so that unassigned tickets can be picked up
------------------------------------------------------------------------*/

--select * from dbo.vwTicketsOverview_Gallo
AS


	SELECT TOP 100 PERCENT 
		C.ClientName, 
		P.ProjectName,
		TicketTypeName as 'Type', 
		TicketPriorityName as 'Priority', 
		TicketReference,  
		TicketSubject as 'Subject', --Details as 'Custome Subject', 
		TicketStatusName as 'Status',
		FromAddress as 'Created By', 
		DateCreated as 'Created',
		UserName as 'Assigned To', 
		DateModified as 'Last Updated',
		TimeSpent,
		IsDeleted
	FROM Clients C
	JOIN Tickets T on T.ClientID = C.ClientID
	LEFT OUTER JOIN Projects P on P.ProjectID = T.ProjectID
	LEFT OUTER JOIN TicketTypes TT on TT.TicketTypeID = T.TicketType
	LEFT OUTER JOIN TicketPriorities TP on TP.TicketPriorityID = T.[Priority]
	LEFT OUTER JOIN TicketStatus TS on TS.TicketStatusID = T.TicketStatus
	LEFT OUTER JOIN Users U on U.UserID = T.UserAssigned
	WHERE 
		C.ClientID in ( 10, 39, 82,83)   --10: Gallo Images SA; 39: Gallo Images Turkey; 82: Gallo Images Brazil; 83: Gallo Images Middle East 
	ORDER BY 
		DateCreated


--select * from clients order by clientname
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwTicketsOverview_Gallo] TO PUBLIC
    AS [dbo];

