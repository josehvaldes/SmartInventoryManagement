using Newtonsoft.Json;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Identity;
using SmartInventory.IntegrationTests.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.IntegrationTests.Sessions
{
    public static class UserData
    {
        private static readonly string UsersFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "users.json");

        private static readonly JsonSerializerSettings _deserializationSettings = new()
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new PrivateSetterContractResolver()
        };

        public static List<User> LoadUsersFromFile()
        {
            if (string.IsNullOrWhiteSpace(UsersFilePath) || !File.Exists(UsersFilePath))
                throw new FileNotFoundException("File path is not provided or file does not exist. Skipping user seeding.");

            using (var reader = new StreamReader(UsersFilePath))
            {
                var users = new List<User>();
                var json = reader.ReadToEnd();
                var seeds = JsonConvert.DeserializeObject<List<User>>(json, _deserializationSettings) ?? new List<User>();
                users.AddRange(seeds);

                return users;
            }
        }

    }
}
