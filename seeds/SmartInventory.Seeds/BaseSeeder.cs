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

    internal static class SeederSettings
    {
        internal static readonly JsonSerializerSettings DeserializationSettings = new()
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver    = new PrivateSetterContractResolver()
        };
    }

    public abstract class BaseSeeder<T>
    {
        protected SmartInventoryDbContext _context { get; set; }

        public BaseSeeder(SmartInventoryDbContext context) 
        {
            _context = context;
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
                var seeds = JsonConvert.DeserializeObject<List<T>>(json, SeederSettings.DeserializationSettings) ?? new List<T>();

                ProcessSeed(_context, seeds);
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
