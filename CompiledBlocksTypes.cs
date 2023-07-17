// ------------------------------------------------------------------
//
// Copyright (c) 2013 Bruno Vier Hoffmeister <bruno at hoffmeister.us>.
// BSB Licence (See the file License.txt for the license details.)
// All rights reserved.
//
// ------------------------------------------------------------------

namespace NumericRegularExpressions
{
    internal enum CompiledBlocksTypes
    {
        StartAnchor,
        EndAnchor,
        Number,
        Integer,
        Percent,
        OpenParentheses,
        CloseParentheses,
        OpenLimiter,
        CloseLimiter,
        SeparatorColon,
        SeparatorSemiColon,
        OpenInterval,
        CloseInterval,
        Function,
        Asterisk,
        Plus,
        Question,
        GreaterThan,
        LessThan,
        Equals,
        EOF
    }
}
