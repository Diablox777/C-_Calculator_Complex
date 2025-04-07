using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ComplexCalcSeparated;
using System.Reflection;
using System.Windows.Forms;
using System.Reflection.Emit;

namespace ComplexCalcSeparatedTests
{
    [TestClass]
    public class CalculatorCoreBranchCoverageTests
    {
        private CalculatorCore calc;

        [TestInitialize]
        public void Setup()
        {
            calc = new CalculatorCore();
        }

        [TestMethod]
        public void TestPressDigit_BasicEntry()
        {
            // Проверяем ветку, когда вводим первую цифру.
            calc.PressDigit("5");
            Assert.AreEqual("5", calc.DisplayText, "Первый ввод цифры должен заменять '0'.");
        }

        [TestMethod]
        public void TestPressDigit_SecondDigit()
        {
            // Проверяем ветку, когда вводим вторую цифру (isNewEntry = false).
            calc.PressDigit("3");   // Первый символ
            calc.PressDigit("7");   // Второй символ
            Assert.AreEqual("37", calc.DisplayText, "Ожидалось склеивание цифр.");
        }

        [TestMethod]
        public void TestPressI_NewEntry()
        {
            // Проверяем ветку, когда сразу нажимаем "i" на новом вводе
            calc.PressI();
            Assert.AreEqual("0 i* ", calc.DisplayText, "Должна добавляться мнимая часть при новом вводе.");
        }

        [TestMethod]
        public void TestPressI_AlreadyContainsI()
        {
            // Проверяем ветку, когда в DisplayText уже есть "i*".
            calc.PressI(); // первая вставка
            calc.PressI(); // повторная должна игнорироваться
            Assert.AreEqual("0 i* ", calc.DisplayText,
                "При повторном нажатии 'i', когда 'i*' уже есть, изменений не должно быть.");
        }

        [TestMethod]
        public void TestPressOperator_FirstOperator()
        {
            // Первая операция
            calc.PressDigit("3");
            calc.PressOperator("+");
            Assert.AreEqual("3", calc.DisplayText, "Дисплей должен показывать 3 после установки первого операнда.");
        }

        [TestMethod]
        public void TestPressOperator_ApplyPendingOperator()
        {
            // Проверяем ветку, когда уже есть одна операция и мы вводим вторую.
            calc.PressDigit("3");
            calc.PressOperator("+");
            calc.PressDigit("2");
            calc.PressOperator("*"); // здесь должна сработать ApplyPendingOperator
            Assert.AreEqual("5 i* 0", calc.DisplayText,
                "После нажатия второго оператора должна произойти предыдущая операция (3+2=5).");
        }

        [TestMethod]
        public void TestPressOperator_AfterEquals()
        {
            // Ветка, когда lastOpEquals = true, и сразу жмём новый оператор
            calc.PressDigit("3");
            calc.PressOperator("+");
            calc.PressDigit("2");
            calc.PressEquals(); // Получили 5
            Assert.AreEqual("5 i* 0", calc.DisplayText);

            // теперь сразу новый оператор
            calc.PressOperator("*"); // lastOpEquals => сбрасывается
            calc.PressDigit("4");
            calc.PressEquals();
            Assert.AreEqual("20 i* 0", calc.DisplayText, "Результат 5*4=20");
        }

        [TestMethod]
        public void TestPressEquals_NoPendingOperator_RepeatedEquals()
        {
            // Ветка: pendingOperator == null => проверяем повторное =
            calc.PressDigit("3");
            calc.PressOperator("+");
            calc.PressDigit("2");
            calc.PressEquals(); // теперь pendingOperator сбросился
            Assert.AreEqual("5 i* 0", calc.DisplayText);

            // жмём равно повторно
            calc.PressEquals(); // Должно повториться +2
            Assert.AreEqual("7 i* 0", calc.DisplayText,
                "Повторное нажатие '=' должно повторять последнюю операцию: 5+2=7");
        }

        [TestMethod]
        public void TestPressEquals_PendingOperator_DivisionByZero()
        {
            // Проверяем ветвь, где при вычислении произойдёт ошибка (деление на ноль).
            calc.PressDigit("1");
            calc.PressOperator("/");
            calc.PressDigit("0");
            calc.PressEquals();
            StringAssert.StartsWith(calc.DisplayText, "Ошибка:", "Должна возникать ошибка деления на ноль.");
        }

        [TestMethod]
        public void TestPressClear()
        {
            // Проверяем сброс всех полей
            calc.SetDisplayTextDirect("123");
            calc.PressOperator("+");
            calc.PressClear();
            Assert.AreEqual("0", calc.DisplayText);
            Assert.IsFalse(calc.MemoryHasValue);
        }

