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
        public OperationResult Validate(in TValue value) => OperationResult.Successful;

        public OperationResult Validate(in TKey key) => OperationResult.Successful;
    }
}
