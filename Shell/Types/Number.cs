using System;
using System.Globalization;

namespace Shell.Types
{
    public class Number : IShellData
    {
        public readonly bool isInt;
        public readonly int integerValue;
        public readonly decimal decimalValue;

        public Number()
        {
            isInt = true;
            integerValue = 0;
        }

        public Number(int value)
        {
            isInt = true;
            integerValue = value;
        }

        public Number(decimal value)
        {
            if (value % 1 == 0) {
                isInt = true;
                integerValue = (int) value;
            } else {
                isInt = false;
                decimalValue = value;
            }
        }

        public Number(string token)
        {
            bool result;
            NumberStyles style;
            IFormatProvider provider = CultureInfo.InvariantCulture;

            style = NumberStyles.Integer | NumberStyles.AllowExponent;
            result = int.TryParse(token, style, provider, out integerValue);
            if (result) {
                isInt = true;
                return;
            }

            style = NumberStyles.Float;

            result = decimal.TryParse(token, style, provider, out decimalValue);

            if (result) {
                isInt = false;
                return;
            }
        }

        public static Number operator +(Number a) => a;
        public static Number operator -(Number a) => a.isInt ? new Number(-a.integerValue) : new Number(-a.decimalValue);
        public static Number operator +(Number a, Number b)
        {
            if (a.isInt && b.isInt)
                return new Number(a.integerValue + b.integerValue);
            else if (a.isInt && !b.isInt)
                return new Number(a.integerValue + b.decimalValue);
            else if (!a.isInt && b.isInt)
                return new Number(a.decimalValue + b.integerValue);
            else
                return new Number(a.decimalValue + b.decimalValue);
        }
        public static Number operator -(Number a, Number b)
        {
            if (a.isInt && b.isInt)
                return new Number(a.integerValue - b.integerValue);
            else if (a.isInt && !b.isInt)
                return new Number(a.integerValue - b.decimalValue);
            else if (!a.isInt && b.isInt)
                return new Number(a.decimalValue - b.integerValue);
            else
                return new Number(a.decimalValue - b.decimalValue);
        }
        public static Number operator *(Number a, Number b)
        {
            if (a.isInt && b.isInt)
                return new Number(a.integerValue * b.integerValue);
            else if (a.isInt && !b.isInt)
                return new Number(a.integerValue * b.decimalValue);
            else if (!a.isInt && b.isInt)
                return new Number(a.decimalValue * b.integerValue);
            else
                return new Number(a.decimalValue * b.decimalValue);
        }
        public static Number operator /(Number a, Number b)
        {
            if (a.isInt && b.isInt)
                if (a.integerValue % b.integerValue == 0)
                    return new Number(a.integerValue / b.integerValue);
                else
                    return new Number(decimal.Divide(a.integerValue, b.integerValue));
            else if (a.isInt && !b.isInt)
                return new Number(a.integerValue / b.decimalValue);
            else if (!a.isInt && b.isInt)
                return new Number(a.decimalValue / b.integerValue);
            else
                return new Number(a.decimalValue / b.decimalValue);
        }
        public static Number operator %(Number a, Number b)
        {
            if (a.isInt && b.isInt)
                return new Number(a.integerValue % b.integerValue);
            else if (a.isInt && !b.isInt)
                return new Number(a.integerValue % b.decimalValue);
            else if (!a.isInt && b.isInt)
                return new Number(a.decimalValue % b.integerValue);
            else
                return new Number(a.decimalValue % b.decimalValue);
        }

        public static Boolean operator !(Number a) => throw new Shell.Problems.InvalidOperation("!", a);
        public static Boolean operator ==(Number a, Number b) => new Boolean(a.Equals(b));
        public static Boolean operator !=(Number a, Number b) => new Boolean(!a.Equals(b));
        public static Boolean operator >(Number a, Number b)
        {
            if (a.isInt && b.isInt)
                return new Boolean(a.integerValue > b.integerValue);
            else if (a.isInt && !b.isInt)
                return new Boolean(a.integerValue > b.decimalValue);
            else if (!a.isInt && b.isInt)
                return new Boolean(a.decimalValue > b.integerValue);
            else
                return new Boolean(a.decimalValue > b.decimalValue);
        }
        public static Boolean operator >=(Number a, Number b)
        {
            if (a.isInt && b.isInt)
                return new Boolean(a.integerValue >= b.integerValue);
            else if (a.isInt && !b.isInt)
                return new Boolean(a.integerValue >= b.decimalValue);
            else if (!a.isInt && b.isInt)
                return new Boolean(a.decimalValue >= b.integerValue);
            else
                return new Boolean(a.decimalValue >= b.decimalValue);
        }
        public static Boolean operator <(Number a, Number b)
        {
            if (a.isInt && b.isInt)
                return new Boolean(a.integerValue < b.integerValue);
            else if (a.isInt && !b.isInt)
                return new Boolean(a.integerValue < b.decimalValue);
            else if (!a.isInt && b.isInt)
                return new Boolean(a.decimalValue < b.integerValue);
            else
                return new Boolean(a.decimalValue < b.decimalValue);
        }
        public static Boolean operator <=(Number a, Number b)
        {
            if (a.isInt && b.isInt)
                return new Boolean(a.integerValue <= b.integerValue);
            else if (a.isInt && !b.isInt)
                return new Boolean(a.integerValue <= b.decimalValue);
            else if (!a.isInt && b.isInt)
                return new Boolean(a.decimalValue <= b.integerValue);
            else
                return new Boolean(a.decimalValue <= b.decimalValue);
        }

        public override string ToString() => isInt ? integerValue.ToString() : decimalValue.ToString();
        public override int GetHashCode() => isInt ? integerValue.GetHashCode() : decimalValue.GetHashCode();
        public override bool Equals(object obj) {
            if (obj is Number n)
                if (isInt && n.isInt)
                    return integerValue == n.integerValue;
                else if (!isInt && !n.isInt)
                    return decimalValue == n.decimalValue;
                else if (!isInt && n.isInt)
                    return decimalValue == n.integerValue;
                else
                    return integerValue == n.decimalValue;
                    
            return false;
        }
    }
}
