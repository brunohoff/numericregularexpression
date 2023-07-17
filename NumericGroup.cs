// ------------------------------------------------------------------
//
// Copyright (c) 2013 Bruno Vier Hoffmeister <bruno at hoffmeister.us>.
// BSB Licence (See the file License.txt for the license details.)
// All rights reserved.
//
// ------------------------------------------------------------------

using System;

namespace NumericRegularExpressions
{
    public class NumericGroup<T> where T : IConvertible, IComparable<T>
    {
        public T[] Captures { get; private set; }

        internal NumericGroup(T[] Captures)
        {
            this.Captures = Captures;
        }
    }
}