        [TestMethod]
        public void TestPressBackspace_NothingToDelete()
        {
            // Ветка: isNewEntry == true, значит стирать нечего
            calc.PressBackspace();
            Assert.AreEqual("0", calc.DisplayText, "Backspace не должен менять '0', если ввод нового числа.");
        }

        [TestMethod]
        public void TestPressBackspace_Deleting()
        {
            // Удаляем последний символ
            calc.PressDigit("1");
            calc.PressDigit("2");
            calc.PressBackspace();
            Assert.AreEqual("1", calc.DisplayText, "Backspace должен удалить последний символ.");
        }

        [TestMethod]
        public void TestPressSqr_NoOperatorInProgress()
        {
            // Возведение в квадрат
            calc.SetDisplayTextDirect("2");
            calc.PressSqr();
            Assert.AreEqual("4 i* 0", calc.DisplayText, "2^2 = 4");
        }

        [TestMethod]
        public void TestPressRev_Normal()
        {
            // Проверяем обратное число 1/(1+2i)
            calc.SetDisplayTextDirect("1 i* 2");
            calc.PressRev();
            // Результат 1 / (1+2i) = 0.2 - 0.4i
            Assert.IsTrue(calc.DisplayText.Contains("0,2") || calc.DisplayText.Contains("0.2"),
                "Результат должен содержать 0.2 (реальная часть).");
            Assert.IsTrue(calc.DisplayText.Contains("-0,4") || calc.DisplayText.Contains("-0.4"),
                "Результат должен содержать -0.4 (мнимая часть).");
        }

        [TestMethod]
        public void TestPressRev_DivisionByZero()
        {
            // Проверка исключения при обратном числе от 0
            calc.SetDisplayTextDirect("0");
            calc.PressRev();
            StringAssert.StartsWith(calc.DisplayText, "Ошибка:", "Обратное к нулю – ошибка.");
        }

        [TestMethod]
        public void TestPressMdl()
        {
            // Модуль (1+2i) = sqrt(1+4)= sqrt(5)
            calc.SetDisplayTextDirect("1 i* 2");
            double mod = calc.PressMdl();
            Assert.AreEqual(Math.Sqrt(5), mod, 1e-9, "Модуль должен совпадать со sqrt(5).");
        }

        [TestMethod]
        public void TestPressPower()
        {
            // (1+i)² = (1+i)*(1+i)=1+2i+i²=1+2i-1=2i => то есть 1 + i => Power(2)= 2i
            calc.SetDisplayTextDirect("1 i* 1");
            calc.PressPower(2);
            // Ожидаем "0 i* 2" (или приблизительно)
            StringAssert.Contains(calc.DisplayText, "i* 2", "Должна получиться чисто мнимая часть 2i.");
        }

        [TestMethod]
        public void TestPressRoot()
        {
            // Корень 2 степени из 4 (4+0i)
            calc.SetDisplayTextDirect("4");
            var roots = calc.PressRoot(2);
            Assert.AreEqual(2.0, roots[0].Real, 1e-9, "Ожидается главный корень 2 + 0i.");
            Assert.IsTrue(roots.Length == 2, "У числа 4 два квадратных корня (2, -2).");
            // В дисплее тоже первый корень
            Assert.AreEqual("2 i* 0", calc.DisplayText, "Должен показываться главный корень.");
        }
    }

    [TestClass]
    public class ComplexNumberBranchCoverageTests
    {
        #region ToString()

        [TestMethod]
        public void TestToString_ImaginaryZero()
        {
            var c = new ComplexNumber(3.5, 0.0);
            Assert.AreEqual("3,5 i* 0", c.ToString());
        }


        [TestMethod]
        public void TestToString_ImagPositive()
        {
            // Если мнимая часть положительная.
            var c = new ComplexNumber(2.0, 3.0);
            Assert.AreEqual("2 i* 3", c.ToString());
        }

        [TestMethod]
        public void TestToString_ImagNegative()
        {
            // Если мнимая часть отрицательная, ожидается формат "a i* -b".
            var c = new ComplexNumber(2.0, -3.0);
            Assert.AreEqual("2 i* -3", c.ToString());
        }

        #endregion

        #region Арифметические операторы

        [TestMethod]
        public void TestOperator_Add()
        {
            var a = new ComplexNumber(1, 2);
            var b = new ComplexNumber(3, 4);
            var sum = a + b;
            Assert.AreEqual(4, sum.Real);
            Assert.AreEqual(6, sum.Imag);
        }

