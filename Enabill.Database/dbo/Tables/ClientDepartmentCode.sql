CREATE TABLE [dbo].[ClientDepartmentCode] (
    [ClientDepartmentCodeID] INT          IDENTITY (1, 1) NOT NULL,
    [ClientID]               INT          NOT NULL,
    [DepartmentCode]         VARCHAR (50) NULL,
    [IsActive]               BIT          CONSTRAINT [DF_ClientDepartmentCode_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_ClientDepartmentCode] PRIMARY KEY CLUSTERED ([ClientDepartmentCodeID] ASC),
    CONSTRAINT [FK_ClientDepartmentCode_Clients] FOREIGN KEY ([ClientID]) REFERENCES [dbo].[Clients] ([ClientID])
);

