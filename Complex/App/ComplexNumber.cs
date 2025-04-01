using System;

namespace ComplexCalcSeparated
{
    /// <summary>
    /// Класс комплексного числа a + i*b
    /// </summary>
    public class ComplexNumber
    {
        public double Real { get; set; }
        public double Imag { get; set; }

        public ComplexNumber(double real = 0, double imag = 0)
        {
            Real = real;
            Imag = imag;
        }

        // Текстовое представление: "a i* b" (при b>=0) или "a i* -b" (при b<0)
        public override string ToString()
        {
            if (double.IsNaN(Real) || double.IsInfinity(Real))
                return $"{Real.ToString("G")} i* {Imag.ToString("G")}";

            double b = Imag;
            if (double.IsNaN(b) || double.IsInfinity(b))
                return $"{Real} i* {b.ToString("G")}";

            if (b >= 0)
                return $"{Real} i* {b}";
            else
                return $"{Real} i* -{Math.Abs(b)}";
        }

        // Операторы + - * /
        public static ComplexNumber operator +(ComplexNumber x, ComplexNumber y)
        {
            return new ComplexNumber(x.Real + y.Real, x.Imag + y.Imag);
        }
        public static ComplexNumber operator -(ComplexNumber x, ComplexNumber y)
        {
            return new ComplexNumber(x.Real - y.Real, x.Imag - y.Imag);
        }
        public static ComplexNumber operator *(ComplexNumber x, ComplexNumber y)
        {
            double real = x.Real * y.Real - x.Imag * y.Imag;
            double imag = x.Real * y.Imag + x.Imag * y.Real;
            return new ComplexNumber(real, imag);
        }
        public static ComplexNumber operator /(ComplexNumber x, ComplexNumber y)
        {
            double denom = y.Real * y.Real + y.Imag * y.Imag;
            if (denom == 0) throw new DivideByZeroException("Деление на 0 (комплексное)");
            double real = (x.Real * y.Real + x.Imag * y.Imag) / denom;
            double imag = (x.Imag * y.Real - x.Real * y.Imag) / denom;
            return new ComplexNumber(real, imag);
        }

        // Унарные операции
        public ComplexNumber Square()
        {
            double real = Real * Real - Imag * Imag;
            double imag = 2 * Real * Imag;
            return new ComplexNumber(real, imag);
        }

        public ComplexNumber Reciprocal()
        {
            double denom = Real * Real + Imag * Imag;
            if (denom == 0)
                throw new DivideByZeroException("Комплексное число равно 0, обратного нет");
            return new ComplexNumber(Real / denom, -Imag / denom);
        }

        // Модуль, аргумент
        public double Modulus() => Math.Sqrt(Real * Real + Imag * Imag);

        public double ArgumentRadians() => Math.Atan2(Imag, Real);

        public double ArgumentDegrees() => ArgumentRadians() * 180.0 / Math.PI;

        // Возведение в степень
        public ComplexNumber Power(int n)
        {
            if (n == 0) return new ComplexNumber(1, 0);
            ComplexNumber result = new ComplexNumber(Real, Imag);
            if (n < 0)
            {
                result = result.Power(-n);
                return result.Reciprocal();
            }
            ComplexNumber temp = new ComplexNumber(Real, Imag);
            for (int i = 1; i < n; i++)
                result = result * temp;
            return result;
        }

        // Корни n-й степени
        public ComplexNumber[] Roots(int n)
        {
            if (n <= 0)
                throw new ArgumentException("Степень корня должна быть > 0");
            var arr = new ComplexNumber[n];
            double r = Modulus();
            double theta = ArgumentRadians();
            double rootR = Math.Pow(r, 1.0 / n);
            for (int k = 0; k < n; k++)
            {
                double rootTheta = (theta + 2 * Math.PI * k) / n;
                double real = rootR * Math.Cos(rootTheta);
                double imag = rootR * Math.Sin(rootTheta);
                arr[k] = new ComplexNumber(real, imag);
            }
            return arr;
        }
    }
}
