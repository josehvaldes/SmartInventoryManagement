using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Seeds
{
    public class UserSeeder
    {
        private string _connectionString;

        public UserSeeder(string connectionString)
        {
            _connectionString = connectionString;
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
                var users = JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();

                var options = new DbContextOptionsBuilder<AuthDbContext>()
                    .UseSqlServer(_connectionString)
                    .Options;

                using (var context = new AuthDbContext(options))
                {
                    foreach (var user in users)
                    {
                        var existingUser = await context.Users.FindAsync(user.Id);
                        if (existingUser == null)
                        {
                            context.Users.Add(user);
                        }
                        else
                        {
                            existingUser.Name = user.Name;
                            existingUser.Email = user.Email;
                            existingUser.IsActive = user.IsActive;
                        }
                    }
                    await context.SaveChangesAsync();
                }
            }
        }

    }
}
