

CREATE view [dbo].[vwTicketTimeAllocation] AS
SELECT DISTINCT 
       T.TicketReference,
       T.TicketSubject, 
       T.DateCreated,
       C.ClientName, 
       P.ProjectName,
       A.ActivityName,
       U.UserName,
       U.FullName,
       W.Period,
       W.DayWorked,
       W.HoursWorked,
       W.Remark,
       W.WorkAllocationID
FROM Tickets T INNER JOIN
Clients C ON T.ClientID = C.ClientID INNER JOIN
Projects P ON T.ProjectID = P.ProjectID INNER JOIN 
Activities A ON P.ProjectID = A.ProjectID INNER JOIN
Workallocations W ON W.ActivityID = A.ActivityID INNER JOIN
Users U ON U.UserID = W.Userid
WHERE W.TicketReference = T.TicketReference