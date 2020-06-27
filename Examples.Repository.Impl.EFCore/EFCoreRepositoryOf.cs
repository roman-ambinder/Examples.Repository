using Examples.Repository.Impl.EFCore.Internal;
using Examples.Respository.Common.DataTypes;
using Examples.Respository.Common.Interfaces;
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
        private readonly Func<DbContext> _dbContextProvider;
        private readonly IKeyValueValidatorOf<TEntity, TKey> _keyVallueValidtor;
        private readonly IPrimaryKeyExpressionBuilder<TEntity, TKey> _primaryKeyExpressionBuilder;

        public EFCoreRepositoryOf(Func<DbContext> dbContextProvider,
            IKeyValueValidatorOf<TEntity, TKey> keyVallueValidtor)
        {
            _dbContextProvider = dbContextProvider;
            _keyVallueValidtor = keyVallueValidtor;
            _primaryKeyExpressionBuilder = new PrimaryKeyExpressionBuilder<TEntity, TKey>();
        }

        public async Task<OperationResultOf<TEntity>> TryGetSingleAsync(TKey key,
             CancellationToken cancellation = default,
             params Expression<Func<TEntity, object>>[] toBeIncluded)
        {
            try
            {
                var validationOpRes = _keyVallueValidtor.Validate(key);
                if (!validationOpRes)
                    return validationOpRes.AsFailedOpResOf<TEntity>();

                using var dbSession = _dbContextProvider();
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
            }
            catch (Exception ex)
            {
                return new OperationResultOf<TEntity>(ex);
            }
        }

        public async Task<OperationResultOf<IReadOnlyCollection<TEntity>>> TryGetMultipleAsync(
            Expression<Func<TEntity, bool>> filter,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] toBeIncluded)
        {
            try
            {
                IReadOnlyCollection<TEntity> foundEntites = null;
                using (var dbSession = _dbContextProvider())
                {
                    IQueryable<TEntity> query = dbSession.Set<TEntity>()
                        .AsNoTracking()
                        .Where(filter)
                        .AppendIncludeExpressions(toBeIncluded); ;

                    foundEntites = await query.ToArrayAsync(cancellationToken)
                        .ConfigureAwait(false);
                }

                var success = foundEntites != null && foundEntites.Count > 0;

                if (success)
                    foundEntites.AsSuccessfullOpRes();

                return success ?
                    foundEntites.AsSuccessfullOpRes() :
                    "Failed to find any filter matching entities"
                    .AsFailedOpResOf<IReadOnlyCollection<TEntity>>();
            }
            catch (Exception ex)
            {
                return new OperationResultOf<IReadOnlyCollection<TEntity>>(ex);
            }
        }

        public async Task<OperationResultOf<TEntity>> TryAddAsync(
            Func<TEntity, TEntity> initAction = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var newEntity = new TEntity();
                newEntity = initAction?.Invoke(newEntity);

                var validationOpRes = _keyVallueValidtor.Validate(newEntity);
                if (!validationOpRes)
                    return validationOpRes.AsFailedOpResOf<TEntity>();

                using var dbSession = _dbContextProvider();
                var entitySet = dbSession.Set<TEntity>();

                await entitySet.AddAsync(newEntity, cancellationToken)
                    .ConfigureAwait(false);

                return await SaveChangesAndReturnResultAsync(dbSession,
                    newEntity,
                    cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new OperationResultOf<TEntity>(ex);
            }
        }

        public async Task<OperationResultOf<TEntity>> TryUpdateAsync(TKey key,
           Func<TEntity, TEntity> updateAction,
          CancellationToken cancellationToken = default)
        {
            try
            {
                var validationOpRes = _keyVallueValidtor.Validate(key);
                if (!validationOpRes)
                    return validationOpRes.AsFailedOpResOf<TEntity>();

                var getOpRes = await TryGetSingleAsync(key, cancellationToken)
                    .ConfigureAwait(false);

                if (!getOpRes)
                    return getOpRes;

                var foundEntity = getOpRes.Value;

                var updatedEntity = updateAction(foundEntity);

                validationOpRes = _keyVallueValidtor.Validate(updatedEntity);
                if (!validationOpRes)
                    return validationOpRes.AsFailedOpResOf<TEntity>();

                using var dbSession = _dbContextProvider();
                var dbSet = dbSession.Set<TEntity>();
                dbSet.Attach(updatedEntity);

                return await SaveChangesAndReturnResultAsync(dbSession, updatedEntity, cancellationToken)
                 .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new OperationResultOf<TEntity>(ex);
            }
        }

        public async Task<OperationResultOf<TEntity>> TryRemoveAsync(TKey key,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var validationOpRes = _keyVallueValidtor.Validate(key);
                if (!validationOpRes)
                    return validationOpRes.AsFailedOpResOf<TEntity>();

                var getOpRes = await TryGetSingleAsync(key, cancellationToken)
                    .ConfigureAwait(false);

                if (!getOpRes)
                    return getOpRes;

                var foundEntity = getOpRes.Value;

                using var dbSession = _dbContextProvider();
                var dbSet = dbSession.Set<TEntity>();
                dbSet.Attach(foundEntity);

                dbSet.Remove(foundEntity);

                return await SaveChangesAndReturnResultAsync(dbSession, foundEntity, cancellationToken)
                      .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new OperationResultOf<TEntity>(ex);
            }
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