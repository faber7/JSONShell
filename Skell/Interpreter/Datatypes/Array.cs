using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skell.Data
{
    public class Array : ISkellIndexableData, IEnumerator, IEnumerable
    {
        private readonly List<ISkellData> contents;

        public Array()
        {
            contents = new List<ISkellData>();
        }

        public Array(ISkellData[] values)
        {
            contents = new List<ISkellData>();
            foreach (var value in values) {
                contents.Add(value);
            }
        }

        public int Count() => contents.Count;

        public ISkellData GetMember(ISkellData index)
        {
            if (index is Number i && i.isInt) {
                if (i.integerValue < contents.Count) {
                    return contents[i.integerValue];
                } else {
                    throw new System.NotImplementedException();
                }
            }
            throw new System.NotImplementedException();
        }

        public override string ToString() 
        {
            StringBuilder s = new StringBuilder();

            s.Append("[");
            for (int i = 0; i < contents.Count; i++)
            {
                s.Append(contents[i].ToString());
                if (i != contents.Count - 1)
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
            return position < contents.Count;
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