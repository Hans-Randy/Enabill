create view [dbo].[vwLeonProjectMISSamROllUp]
as
/*------------------------------------------------------
Function : Easy view on MIS. For Sam stuff
Created by: Leon
----------------------------------------------------*/


select distinct
	Period, Name, DivisionName, DivisionCode, PayRollRefNo, TotalHours, TotalCost
from	
	vwLeonProjectMISSam
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwLeonProjectMISSamROllUp] TO [EnabillReportRole]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[vwLeonProjectMISSamROllUp] TO PUBLIC
    AS [dbo];

