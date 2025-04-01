using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
// Обязательно подключите пространство имён, в котором у вас лежит CalculatorCore.
// Например:
using ComplexCalcSeparated; // Или ваш namespace, где объявлен CalculatorCore

namespace UnitTest
{
    [TestClass]
    public class CalculatorCoreTests
    {
        [TestMethod]
        public void PressDigit_WhenNewEntry_ResetsDisplay()
        {
            // Arrange
            var calc = new CalculatorCore();
            // Act
            calc.PressDigit("5");
            // Assert
            Assert.AreEqual("5", calc.DisplayText, "Должно отобразиться '5' при первом нажатии цифры");
        }

        [TestMethod]
        public void PressDigit_WhenNotNewEntry_AppendsDigit()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("5"); // isNewEntry=true сначала, вводим "5"
            calc.PressDigit("6"); // теперь isNewEntry=false => добавляет символ
            Assert.AreEqual("56", calc.DisplayText);
        }

        [TestMethod]
        public void PressDigit_Point_NoExistingDot_AddsDot()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("1");
            calc.PressDigit(".");
            Assert.AreEqual("1.", calc.DisplayText);
        }

        [TestMethod]
        public void PressDigit_Point_AlreadyHasDotInRealPart_Ignored()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("1");
            calc.PressDigit("."); // стало "1."
            Assert.AreEqual("1.", calc.DisplayText);

            // Попробуем ввести ещё одну точку
            calc.PressDigit(".");
            Assert.AreEqual("1.", calc.DisplayText, "Повторная точка не должна добавляться");
        }

        [TestMethod]
        public void PressI_WhenNewEntry()
        {
            var calc = new CalculatorCore();
            calc.PressI();
            Assert.AreEqual("0 i* ", calc.DisplayText, "При новом вводе 'i' => 0 i* ");
        }

        [TestMethod]
        public void PressI_AlreadyHasImagPart_Ignored()
        {
            var calc = new CalculatorCore();
            calc.PressI();
            Assert.AreEqual("0 i* ", calc.DisplayText);

            // Повторный вызов должен игнорироваться
            calc.PressI();
            Assert.AreEqual("0 i* ", calc.DisplayText, "Повторная вставка 'i*' игнорируется");
        }

        [TestMethod]
        public void PressOperator_WhenNoPendingOp_SetsPending()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("5");
            calc.PressOperator("+");
            calc.PressDigit("2");
            calc.PressEquals();
            Assert.AreEqual("7", calc.DisplayText, "5+2=7");
        }

        [TestMethod]
        public void PressOperator_WhenPendingOpExistsAndNotNewEntry_ExecutesPreviousOp()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("2");
            calc.PressOperator("+");
            calc.PressDigit("3");
            // Снова оператор => должна выполниться старая операция (2+3=5), а потом pendingOp = "*"
            calc.PressOperator("*");
            Assert.AreEqual("5", calc.DisplayText, "Должно быть 5 после (2+3).");

            calc.PressDigit("4");
            calc.PressEquals();
            Assert.AreEqual("20", calc.DisplayText, "5*4=20");
        }

        [TestMethod]
        public void PressEquals_WhenNoPendingOpAndNoLastEquals_DoesNothing()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("5");
            calc.PressEquals();
            Assert.AreEqual("5", calc.DisplayText, "Нет операции => '=' не меняет дисплей");
        }

        [TestMethod]
        public void PressEquals_WhenPendingOp_UsesSecondOperand()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("6");
            calc.PressOperator("+");
            calc.PressDigit("2");
            calc.PressEquals();
            Assert.AreEqual("8", calc.DisplayText, "6+2=8");
        }

        [TestMethod]
        public void PressEquals_WhenPendingOpAndIsNewEntry_UsesSameOperand()
        {
            // "5 + =" => 5 + 5 = 10
            var calc = new CalculatorCore();
            calc.PressDigit("5");
            calc.PressOperator("+");
            calc.PressEquals();
            Assert.AreEqual("10", calc.DisplayText, "5+5=10");
        }

        [TestMethod]
        public void PressEquals_RepeatEquals_CallsLastOperatorAgain()
        {
            // 5 + 4 = 9 => повтор '=' => 9+4=13
            var calc = new CalculatorCore();
            calc.PressDigit("5");
            calc.PressOperator("+");
            calc.PressDigit("4");
            calc.PressEquals();
            Assert.AreEqual("9", calc.DisplayText);

            calc.PressEquals();
            Assert.AreEqual("13", calc.DisplayText, "Повторное '=' => 9+4=13");
        }

        [TestMethod]
        public void PressClear_ResetsEverything()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("6");
            calc.PressOperator("+");
            calc.PressDigit("4");
            calc.PressClear();

            Assert.AreEqual("0", calc.DisplayText, "Display должен сброситься в 0");
            // Можно проверить, что другие флаги сброшены, но это внутреннее:
            // Для C1 важно, что ветвь очистки выполнена
        }

        [TestMethod]
        public void PressBackspace_WhenIsNewEntry_DoesNothing()
        {
            var calc = new CalculatorCore();
            // Изначально isNewEntry=true => backspace игнорируется
            calc.PressBackspace();
            Assert.AreEqual("0", calc.DisplayText);
        }

        [TestMethod]
        public void PressBackspace_RemovesLastChar()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("1");
            calc.PressDigit("2");
            calc.PressBackspace();
            Assert.AreEqual("1", calc.DisplayText);

            calc.PressBackspace();
            Assert.AreEqual("0", calc.DisplayText, "Удалили последний символ => стало 0");
        }

        [TestMethod]
        public void PressSqr_SquaresTheNumber()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("3");
            calc.PressSqr();
            Assert.AreEqual("9 i* 0", calc.DisplayText, "3^2=9");
        }

        [TestMethod]
        public void PressRev_WhenZero_ShowsError()
        {
            var calc = new CalculatorCore();
            // Display=0 => Rev => 1/0 => исключение => отображаем "Ошибка"
            calc.PressRev();
            StringAssert.Contains(calc.DisplayText, "Ошибка");
        }

        [TestMethod]
        public void PressMdl_OnComplexNumber()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("3");
            calc.PressI();
            calc.PressDigit("4"); // => "3 i* 4"
            double m = calc.PressMdl();
            Assert.AreEqual(5.0, m, 1e-6, "Модуль 3+4i=5");
        }

        [TestMethod]
        public void PressCnr_OnComplexNumber()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("1");
            calc.PressI();
            calc.PressDigit("1"); // => "1 i* 1"
            var (deg, rad) = calc.PressCnr();
            // 1+1i => модуль sqrt(2), арг ~45° => 0.785 rad
            Assert.IsTrue(deg >= 44.9 && deg <= 45.1, "Arg ~45°");
            Assert.IsTrue(rad >= 0.78 && rad <= 0.79, "Arg ~0.785 rad");
        }

        [TestMethod]
        public void PressPower_WithNegativeN()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("2");
            calc.PressPower(-1); // => 1/2
            StringAssert.StartsWith(calc.DisplayText, "0.5 i* 0", "2^(-1)=0.5");
        }

        [TestMethod]
        public void PressRoot_NegativeDegree_ThrowsException()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("2");
            // Проверяем исключение:
            Assert.ThrowsException<ArgumentException>(() => calc.PressRoot(-2));
        }

        [TestMethod]
        public void MemoryStore_ThenRecall()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("5");
            calc.MemoryStore();
            Assert.IsTrue(calc.MemoryHasValue, "Память не пуста");

            calc.PressClear();
            calc.MemoryRecall();
            Assert.AreEqual("5 i* 0", calc.DisplayText);
        }

        [TestMethod]
        public void MemoryAdd_AddsCurrentValueToMemory()
        {
            var calc = new CalculatorCore();
            // Память=0, Store(10), Add(2)=12
            calc.PressDigit("1");
            calc.PressDigit("0"); // => "10"
            calc.MemoryStore(); // memory=10
            calc.PressClear();

            calc.PressDigit("2");
            calc.MemoryAdd(); // memory=10+2=12

            calc.PressClear();
            calc.MemoryRecall();
            Assert.AreEqual("12 i* 0", calc.DisplayText);
        }

        [TestMethod]
        public void MemoryClear_SetsMemoryEmpty()
        {
            var calc = new CalculatorCore();
            calc.PressDigit("3");
            calc.MemoryStore();
            Assert.IsTrue(calc.MemoryHasValue);

            calc.MemoryClear();
            Assert.IsFalse(calc.MemoryHasValue);
        }
    }
}
