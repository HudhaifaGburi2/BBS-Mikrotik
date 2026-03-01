BEGIN TRANSACTION;
DROP INDEX [IX_Subscribers_UserId] ON [Subscribers];

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscribers]') AND [c].[name] = N'UserId');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Subscribers] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Subscribers] ALTER COLUMN [UserId] uniqueidentifier NULL;

CREATE UNIQUE INDEX [IX_Subscribers_UserId] ON [Subscribers] ([UserId]) WHERE [UserId] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260214042244_MakeSubscriberUserIdNullable', N'9.0.1');

ALTER TABLE [PppoeAccounts] DROP CONSTRAINT [FK_PppoeAccounts_Subscribers_SubscriberId1];

DROP INDEX [IX_PppoeAccounts_SubscriberId1] ON [PppoeAccounts];

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PppoeAccounts]') AND [c].[name] = N'SubscriberId1');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [PppoeAccounts] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [PppoeAccounts] DROP COLUMN [SubscriberId1];

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscribers]') AND [c].[name] = N'MikroTikUsername');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Subscribers] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Subscribers] ALTER COLUMN [MikroTikUsername] NVARCHAR(MAX) NULL;

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscribers]') AND [c].[name] = N'MacAddress');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Subscribers] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Subscribers] ALTER COLUMN [MacAddress] NVARCHAR(MAX) NULL;

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscribers]') AND [c].[name] = N'LastLoginOS');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Subscribers] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Subscribers] ALTER COLUMN [LastLoginOS] NVARCHAR(MAX) NULL;

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscribers]') AND [c].[name] = N'LastLoginDevice');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Subscribers] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Subscribers] ALTER COLUMN [LastLoginDevice] NVARCHAR(MAX) NULL;

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscribers]') AND [c].[name] = N'LastLoginBrowser');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Subscribers] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Subscribers] ALTER COLUMN [LastLoginBrowser] NVARCHAR(MAX) NULL;

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscribers]') AND [c].[name] = N'IpAddress');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Subscribers] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Subscribers] ALTER COLUMN [IpAddress] NVARCHAR(MAX) NULL;

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LoginHistory]') AND [c].[name] = N'Browser');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [LoginHistory] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [LoginHistory] ALTER COLUMN [Browser] nvarchar(500) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260301053431_AddSubscriberNetworkInfoAndFixPppoeFK', N'9.0.1');

COMMIT;
GO

