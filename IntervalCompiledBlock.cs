// ------------------------------------------------------------------
//
// Copyright (c) 2013 Bruno Vier Hoffmeister <bruno at hoffmeister.us>.
// BSB Licence (See the file License.txt for the license details.)
// All rights reserved.
//
// ------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NumericRegularExpressions
{
    class IntervalCompiledBlock<T> : CompiledBlockBase where T : IConvertible, IComparable<T>
    {
        public T StartNumber { get; protected set; }
        public T EndNumber { get; protected set; }
        public T StartPercent { get; protected set; }
        public T EndPercent { get; protected set; }
        public string Function { get; protected set; }
        public T[] Parameters { get; protected set; }
        public bool isStartNumber { get; protected set; }
        public bool isEndNumber { get; protected set; }

        public IntervalCompiledBlock(string StartNumber = null, string EndNumber = null, string StartPercent = null, string EndPercent = null, string Function = null, T[] Parameters = null)
            :base(CompiledBlocksTypes.OpenInterval)
        {
            this.Function = Function;
            this.Parameters = Parameters;
            dynamic Base100 = 100;

            if (!string.IsNullOrEmpty(StartNumber))
            {
                isStartNumber = true;
                this.StartNumber = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(StartNumber);
            }
            if (!string.IsNullOrEmpty(EndNumber))
            {
                isEndNumber = true;
                this.EndNumber = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(EndNumber);
            }
            if (!string.IsNullOrEmpty(StartPercent))
            {
                isStartNumber = false;
                this.StartPercent = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(StartPercent) / Base100;
            }
            if (!string.IsNullOrEmpty(EndPercent))
            {
                isEndNumber = false;
                this.EndPercent = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(EndPercent) / Base100;
            }
        }
    }
}
