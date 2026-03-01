-- Migration: AddSubscriberNetworkInfoAndFixPppoeFK
-- This script fixes the shadow FK issue and ensures all subscriber columns exist
-- Run this script on your BroadbandBillingDb database

BEGIN TRANSACTION;
GO

-- Step 1: Drop the shadow foreign key SubscriberId1 from PppoeAccounts
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PppoeAccounts_Subscribers_SubscriberId1')
BEGIN
    ALTER TABLE [PppoeAccounts] DROP CONSTRAINT [FK_PppoeAccounts_Subscribers_SubscriberId1];
END
GO

-- Step 2: Drop the shadow index
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PppoeAccounts_SubscriberId1' AND object_id = OBJECT_ID('PppoeAccounts'))
BEGIN
    DROP INDEX [IX_PppoeAccounts_SubscriberId1] ON [PppoeAccounts];
END
GO

-- Step 3: Drop the shadow column SubscriberId1
IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'SubscriberId1' AND object_id = OBJECT_ID('PppoeAccounts'))
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PppoeAccounts]') AND [c].[name] = N'SubscriberId1');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [PppoeAccounts] DROP CONSTRAINT [' + @var1 + '];');
    
    ALTER TABLE [PppoeAccounts] DROP COLUMN [SubscriberId1];
END
GO

-- Step 4: Add missing columns to Subscribers table if they don't exist
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'IpAddress' AND object_id = OBJECT_ID('Subscribers'))
BEGIN
    ALTER TABLE [Subscribers] ADD [IpAddress] NVARCHAR(MAX) NULL;
END
ELSE
BEGIN
    -- Column exists, just ensure it has the correct type
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscribers]') AND [c].[name] = N'IpAddress');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Subscribers] DROP CONSTRAINT [' + @var2 + '];');
    
    ALTER TABLE [Subscribers] ALTER COLUMN [IpAddress] NVARCHAR(MAX) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'MacAddress' AND object_id = OBJECT_ID('Subscribers'))
BEGIN
    ALTER TABLE [Subscribers] ADD [MacAddress] NVARCHAR(MAX) NULL;
END
ELSE
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscribers]') AND [c].[name] = N'MacAddress');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Subscribers] DROP CONSTRAINT [' + @var3 + '];');
    
    ALTER TABLE [Subscribers] ALTER COLUMN [MacAddress] NVARCHAR(MAX) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'MikroTikUsername' AND object_id = OBJECT_ID('Subscribers'))
BEGIN
    ALTER TABLE [Subscribers] ADD [MikroTikUsername] NVARCHAR(MAX) NULL;
END
ELSE
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscribers]') AND [c].[name] = N'MikroTikUsername');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Subscribers] DROP CONSTRAINT [' + @var4 + '];');
    
    ALTER TABLE [Subscribers] ALTER COLUMN [MikroTikUsername] NVARCHAR(MAX) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'LastLoginDevice' AND object_id = OBJECT_ID('Subscribers'))
BEGIN
    ALTER TABLE [Subscribers] ADD [LastLoginDevice] NVARCHAR(MAX) NULL;
END
ELSE
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscribers]') AND [c].[name] = N'LastLoginDevice');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Subscribers] DROP CONSTRAINT [' + @var5 + '];');
    
    ALTER TABLE [Subscribers] ALTER COLUMN [LastLoginDevice] NVARCHAR(MAX) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'LastLoginBrowser' AND object_id = OBJECT_ID('Subscribers'))
BEGIN
    ALTER TABLE [Subscribers] ADD [LastLoginBrowser] NVARCHAR(MAX) NULL;
END
ELSE
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscribers]') AND [c].[name] = N'LastLoginBrowser');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Subscribers] DROP CONSTRAINT [' + @var6 + '];');
    
    ALTER TABLE [Subscribers] ALTER COLUMN [LastLoginBrowser] NVARCHAR(MAX) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'LastLoginOS' AND object_id = OBJECT_ID('Subscribers'))
BEGIN
    ALTER TABLE [Subscribers] ADD [LastLoginOS] NVARCHAR(MAX) NULL;
END
ELSE
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscribers]') AND [c].[name] = N'LastLoginOS');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Subscribers] DROP CONSTRAINT [' + @var7 + '];');
    
    ALTER TABLE [Subscribers] ALTER COLUMN [LastLoginOS] NVARCHAR(MAX) NULL;
END
GO

-- Step 5: Update migration history (only if using EF Core migrations tracking)
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260301053431_AddSubscriberNetworkInfoAndFixPppoeFK')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260301053431_AddSubscriberNetworkInfoAndFixPppoeFK', N'9.0.1');
END
GO

COMMIT TRANSACTION;
GO

PRINT 'Migration completed successfully!';
GO
