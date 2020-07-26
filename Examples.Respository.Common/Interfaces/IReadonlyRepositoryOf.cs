using Examples.Repository.Common.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Examples.Repository.Common.Interfaces
{
    public interface IReadonlyRepositoryOf<TEntity, in TKey>
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="toBeIncluded"></param>
        /// <returns></returns>
        Task<OperationResultOf<TEntity>> TryGetSingleAsync(TKey key,
             CancellationToken cancellationToken = default,
             params Expression<Func<TEntity, object>>[] toBeIncluded);

        /// <summary>
        ///
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="toBeIncluded"></param>
        /// <returns></returns>
        Task<OperationResultOf<IReadOnlyCollection<TEntity>>> TryGetMultipleAsync(
            Expression<Func<TEntity, bool>> filter,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] toBeIncluded);
    }
}