using Examples.Repository.Common.DataTypes;
using Examples.Repository.Common.Interfaces;

namespace Examples.Repository.Common.VoidImpl
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