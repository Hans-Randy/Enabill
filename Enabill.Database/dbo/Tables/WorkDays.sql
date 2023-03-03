CREATE TABLE [dbo].[WorkDays] (
    [WorkDate]   DATETIME NOT NULL,
    [IsWorkable] BIT      NOT NULL,
    CONSTRAINT [PK_WorkDays] PRIMARY KEY CLUSTERED ([WorkDate] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_WorkDays_IsWorkable]
    ON [dbo].[WorkDays]([IsWorkable] ASC);

