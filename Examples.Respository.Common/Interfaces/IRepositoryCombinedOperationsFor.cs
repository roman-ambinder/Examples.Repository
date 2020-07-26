using System;
using System.Threading;
using System.Threading.Tasks;
using Examples.Repository.Common.DataTypes;

namespace Examples.Repository.Common.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface IRepositoryCombinedOperationsFor<TEntity, in TKey> :
        IRepositoryOf<TEntity, TKey>
        where TEntity : class
    {
        Task<OperationResultOf<TEntity>> TryAddOrUpdateAsync(TKey key,
            Func<TEntity> newEntityFactory,
            Action<TEntity> updateAction,
            CancellationToken cancellationToken = default);

        Task<OperationResultOf<TEntity>> TryGetOrAdd(TKey key,
            Func<TEntity> newEntityFactory,
            CancellationToken cancellationToken = default);
    }
}