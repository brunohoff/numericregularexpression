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
    [Serializable]
    class NumericRegexLexicalException : NumericRegexException
    {
        public int Position { get; private set; }

        public NumericRegexLexicalException(string Message, int Position) : base(Message)
        {
            this.Position = Position;
        }
    }
}
