using System.Collections;
using System.Linq;
using System.Text;

namespace Skell.Data
{
    public class Array : SkellData, IEnumerator, IEnumerable
    {
        public int Length;
        private SkellData[] contents;

        public Array(SkellData[] values)
        {
            Length = values.Length;
            contents = values;
        }

        public override string ToString() 
        {
            StringBuilder s = new StringBuilder();

            s.Append("[");
            for (int i = 0; i < Length; i++)
            {
                s.Append(contents[i].ToString());
                if (i != Length - 1)
                {
                    s.Append(", ");
                }
            }
            s.Append("]");

            return s.ToString();
        }

        // IEnumerator implementation
        public IEnumerator GetEnumerator()
        {
            return contents.GetEnumerator();
        }

        // IEnumerable implementation
        public object Current => contents[position];
        private int position = -1;

        public bool MoveNext()
        {
            position++;
            return (position < Length);
        }

        public void Reset()
        {
            position = 0;
        }

        public static bool operator ==(Skell.Data.Array a1, Skell.Data.Array a2)
        {
            return a1.Equals(a2);
        }

        public static bool operator !=(Skell.Data.Array a1, Skell.Data.Array a2)
        {
            return !a1.Equals(a2);
        }

        public override int GetHashCode() => contents.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is Skell.Data.Array arr) {
                return contents.SequenceEqual(arr.contents);
            }
            return false;
        }
    }
}