﻿using Examples.Respository.Common.DataTypes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Examples.Repository.Impl.EFCore.Internal
{
    /// <summary>
    /// 
    /// </summary>
    public class PerCallDbContextProvider : IDbContextProvider
    {
        private readonly IDbContextFactory _dbContextFactory;

        public PerCallDbContextProvider(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<OperationResultOf<TResult>> TryUseAsync<TResult>(
            Func<DbContext, Task<TResult>> usage)
        {
            try
            {
                TResult res;
                using (var dbSession = _dbContextFactory.Create())
                {
                    res = await usage(dbSession)
                        .ConfigureAwait(false);
                }

                return res.AsSuccessfullOpRes();
            }
            catch (Exception ex)
            {
                return ex.AsFailedOpRes<TResult>();
            }
        }
    }
}