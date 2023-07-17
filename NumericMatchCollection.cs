using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NumericRegularExpressions
{
    public class NumericMatchCollection<T> : ICollection, IEnumerable where T : IComparable<T>, IConvertible
    {
        private NumericMatch<T>[] itens;

        public int Count { get; private set; }

        public bool IsReadOnly { get; private set; }

        public bool IsSynchronized { get; private set; }

        public object SyncRoot { get; private set; }

        public NumericMatchCollection(NumericMatch<T>[] itens)
        {
            this.itens = itens;
        }

        public virtual NumericMatch<T> this[int i]
        {
            get
            {
                return itens[i];
            }
        }

        public void CopyTo(Array array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            return itens.GetEnumerator();
        }
    }
}
