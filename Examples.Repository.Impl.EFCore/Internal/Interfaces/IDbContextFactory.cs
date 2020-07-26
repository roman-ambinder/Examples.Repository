using Microsoft.EntityFrameworkCore;

namespace Examples.Repository.Impl.EFCore.Internal.Interfaces
{
    public interface IDbContextFactory
    {
        DbContext Create();
    }
}