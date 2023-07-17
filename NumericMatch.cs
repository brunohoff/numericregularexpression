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
    public class NumericMatch<T> where T : IConvertible, IComparable<T>
    {
        public bool Success { get; private set; }
        public int Index { get; private set; }
        public int Length { get; private set; }

        public T[] Value { get; private set; }
        public NumericGroup<T>[] Groups { get; private set; }

        internal NumericMatch(int Index = -1, int Length = -1, T[] NumericList = null, NumericGroup<T>[] Groups = null)
        {
            Success = (Index >= 0);
            if (Index >= 0)
            {
                this.Index = Index;
                this.Length = ((Index + Length > NumericList.Length) ? NumericList.Length - Index : Length);
                Value = new T[this.Length];
                Array.Copy(NumericList, Index, this.Value, 0, this.Length);
                this.Groups = Groups;
            }
        }
    }
}
