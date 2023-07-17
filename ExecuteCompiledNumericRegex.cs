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
    class ExecuteCompiledNumericRegex<T> where T : IConvertible, IComparable<T>
    {
        CompiledNumericRegex<T> CompiledRegex;
        List<T> CaptureList;
        List<NumericGroup<T>> CaptureGroups;
        List<T> IntervalList;
        Dictionary<string, CallbackFunction<T>> Functions;
        T[] NumericList;
        int ArrayPosition;
        bool isInLimiter = false;
        int Position = 0;
        public bool Found { get; private set; }

        public ExecuteCompiledNumericRegex(CompiledNumericRegex<T> CompiledRegex, T[] NumericList, int Count = 0, Dictionary<string, CallbackFunction<T>> Functions = null)
        {
            this.CompiledRegex = CompiledRegex;
            this.NumericList = NumericList;
            this.Functions = Functions;
            ArrayPosition = 0;
            CheckFunctions();
        }

        public void SetArrayPosition(int Position)
        {
            this.ArrayPosition = Position;
        }

        public NumericMatch<T> FindNext()
        {
            CaptureList = null;
            IntervalList = new List<T>();
            CaptureGroups = new List<NumericGroup<T>>();
            isInLimiter = false;
            Position = 0;
            while (ArrayPosition < NumericList.Length)
            {
                Position = ArrayPosition;
                if (ExecuteBlocks(CompiledRegex.Blocks))
                {
                    if (CompiledRegex.EndAnchor && ArrayPosition < NumericList.Length)
                    {
                        break;   
                    }
                    int PositionFounded = ArrayPosition;
                    ArrayPosition = Position + 1;
                    return new NumericMatch<T>(Position, PositionFounded - Position + 1, NumericList, CaptureGroups.ToArray());
                }

                if (CompiledRegex.StartAnchor)
                {
                    break;
                }
                ArrayPosition = Position + 1;
            }

            return new NumericMatch<T>();
        }

        private bool ExecuteBlocks(List<CompiledBlockBase> Blocks)
        {
            for (int i = 0; i < Blocks.Count; i++)
            {
                switch (Blocks[i].Token)
                { 
                    case CompiledBlocksTypes.Number:
                        if (ArrayPosition >= NumericList.Length || ((NumericCompiledBlock<T>)Blocks[i]).Value.CompareTo(NumericList[ArrayPosition]) != 0)
                        {
                            return false;
                        }
                        if (CaptureList != null)
                        {
                            CaptureList.Add(NumericList[ArrayPosition]);
                        }
                        ArrayPosition++;
                        break;
                    case CompiledBlocksTypes.OpenLimiter:
                        dynamic FirstValue;
                        int StartPosition = ArrayPosition;
                        int LimiterCounter = 0;
                        LimiterCompiledBlock<T> Limiter = (LimiterCompiledBlock<T>)Blocks[i];

                        if (Limiter.Blocks[0].Token == CompiledBlocksTypes.OpenParentheses && CaptureList == null)
                        {
                            CaptureList = new List<T>();
                        }
                        isInLimiter = true;
                        while (ExecuteBlocks(Limiter.Blocks) && ArrayPosition <= NumericList.Length)
                        {
                            FirstValue = NumericList[((StartPosition == 0) ? 0 : StartPosition - 1)];
                            LimiterCounter++;

                            if (Limiter.EndPercent != (dynamic)0 && ArrayPosition < NumericList.Length)
                            {
                                dynamic PercentVariation = (NumericList[ArrayPosition] - FirstValue) / FirstValue;
                                if ((PercentVariation > Limiter.EndPercent) ||
                                    (PercentVariation < Limiter.StartPercent))
                                {
                                    LimiterCounter--;
                                    ArrayPosition--;
                                    break;
                                }
                            }

                            if ((Limiter.EndNumber > 0 && LimiterCounter >= Limiter.EndNumber))
                            {
                                break;
                            }
                        }
                        isInLimiter = false;

                        //Limiter post processing
                        IntervalList.Clear();

                        if (LimiterCounter < Limiter.StartNumber)
                        {
                            return false;
                        }

                        //if is a percentage interval, require 1 number at least
                        if (Limiter.EndPercent != (dynamic)0 && LimiterCounter < 1)
                        {
                            return false;
                        }

                        //Add to the group list
                        if (Limiter.Blocks[0].Token == CompiledBlocksTypes.OpenParentheses && CaptureList != null)
                        {
                            CaptureGroups.Add(new NumericGroup<T>(CaptureList.ToArray()));
                            CaptureList = null;
                        }

                        break;
                    case CompiledBlocksTypes.OpenInterval:
                        if (ArrayPosition >= NumericList.Length)
                        {
                            return false;
                        }
                        IntervalCompiledBlock<T> Interval = (IntervalCompiledBlock<T>)Blocks[i];
                        dynamic AtualValue = NumericList[ArrayPosition];
                        dynamic LastValue = ((IntervalList.Count > 0) ? IntervalList[IntervalList.Count - 1] : ((ArrayPosition > 0)) ? NumericList[ArrayPosition - 1] : NumericList[ArrayPosition]);
                        dynamic IPercentVariation = ((NumericList[ArrayPosition] - LastValue) / LastValue);

                        if (Interval.isStartNumber)
                        {
                            if (Interval.StartNumber.CompareTo(NumericList[ArrayPosition]) > 0)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (IPercentVariation < Interval.StartPercent)
                            {
                                return false;
                            }
                        }
                        if (Interval.isEndNumber)
                        {
                            if (Interval.EndNumber.CompareTo(NumericList[ArrayPosition]) < 0)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (IPercentVariation > Interval.EndPercent)
                            {
                                return false;
                            }
                        }

                        IntervalList.Add(NumericList[ArrayPosition]);
                        if (Interval.Function != null)
                        {
                            if (!Functions[Interval.Function].Function(Interval.Parameters, NumericList[ArrayPosition], LastValue, IntervalList))
                            {
                                return false;
                            }
                        }
                        
                        if (CaptureList != null)
                        {
                            CaptureList.Add(NumericList[ArrayPosition]);
                        }

                        ArrayPosition++;
                        break;
                    case CompiledBlocksTypes.Equals:
                        CheckerCompiledBlock<T> Checker = (CheckerCompiledBlock<T>)Blocks[i];
                        int EArrayPosition = ((ArrayPosition == 0) ? 0 : ((this.isInLimiter) ? ArrayPosition : ArrayPosition - 1));
                        if (EArrayPosition >= NumericList.Length)
                        {
                            return false;
                        }
                        dynamic ArrayValue = NumericList[EArrayPosition];
                        if (Checker.isPercent)
                        {
                            if (ArrayPosition < 1)
                            {
                                return false;
                            }
                            ArrayValue = (ArrayValue - (dynamic)NumericList[Position]) / NumericList[Position];
                        }

                        if (Checker.Type == CheckerType.Equals && (ArrayValue.CompareTo(Checker.Value) != 0))
                        {
                            return false;
                        }
                        else if (Checker.Type == CheckerType.EqualsOrGreaterThan && ArrayValue.CompareTo(Checker.Value) < 0)
                        {
                            return false;
                        }
                        else if (Checker.Type == CheckerType.EqualsOrLessThan && ArrayValue.CompareTo(Checker.Value) > 0)
                        {
                            return false;
                        }
                        else if (Checker.Type == CheckerType.GreaterThan && ArrayValue.CompareTo(Checker.Value) <= 0)
                        {
                            return false;
                        }
                        else if (Checker.Type == CheckerType.LessThan && ArrayValue.CompareTo(Checker.Value) >= 0)
                        {
                            return false;
                        }

                        if (this.isInLimiter)
                        {
                            ArrayPosition++;
                        }

                        break;
                }
            }

            return true;
        }

        private void CheckFunctions()
        {
            for (int i = 0; i < CompiledRegex.Functions.Count; i++)
            {
                if (!Functions.ContainsKey(CompiledRegex.Functions[i].Name))
                {
                    throw new NumericRegexException(string.Format("Function {0} not found", CompiledRegex.Functions[i].Name));
                }
                if (Functions[CompiledRegex.Functions[i].Name].Parameters != CompiledRegex.Functions[i].Parameters)
                {
                    throw new NumericRegexException(string.Format("Function arguments {0}. Expected {1}, given {2}", CompiledRegex.Functions[i].Name));
                }
            }
        }
    }

    enum ExecutionState
    { 
        Next,
        NotFound,
        NextBlock
    }
}
