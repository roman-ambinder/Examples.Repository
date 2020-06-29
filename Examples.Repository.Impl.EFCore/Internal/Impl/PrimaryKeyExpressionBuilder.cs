using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Examples.Repository.Impl.EFCore.Internal
{
    internal sealed class PrimaryKeyExpressionBuilder<TEntity, TKey> :
        IPrimaryKeyExpressionBuilder<TEntity, TKey>
    {
        public Expression<Func<TEntity, bool>> Build(
            DbContext dbContext,
            in TKey id)
        {
            var propertyName = dbContext
                .Model.FindEntityType(typeof(TEntity))
                .FindPrimaryKey().Properties
                .Select(x => x.Name)
                .Single();

            var item = Expression.Parameter(typeof(TEntity), "entity");
            var property = Expression.Property(item, propertyName);
            var value = Expression.Constant(id);
            var equals = Expression.Equal(property, value);
            var filter = Expression.Lambda<Func<TEntity, bool>>(equals, item);

            return filter;
        }
    }
}