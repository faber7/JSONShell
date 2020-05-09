using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skell.Types
{
    public class Array : ISkellIndexable, IEnumerator, IEnumerable
    {
        private readonly List<ISkellType> contents;

        public Array()
        {
            contents = new List<ISkellType>();
        }

        public Array(ISkellType value)
        {
            contents = new List<ISkellType>();
            contents.Add(value);
        }

        public Array(ISkellType[] values)
        {
            contents = new List<ISkellType>();
            foreach (var value in values)
                contents.Add(value);
        }

        public Number Count() => new Number(contents.Count);

        public ISkellType[] ListIndices()
        {
            return Enumerable.Range(0, contents.Count)
                    .ToList()
                    .Select((i) => new Skell.Types.Number(i))
                    .ToArray();
        }

        public ISkellType[] ListValues() => contents.ToArray();

        public bool Exists(ISkellType index)
        {
            return (index is Number n && n.isInt && n.integerValue < contents.Count);
        }

        public ISkellType GetMember(ISkellType index) => contents[((Number) index).integerValue];

        public void Replace(ISkellType index, ISkellType value)
        {
            contents[((Number) index).integerValue] = value;
        }

        public bool IsHomogeneous(Specifier type)
        {
            foreach (var element in contents)
                if (!Skell.Interpreter.Utility.MatchType(element, type))
                    return false;
            
            return true;
        }

        /// <summary>
        /// Allows insertion at an index.
        /// </summary>
        /// <remark>
        /// Insertion at Count + 1 => appending to the array
        /// </remark>
        public void Insert(ISkellType index, ISkellType value)
        {
            contents.Insert(((Skell.Types.Number) index).integerValue, value);
        }

        public void Delete(ISkellType index) => contents.RemoveAt(((Number) index).integerValue);

        public ISkellReturnable IndexOf(ISkellType value)
        {
            if (contents.Contains(value)) {
                return value;
            } else {
                return new Skell.Types.None();
            }
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
            if (obj is Skell.Types.Array arr)
                return contents.Count == arr.contents.Count && contents.SequenceEqual(arr.contents);

            return false;
        }
    }
}
