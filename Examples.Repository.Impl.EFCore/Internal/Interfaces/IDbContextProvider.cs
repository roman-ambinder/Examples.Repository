using Examples.Respository.Common.DataTypes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Examples.Repository.Impl.EFCore.Internal
{
    public interface IDbContextProvider
    {
        Task<OperationResultOf<TResult>> TryUseAsync<TResult>(
            Func<DbContext, Task<TResult>> usage);
    }
}