using Examples.Respository.Common.DataTypes;
using Examples.Respository.Common.Interfaces;

namespace Examples.Respository.Common.VoidImpl
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class VoidKeyValueValidatorOf<TKey, TValue> :
        IKeyValueValidatorOf<TKey, TValue>
    {
        public ref readonly OperationResult Validate(in TValue value) => ref OperationResult.Successful;

        public ref readonly OperationResult Validate(in TKey key) => ref OperationResult.Successful;
    }
}