// ------------------------------------------------------------------
//
// Copyright (c) 2013 Bruno Vier Hoffmeister <bruno at hoffmeister.us>.
// BSB Licence (See the file License.txt for the license details.)
// All rights reserved.
//
// ------------------------------------------------------------------


namespace NumericRegularExpressions
{
    internal abstract class CompiledBlockBase
    {
        public CompiledBlocksTypes Token { get; private set; }

        public CompiledBlockBase(CompiledBlocksTypes Token)
        {
            this.Token = Token;
        }
    }
}
