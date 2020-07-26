using System.Diagnostics.CodeAnalysis;
using Examples.Repository.EFCoreRepositoryTests.Entities;
using Examples.Repository.Impl.EFCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Examples.Repository.EFCoreRepositoryTests
{
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class EFCoreRepositoryNonHierarchicalEntity_CRUDTests
    {
        [TestMethod]
        public async Task NonExistingPerson_Add_Added()
        {
            //Arrange
            var repository = await TryGetRepositoryAsync().ConfigureAwait(false);

            //Act
            var opRes = await repository.TryAddAsync(newEntity: CreatePerson()).ConfigureAwait(false);

            //Assert
            Assert.IsTrue(opRes, opRes.ErrorMessage);
        }

        [TestMethod]
        public async Task ExistingPerson_Get_ReturnedExistingPerson()
        {
            //Arrange
            var repository = await TryGetRepositoryAsync().ConfigureAwait(false);
            var addOpRes = await repository.TryAddAsync(newEntity: CreatePerson()).ConfigureAwait(false);
            Assert.IsTrue(addOpRes, addOpRes.ErrorMessage);

            //Act
            var getOpRes = await repository.TryGetSingleAsync(addOpRes.Value.Id)
                .ConfigureAwait(false);

            //Assert
            Assert.IsTrue(getOpRes, getOpRes.ErrorMessage);
            Assert.AreEqual(getOpRes.Value, addOpRes.Value);
        }

        [TestMethod]
        public async Task ExistingPerson_Update_Updated()
        {
            //Arrange
            const string updatedValue = "Updated";
            var repository = await TryGetRepositoryAsync().ConfigureAwait(false);
            var addOpRes = await repository.TryAddAsync(newEntity: CreatePerson()).ConfigureAwait(false);
            Assert.IsTrue(addOpRes, addOpRes.ErrorMessage);
            var existingEntityId = addOpRes.Value.Id;
            var getOpRes = await repository.TryGetSingleAsync(existingEntityId).ConfigureAwait(false);
            Assert.IsTrue(getOpRes, getOpRes.ErrorMessage);
            Assert.AreEqual(getOpRes.Value, addOpRes.Value);

            //Act
            var updateOpRes = await repository.TryUpdateAsync(existingEntityId,
                p =>
                {
                    p.FirstName = updatedValue;
                    p.LastName = updatedValue;
                }).ConfigureAwait(false);

            //Assert
            Assert.IsTrue(updateOpRes, updateOpRes.ErrorMessage);
            getOpRes = await repository.TryGetSingleAsync(existingEntityId).ConfigureAwait(false);
            Assert.AreEqual(getOpRes.Value.FirstName, updatedValue);
            Assert.AreEqual(getOpRes.Value.LastName, updatedValue);
        }


        [TestMethod]
        public async Task ExistingPerson_Remove_Removed()
        {
            //Arrange
            var repository = await TryGetRepositoryAsync().ConfigureAwait(false);
            var addOpRes = await repository.TryAddAsync(newEntity: CreatePerson()).ConfigureAwait(false);
            var existingEntityId = addOpRes.Value.Id;

            //Act
            var removeOpRes = await repository.TryRemoveAsync(existingEntityId)
                .ConfigureAwait(false);

            //Assert
            Assert.IsTrue(removeOpRes, removeOpRes.ErrorMessage);
            var getOpRes = await repository.TryGetSingleAsync(existingEntityId).ConfigureAwait(false);
            Assert.IsFalse(getOpRes);
        }

        private static Person CreatePerson() => new Person(age: 10, firstName: "Roman", lastName: "Ambinder");

        private static async Task<EFCoreRepositoryOf<Person, int>> TryGetRepositoryAsync()
        {
            var dbContextProvider = new PreCallPeopleDbContextProvider();
            var repository = new EFCoreRepositoryOf<Person, int>(dbContextProvider);
            await dbContextProvider.TryMigrateAsync(recreate: true).ConfigureAwait(false);
            return repository;
        }
    }
}