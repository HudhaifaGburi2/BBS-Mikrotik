using System;

var password = "Admin@123";

Console.WriteLine("=== BCrypt Password Hash Generator ===");
Console.WriteLine();

// Generate new hash with work factor 11
var hash11 = BCrypt.Net.BCrypt.HashPassword(password, 11);
Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hash (work factor 11): {hash11}");
Console.WriteLine();

// Test the seeded hash from migration
var seededHash = "$2a$11$LQv3c1yqBWLFJGa5Lw4OYeYKvU5hFr5JJ5J0HvG8QwF5J0HvG8Qw.";
Console.WriteLine("Testing seeded hash:");
Console.WriteLine($"Hash: {seededHash}");

try
{
    var isValid = BCrypt.Net.BCrypt.Verify(password, seededHash);
    Console.WriteLine($"Verification result: {isValid}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error verifying: {ex.Message}");
}

Console.WriteLine();
Console.WriteLine("--- Use this SQL to update admin password ---");
Console.WriteLine($"UPDATE Users SET PasswordHash = '{hash11}', AccessFailedCount = 0, LockoutEnd = NULL WHERE Username = 'admin';");
Console.WriteLine();

// Also test with work factor 12 (what PasswordHasher uses)
var hash12 = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
Console.WriteLine($"Hash (work factor 12): {hash12}");
Console.WriteLine();
Console.WriteLine("--- Alternative SQL with work factor 12 ---");
Console.WriteLine($"UPDATE Users SET PasswordHash = '{hash12}', AccessFailedCount = 0, LockoutEnd = NULL WHERE Username = 'admin';");
