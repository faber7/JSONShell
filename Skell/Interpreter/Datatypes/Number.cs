namespace Skell.Data
{
    public class Number : ISkellData
    {
        public readonly bool isInt;
        public readonly int integerValue;
        public readonly decimal decimalValue;

        public Number(int value)
        {
            isInt = true;
            integerValue = value;
        }

        public Number(decimal value)
        {
            isInt = false;
            decimalValue = value;
        }

        public Number(string token)
        {
            if (int.TryParse(token, out integerValue)) {
                isInt = true;
            } else if (decimal.TryParse(token, out decimalValue)) {
                isInt = false;
            } else {
                throw new Skell.Error.NativeParseError(token);
            }
        }

        public static Number operator +(Number a) => a;
        public static Number operator +(Number a, Number b)
        {
            if (a.isInt && b.isInt) {
                return new Number(a.integerValue + b.integerValue);
            } else if (a.isInt && !(b.isInt)) {
                return new Number(a.integerValue + b.decimalValue);
            } else if (!(a.isInt) && b.isInt) {
                return new Number(a.decimalValue + b.integerValue);
            } else {
                return new Number(a.decimalValue + b.decimalValue);
            }
        }

        public static Number operator -(Number a) => a.isInt ? 
            new Number(-a.integerValue) : new Number(-a.decimalValue);
        public static Number operator -(Number a, Number b)
        {
            if (a.isInt && b.isInt) {
                return new Number(a.integerValue - b.integerValue);
            } else if (a.isInt && !(b.isInt)) {
                return new Number(a.integerValue - b.decimalValue);
            } else if (!(a.isInt) && b.isInt) {
                return new Number(a.decimalValue - b.integerValue);
            } else {
                return new Number(a.decimalValue - b.decimalValue);
            }
        }

        public static Number operator *(Number a, Number b)
        {
            if (a.isInt && b.isInt) {
                return new Number(a.integerValue * b.integerValue);
            } else if (a.isInt && !(b.isInt)) {
                return new Number(a.integerValue * b.decimalValue);
            } else if (!(a.isInt) && b.isInt) {
                return new Number(a.decimalValue * b.integerValue);
            } else {
                return new Number(a.decimalValue * b.decimalValue);
            }
        }
        public static Number operator /(Number a, Number b)
        {
            if (a.isInt && b.isInt) {
                if (a.integerValue % b.integerValue == 0) {
                    return new Number(a.integerValue / b.integerValue);
                } else {
                    return new Number(decimal.Divide(a.integerValue, b.integerValue));
                }
            } else if (a.isInt && !(b.isInt)) {
                return new Number(a.integerValue / b.decimalValue);
            } else if (!(a.isInt) && b.isInt) {
                return new Number(a.decimalValue / b.integerValue);
            } else {
                return new Number(a.decimalValue / b.decimalValue);
            }
        }

        public static bool operator >(Number a, Number b)
        {
            if (a.isInt && b.isInt) {
                return a.integerValue > b.integerValue;
            } else if (a.isInt && !(b.isInt)) {
                return a.integerValue > b.decimalValue;
            } else if (!(a.isInt) && b.isInt) {
                return a.decimalValue > b.integerValue;
            } else {
                return a.decimalValue > b.decimalValue;
            }
        }
        public static bool operator >=(Number a, Number b)
        {
            if (a.isInt && b.isInt) {
                return a.integerValue >= b.integerValue;
            } else if (a.isInt && !(b.isInt)) {
                return a.integerValue >= b.decimalValue;
            } else if (!(a.isInt) && b.isInt) {
                return a.decimalValue >= b.integerValue;
            } else {
                return a.decimalValue >= b.decimalValue;
            }
        }
        
        public static bool operator <(Number a, Number b)
        {
            if (a.isInt && b.isInt) {
                return a.integerValue < b.integerValue;
            } else if (a.isInt && !(b.isInt)) {
                return a.integerValue < b.decimalValue;
            } else if (!(a.isInt) && b.isInt) {
                return a.decimalValue < b.integerValue;
            } else {
                return a.decimalValue < b.decimalValue;
            }
        }
        public static bool operator <=(Number a, Number b)
        {
            if (a.isInt && b.isInt) {
                return a.integerValue <= b.integerValue;
            } else if (a.isInt && !(b.isInt)) {
                return a.integerValue <= b.decimalValue;
            } else if (!(a.isInt) && b.isInt) {
                return a.decimalValue <= b.integerValue;
            } else {
                return a.decimalValue <= b.decimalValue;
            }
        }
        
        public static bool operator ==(Number a, Number b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(Number a, Number b)
        {
            return !a.Equals(b);
        }

        public override string ToString() => isInt ? integerValue.ToString() : decimalValue.ToString();

        public override int GetHashCode() => isInt ? integerValue.GetHashCode() : decimalValue.GetHashCode();

        public override bool Equals(object obj) {
            if (obj is Number n) {
                if (isInt && n.isInt) {
                    return integerValue == n.integerValue;
                } else if (!isInt && !(n.isInt)) {
                    return decimalValue == n.decimalValue;
                } else if (!isInt && n.isInt) {
                    return decimalValue == n.integerValue;
                } else {
                    return integerValue == n.decimalValue;
                }
            }
            return false;
        }
    }
}
