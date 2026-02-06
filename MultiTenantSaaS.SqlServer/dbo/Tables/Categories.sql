CREATE TABLE [dbo].[Categories]
(
    [Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [TenantId] UNIQUEIDENTIFIER NOT NULL, 
    [Name] NVARCHAR(100) NOT NULL, 
    [Description] NVARCHAR(255) NULL, 
    [Color] NVARCHAR(20) NOT NULL, 
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(), 
    [CreatedBy] NVARCHAR(255) NOT NULL, 
    [ModifiedAt] DATETIME2 NULL, 
    [ModifiedBy] NVARCHAR(255) NULL,
    CONSTRAINT [FK_Categories_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]) ON DELETE CASCADE
)

GO

CREATE INDEX [IX_Categories_TenantId] ON [dbo].[Categories]([TenantId])