using Examples.Repository.Impl.EFCore.Internal.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace Examples.Repository.Impl.EFCore.Internal.Impl
{
    public class CallbackDbContextFactory : IDbContextFactory
    {
        private readonly Func<DbContext> _factoryCallback;

        public CallbackDbContextFactory(Func<DbContext> factoryCallback)
        {
            _factoryCallback = factoryCallback;
        }

        public DbContext Create() => _factoryCallback();
    }
}