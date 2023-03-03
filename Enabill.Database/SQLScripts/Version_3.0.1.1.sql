USE [Enabill]
GO

/****** Object:  Table [dbo].[ContractAttachments]    Script Date: 2021/03/26 15:43:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ContractAttachments](
	[ContractAttachmentID] [int] IDENTITY(1,1) NOT NULL,
	[ClientID] [int] NOT NULL,
	[ProjectID] [int] NOT NULL,
	[FilePath] [nvarchar](100) NOT NULL,
	[FileName] [nvarchar](30) NOT NULL,
	[MimeType] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ContractAttachment] PRIMARY KEY CLUSTERED 
(
	[ContractAttachmentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 99) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ContractAttachments]  WITH CHECK ADD  CONSTRAINT [FK_ContractAttachments_Projects] FOREIGN KEY([ProjectID])
REFERENCES [dbo].[Projects] ([ProjectID])
GO

ALTER TABLE [dbo].[ContractAttachments]  WITH CHECK ADD  CONSTRAINT [FK_ContractAttachments_Clients] FOREIGN KEY([ClientID])
REFERENCES [dbo].[Clients] ([ClientID])
GO

ALTER TABLE [dbo].[ContractAttachments] CHECK CONSTRAINT [FK_ContractAttachments_Projects]
GO

-- Add Date Column to Projects table
BEGIN TRANSACTION
	SET QUOTED_IDENTIFIER ON
	SET ARITHABORT ON
	SET NUMERIC_ROUNDABORT OFF
	SET CONCAT_NULL_YIELDS_NULL ON
	SET ANSI_NULLS ON
	SET ANSI_PADDING ON
	SET ANSI_WARNINGS ON
	COMMIT
	BEGIN TRANSACTION
	GO
	ALTER TABLE dbo.Projects ADD
		CreatedDate date NULL
	GO
	ALTER TABLE dbo.Projects ADD CONSTRAINT
		DF_Projects_CreatedDate DEFAULT GETDATE() FOR CreatedDate
	GO
	ALTER TABLE dbo.Projects SET (LOCK_ESCALATION = TABLE)
	GO
COMMIT

-- Set CreatedDate column to the StartDate column
UPDATE
	dbo.Projects
SET
	CreatedDate = StartDate