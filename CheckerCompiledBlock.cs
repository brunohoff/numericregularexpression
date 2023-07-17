// ------------------------------------------------------------------
//
// Copyright (c) 2013 Bruno Vier Hoffmeister <bruno at hoffmeister.us>.
// BSB Licence (See the file License.txt for the license details.)
// All rights reserved.
//
// ------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace NumericRegularExpressions
{
    class CheckerCompiledBlock<T> : CompiledBlockBase where T : IConvertible, IComparable<T>
    {
        public CheckerType Type { get; private set; }
        public bool isPercent { get; private set; }
        public T Value { get; private set; }

        public CheckerCompiledBlock(CheckerType Type, string Number, bool isPercent)
            : base(CompiledBlocksTypes.Equals)
        {
            this.Type = Type;
            this.isPercent = isPercent;
            if (isPercent)
            {
                dynamic Base100 = 100;
                this.Value = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(Number) / Base100;
            }
            else
            {
                this.Value = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(Number);
            }
        }
    }

    enum CheckerType
    { 
        Equals,
        EqualsOrGreaterThan,
        GreaterThan,
        EqualsOrLessThan,
        LessThan
    }
}
