using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NumericRegularExpressions
{
    class CallbackFunction<T> where T : IConvertible, IComparable<T>
    {
        public int Parameters { get; private set; }
        public string Name { get; private set; }
        public NumericRegex<T>.FunctionCallback Function { get; private set; }

        public CallbackFunction(string Name, NumericRegex<T>.FunctionCallback Function, int Parameters = 0)
        {
            this.Name = Name;
            this.Function = Function;
            this.Parameters = Parameters;
        }
    }
}
