using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Examples.Repository.Impl.EFCore.Internal
{
    internal static class QueryableExtensions
    {
        public static IQueryable<TEntity> AppendIncludeExpressions<TEntity>(
          this IQueryable<TEntity> query,
          Expression<Func<TEntity, object>>[] toBeIncluded)
            where TEntity : class
        {
            if (toBeIncluded != null && toBeIncluded.Length > 0)
            {
                query = toBeIncluded.Aggregate(query,
                    (current, includeExpression) => current.Include(includeExpression));
            }

            return query;
        }
    }
}