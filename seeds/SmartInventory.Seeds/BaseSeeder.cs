using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SmartInventory.Seeds
{
    public abstract class BaseSeeder<T>
    {

        protected string _connectionString { get; set; }

        private static readonly JsonSerializerSettings _deserializationSettings = new()
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver    = new PrivateSetterContractResolver()
        };

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
                var seeds = JsonConvert.DeserializeObject<List<T>>(json, _deserializationSettings) ?? new List<T>();
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

    internal sealed class PrivateSetterContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            if (!prop.Writable && member is PropertyInfo pi && pi.GetSetMethod(nonPublic: true) != null)
                prop.Writable = true;
            return prop;
        }
    }
}
