-- Migration: Add BillingCycleHours to Plans and DataUsage fields to Subscriptions
-- Date: 2026-03-04
-- Description: Adds hourly billing support and data usage tracking for auto-stop when GB limit reached

-- Add BillingCycleHours to Plans table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Plans]') AND name = 'BillingCycleHours')
BEGIN
    ALTER TABLE [dbo].[Plans]
    ADD [BillingCycleHours] INT NULL;
    
    PRINT 'Added BillingCycleHours column to Plans table';
END
GO

-- Add DataUsedBytes to Subscriptions table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND name = 'DataUsedBytes')
BEGIN
    ALTER TABLE [dbo].[Subscriptions]
    ADD [DataUsedBytes] BIGINT NOT NULL DEFAULT 0;
    
    PRINT 'Added DataUsedBytes column to Subscriptions table';
END
GO

-- Add DataLimitExceeded to Subscriptions table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND name = 'DataLimitExceeded')
BEGIN
    ALTER TABLE [dbo].[Subscriptions]
    ADD [DataLimitExceeded] BIT NOT NULL DEFAULT 0;
    
    PRINT 'Added DataLimitExceeded column to Subscriptions table';
END
GO

-- Add DataLimitExceededAt to Subscriptions table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND name = 'DataLimitExceededAt')
BEGIN
    ALTER TABLE [dbo].[Subscriptions]
    ADD [DataLimitExceededAt] DATETIME2 NULL;
    
    PRINT 'Added DataLimitExceededAt column to Subscriptions table';
END
GO

-- Add Amount to Subscriptions table (if not exists from previous migration)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND name = 'Amount')
BEGIN
    ALTER TABLE [dbo].[Subscriptions]
    ADD [Amount] DECIMAL(18,2) NOT NULL DEFAULT 0;
    
    PRINT 'Added Amount column to Subscriptions table';
END
GO

-- Add GatewayPaymentId to Subscriptions table (if not exists from previous migration)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND name = 'GatewayPaymentId')
BEGIN
    ALTER TABLE [dbo].[Subscriptions]
    ADD [GatewayPaymentId] NVARCHAR(500) NULL;
    
    PRINT 'Added GatewayPaymentId column to Subscriptions table';
END
GO

-- Add IsPaid to Subscriptions table (if not exists from previous migration)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND name = 'IsPaid')
BEGIN
    ALTER TABLE [dbo].[Subscriptions]
    ADD [IsPaid] BIT NOT NULL DEFAULT 0;
    
    PRINT 'Added IsPaid column to Subscriptions table';
END
GO

-- Add PaidAt to Subscriptions table (if not exists from previous migration)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND name = 'PaidAt')
BEGIN
    ALTER TABLE [dbo].[Subscriptions]
    ADD [PaidAt] DATETIME2 NULL;
    
    PRINT 'Added PaidAt column to Subscriptions table';
END
GO

-- Add MikroTikSynced to Subscriptions table (if not exists from previous migration)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND name = 'MikroTikSynced')
BEGIN
    ALTER TABLE [dbo].[Subscriptions]
    ADD [MikroTikSynced] BIT NOT NULL DEFAULT 0;
    
    PRINT 'Added MikroTikSynced column to Subscriptions table';
END
GO

-- Add MikroTikSyncedAt to Subscriptions table (if not exists from previous migration)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND name = 'MikroTikSyncedAt')
BEGIN
    ALTER TABLE [dbo].[Subscriptions]
    ADD [MikroTikSyncedAt] DATETIME2 NULL;
    
    PRINT 'Added MikroTikSyncedAt column to Subscriptions table';
END
GO

-- Add MikroTikSyncError to Subscriptions table (if not exists from previous migration)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND name = 'MikroTikSyncError')
BEGIN
    ALTER TABLE [dbo].[Subscriptions]
    ADD [MikroTikSyncError] NVARCHAR(1000) NULL;
    
    PRINT 'Added MikroTikSyncError column to Subscriptions table';
END
GO

-- Create PaymentTransactions table if not exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentTransactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PaymentTransactions] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [SubscriptionId] UNIQUEIDENTIFIER NOT NULL,
        [SubscriberId] UNIQUEIDENTIFIER NOT NULL,
        [Gateway] NVARCHAR(50) NOT NULL,
        [SessionId] NVARCHAR(500) NULL,
        [GatewayTransactionId] NVARCHAR(500) NULL,
        [GatewayOrderId] NVARCHAR(500) NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [Currency] NVARCHAR(10) NOT NULL,
        [Status] INT NOT NULL,
        [CardBrand] NVARCHAR(50) NULL,
        [CardLast4] NVARCHAR(4) NULL,
        [RawRequest] NVARCHAR(MAX) NULL,
        [RawResponse] NVARCHAR(MAX) NULL,
        [FailureReason] NVARCHAR(1000) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CompletedAt] DATETIME2 NULL,
        
        CONSTRAINT [FK_PaymentTransactions_Subscriptions] FOREIGN KEY ([SubscriptionId]) 
            REFERENCES [dbo].[Subscriptions]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PaymentTransactions_Subscribers] FOREIGN KEY ([SubscriberId]) 
            REFERENCES [dbo].[Subscribers]([Id]) ON DELETE NO ACTION
    );
    
    CREATE INDEX [IX_PaymentTransactions_SubscriptionId] ON [dbo].[PaymentTransactions]([SubscriptionId]);
    CREATE INDEX [IX_PaymentTransactions_SubscriberId] ON [dbo].[PaymentTransactions]([SubscriberId]);
    CREATE INDEX [IX_PaymentTransactions_SessionId] ON [dbo].[PaymentTransactions]([SessionId]);
    CREATE INDEX [IX_PaymentTransactions_GatewayTransactionId] ON [dbo].[PaymentTransactions]([GatewayTransactionId]);
    CREATE INDEX [IX_PaymentTransactions_Status] ON [dbo].[PaymentTransactions]([Status]);
    
    PRINT 'Created PaymentTransactions table';
END
GO

PRINT 'Migration completed successfully';
GO
