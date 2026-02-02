// Test BCrypt Hash Generation
// Run this with: dotnet script test_bcrypt_hash.cs

using System;

// You'll need to install BCrypt.Net-Next package:
// dotnet add package BCrypt.Net-Next

var password = "Admin@123";

// Generate a new hash
var newHash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(11));

Console.WriteLine("Password: " + password);
Console.WriteLine("Generated Hash: " + newHash);
Console.WriteLine();

// Test the predefined hash
var predefinedHash = "$2a$11$LQv3c1yqBWLFJGa5Lw4OYeYKvU5hFr5JJ5J0HvG8QwF5J0HvG8Qw.";

Console.WriteLine("Testing predefined hash:");
Console.WriteLine("Hash: " + predefinedHash);
Console.WriteLine("Verifies: " + BCrypt.Net.BCrypt.Verify(password, predefinedHash));
Console.WriteLine();

// Test the new hash
Console.WriteLine("Testing new hash:");
Console.WriteLine("Hash: " + newHash);
Console.WriteLine("Verifies: " + BCrypt.Net.BCrypt.Verify(password, newHash));
