using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.Seeds.CustomModel;

namespace SmartInventory.Seeds
{
    public class UserSeeder
    {
        private PasswordHasher<User> _hasher;
        private AuthDbContext _authContext;

        public UserSeeder(AuthDbContext authContext)
        {
            _authContext = authContext;
            _hasher = new PasswordHasher<User>();
        }

        public async Task Seed(string filePath) 
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("File path is not provided or file does not exist. Skipping user seeding.");
                return;
            }

            using (var reader = new StreamReader(filePath))
            {
                var json = await reader.ReadToEndAsync();
                var users = JsonConvert.DeserializeObject<List<CustomUser>>(json) ?? new List<CustomUser>();


                foreach (var user in users)
                {
                    var exists = await _authContext.Users.AnyAsync(u => u.Id == user.Id);
                    if (!exists)
                    {
                        user.PasswordHash = HashPassword(user, user.Password);
                        _authContext.Users.Add(user);
                    }
                }
                await _authContext.SaveChangesAsync();
                
            }
        }

        private string HashPassword(User user, string password)
        {
            return _hasher.HashPassword(user, password);
        }
    }
}
