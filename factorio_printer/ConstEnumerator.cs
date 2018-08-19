using System;
using System.Collections;
using System.Collections.Generic;
namespace FactorioPrinter
{
    class ConstRange<T> : IEnumerable, IEnumerable<T>
    {
        T value;
        int length;
        public ConstRange(T value, int length)
        {
            this.value = value;
            this.length = length;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            int ijk = 0;
            while(ijk++!=length)
                yield return value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            int ijk = 0;
            while(ijk++!=length)
                yield return value;
        }
    }
}