CREATE TABLE [dbo].[EmploymentTypes] (
    [EmploymentTypeID]   INT          NOT NULL,
    [EmploymentTypeName] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_EmploymentType] PRIMARY KEY CLUSTERED ([EmploymentTypeID] ASC)
);

