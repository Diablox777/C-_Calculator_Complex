using System;

namespace ComplexCalcSeparated
{
    public class Memory<T>
    {
        private T _value;
        public T Value => _value;

        public Memory()
        {
            _value = default(T);
            if (typeof(T) == typeof(ComplexNumber))
            {
                _value = (T)(object)new ComplexNumber(0, 0);
            }
        }

        public void Store(T val)
        {
            _value = val;
        }

        public void Clear()
        {
            _value = default(T);
            if (typeof(T) == typeof(ComplexNumber))
            {
                _value = (T)(object)new ComplexNumber(0, 0);
            }
        }

        public void Add(T val)
        {
            dynamic curr = _value;
            dynamic addend = val;
            _value = (T)(curr + addend);
        }

        public bool HasValue
        {
            get
            {
                if (typeof(T) == typeof(ComplexNumber))
                {
                    var value = _value as ComplexNumber;
                    return !(Math.Abs(value.Real) < 1e-14 && Math.Abs(value.Imag) < 1e-14);
                }
                return !Equals(_value, default(T));
            }
        }
    }
}
