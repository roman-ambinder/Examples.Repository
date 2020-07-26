using Examples.Repository.Impl.EFCore.Internal.Impl;

namespace Examples.Repository.EFCoreRepositoryTests.Entities
{
    public class PreCallPeopleDbContextProvider : PerCallDbContextProvider
    {
        public PreCallPeopleDbContextProvider()
            : base(new CallbackDbContextFactory(() => new PeopleDbContext()))
        {
        }
    }
}