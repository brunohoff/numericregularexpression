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
    internal class NumericCompiledBlock<T> : CompiledBlockBase where T : IConvertible, IComparable<T>
    {
        public T Value { get; protected set; }

        public NumericCompiledBlock(string Value)
            : base(CompiledBlocksTypes.Number)
        {
            if (!string.IsNullOrEmpty(Value))
            {
                this.Value = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(Value);
            }
        }
    }
}
