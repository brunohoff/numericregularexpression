// ------------------------------------------------------------------
//
// Copyright (c) 2013 Bruno Vier Hoffmeister <bruno at hoffmeister.us>.
// BSB Licence (See the file License.txt for the license details.)
// All rights reserved.
//
// ------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace NumericRegularExpressions
{
    internal class LimiterCompiledBlock<T> : CompiledBlockBase where T : IConvertible, IComparable<T>
    {
        public int StartNumber { get; protected set; }
        public int EndNumber { get; protected set; }
        public T StartPercent { get; protected set; }
        public T EndPercent { get; protected set; }
        public List<CompiledBlockBase> Blocks { get; protected set; }

        public LimiterCompiledBlock(List<CompiledBlockBase> Blocks, string EndNumber = null, string StartNumber = null, string EndPercent = null, string StartPercent = null)
            : base(CompiledBlocksTypes.OpenLimiter)
        {
            this.Blocks = Blocks;

            if (!string.IsNullOrEmpty(StartNumber))
            {
                this.StartNumber = int.Parse(StartNumber);
            }
            if (!string.IsNullOrEmpty(EndNumber))
            {
                this.EndNumber = int.Parse(EndNumber);
            }
            if (!string.IsNullOrEmpty(StartPercent))
            {
                this.StartPercent = (dynamic) Convert.ChangeType(StartPercent, typeof(T)) / (dynamic)100;
            }
            if (!string.IsNullOrEmpty(EndPercent))
            {
                this.EndPercent = (dynamic)Convert.ChangeType(EndPercent, typeof(T)) / (dynamic)100;
            }

        }
    }

}
