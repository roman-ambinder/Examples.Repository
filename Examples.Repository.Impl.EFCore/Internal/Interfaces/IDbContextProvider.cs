using Examples.Repository.Common.DataTypes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Examples.Repository.Impl.EFCore.Internal.Interfaces
{
    public interface IDbContextProvider
    {
        Task<OperationResultOf<TResult>> TryUseAsync<TResult>(
            Func<DbContext, Task<OperationResultOf<TResult>>> usage);

        Task<OperationResult> TryMigrateAsync(bool recreated = false);
    }
}