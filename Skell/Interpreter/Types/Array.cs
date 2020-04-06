using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skell.Types
{
    public class Array : ISkellIndexableType, IEnumerator, IEnumerable
    {
        private readonly List<ISkellType> contents;

        public Array()
        {
            contents = new List<ISkellType>();
        }

        public Array(ISkellType[] values)
        {
            contents = new List<ISkellType>();
            foreach (var value in values) {
                contents.Add(value);
            }
        }

        public int Count() => contents.Count;

        public ISkellType GetMember(ISkellType index)
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

        public static bool operator ==(Skell.Types.Array a1, Skell.Types.Array a2)
        {
            return a1.Equals(a2);
        }

        public static bool operator !=(Skell.Types.Array a1, Skell.Types.Array a2)
        {
            return !a1.Equals(a2);
        }

        public override int GetHashCode() => contents.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is Skell.Types.Array arr) {
                return contents.SequenceEqual(arr.contents);
            }
            return false;
        }
    }
}