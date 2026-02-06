-- SQL Script to Create a New Subscriber/Client
-- استعلام SQL لإنشاء مشترك/عميل جديد

-- Step 1: Create the Subscriber record first
-- الخطوة 1: إنشاء سجل المشترك أولاً
DECLARE @SubscriberId UNIQUEIDENTIFIER = NEWID();
DECLARE @UserId UNIQUEIDENTIFIER = NEWID();
DECLARE @Now DATETIME2 = GETUTCDATE();

-- Insert Subscriber
INSERT INTO Subscribers (
    Id,
    UserId,
    FullName,
    Email,
    PhoneNumber,
    Address,
    NationalId,
    City,
    PostalCode,
    IsActive,
    CreatedAt,
    UpdatedAt
)
VALUES (
    @SubscriberId,
    @UserId,
    N'اسم المشترك',           -- Full Name (change this)
    'subscriber@example.com', -- Email (change this)
    '+966500000000',          -- Phone Number (change this)
    N'العنوان الكامل',        -- Address (change this)
    '1234567890',             -- National ID (optional)
    N'الرياض',                -- City (optional)
    '12345',                  -- Postal Code (optional)
    1,                        -- IsActive (1 = active)
    @Now,
    @Now
);

-- Step 2: Create the User record with hashed password
-- الخطوة 2: إنشاء سجل المستخدم مع كلمة المرور المشفرة
-- Note: Password hash should be generated using BCrypt
-- ملاحظة: يجب إنشاء تشفير كلمة المرور باستخدام BCrypt

-- This is a BCrypt hash for password "Admin@123" - CHANGE THIS IN PRODUCTION
-- هذا تشفير BCrypt لكلمة المرور "Admin@123" - غيّر هذا في الإنتاج
-- Generate your own hash using: dotnet run --project HashGenerator
DECLARE @PasswordHash NVARCHAR(MAX) = '$2a$11$YhCQa60gMr1rBp/d1ITZ4.R00CDdQc7OFQWrds9aeheXA/q3.Is0q';

INSERT INTO Users (
    Id,
    Username,
    Email,
    PasswordHash,
    UserType,
    IsActive,
    EmailConfirmed,
    PhoneNumber,
    PhoneNumberConfirmed,
    TwoFactorEnabled,
    AccessFailedCount,
    SubscriberId,
    CreatedAt,
    UpdatedAt
)
VALUES (
    @UserId,
    'subscriber_username',    -- Username for login (change this)
    'subscriber@example.com', -- Email (should match subscriber email)
    @PasswordHash,            -- BCrypt hashed password
    1,                        -- UserType: 0 = Admin, 1 = Subscriber
    1,                        -- IsActive
    1,                        -- EmailConfirmed
    '+966500000000',          -- Phone Number
    0,                        -- PhoneNumberConfirmed
    0,                        -- TwoFactorEnabled
    0,                        -- AccessFailedCount
    @SubscriberId,            -- Link to Subscriber
    @Now,
    @Now
);

-- Update Subscriber with UserId
UPDATE Subscribers SET UserId = @UserId WHERE Id = @SubscriberId;

-- Verify the creation
-- التحقق من الإنشاء
SELECT 
    s.Id AS SubscriberId,
    s.FullName,
    s.Email AS SubscriberEmail,
    u.Id AS UserId,
    u.Username,
    u.Email AS UserEmail,
    u.UserType,
    u.IsActive
FROM Subscribers s
INNER JOIN Users u ON s.UserId = u.Id
WHERE s.Id = @SubscriberId;

PRINT N'تم إنشاء المشترك بنجاح - Subscriber created successfully';
PRINT N'Username: subscriber_username';
PRINT N'Password: Admin@123 (change this!)';
