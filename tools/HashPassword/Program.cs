using Microsoft.Data.SqlClient;

var hash = BCrypt.Net.BCrypt.HashPassword("gburi@admin", BCrypt.Net.BCrypt.GenerateSalt(12));
Console.WriteLine($"Generated hash: {hash}");

var connStr = "Server=localhost;Database=BroadbandBillingDb;User Id=sa;Password=BBS@Strong2024!;TrustServerCertificate=True;";
using var conn = new SqlConnection(connStr);
conn.Open();

using var cmd = new SqlCommand("UPDATE Users SET PasswordHash = @hash WHERE Username = 'admin'", conn);
cmd.Parameters.AddWithValue("@hash", hash);
var rows = cmd.ExecuteNonQuery();
Console.WriteLine($"Updated {rows} row(s)");

// Verify
using var verify = new SqlCommand("SELECT Username, PasswordHash FROM Users WHERE Username = 'admin'", conn);
using var reader = verify.ExecuteReader();
if (reader.Read())
{
    Console.WriteLine($"Username: {reader.GetString(0)}");
    Console.WriteLine($"Hash: {reader.GetString(1)}");
    Console.WriteLine($"Verify: {BCrypt.Net.BCrypt.Verify("gburi@admin", reader.GetString(1))}");
}
