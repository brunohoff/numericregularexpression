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
    class TokenCompiledBlock<T> : CompiledBlockBase where T : IConvertible, IComparable<T>
    {
        public TokenCompiledBlock(CompiledBlocksTypes Token)
            : base(Token)
        {
        }
    }
}
