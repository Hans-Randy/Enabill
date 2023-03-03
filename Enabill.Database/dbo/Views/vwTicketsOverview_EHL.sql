

CREATE VIEW [dbo].[vwTicketsOverview_EHL]
/*------------------------------------------------------------------------
Create by:	Izaan Mobey
ON:			17 April 2014
Reason:		Ticket overview for Ellerines tickets for Denouvre
------------------------------------------------------------------------*/

--select * from dbo.vwTicketsOverview_EHL
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
		C.ClientID = 70  --Argility
	and projectid = 205 -- ellerines
	ORDER BY 
		DateCreated


--select * from clients order by clientname
--select * from projects where clientid = 70
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwTicketsOverview_EHL] TO PUBLIC
    AS [dbo];

