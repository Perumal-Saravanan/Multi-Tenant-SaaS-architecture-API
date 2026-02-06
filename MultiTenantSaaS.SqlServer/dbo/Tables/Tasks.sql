CREATE TABLE [dbo].[Tasks]
(
    [Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [TenantId] UNIQUEIDENTIFIER NOT NULL, 
    [Title] NVARCHAR(200) NOT NULL, 
    [Description] NVARCHAR(MAX) NULL, 
    [IsCompleted] BIT NOT NULL DEFAULT 0, 
    [DueDate] DATETIME2 NULL, 
    [Priority] INT NOT NULL DEFAULT 1, 
    [AssignedToUserId] UNIQUEIDENTIFIER NOT NULL, 
    [CategoryId] INT NULL, 
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(), 
    [CreatedBy] NVARCHAR(255) NOT NULL, 
    [ModifiedAt] DATETIME2 NULL, 
    [ModifiedBy] NVARCHAR(255) NULL,
    CONSTRAINT [FK_Tasks_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Tasks_User] FOREIGN KEY ([AssignedToUserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Tasks_Category] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories]([Id]) ON DELETE SET NULL
)

GO

CREATE INDEX [IX_Tasks_TenantId] ON [dbo].[Tasks]([TenantId])
