using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SmartInventory.UnitTests.Common
{
    internal class TestAsyncQueryProvider<TEntity>(IQueryProvider inner)
    : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) =>
            new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
            new TestAsyncEnumerable<TElement>(expression);

        public object Execute(Expression expression) => inner.Execute(expression)!;

        public TResult Execute<TResult>(Expression expression) =>
            inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression,
            CancellationToken ct = default)
        {
            var result = Execute(expression);
            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(typeof(TResult).GetGenericArguments()[0])
                .Invoke(null, [result])!;
        }
    }
}
