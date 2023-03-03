create view dbo.vwLeonActivities
as
/*------------------------------------------------------
Function : Easy view on Activities. Leave it alone!!!
Created by: Leon
----------------------------------------------------*/

select 
	C.ClientName, P.ProjectName, P.ProjectID, A.ActivityName, A.ActivityID, U.FirstName, U.LastName, U.UserID, 
	W.WorkAllocationID, W.DayWorked, W.HoursWorked, W.HoursBilled, W.Remark, InvoiceID, HourlyRate
from
	WorkAllocations W
JOIN Users U on U.UserID = W.UserID
JOIN Activities A on A.ActivityID = W.ActivityID
JOIN Projects P on P.ProjectID = A.ProjectID
JOIN Clients C on C.ClientID = P.ClientID
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwLeonActivities] TO PUBLIC
    AS [dbo];

