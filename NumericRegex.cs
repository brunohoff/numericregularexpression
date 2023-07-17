// ------------------------------------------------------------------
//
// Copyright (c) 2013 Bruno Vier Hoffmeister <bruno at hoffmeister.us>.
// BSB Licence (See the file License.txt for the license details.)
// All rights reserved.
//
// ------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NumericRegularExpressions
{
    /// <summary>
    /// Represents a immutable Numeric Regular Expression
    /// </summary>
    /// <typeparam name="T">The numeric type of the elements analysed in the regular expression</typeparam>
    public class NumericRegex<T> where T : IConvertible, IComparable<T>
    {
        public delegate bool FunctionCallback(T[] Parameters, T CurrentValue, T LastValue, List<T> Values);

        private string Pattern;
        private CompiledNumericRegex<T> CompiledPattern;
        private Dictionary<string, CallbackFunction<T>> Functions;

        private static Hashtable CompiledCache = new Hashtable();
        private static Dictionary<string, CallbackFunction<T>> DefaultFunctions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Pattern">The Numeric Regular Expression Pattern</param>
        public NumericRegex(string Pattern)
        {
            //Checking for multiplication operator
            try
            {
                var c = Expression.Constant(default(T), typeof(T));
                Expression.Multiply(c, c); // Throws an exception if + is not defined
            }
            catch
            {
                throw new NumericRegexException(string.Format("The type {0} doesn't implement * operator", typeof(T).Name));
            }

            //Checking for division operator
            try
            {
                var c = Expression.Constant(default(T), typeof(T));
                Expression.Divide(c, c); // Throws an exception if + is not defined
            }
            catch
            {
                throw new NumericRegexException(string.Format("The type {0} doesn't implement / operator", typeof(T).Name));
            }

            //Checking for subtract operator
            try
            {
                var c = Expression.Constant(default(T), typeof(T));
                Expression.Subtract(c, c); // Throws an exception if + is not defined
            }
            catch
            {
                throw new NumericRegexException(string.Format("The type {0} doesn't implement - operator", typeof(T).Name));
            }

            this.Pattern = Pattern;
            CompiledPattern = CompilePattern(Pattern);
            Functions = LoadDefaultFunctions();
        }

        /// <summary>
        /// Determines whether the Numeric Regular Expressions contains the specific function name.
        /// </summary>
        /// <param name="FunctionName">The function name to locate</param>
        /// <returns></returns>
        public bool ContainsFunction(string FunctionName)
        {
            return Functions.ContainsKey(FunctionName);
        }

        /// <summary>
        /// Add a specific function to the numeric regular expression
        /// </summary>
        /// <param name="FunctionName">Function name</param>
        /// <param name="Function">Callback delegate</param>
        /// <param name="Parameters">Parameters count</param>
        public void AddFunction(string FunctionName, FunctionCallback Function, int Parameters = 0)
        {
            if (Functions.ContainsKey(FunctionName))
            {
                throw new NumericRegexException(string.Format("{0} already exists", FunctionName));
            }
            Functions.Add(FunctionName, new CallbackFunction<T>(FunctionName, Function, Parameters));
        }

        /// <summary>
        /// Search the specified input numeric list for the first occurrence of the numeric regular expression specified in the NumericRegex constructor.
        /// </summary>
        /// <param name="NumericList">The numeric list to search a match</param>
        /// <returns></returns>
        public NumericMatch<T> Match(T[] NumericList)
        {
            ExecuteCompiledNumericRegex<T> e = new ExecuteCompiledNumericRegex<T>(CompiledPattern, NumericList, 0, Functions);
            return e.FindNext();
        }

        /// <summary>
        /// Search the specified input numeric list for the first occurrence of the numeric regular expression specified in the NumericRegex constructor.
        /// </summary>
        /// <param name="NumericList">The numeric list to search a match</param>
        /// <param name="Index">Start position</param>
        /// <returns></returns>
        public NumericMatch<T> Match(T[] NumericList, int Index)
        {
            ExecuteCompiledNumericRegex<T> e = new ExecuteCompiledNumericRegex<T>(CompiledPattern, NumericList, 0, Functions);
            e.SetArrayPosition(Index);
            return e.FindNext();
        }


        /// <summary>
        /// Search the specified input numeric list for the first occurrence of the numeric regular expression specified in the NumericRegex constructor.
        /// </summary>
        /// <param name="NumericList">The string numeric list to search a match. The list will be converted to specified type.</param>
        /// <returns></returns>
        public NumericMatch<T> Match(string[] NumericList)
        {
            return Match(Array.ConvertAll<string, T>(NumericList, delegate(string str) { return (T)Convert.ChangeType(str, typeof(T)); }));
        }

        /// <summary>
        /// Search the specified input numeric list for all occurrences of the numeric regular expression specified in the NumericRegex constructor.
        /// </summary>
        /// <param name="NumericList">The numeric list to search a match</param>
        /// <returns></returns>
        public NumericMatchCollection<T> Matches(T[] NumericList)
        {
            List<NumericMatch<T>> Matches = new List<NumericMatch<T>>();
            ExecuteCompiledNumericRegex<T> e = new ExecuteCompiledNumericRegex<T>(CompiledPattern, NumericList, 0, Functions);

            NumericMatch<T> Match;
            while ((Match = e.FindNext()).Success)
            {
                Matches.Add(Match);
            }

            return new NumericMatchCollection<T>(Matches.ToArray());
        }

        /// <summary>
        /// Search the specified input numeric list for all occurrences of the numeric regular expression specified in the NumericRegex constructor.
        /// </summary>
        /// <param name="NumericList">The string numeric list to search a match. The list will be converted to specified type.</param>
        /// <returns></returns>
        public NumericMatchCollection<T> Matches(string[] NumericList)
        {
            return Matches(Array.ConvertAll<string, T>(NumericList, delegate(string str) { return (T)Convert.ChangeType(str, typeof(T)); }));
        }

        /// <summary>
        /// Within a specified input numeric list, replaces all entries that match a specified numeric regular expression.
        /// </summary>
        /// <param name="Input">Input numeric list</param>
        /// <param name="Replacement">Replacement numeric list</param>
        /// <returns></returns>
        public T[] Replace(T[] Input, T[] Replacement)
        {
            List<T> Result = new List<T>(Input);
            ExecuteCompiledNumericRegex<T> e = new ExecuteCompiledNumericRegex<T>(CompiledPattern, Input, 0, Functions);

            NumericMatch<T> Match;
            int off = 0;
            while ((Match = e.FindNext()).Success)
            {
                Result.RemoveRange(Match.Index + off, Match.Length);
                Result.InsertRange(Match.Index + off, Replacement);
                off = off + Replacement.Length - Match.Length;
            }

            return Result.ToArray();
        }

        /// <summary>
        /// Within a specified input numeric list, replaces all entries that match a specified numeric regular expression.
        /// </summary>
        /// <param name="Input">Input numeric list. The list will be converted to specified type.</param>
        /// <param name="Replacement">Replacement numeric list. The list will be converted to specified type.</param>
        /// <returns></returns>
        public T[] Replace(string[] Input, string[] Replacement)
        {
            return Replace(Array.ConvertAll<string, T>(Input, delegate(string str) { return (T)Convert.ChangeType(str, typeof(T)); }), Array.ConvertAll<string, T>(Replacement, delegate(string str) { return (T)Convert.ChangeType(str, typeof(T)); }));
        }

        /// <summary>
        /// Indicates whether the regular expressions specified in the NumericRegex constructor finds a match in a specified input numeric list.
        /// </summary>
        /// <param name="NumericList">Input numeric list.</param>
        /// <returns></returns>
        public bool IsMatch(T[] NumericList)
        {
            NumericMatch<T> match = this.Match(NumericList);
            return (match.Index == 0 && match.Length == NumericList.Length);
        }

        /// <summary>
        /// Indicates whether the regular expressions specified in the NumericRegex constructor finds a match in a specified input numeric list.
        /// </summary>
        /// <param name="NumericList">Input numeric list.The list will be converted to specified type.</param>
        /// <returns></returns>
        public bool IsMatch(string[] NumericList)
        {
            NumericMatch<T> match = this.Match(NumericList);
            return (match.Index == 0 && match.Length == NumericList.Length);
        }

        /// <summary>
        /// Indicates whether the regular expressions specified in the NumericRegex constructor finds a match in a specified input numeric list.
        /// </summary>
        /// <param name="Pattern">A Numeric Regular Expression pattern</param>
        /// <param name="NumericList">Input numeric list.</param>
        /// <returns></returns>
        public static bool IsMatch(string Pattern, T[] NumericList)
        {
            NumericRegex<T> r = new NumericRegex<T>(Pattern);
            return r.IsMatch(NumericList);
        }

        /// <summary>
        /// Indicates whether the regular expressions specified in the NumericRegex constructor finds a match in a specified input numeric list.
        /// </summary>
        /// <param name="Pattern">A Numeric Regular Expression pattern</param>
        /// <param name="NumericList">Input numeric list.The list will be converted to specified type.</param>
        /// <returns></returns>
        public static bool IsMatch(string Pattern, string[] NumericList)
        {
            NumericRegex<T> r = new NumericRegex<T>(Pattern);
            return r.IsMatch(NumericList);
        }

        /// <summary>
        /// Compile and cache a numeric regular expression pattern
        /// </summary>
        /// <param name="Pattern">A numeric regular expression pattern</param>
        /// <returns></returns>
        private static CompiledNumericRegex<T> CompilePattern(string Pattern)
        {
            lock (CompiledCache)
            {
                if (CompiledCache.ContainsKey(Pattern))
                {
                    return (CompiledNumericRegex<T>)CompiledCache[Pattern];
                }
            }

            CompiledNumericRegex<T> compiled = new CompiledNumericRegex<T>(Pattern);
            CompiledCache.Add(Pattern, compiled);
            return compiled;
        }

        /// <summary>
        /// Load the defaults functions
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, CallbackFunction<T>> LoadDefaultFunctions()
        {
            if (DefaultFunctions == null)
            {
                DefaultFunctions = new Dictionary<string, CallbackFunction<T>>();
                DefaultFunctions.Add("asc", new CallbackFunction<T>("asc", CallbackFunctions<T>.asc));
                DefaultFunctions.Add("desc", new CallbackFunction<T>("desc", CallbackFunctions<T>.desc));
                DefaultFunctions.Add("avgasc", new CallbackFunction<T>("avgasc", CallbackFunctions<T>.avgasc));
                DefaultFunctions.Add("avgdesc", new CallbackFunction<T>("avgdesc", CallbackFunctions<T>.avgdesc));
                DefaultFunctions.Add("mavgasc", new CallbackFunction<T>("mavgasc", CallbackFunctions<T>.mavgasc, 1));
                DefaultFunctions.Add("mavgdesc", new CallbackFunction<T>("mavgdesc", CallbackFunctions<T>.mavgdesc, 1));
                DefaultFunctions.Add("cmavgasc", new CallbackFunction<T>("cmavgasc", CallbackFunctions<T>.cmavgasc, 2));
                DefaultFunctions.Add("cmavgdesc", new CallbackFunction<T>("cmavgdesc", CallbackFunctions<T>.cmavgdesc, 2));
                DefaultFunctions.Add("cavg", new CallbackFunction<T>("cavg", CallbackFunctions<T>.cavg, 1));
                DefaultFunctions.Add("cdesc", new CallbackFunction<T>("cdesc", CallbackFunctions<T>.cdesc, 1));
                DefaultFunctions.Add("min", new CallbackFunction<T>("min", CallbackFunctions<T>.min, 1));
                DefaultFunctions.Add("max", new CallbackFunction<T>("max", CallbackFunctions<T>.max, 1));
            }
            return DefaultFunctions;
        }
    }
}
