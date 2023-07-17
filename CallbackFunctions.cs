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
    static class CallbackFunctions<T> where T : IConvertible, IComparable<T>
    {
        public static bool asc(T[] Parameters, T CurrentValue, T LastValue, List<T> Values)
        {
            return ((dynamic)CurrentValue > LastValue);
        }

        public static bool desc(T[] Parameters, T CurrentValue, T LastValue, List<T> Values)
        {
            return ((dynamic)CurrentValue < LastValue);
        }

        public static bool avgasc(T[] Parameters, T CurrentValue, T LastValue, List<T> Values)
        {
            dynamic Avg = Values[0];
            for (int i = 1; i < Values.Count; i++)
            {
                Avg += Values[i];
            }
            Avg = Avg / Values.Count;
            return (Avg >= CurrentValue);
        }

        public static bool avgdesc(T[] Parameters, T CurrentValue, T LastValue, List<T> Values)
        {
            dynamic Avg = Values[0];
            for (int i = 1; i < Values.Count; i++)
            {
                Avg += Values[i];
            }
            Avg = Avg / Values.Count;
            return (Avg <= CurrentValue);
        }

        public static bool mavgasc(T[] Parameters, T CurrentValue, T LastValue, List<T> Values)
        {
            int Param = (int)Convert.ChangeType(Parameters[0], typeof(int));
            int Max = ((Param > Values.Count) ? 0 : Values.Count - Param);
            dynamic Avg = CurrentValue;
            for (int i = Values.Count - 2; i >= Max; i--)
            {
                Avg += Values[i];
            }
            Avg = Avg / (Values.Count-Max);
            return (Avg <= CurrentValue);
        }

        public static bool mavgdesc(T[] Parameters, T CurrentValue, T LastValue, List<T> Values)
        {
            int Param = (int)Convert.ChangeType(Parameters[0], typeof(int));
            int Max = ((Param > Values.Count) ? 0 : Values.Count - Param);
            dynamic Avg = CurrentValue;
            for (int i = Values.Count - 2; i >= Max; i--)
            {
                Avg += Values[i];
            }
            Avg = Avg / (Values.Count - Max);
            return (Avg >= CurrentValue);
        }

        public static bool cmavgasc(T[] Parameters, T CurrentValue, T LastValue, List<T> Values)
        {
            int Param = (int)Convert.ChangeType(Parameters[0], typeof(int));
            int Max = ((Param > Values.Count) ? 0 : Values.Count - Param);
            dynamic Avg = CurrentValue;
            for (int i = Values.Count - 2; i >= Max; i--)
            {
                Avg += Values[i];
            }
            Avg = Avg / (Values.Count - Max);
            return ((Avg - Parameters[1]) <= CurrentValue);
        }

        public static bool cmavgdesc(T[] Parameters, T CurrentValue, T LastValue, List<T> Values)
        {
            int Param = (int)Convert.ChangeType(Parameters[0], typeof(int));
            int Max = ((Param > Values.Count) ? 0 : Values.Count - Param);
            dynamic Avg = CurrentValue;
            for (int i = Values.Count - 2; i >= Max; i--)
            {
                Avg += Values[i];
            }
            Avg = Avg / (Values.Count - Max);
            return ((Avg + Parameters[1]) >= CurrentValue);
        }

        public static bool cavg(T[] Parameters, T CurrentValue, T LastValue, List<T> Values)
        {
            return ((dynamic)CurrentValue + Parameters[0] > LastValue);
        }

        public static bool cdesc(T[] Parameters, T CurrentValue, T LastValue, List<T> Values)
        {
            return ((dynamic)CurrentValue - Parameters[0] < LastValue);
        }

        public static bool min(T[] Parameters, T CurrentValue, T LastValue, List<T> Values)
        { 
            dynamic maxValue = CurrentValue;
            for (int i = 0; i < Values.Count; i++)
            {
                if (Values[i] > maxValue)
                {
                    maxValue = Values[i];
                }
            }

            return ((dynamic)(maxValue - CurrentValue) / maxValue) >= (dynamic)Parameters[0] / 100;
        }

        public static bool max(T[] Parameters, T CurrentValue, T LastValue, List<T> Values)
        {
            dynamic minValue = CurrentValue;
            for (int i = 0; i < Values.Count; i++)
            {
                if (Values[i] < minValue)
                {
                    minValue = Values[i];
                }
            }

            return ((dynamic)(minValue - CurrentValue) / minValue) <= (dynamic)Parameters[0] / 100;
        }
    }
}