        [TestMethod]
        public void TestOperator_Subtract()
        {
            var a = new ComplexNumber(5, 6);
            var b = new ComplexNumber(3, 4);
            var diff = a - b;
            Assert.AreEqual(2, diff.Real);
            Assert.AreEqual(2, diff.Imag);
        }

        [TestMethod]
        public void TestOperator_Multiply()
        {
            // (a+ib)*(c+id) = (ac-bd) + i(ad+bc)
            var a = new ComplexNumber(2, 3);
            var b = new ComplexNumber(4, 5);
            var prod = a * b;
            Assert.AreEqual(2 * 4 - 3 * 5, prod.Real);
            Assert.AreEqual(2 * 5 + 3 * 4, prod.Imag);
        }

        [TestMethod]
        public void TestOperator_Divide_Normal()
        {
            var a = new ComplexNumber(4, 6);
            var b = new ComplexNumber(1, 2); // знаменатель не равен 0
            var div = a / b;
            double denom = 1 * 1 + 2 * 2;
            Assert.AreEqual((4 * 1 + 6 * 2) / denom, div.Real, 1e-9);
            Assert.AreEqual((6 * 1 - 4 * 2) / denom, div.Imag, 1e-9);
        }

        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void TestOperator_Divide_DivideByZero()
        {
            var a = new ComplexNumber(1, 1);
            var b = new ComplexNumber(0, 0);
            var res = a / b; // должно выбросить исключение
        }

        #endregion

        #region Унарные операции

        [TestMethod]
        public void TestSquare()
        {
            // (3+4i)^2 = 9-16 + 2*3*4i = -7+24i
            var c = new ComplexNumber(3, 4);
            var sq = c.Square();
            Assert.AreEqual(-7, sq.Real, 1e-9);
            Assert.AreEqual(24, sq.Imag, 1e-9);
        }

        [TestMethod]
        public void TestReciprocal_Normal()
        {
            // Проверка обратного числа 1/(1+2i)
            var c = new ComplexNumber(1, 2);
            var rec = c.Reciprocal();
            double denom = 1 * 1 + 2 * 2; // 5
            Assert.AreEqual(1 / 5.0, rec.Real, 1e-9);
            Assert.AreEqual(-2 / 5.0, rec.Imag, 1e-9);
        }

        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void TestReciprocal_Zero()
        {
            // Если число равно 0, Reciprocal должен выбрасывать исключение.
            var c = new ComplexNumber(0, 0);
            var rec = c.Reciprocal();
        }

        [TestMethod]
        public void TestModulus()
        {
            // Для 3+4i модуль равен 5.
            var c = new ComplexNumber(3, 4);
            Assert.AreEqual(5, c.Modulus(), 1e-9);
        }

        [TestMethod]
        public void TestArguments()
        {
            // Для числа (1, sqrt(3)) угол должен быть ≈ PI/3 радиан, т.е. ≈ 60 градусов.
            var c = new ComplexNumber(1, Math.Sqrt(3));
            double rad = c.ArgumentRadians();
            double deg = c.ArgumentDegrees();
            Assert.AreEqual(Math.PI / 3, rad, 1e-9);
            Assert.AreEqual(60, deg, 1e-9);
        }

        [TestMethod]
        public void TestPower_ZeroExponent()
        {
            // Любое число в степени 0 равно 1.
            var c = new ComplexNumber(5, -3);
            var p = c.Power(0);
            Assert.AreEqual(1, p.Real, 1e-9);
            Assert.AreEqual(0, p.Imag, 1e-9);
        }

        [TestMethod]
        public void TestPower_PositiveExponent()
        {
            // (1+i)^3 = -2+2i
            var c = new ComplexNumber(1, 1);
            var p = c.Power(3);
            Assert.AreEqual(-2, p.Real, 1e-9);
            Assert.AreEqual(2, p.Imag, 1e-9);
        }

        [TestMethod]
        public void TestPower_NegativeExponent()
        {
            // (1+i)^-1 должно равняться обратному числу для (1+i)
            var c = new ComplexNumber(1, 1);
            var p = c.Power(-1);
            var rec = c.Reciprocal();
            Assert.AreEqual(rec.Real, p.Real, 1e-9);
            Assert.AreEqual(rec.Imag, p.Imag, 1e-9);
        }

        #endregion

        #region Корни

