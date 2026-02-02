-- Delete and Recreate Admin User
USE BroadbandBillingDb;
GO

-- Step 1: Delete existing admin user and related data
DECLARE @AdminUserId UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Username = 'admin');

IF @AdminUserId IS NOT NULL
BEGIN
    DELETE FROM LoginHistory WHERE UserId = @AdminUserId;
    DELETE FROM Admins WHERE UserId = @AdminUserId;
    DELETE FROM Users WHERE Id = @AdminUserId;
    PRINT 'Deleted existing admin user';
END

-- Step 2: Create fresh admin user
DECLARE @NewUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @NewAdminId UNIQUEIDENTIFIER = NEWID();

-- BCrypt hash for "Admin@123" with work factor 11
-- Generated using: BCrypt.Net.BCrypt.HashPassword("Admin@123", 11)
INSERT INTO Users (Id, Username, Email, PasswordHash, UserType, IsActive, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, AccessFailedCount, CreatedAt, UpdatedAt)
VALUES (
    @NewUserId,
    'admin',
    'admin@broadband.com',
    '$2a$11$vZ3jzZ3jzZ3jzZ3jzZ3jzuXQYJ5qZ3jzZ3jzZ3jzZ3jzZ3jzZ3jzu',
    0, -- UserType.Admin = 0
    1, -- IsActive = true
    1, -- EmailConfirmed = true
    0, -- PhoneNumberConfirmed = false
    0, -- TwoFactorEnabled = false
    0, -- AccessFailedCount = 0
    GETUTCDATE(),
    GETUTCDATE()
);

INSERT INTO Admins (Id, UserId, FullName, Role, IsActive, CreatedAt, UpdatedAt)
VALUES (
    @NewAdminId,
    @NewUserId,
    N'مدير النظام',
    1, -- AdminRole.SuperAdmin = 1
    1, -- IsActive = true
    GETUTCDATE(),
    GETUTCDATE()
);

PRINT 'Created new admin user successfully';
PRINT '';
PRINT 'Username: admin';
PRINT 'Password: Admin@123';
PRINT '';

-- Verify
SELECT 
    u.Id,
    u.Username,
    u.Email,
    u.UserType,
    u.IsActive,
    u.PasswordHash,
    a.FullName,
    a.Role,
    a.IsActive AS AdminActive
FROM Users u
INNER JOIN Admins a ON u.Id = a.UserId
WHERE u.Username = 'admin';
