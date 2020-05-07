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
                    var indices = new Skell.Types.Array(
                        Enumerable.Range(0, contents.Count)
                        .Select(i => (Skell.Types.ISkellType) new Skell.Types.Number(i))
                        .ToArray()
                    );
                    throw new Skell.Problems.IndexOutOfRange(index, indices);
                }
            }
            throw new Skell.Problems.UnexpectedType(index, typeof(Skell.Types.Number));
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

        public static Number operator +(Array a) => throw new Skell.Problems.InvalidOperation("+", a);
        public static Number operator -(Array a) => throw new Skell.Problems.InvalidOperation("-", a);
        public static Number operator +(Array a, Array b) => throw new Skell.Problems.InvalidOperation("+", a, b);
        public static Number operator -(Array a, Array b) => throw new Skell.Problems.InvalidOperation("-", a, b);
        public static Number operator *(Array a, Array b) => throw new Skell.Problems.InvalidOperation("*", a, b);
        public static Number operator /(Array a, Array b) => throw new Skell.Problems.InvalidOperation("/", a, b);

        public static Boolean operator !(Array a) => throw new Skell.Problems.InvalidOperation("!", a);
        public static Boolean operator ==(Array a1, Array a2) => new Boolean(a1.Equals(a2));
        public static Boolean operator !=(Array a1, Array a2) => new Boolean(!a1.Equals(a2));
        public static Boolean operator >(Array a, Array b) => throw new Skell.Problems.InvalidOperation(">", a, b);
        public static Boolean operator >=(Array a, Array b) => throw new Skell.Problems.InvalidOperation(">=", a, b);
        public static Boolean operator <(Array a, Array b) => throw new Skell.Problems.InvalidOperation("<", a, b);
        public static Boolean operator <=(Array a, Array b) => throw new Skell.Problems.InvalidOperation("<=", a, b);

        public override int GetHashCode() => contents.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is Skell.Types.Array arr) {
                return contents.Count == arr.contents.Count && contents.SequenceEqual(arr.contents);
            }
            return false;
        }
    }
}
