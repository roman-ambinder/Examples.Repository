using Examples.Repository.Common.DataTypes;
using Examples.Repository.Common.Interfaces;
using Examples.Repository.Common.VoidImpl;
using Examples.Repository.Impl.EFCore.Internal;
using Examples.Repository.Impl.EFCore.Internal.Impl;
using Examples.Repository.Impl.EFCore.Internal.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Examples.Repository.Impl.EFCore
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class EFCoreRepositoryOf<TEntity, TKey> :
        IRepositoryCombinedOperationsFor<TEntity, TKey>
        where TEntity : class, new()
    {
        private readonly IDbContextProvider _dbContextProvider;
        private readonly IKeyValueValidatorOf<TKey, TEntity> _keyValueValidator;
        private readonly IPrimaryKeyExpressionBuilder<TEntity, TKey> _primaryKeyExpressionBuilder;

        public EFCoreRepositoryOf(IDbContextProvider dbContextProvider,
            IKeyValueValidatorOf<TKey, TEntity> keyValueValidator = null)
        {
            _dbContextProvider = dbContextProvider;
            _keyValueValidator = keyValueValidator ?? new VoidKeyValueValidatorOf<TKey, TEntity>();
            _primaryKeyExpressionBuilder = new PrimaryKeyExpressionBuilder<TEntity, TKey>();
        }

        public Task<OperationResultOf<TEntity>> TryGetSingleAsync(TKey key,
             CancellationToken cancellation = default,
             params Expression<Func<TEntity, object>>[] toBeIncluded)
        {
            var validationOpRes = _keyValueValidator.Validate(key);
            if (!validationOpRes)
                return Task.FromResult(validationOpRes.AsFailedOpResOf<TEntity>());

            return _dbContextProvider.TryUseAsync(dbSession =>
                InternalTryGetSingleAsync(dbSession, key,
                    trackChanges: false,
                    cancellation,
                    toBeIncluded));
        }

        public Task<OperationResultOf<IReadOnlyCollection<TEntity>>> TryGetMultipleAsync(
            Expression<Func<TEntity, bool>> filter,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] toBeIncluded)
        {
            return _dbContextProvider.TryUseAsync(async dbSession =>
            {
                var query = dbSession.Set<TEntity>()
                   .AsNoTracking()
                   .Where(filter)
                   .AppendIncludeExpressions(toBeIncluded);

                IReadOnlyCollection<TEntity> foundEntities =
                 await query.ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);

                var success = foundEntities != null && foundEntities.Count > 0;

                if (success)
                    return foundEntities.AsSuccessfulOpRes();

                return "Failed to find any filter matching entities"
                    .AsFailedOpResOf<IReadOnlyCollection<TEntity>>();
            });
        }

        public Task<OperationResultOf<TEntity>> TryAddAsync(
            Action<TEntity> initAction = null,
            CancellationToken cancellationToken = default)
        {
            var newEntity = new TEntity();
            initAction?.Invoke(newEntity);
            return TryAddAsync(newEntity, cancellationToken);
        }

        public Task<OperationResultOf<TEntity>> TryAddAsync(TEntity newEntity,
            CancellationToken cancellationToken = default)
        {
            return _dbContextProvider.TryUseAsync(async dbSession =>
            {
                var validationOpRes = _keyValueValidator.Validate(newEntity);
                if (!validationOpRes)
                    return validationOpRes.AsFailedOpResOf<TEntity>();

                var entitySet = dbSession.Set<TEntity>();

                await entitySet.AddAsync(newEntity, cancellationToken)
                    .ConfigureAwait(false);

                return await SaveChangesAndReturnResultAsync(dbSession,
                        newEntity,
                        cancellationToken)
                    .ConfigureAwait(false);
            });
        }

        public Task<OperationResultOf<TEntity>> TryUpdateAsync(TKey key,
            Action<TEntity> updateAction,
            CancellationToken cancellationToken = default)
        {
            var validationOpRes = _keyValueValidator.Validate(key);
            if (!validationOpRes)
                return Task.FromResult(validationOpRes.AsFailedOpResOf<TEntity>());

            return _dbContextProvider.TryUseAsync(async dbSession =>
            {
                var getOpRes = await InternalTryGetSingleAsync(dbSession, key,
                        trackChanges: true,
                        cancellationToken)
                    .ConfigureAwait(false);

                if (!getOpRes)
                    return getOpRes;

                var targetEntity = getOpRes.Value;

                updateAction(targetEntity);

                var valueValidationOpRes = _keyValueValidator.Validate(targetEntity);
                if (!valueValidationOpRes)
                    return valueValidationOpRes.AsFailedOpResOf<TEntity>();

                return await SaveChangesAndReturnResultAsync(dbSession, targetEntity, cancellationToken)
                    .ConfigureAwait(false);
            });
        }


        public async Task<OperationResultOf<TEntity>> TryRemoveAsync(TKey key,
            CancellationToken cancellationToken = default)
        {
            var validationOpRes = _keyValueValidator.Validate(key);
            if (!validationOpRes)
                return validationOpRes.AsFailedOpResOf<TEntity>();

            var getOpRes = await TryGetSingleAsync(key, cancellationToken)
                .ConfigureAwait(false);

            if (!getOpRes)
                return getOpRes;

            return await _dbContextProvider.TryUseAsync(async dbSession =>
            {
                var foundEntity = getOpRes.Value;

                var dbSet = dbSession.Set<TEntity>();
                dbSet.Attach(foundEntity);

                dbSet.Remove(foundEntity);

                return await SaveChangesAndReturnResultAsync(dbSession, foundEntity, cancellationToken)
                      .ConfigureAwait(false);
            });
        }

        public async Task<OperationResultOf<TEntity>> TryGetOrAdd(TKey key,
            Func<TEntity> newEntityFactory,
            CancellationToken cancellationToken = default)
        {
            var getOpRes = await TryGetSingleAsync(key, cancellationToken).ConfigureAwait(false);
            if (getOpRes)
                return getOpRes.Value.AsSuccessfulOpRes();

            return await TryAddAsync(newEntityFactory(), cancellationToken).ConfigureAwait(false);
        }

        public async Task<OperationResultOf<TEntity>> TryAddOrUpdateAsync(TKey key,
            Func<TEntity> newEntityFactory,
            Action<TEntity> updateAction,
            CancellationToken cancellationToken = default)
        {
            var getOpRes = await TryGetSingleAsync(key, cancellationToken).ConfigureAwait(false);
            if (!getOpRes)
            {
                var entity = newEntityFactory();
                return await TryAddAsync(entity, cancellationToken).ConfigureAwait(false);
            }

            return await TryUpdateAsync(key, updateAction, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<OperationResultOf<TEntity>> InternalTryGetSingleAsync(
            DbContext dbSession,
            TKey key,
            bool trackChanges,
            CancellationToken cancellation,
            params Expression<Func<TEntity, object>>[] toBeIncluded)
        {
            var query = trackChanges ?
                dbSession.Set<TEntity>().AppendIncludeExpressions(toBeIncluded) :
                dbSession.Set<TEntity>().AsNoTracking().AppendIncludeExpressions(toBeIncluded);

            var filterExpression = _primaryKeyExpressionBuilder.Build(dbSession, key);
            var foundEntity = await query.SingleOrDefaultAsync(filterExpression, cancellation)
                .ConfigureAwait(false);

            var success = foundEntity != null;

            return success
                ? foundEntity.AsSuccessfulOpRes()
                : $"Failed to find '{key}' matching entity'"
                    .AsFailedOpResOf<TEntity>();
        }

        private static async Task<OperationResultOf<TResult>> SaveChangesAndReturnResultAsync<TResult>(
          DbContext dbSession,
          TResult result,
          CancellationToken cancellationToken)
        {
            var dbChangesMade = await dbSession.SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            var success = dbChangesMade > 0;

            return success ? result.AsSuccessfulOpRes() :
                $"Expected for at least a single database modification to be made for {nameof(TEntity)}"
                .AsFailedOpResOf<TResult>();
        }
    }
}