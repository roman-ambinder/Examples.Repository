using Examples.Repository.Common.DataTypes;

namespace Examples.Repository.Common.Interfaces
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