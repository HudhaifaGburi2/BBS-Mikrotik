/* =========================================
   Add new columns to Subscriptions
========================================= */

ALTER TABLE [Subscriptions]
ADD 
    [Amount] DECIMAL(18,2) NOT NULL CONSTRAINT DF_Subscriptions_Amount DEFAULT (0),

    [GatewayPaymentId] NVARCHAR(500) NULL,

    [IsPaid] BIT NOT NULL CONSTRAINT DF_Subscriptions_IsPaid DEFAULT (0),

    [MikroTikSyncError] NVARCHAR(1000) NULL,

    [MikroTikSynced] BIT NOT NULL CONSTRAINT DF_Subscriptions_MikroTikSynced DEFAULT (0),

    [MikroTikSyncedAt] DATETIME2 NULL,

    [PaidAt] DATETIME2 NULL;
GO


/* =========================================
   Create PaymentTransactions Table
========================================= */

CREATE TABLE [PaymentTransactions] (
    [Id] UNIQUEIDENTIFIER NOT NULL,
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

    CONSTRAINT [PK_PaymentTransactions] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_PaymentTransactions_Subscriptions]
        FOREIGN KEY ([SubscriptionId])
        REFERENCES [Subscriptions]([Id])
        ON DELETE NO ACTION,

    CONSTRAINT [FK_PaymentTransactions_Subscribers]
        FOREIGN KEY ([SubscriberId])
        REFERENCES [Subscribers]([Id])
        ON DELETE NO ACTION
);
GO


/* =========================================
   Create Indexes
========================================= */

CREATE INDEX [IX_PaymentTransactions_GatewayTransactionId]
ON [PaymentTransactions] ([GatewayTransactionId]);

CREATE INDEX [IX_PaymentTransactions_SessionId]
ON [PaymentTransactions] ([SessionId]);

CREATE INDEX [IX_PaymentTransactions_Status]
ON [PaymentTransactions] ([Status]);

CREATE INDEX [IX_PaymentTransactions_SubscriberId]
ON [PaymentTransactions] ([SubscriberId]);

CREATE INDEX [IX_PaymentTransactions_SubscriptionId]
ON [PaymentTransactions] ([SubscriptionId]);
GO