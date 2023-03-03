BEGIN TRANSACTION
BEGIN TRY

DROP INDEX [IX_Users_UserName_Include] ON [dbo].[Users]


ALTER TABLE Users
DROP COLUMN UserName


ALTER TABLE Users
ADD UserName AS SUBSTRING(Email, 0, CHARINDEX('@', Email, 0)) PERSISTED


CREATE NONCLUSTERED INDEX [IX_Users_UserName_Include] ON [dbo].[Users]
(
	[UserName] ASC
)
INCLUDE([WorkHours],[IsActive],[DivisionID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF, DATA_COMPRESSION = ROW) ON [PRIMARY]


COMMIT TRANSACTION
END TRY
BEGIN CATCH
        ROLLBACK TRANSACTION
        DECLARE @Msg NVARCHAR(MAX)
        SELECT @Msg=ERROR_MESSAGE()
        RAISERROR('Error Occured: %s', 20, 101,@msg) WITH LOG
END CATCH
