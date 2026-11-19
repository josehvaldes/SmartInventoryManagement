using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Seeds
{
    public static class SeederSettings
    {
        internal static readonly JsonSerializerSettings DeserializationSettings = new()
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new PrivateSetterContractResolver()
        };
    }
}
