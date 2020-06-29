using Examples.Repository.Impl.EFCore;
using Examples.Repository.Impl.EFCore.Internal;
using Examples.Repository.Impl.EFCore.Internal.Impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Examples.Repository.EFCoreRepositoryTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var repository = new EFCoreRepositoryOf<Person, int>(
                new PerCallDbContextProvider(new CallbackDbContextFactory(() => new UniversityDb())));

            var addOpRes = await repository.TryAddAsync(student => student.Name = "Roman Ambinder")
                .ConfigureAwait(false);



        }
    }

    public class UniversityDb : DbContext
    {
        public DbSet<Person> People { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}