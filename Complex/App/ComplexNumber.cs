using System;

namespace ComplexCalcSeparated
{
    public class ComplexNumber
    {
        public double Real { get; set; }
        public double Imag { get; set; }

        public ComplexNumber(double real = 0, double imag = 0)
        {
            Real = real;
            Imag = imag;
        }

        public string ToString(DisplayFormat format)
        {
            if (double.IsNaN(Real) || double.IsInfinity(Real))
                return $"{Real.ToString("G")} i* {Imag.ToString("G")}";

            if (double.IsNaN(Imag) || double.IsInfinity(Imag))
                return $"{Real} i* {Imag.ToString("G")}";

            if (format == DisplayFormat.Real && Imag == 0)
                return Real.ToString();

            if (Imag >= 0)
                return $"{Real} i* {Imag}";
            else
                return $"{Real} i* -{Math.Abs(Imag)}";
        }

        public override string ToString()
        {
            return ToString(DisplayFormat.Complex);
        }

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

        public double Modulus() => Math.Sqrt(Real * Real + Imag * Imag);

        public double ArgumentRadians() => Math.Atan2(Imag, Real);

        public double ArgumentDegrees() => ArgumentRadians() * 180.0 / Math.PI;

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