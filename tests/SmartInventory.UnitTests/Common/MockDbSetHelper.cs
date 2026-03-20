using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.UnitTests.Common
{
    public static class MockDbSetHelper
    {

        public static DbSet<T> CreateMockDbSet<T>(List<T> data) where T : class 
        {
            var queryable = data.AsQueryable();
            var mockSet = Substitute.For<DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>>();

            var queryableMock = (IQueryable<T>)mockSet;

            queryableMock.Provider.Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            queryableMock.Expression.Returns(queryable.Expression);
            queryableMock.ElementType.Returns(queryable.ElementType);
            queryableMock.GetEnumerator().Returns(queryable.GetEnumerator());
            
            var asyncMock = (IAsyncEnumerable<T>)mockSet;
            asyncMock.GetAsyncEnumerator(Arg.Any<CancellationToken>())
                    .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));    

            return mockSet;
        }

    }
}
