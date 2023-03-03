

CREATE VIEW [dbo].[vwSupportEmails] AS
SELECT cast(ClientID as varchar(10)) + '_0' as UKey,ClientID, 0 as ProjectID, SupportEmailAddress
FROM Clients
WHERE supportemailaddress is not null and rtrim(supportemailaddress)<>''
UNION
SELECT cast(ClientID as varchar(10)) + '_' + cast(ProjectID as varchar(10)) as UKey, ClientID, ProjectID, SupportEmailAddress
FROM PROJECTS
WHERE supportemailaddress is not null and rtrim(supportemailaddress)<>''