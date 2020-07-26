﻿using System;

namespace Examples.Repository.Common.DataTypes
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "U2U1004:Public value types should implement equality",
        Justification = "<Pending>")]
    public readonly struct OperationResultOf<TValue>
    {
        public OperationResultOf(Exception ex)
        : this(success: false, value: default, errorMessage: ex?.Message)
        { }

        public OperationResultOf(in TValue value)
            : this(success: true, value: value)
        { }

        public OperationResultOf(
            bool success,
            in TValue value,
            string errorMessage = null)
        {
            Success = success;
            Value = value;
            ErrorMessage = errorMessage;
        }

        public static implicit operator bool(in OperationResultOf<TValue> opRes)
            => opRes.Success;

        public static implicit operator OperationResult(in OperationResultOf<TValue> opRes)
            => new OperationResult(opRes.Success, opRes.ErrorMessage);

        public static implicit operator TValue(in OperationResultOf<TValue> opRes)
            => opRes.Value;

        public override string ToString()
            => $"{nameof(Success)}: {Success},{nameof(Value)}: {Value}, {nameof(ErrorMessage)}: {ErrorMessage}";

        public bool Success { get; }

        public string ErrorMessage { get; }

        public TValue Value { get; }
    }
}