        [TestMethod]
        public void TestRoots_Normal()
        {
            // Для 4+0i квадратные корни должны быть 2 и -2.
            var c = new ComplexNumber(4, 0);
            var roots = c.Roots(2);
            Assert.AreEqual(2, roots.Length);
            // Проверяем, что один из корней имеет действительную часть, близкую к 2.
            bool found = false;
            foreach (var r in roots)
            {
                if (Math.Abs(r.Real - 2) < 1e-9)
                    found = true;
            }
            Assert.IsTrue(found, "Должен быть корень равный 2.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestRoots_InvalidDegree()
        {
            // Если степень корня меньше или равна 0, должно выбрасываться исключение.
            var c = new ComplexNumber(1, 1);
            var roots = c.Roots(0);
        }

        #endregion
    }

    [TestClass]
    public class MemoryTests
    {
        #region Тесты для ComplexNumber

        [TestMethod]
        public void TestMemoryComplex_Default()
        {
            // По умолчанию значение должно быть 0+0, что считается отсутствием значения.
            var mem = new Memory<ComplexNumber>();
            Assert.IsFalse(mem.HasValue, "Default memory for ComplexNumber should not have a value (0+0).");
            var value = mem.Value as ComplexNumber;
            Assert.AreEqual(0, value.Real, 1e-9);
            Assert.AreEqual(0, value.Imag, 1e-9);
        }

        [TestMethod]
        public void TestMemoryComplex_Store()
        {
            // При сохранении ненулевого комплексного числа memory.HasValue должно стать true.
            var mem = new Memory<ComplexNumber>();
            var c = new ComplexNumber(1.0, 2.0);
            mem.Store(c);
            Assert.IsTrue(mem.HasValue, "After storing a non-zero value, memory should have a value.");
            var value = mem.Value as ComplexNumber;
            Assert.AreEqual(1.0, value.Real, 1e-9);
            Assert.AreEqual(2.0, value.Imag, 1e-9);
        }

        [TestMethod]
        public void TestMemoryComplex_Clear()
        {
            // После сохранения значения и последующего вызова Clear память должна возвращаться к 0+0.
            var mem = new Memory<ComplexNumber>();
            mem.Store(new ComplexNumber(3.0, 4.0));
            Assert.IsTrue(mem.HasValue, "Memory should have a value after storing.");
            mem.Clear();
            Assert.IsFalse(mem.HasValue, "Memory should be cleared (0+0 is considered as no value).");
            var value = mem.Value as ComplexNumber;
            Assert.AreEqual(0, value.Real, 1e-9);
            Assert.AreEqual(0, value.Imag, 1e-9);
        }

        [TestMethod]
        public void TestMemoryComplex_Add()
        {
            // Начальное значение 0+0; после прибавления (1,2) должно получиться (1,2)
            var mem = new Memory<ComplexNumber>();
            mem.Add(new ComplexNumber(1.0, 2.0));
            Assert.IsTrue(mem.HasValue, "Memory should have a value after adding a non-zero complex number.");
            var value = mem.Value as ComplexNumber;
            Assert.AreEqual(1.0, value.Real, 1e-9);
            Assert.AreEqual(2.0, value.Imag, 1e-9);

            // Прибавляем (2,3): (1+2, 2+3) = (3,5)
            mem.Add(new ComplexNumber(2.0, 3.0));
            value = mem.Value as ComplexNumber;
            Assert.AreEqual(3.0, value.Real, 1e-9);
            Assert.AreEqual(5.0, value.Imag, 1e-9);
        }

        #endregion

        #region Тесты для int

        [TestMethod]
        public void TestMemoryInt_Default()
        {
            // Для типа int по умолчанию 0 считается отсутствующим значением.
            var mem = new Memory<int>();
            Assert.IsFalse(mem.HasValue, "Default memory for int should not have a value (0).");
            Assert.AreEqual(0, mem.Value);
        }

        [TestMethod]
        public void TestMemoryInt_StoreAndClear()
        {
            // При сохранении ненулевого значения memory.HasValue должно стать true.
            var mem = new Memory<int>();
            mem.Store(10);
            Assert.IsTrue(mem.HasValue, "After storing a non-zero int, memory should have a value.");
            Assert.AreEqual(10, mem.Value);

            // При вызове Clear значение возвращается к 0 и HasValue становится false.
            mem.Clear();
            Assert.IsFalse(mem.HasValue, "Memory should be cleared to default (0) and considered as no value.");
            Assert.AreEqual(0, mem.Value);
        }

        [TestMethod]
        public void TestMemoryInt_Add()
        {
            // Начинаем с 0, прибавляем 5 => получаем 5.
            var mem = new Memory<int>();
            mem.Add(5);
            Assert.IsTrue(mem.HasValue, "Memory should have a value after adding a non-zero int.");
            Assert.AreEqual(5, mem.Value);

            // Прибавляем еще 3: 5+3 = 8.
            mem.Add(3);
            Assert.AreEqual(8, mem.Value);
        }

        #endregion
    }
}