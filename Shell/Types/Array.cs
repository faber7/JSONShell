using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shell.Types
{
    public class Array : IShellIndexable, IEnumerator, IEnumerable
    {
        private readonly List<IShellData> contents;

        public Array()
        {
            contents = new List<IShellData>();
        }

        public Array(IShellData value)
        {
            contents = new List<IShellData>
            {
                value
            };
        }

        public Array(IShellData[] values)
        {
            contents = new List<IShellData>();
            foreach (var value in values)
                contents.Add(value);
        }

        public Number Count() => new Number(contents.Count);

        public IShellData[] ListIndices()
        {
            return Enumerable.Range(0, contents.Count)
                    .ToList()
                    .Select((i) => new Shell.Types.Number(i))
                    .ToArray();
        }

        public IShellData[] ListValues() => contents.ToArray();

        public bool Exists(IShellData index)
        {
            return (index is Number n && n.isInt && n.integerValue < contents.Count);
        }

        public IShellData GetMember(IShellData index) => contents[((Number) index).integerValue];

        public void Replace(IShellData index, IShellData value)
        {
            contents[((Number) index).integerValue] = value;
        }

        public bool IsHomogeneous(Specifier type)
        {
            foreach (var element in contents)
                if (!Shell.Interpreter.Utility.MatchType(element, type))
                    return false;
            
            return true;
        }

        /// <summary>
        /// Allows insertion at an index.
        /// </summary>
        /// <remark>
        /// Insertion at Count => appending to the array
        /// </remark>
        public void Insert(IShellData index, IShellData value)
        {
            contents.Insert(((Shell.Types.Number) index).integerValue, value);
        }

        public void Delete(IShellData index) => contents.RemoveAt(((Number) index).integerValue);

        public IShellReturnable IndexOf(IShellData value)
        {
            if (contents.Contains(value)) {
                return value;
            } else {
                return new Shell.Types.None();
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

        public static Number operator +(Array a) => throw new Shell.Problems.InvalidOperation("+", a);
        public static Number operator -(Array a) => throw new Shell.Problems.InvalidOperation("-", a);
        public static Number operator +(Array a, Array b) => throw new Shell.Problems.InvalidOperation("+", a, b);
        public static Number operator -(Array a, Array b) => throw new Shell.Problems.InvalidOperation("-", a, b);
        public static Number operator *(Array a, Array b) => throw new Shell.Problems.InvalidOperation("*", a, b);
        public static Number operator /(Array a, Array b) => throw new Shell.Problems.InvalidOperation("/", a, b);

        public static Boolean operator !(Array a) => throw new Shell.Problems.InvalidOperation("!", a);
        public static Boolean operator ==(Array a1, Array a2) => new Boolean(a1.Equals(a2));
        public static Boolean operator !=(Array a1, Array a2) => new Boolean(!a1.Equals(a2));
        public static Boolean operator >(Array a, Array b) => throw new Shell.Problems.InvalidOperation(">", a, b);
        public static Boolean operator >=(Array a, Array b) => throw new Shell.Problems.InvalidOperation(">=", a, b);
        public static Boolean operator <(Array a, Array b) => throw new Shell.Problems.InvalidOperation("<", a, b);
        public static Boolean operator <=(Array a, Array b) => throw new Shell.Problems.InvalidOperation("<=", a, b);

        public override int GetHashCode() => contents.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is Shell.Types.Array arr)
                return contents.Count == arr.contents.Count && contents.SequenceEqual(arr.contents);

            return false;
        }
    }
}
