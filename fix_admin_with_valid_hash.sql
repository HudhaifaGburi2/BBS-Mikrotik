-- Fix Admin User with Valid BCrypt Hash
USE BroadbandBillingDb;
GO

-- Update admin user with correct BCrypt hash for "Admin@123"
-- Hash generated with work factor 12 (matching PasswordHasher.cs implementation)
UPDATE Users 
SET 
    PasswordHash = '$2a$12$wS3ILaByxnZxrvMLHJ8Zl.LFdMGoT.VDVwMd/SjJhZsi6AtDijL56',
    AccessFailedCount = 0,
    LockoutEnd = NULL,
    IsActive = 1,
    EmailConfirmed = 1
WHERE Username = 'admin';

-- Verify the update
SELECT 
    u.Id,
    u.Username,
    u.Email,
    u.UserType,
    u.IsActive,
    u.EmailConfirmed,
    u.AccessFailedCount,
    u.LockoutEnd,
    LEFT(u.PasswordHash, 30) AS PasswordHashPrefix,
    a.FullName,
    a.Role,
    a.IsActive AS AdminActive
FROM Users u
INNER JOIN Admins a ON u.Id = a.UserId
WHERE u.Username = 'admin';

PRINT '';
PRINT '✓ Admin password updated successfully!';
PRINT 'Username: admin';
PRINT 'Password: Admin@123';
PRINT '';
