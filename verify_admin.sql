-- Verify Admin User in Database
USE BroadbandBillingDb;
GO

-- Check if admin user exists and show details
SELECT 
    u.Id,
    u.Username,
    u.Email,
    u.PasswordHash,
    u.UserType,
    u.IsActive,
    u.EmailConfirmed,
    u.AccessFailedCount,
    u.LockoutEnd,
    LEN(u.PasswordHash) AS PasswordHashLength,
    LEFT(u.PasswordHash, 10) AS PasswordHashPrefix,
    a.Id AS AdminId,
    a.FullName,
    a.Role,
    a.IsActive AS AdminActive
FROM Users u
LEFT JOIN Admins a ON u.Id = a.UserId
WHERE u.Username = 'admin';

-- Show all users (to see if admin exists at all)
SELECT Username, Email, UserType, IsActive FROM Users;
