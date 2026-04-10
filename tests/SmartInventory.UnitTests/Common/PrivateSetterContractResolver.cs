using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SmartInventory.UnitTests.Common
{
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
