CREATE TABLE [dbo].[ReportEmails] (
    [ReportEmailID] INT IDENTITY (1, 1) NOT NULL,
    [ReportID]      INT NOT NULL,
    [UserID]        INT NOT NULL,
    [FrequencyID]   INT NOT NULL,
    CONSTRAINT [PK_ReportEmails] PRIMARY KEY CLUSTERED ([ReportEmailID] ASC),
    CONSTRAINT [FK_ReportEmails_Frequencies] FOREIGN KEY ([FrequencyID]) REFERENCES [dbo].[Frequencies] ([FrequencyID]),
    CONSTRAINT [FK_ReportEmails_Reports] FOREIGN KEY ([ReportID]) REFERENCES [dbo].[Reports] ([ReportID]),
    CONSTRAINT [FK_ReportEmails_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_UserID]
    ON [dbo].[ReportEmails]([UserID] ASC);

