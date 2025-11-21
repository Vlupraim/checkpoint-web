using Microsoft.AspNetCore.Identity;
using checkpoint_web.Models;

// Pequeño programa para generar hashes de password
var hasher = new PasswordHasher<ApplicationUser>();
var user = new ApplicationUser();

Console.WriteLine("=== Generador de Password Hashes ===\n");

string[] passwords = { "Admin123!", "Bodega123!", "Calidad123!" };
string[] emails = { "admin@checkpoint.com", "bodega@checkpoint.com", "calidad@checkpoint.com" };

for (int i = 0; i < passwords.Length; i++)
{
    var hash = hasher.HashPassword(user, passwords[i]);
    Console.WriteLine($"Email: {emails[i]}");
    Console.WriteLine($"Password: {passwords[i]}");
    Console.WriteLine($"Hash: {hash}");
    Console.WriteLine();
}

Console.WriteLine("\nUsa estos hashes en tu SQL script para actualizar los usuarios.");
