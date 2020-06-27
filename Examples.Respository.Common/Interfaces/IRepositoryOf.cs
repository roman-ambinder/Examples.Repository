using Examples.Respository.Common.DataTypes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Examples.Respository.Common.Interfaces
{
    public interface IRepositoryOf<TEntity, TKey> : IReadonlyRepositoryOf<TEntity, TKey>
        where TEntity : class
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="initAction"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<OperationResultOf<TEntity>> TryAddAsync(
            Func<TEntity, TEntity> initAction = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="updateAction"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<OperationResultOf<TEntity>> TryUpdateAsync(
            TKey key,
            Func<TEntity, TEntity> updateAction,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        Task<OperationResultOf<TEntity>> TryRemoveAsync(
            TKey key,
            CancellationToken cancel = default);
    }
}