using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SmartInventory.IntegrationTests.Infrastructure
{
    [CollectionDefinition("Integration")]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }
}
