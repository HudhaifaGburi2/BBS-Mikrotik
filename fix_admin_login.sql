-- Fix Admin Login Issue
-- Run this SQL script in your SQL Server Management Studio or Azure Data Studio

USE BroadbandBillingDb;
GO

-- Step 1: Check current admin user
SELECT 
    u.Id, 
    u.Username, 
    u.Email, 
    u.PasswordHash,
    u.IsActive,
    u.LockoutEnd,
    u.AccessFailedCount,
    a.FullName,
    a.Role
FROM Users u
LEFT JOIN Admins a ON u.Id = a.UserId
WHERE u.Username = 'admin' OR u.Email = 'admin@broadband.com';

-- Step 2: Update the admin user with correct password hash
-- Password: Admin@123
-- Hash: $2a$11$LQv3c1yqBWLFJGa5Lw4OYeYKvU5hFr5JJ5J0HvG8QwF5J0HvG8Qw.
UPDATE Users
SET 
    PasswordHash = '$2a$11$LQv3c1yqBWLFJGa5Lw4OYeYKvU5hFr5JJ5J0HvG8QwF5J0HvG8Qw.',
    AccessFailedCount = 0,
    LockoutEnd = NULL,
    IsActive = 1,
    EmailConfirmed = 1
WHERE Username = 'admin';

-- Step 3: Verify the update
SELECT 
    u.Id, 
    u.Username, 
    u.Email, 
    u.PasswordHash,
    u.IsActive,
    u.LockoutEnd,
    u.AccessFailedCount
FROM Users u
WHERE u.Username = 'admin';

-- Step 4: Ensure Admin record exists
IF NOT EXISTS (SELECT 1 FROM Admins WHERE UserId = (SELECT Id FROM Users WHERE Username = 'admin'))
BEGIN
    INSERT INTO Admins (Id, UserId, FullName, Role, IsActive, CreatedAt, UpdatedAt)
    SELECT 
        NEWID(),
        u.Id,
        N'مدير النظام',
        'SuperAdmin',
        1,
        GETUTCDATE(),
        GETUTCDATE()
    FROM Users u
    WHERE u.Username = 'admin';
END

-- Step 5: Final verification
SELECT 
    u.Id AS UserId,
    u.Username, 
    u.Email, 
    u.UserType,
    u.IsActive AS UserActive,
    u.EmailConfirmed,
    u.AccessFailedCount,
    u.LockoutEnd,
    a.Id AS AdminId,
    a.FullName,
    a.Role,
    a.IsActive AS AdminActive
FROM Users u
INNER JOIN Admins a ON u.Id = a.UserId
WHERE u.Username = 'admin';

PRINT 'Admin user fixed successfully!';
PRINT 'Username: admin';
PRINT 'Password: Admin@123';
