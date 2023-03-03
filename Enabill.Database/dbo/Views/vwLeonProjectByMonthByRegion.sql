create view dbo.vwLeonProjectByMonthByRegion
as
/*------------------------------------------------------
Function : Easy view on Activities. Leave it alone!!!
Created by: Leon
----------------------------------------------------*/

select 
	C.ClientName, P.ProjectName, P.ProjectID, U.FirstName, U.LastName, U.UserID, 
	Year(W.Dayworked) as Year, month(W.DayWorked) as Month, 
	sum(W.HoursWorked) as HoursWorked
from
	WorkAllocations W
JOIN Users U on U.UserID = W.UserID
JOIN Activities A on A.ActivityID = W.ActivityID
JOIN Projects P on P.ProjectID = A.ProjectID
JOIN Clients C on C.ClientID = P.ClientID
GROUP BY
	C.ClientName, P.ProjectName, P.ProjectID, U.FirstName, U.LastName, U.UserID, Year(W.Dayworked), month(W.DayWorked)
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwLeonProjectByMonthByRegion] TO PUBLIC
    AS [dbo];

