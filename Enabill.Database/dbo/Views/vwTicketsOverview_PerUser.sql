
CREATE VIEW [dbo].[vwTicketsOverview_PerUser]
/*------------------------------------------------------------------------
Create by:	Izaan Mobey
ON:			13 May 2014
Reason:		Ticket overview per user
------------------------------------------------------------------------*/

--select * from dbo.[vwTicketsOverview_PerUser]
AS


	SELECT TOP 100 PERCENT 
		isnull(UserName,'Unassigned') as 'Assigned To', 
		C.ClientName, 
		TicketTypeName as 'Type', 
		TicketPriorityName as 'Priority', 
		TicketReference,  
		TicketSubject as 'Subject', --Details as 'Custome Subject', 
		TicketStatusName as 'Status',
		FromAddress as 'Created By', 
		DateCreated as 'Created',		
		DateModified as 'Last Updated',
		TimeSpent--,
		--IsDeleted
	FROM Clients C
	JOIN Tickets T on T.ClientID = C.ClientID
	LEFT OUTER JOIN TicketTypes TT on TT.TicketTypeID = T.TicketType
	LEFT OUTER JOIN TicketPriorities TP on TP.TicketPriorityID = T.[Priority]
	LEFT OUTER JOIN TicketStatus TS on TS.TicketStatusID = T.TicketStatus
	LEFT OUTER JOIN Users U on U.UserID = T.UserAssigned
	WHERE 
		TicketStatus not in (5,6) -- 5: closed; 6: Parked
	AND IsDeleted = 0
	ORDER BY 
		isnull(UserName,'Unassigned')--, TicketStatus, DateCreated


--select * from clients order by clientname
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwTicketsOverview_PerUser] TO PUBLIC
    AS [dbo];

