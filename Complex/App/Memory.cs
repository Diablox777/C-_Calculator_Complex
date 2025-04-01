using System;

namespace ComplexCalcSeparated
{
    /// <summary>
    /// Класс памяти (хранит одно значение типа T)
    /// </summary>
    public class Memory<T>
    {
        private T _value;
        public T Value => _value;

        public Memory()
        {
            _value = default(T);
            // Для ComplexNumber явный 0+i0
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
            // Используем dynamic, предполагая оператор + у T
            dynamic curr = _value;
            dynamic addend = val;
            _value = (T)(curr + addend);
        }

        // Флаг непустоты
        public bool HasValue => !Equals(_value, default(T));
    }
}
