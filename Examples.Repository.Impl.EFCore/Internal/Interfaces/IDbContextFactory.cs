using Microsoft.EntityFrameworkCore;

namespace Examples.Repository.Impl.EFCore.Internal
{
    public interface IDbContextFactory
    {
        DbContext Create();
    }
}