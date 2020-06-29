using Examples.Repository.Impl.EFCore.Internal;
using Examples.Respository.Common.DataTypes;
using Examples.Respository.Common.Interfaces;
using Examples.Respository.Common.VoidImpl;
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
        IRepositoryOf<TEntity, TKey>
        where TEntity : class, new()
    {
        private readonly IDbContextProvider _dbContextProvider;
        private readonly IKeyValueValidatorOf<TKey, TEntity> _keyValueValidtor;
        private readonly IPrimaryKeyExpressionBuilder<TEntity, TKey> _primaryKeyExpressionBuilder;

        public EFCoreRepositoryOf(IDbContextProvider dbContextProvider,
            IKeyValueValidatorOf<TKey, TEntity> keyVallueValidtor = null)
        {
            _dbContextProvider = dbContextProvider;
            _keyValueValidtor = keyVallueValidtor ?? new VoidKeyValueValidatorOf<TKey, TEntity>();
            _primaryKeyExpressionBuilder = new PrimaryKeyExpressionBuilder<TEntity, TKey>();
        }

        public Task<OperationResultOf<TEntity>> TryGetSingleAsync(TKey key,
             CancellationToken cancellation = default,
             params Expression<Func<TEntity, object>>[] toBeIncluded)
        {
            var validationOpRes = _keyValueValidtor.Validate(key);
            if (!validationOpRes)
                return Task.FromResult(validationOpRes.AsFailedOpResOf<TEntity>());

            return _dbContextProvider.TryUseAsync<TEntity>(async dbSession =>
           {
               var entitySet = dbSession.Set<TEntity>();

               IQueryable<TEntity> query = dbSession.Set<TEntity>()
                 .AsNoTracking()
                 .AppendIncludeExpressions(toBeIncluded);

               var filterExpression = _primaryKeyExpressionBuilder.Build(dbSession, key);
               var foundEntity = await query.SingleOrDefaultAsync(filterExpression, cancellation)
                   .ConfigureAwait(false);

               var success = foundEntity != null;

               return success ? foundEntity.AsSuccessfullOpRes() :
                   $"Failed to find '{key}' matching entity'"
                   .AsFailedOpResOf<TEntity>();
           });
        }

        public async Task<OperationResultOf<IReadOnlyCollection<TEntity>>> TryGetMultipleAsync(
            Expression<Func<TEntity, bool>> filter,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] toBeIncluded)
        {
            return await _dbContextProvider.TryUseAsync(async dbSession =>
             {
                 IQueryable<TEntity> query = dbSession.Set<TEntity>()
                    .AsNoTracking()
                    .Where(filter)
                    .AppendIncludeExpressions(toBeIncluded); ;

                 IReadOnlyCollection<TEntity> foundEntites =
                  await query.ToArrayAsync(cancellationToken)
                     .ConfigureAwait(false);

                 var success = foundEntites != null && foundEntites.Count > 0;

                 if (success)
                     return foundEntites.AsSuccessfullOpRes();

                 return "Failed to find any filter matching entities"
                     .AsFailedOpResOf<IReadOnlyCollection<TEntity>>();
             });
        }

        public Task<OperationResultOf<TEntity>> TryAddAsync(
            Action<TEntity> initAction = null,
            CancellationToken cancellationToken = default)
        {
            return _dbContextProvider.TryUseAsync<TEntity>(async dbSession =>
            {
                var newEntity = new TEntity();
                initAction?.Invoke(newEntity);

                var validationOpRes = _keyValueValidtor.Validate(newEntity);
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

        public async Task<OperationResultOf<TEntity>> TryUpdateAsync(TKey key,
           Action<TEntity> updateAction,
          CancellationToken cancellationToken = default)
        {
            var validationOpRes = _keyValueValidtor.Validate(key);
            if (!validationOpRes)
                return validationOpRes.AsFailedOpResOf<TEntity>();

            var getOpRes = await TryGetSingleAsync(key, cancellationToken)
                 .ConfigureAwait(false);

            if (!getOpRes)
                return getOpRes;

            return await _dbContextProvider.TryUseAsync<TEntity>(async dbSession =>
            {
                var targetEntity = getOpRes.Value;

                updateAction(targetEntity);

                var valueValidationOpRes = _keyValueValidtor.Validate(targetEntity);
                if (!valueValidationOpRes)
                    return valueValidationOpRes.AsFailedOpResOf<TEntity>();

                var dbSet = dbSession.Set<TEntity>();
                dbSet.Attach(targetEntity);

                return await SaveChangesAndReturnResultAsync(dbSession, targetEntity, cancellationToken)
                 .ConfigureAwait(false);
            });
        }

        public async Task<OperationResultOf<TEntity>> TryRemoveAsync(TKey key,
            CancellationToken cancellationToken = default)
        {
            var validationOpRes = _keyValueValidtor.Validate(key);
            if (!validationOpRes)
                return validationOpRes.AsFailedOpResOf<TEntity>();

            var getOpRes = await TryGetSingleAsync(key, cancellationToken)
                .ConfigureAwait(false);

            if (!getOpRes)
                return getOpRes;

            return await _dbContextProvider.TryUseAsync<TEntity>(async dbSession =>
            {
                var foundEntity = getOpRes.Value;

                var dbSet = dbSession.Set<TEntity>();
                dbSet.Attach(foundEntity);

                dbSet.Remove(foundEntity);

                return await SaveChangesAndReturnResultAsync(dbSession, foundEntity, cancellationToken)
                      .ConfigureAwait(false);
            });
        }

        private static async Task<OperationResultOf<TResult>> SaveChangesAndReturnResultAsync<TResult>(
          DbContext dbSession,
          TResult result,
          CancellationToken cancellationToken)
        {
            var dbChangesMade = await dbSession.SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            var success = dbChangesMade > 0;

            return success ? result.AsSuccessfullOpRes() :
                $"Expected for at least a single database modification to be made for {nameof(TEntity)}"
                .AsFailedOpResOf<TResult>();
        }
    }
}