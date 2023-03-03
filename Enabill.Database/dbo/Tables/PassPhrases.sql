CREATE TABLE [dbo].[PassPhrases] (
    [PassPhraseID]   INT           IDENTITY (1, 1) NOT NULL,
    [PassPhraseName] VARCHAR (100) NOT NULL,
    [ModifiedDate]   DATETIME      NOT NULL,
    [ModifiedBy]     INT           NOT NULL,
    CONSTRAINT [PK_PassPhrase] PRIMARY KEY CLUSTERED ([PassPhraseID] ASC)
);


GO
GRANT SELECT
    ON OBJECT::[dbo].[PassPhrases] TO [EnabillReportRole]
    AS [dbo];

