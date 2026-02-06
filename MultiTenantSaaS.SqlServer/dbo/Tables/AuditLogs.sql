CREATE TABLE [dbo].[AuditLogs]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [TenantId] UNIQUEIDENTIFIER NULL, 
    [EntityName] NVARCHAR(100) NOT NULL, 
    [EntityId] NVARCHAR(100) NOT NULL, 
    [Action] INT NOT NULL, 
    [Changes] NVARCHAR(MAX) NULL, 
    [PerformedBy] NVARCHAR(255) NOT NULL, 
    [PerformedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(), 
    [IpAddress] NVARCHAR(50) NULL,
    CONSTRAINT [FK_AuditLogs_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]) ON DELETE CASCADE
)

GO

CREATE INDEX [IX_AuditLogs_TenantId] ON [dbo].[AuditLogs]([TenantId])

GO

CREATE INDEX [IX_AuditLogs_EntityName] ON [dbo].[AuditLogs]([EntityName])

GO

CREATE INDEX [IX_AuditLogs_PerformedAt] ON [dbo].[AuditLogs]([PerformedAt])

GO

CREATE NONCLUSTERED INDEX [IX_AuditLogs_TenantId_EntityName_PerformedAt] 
    ON [dbo].[AuditLogs]([TenantId], [EntityName], [PerformedAt])