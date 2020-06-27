using Examples.Respository.Common.DataTypes;

namespace Examples.Respository.Common.Interfaces
{
    public interface IKeyValueValidatorOf<TEntity, Tkey>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        OperationResult Validate(TEntity entity);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        OperationResult Validate(Tkey key);
    }
}