using Examples.Repository.Impl.EFCore.Internal.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Examples.Repository.Common.DataTypes;

namespace Examples.Repository.Impl.EFCore.Internal.Impl
{
    public class PerCallDbContextProvider : IDbContextProvider
    {
        private readonly IDbContextFactory _dbContextFactory;

        public PerCallDbContextProvider(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<OperationResultOf<TResult>> TryUseAsync<TResult>(
            Func<DbContext, Task<OperationResultOf<TResult>>> usage)
        {
            try
            {
                await using var dbSession = _dbContextFactory.Create();
                return await usage(dbSession)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new OperationResultOf<TResult>(ex);
            }
        }

        public async Task<OperationResult> TryMigrateAsync(bool recreate = false)
        {
            try
            {
                await using var context = _dbContextFactory.Create();

                if (recreate)
                    await context.Database.EnsureDeletedAsync().ConfigureAwait(false);

                var newDbCreated = await context.Database.EnsureCreatedAsync();
                //if (newDbCreated)
                //await context.Database.MigrateAsync();

                return OperationResult.Successful;
            }
            catch (Exception ex) { return new OperationResult(ex); }
        }
    }
}