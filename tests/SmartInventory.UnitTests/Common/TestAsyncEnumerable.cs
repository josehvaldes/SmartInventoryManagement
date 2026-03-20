using System.Linq.Expressions;

namespace SmartInventory.UnitTests.Common
{
    internal class TestAsyncEnumerable<T>(Expression expression)
    : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default) =>
            new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        
    }
}
