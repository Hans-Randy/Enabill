




CREATE VIEW [dbo].[vwClientProjectTickets] 
AS
SELECT DISTINCT T.TicketID, T.ProjectID, T.ClientID, T.TicketSubject, L.ToAddress
FROM Tickets AS T
INNER JOIN TicketLines L on T.TicketID = L.TicketID
WHERE T.IsDeleted = 0 AND
      T.TicketStatus <> 5 AND 
      L.FromAddress <> 'no-reply@alacrity.co.za'