using Examples.Respository.Common.DataTypes;

namespace Examples.Respository.Common.Interfaces
{
    public interface IKeyValueValidatorOf<Tkey, TValue>
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
         ref readonly OperationResult Validate(in TValue value);

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        ref readonly OperationResult Validate(in Tkey key);
    }
}