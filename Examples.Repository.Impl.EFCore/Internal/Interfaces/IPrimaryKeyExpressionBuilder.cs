using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace Examples.Repository.Impl.EFCore.Internal
{
    public interface IPrimaryKeyExpressionBuilder<TEntity, TKey>
    {
        Expression<Func<TEntity, bool>> Build(DbContext dbContext, in TKey key);
    }
}