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
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("File path is not provided. Skipping user seeding.");
                return;
            }
            

            using (var reader = new StreamReader(filePath))
            {
                var json = await reader.ReadToEndAsync();
                var users = JsonConvert.DeserializeObject<List<CustomUser>>(json) ?? new List<CustomUser>();

                if (_authContext.Users.Any())
                {
                    return; // Exit if there are already users in the database
                }

                foreach (var user in users)
                {
                    var existingUser = await _authContext.Users.FindAsync(user.Id);
                    if (existingUser == null)
                    {
                            
                        user.PasswordHash = HashPassword(user, user.Password);
                        _authContext.Users.Add(user);
                    }
                    else
                    {
                        existingUser.Name = user.Name;
                        existingUser.Email = user.Email;
                        existingUser.IsActive = user.IsActive;
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
