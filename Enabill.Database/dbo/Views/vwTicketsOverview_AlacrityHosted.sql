
CREATE VIEW [dbo].[vwTicketsOverview_AlacrityHosted]
/*------------------------------------------------------------------------
Create by:	Izaan Mobey
ON:			31 Jan 2014
Reason:		Ticket overview for Alacrity Hosted tickets for Farouk

Modified By:	Izaan Mobey
On:				04 April 2014
Reason:			Left outer join to tables so that unassigned tickets can be picked up
------------------------------------------------------------------------*/

--select * from dbo.[vwTicketsOverview_AlacrityHosted]
AS


	SELECT TOP 100 PERCENT 
		C.ClientName, 
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
	LEFT OUTER JOIN TicketTypes TT on TT.TicketTypeID = T.TicketType
	LEFT OUTER JOIN TicketPriorities TP on TP.TicketPriorityID = T.[Priority]
	LEFT OUTER JOIN TicketStatus TS on TS.TicketStatusID = T.TicketStatus
	LEFT OUTER JOIN Users U on U.UserID = T.UserAssigned
	WHERE 
		C.ClientID = 96 -- Alacrity Infrastructure (hosted)
	ORDER BY 
		DateCreated
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwTicketsOverview_AlacrityHosted] TO PUBLIC
    AS [dbo];

