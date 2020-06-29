using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Examples.Repository.Impl.EFCore.Internal
{
    internal static class QueraybleExtensions
    {
        public static IQueryable<TEntity> AppendIncludeExpressions<TEntity>(
          this IQueryable<TEntity> query,
          Expression<Func<TEntity, object>>[] toBeIncluded)
        {
            if (toBeIncluded != null && toBeIncluded.Length > 0)
            {
                foreach (var includeExression in toBeIncluded)
                {
                    query = query.Include(includeExression);
                }
            }

            return query;
        }
    }
}