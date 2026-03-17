using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Seeds
{
    public abstract class BaseSeeder<T>
    {

        protected string _connectionString { get; set; }

        public BaseSeeder(string connectionString) 
        {
            _connectionString = connectionString;
        }
        public async Task Seed(string filePath) 
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("File path is not provided. Skipping product seeding.");
                return;
            }
            using (var reader = new StreamReader(filePath)) 
            {
                var json = await reader.ReadToEndAsync();
                var seeds = JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
                var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                                .UseSqlServer(_connectionString)
                                .Options;

                using (var context = new SmartInventoryDbContext(options)) 
                {
                    try 
                    {
                        ProcessSeed(context, seeds);
                    }
                    catch (SeederException ex) 
                    {
                        Console.WriteLine($"Seeder failed for file {filePath}: {ex.Message}");
                        return;
                    }
                }
            }
        }

        protected abstract void ProcessSeed(SmartInventoryDbContext context, List<T> seeds);

        
    }
}
