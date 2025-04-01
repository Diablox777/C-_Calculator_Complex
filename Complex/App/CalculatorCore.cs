using System;

namespace ComplexCalcSeparated
{
    /// <summary>
    /// Основная логика (ядро) калькулятора комплексных чисел.
    /// </summary>
    public class CalculatorCore
    {
        // Текущее накопленное значение
        private ComplexNumber currentValue = new ComplexNumber(0, 0);
        // Последний операнд (для повторения "=")
        private ComplexNumber lastOperand = new ComplexNumber(0, 0);
        // Ожидающая бинарная операция (+ - * /)
        private string pendingOperator = null;
        // Состояние
        private bool isNewEntry = true;
        private bool lastOpEquals = false;
        private string lastOperator = null;

        // Память
        private Memory<ComplexNumber> memory = new Memory<ComplexNumber>();
        // Публичный доступ (форма показывает индикатор "M" при true)
        public bool MemoryHasValue => memory.HasValue;

        // Текст, который мы показываем на дисплее
        public string DisplayText { get; set; } = "0";

        // Конструктор
        public CalculatorCore() { }

        // --- ПУБЛИЧНЫЕ МЕТОДЫ, вызываемые из формы ---

        #region Нажатие цифр и символов (0..9, ., i)
        public void PressDigit(string digit)
        {
            // digit может быть "0".."9" или "."
            if (isNewEntry)
            {
                // Начинаем новый ввод
                DisplayText = (digit == ".") ? "0" : "";
                isNewEntry = false;
            }

            // Если вводим точку ".": проверка, нет ли уже точки
            if (digit == ".")
            {
                if (HasDotInCurrentPart(DisplayText))
                {
                    // игнорируем
                    return;
                }
            }

            DisplayText += digit;
        }

        public void PressI()
        {
            // Вставляем " i* " для мнимой части
            if (DisplayText.Contains("i*")) return; // уже есть

            if (isNewEntry)
            {
                DisplayText = "0";
                isNewEntry = false;
            }
            if (string.IsNullOrEmpty(DisplayText) || DisplayText == "-")
            {
                DisplayText += "0";
            }
            DisplayText += " i* ";
        }
        #endregion

        #region Операции + - * /
        public void PressOperator(string op)
        {
            // op: "+", "-", "*", "/"
            if (lastOpEquals)
            {
                // Нажали оператор сразу после "="
                lastOpEquals = false;
            }

            if (pendingOperator != null)
            {
                // Уже есть операция
                if (!isNewEntry)
                {
                    ComplexNumber second = Parse(DisplayText);
                    ApplyPendingOperator(second);
                }
                pendingOperator = op;
                isNewEntry = true;
            }
            else
            {
                // Первая операция
                currentValue = Parse(DisplayText);
                pendingOperator = op;
                isNewEntry = true;
            }
            lastOperator = op;
        }
        #endregion

        #region Равно
        public void PressEquals()
        {
            if (pendingOperator == null)
            {
                // Повторное "="
                if (lastOpEquals && !string.IsNullOrEmpty(lastOperator))
                {
                    // повторяем
                    ApplyLastOperator();
                }
                return;
            }

            // Есть отложенная операция
            ComplexNumber second;
            if (isNewEntry)
            {
                // Второй операнд не вводили => используем currentValue
                second = currentValue;
            }
            else
            {
                second = Parse(DisplayText);
            }
            lastOperand = second;
            ApplyPendingOperator(second);

            // Сбрасываем
            pendingOperator = null;
            lastOpEquals = true;
            isNewEntry = true;
        }
        #endregion

        #region Очистка
        public void PressClear()
        {
            currentValue = new ComplexNumber(0, 0);
            lastOperand = new ComplexNumber(0, 0);
            pendingOperator = null;
            lastOperator = null;
            isNewEntry = true;
            lastOpEquals = false;
            DisplayText = "0";
        }

        public void PressBackspace()
        {
            if (isNewEntry) return; // нечего стирать
            var txt = DisplayText;
            if (string.IsNullOrEmpty(txt) || txt == "0") return;
            txt = txt.Substring(0, txt.Length - 1);
            if (txt.Length == 0) txt = "0";
            DisplayText = txt;
        }
        #endregion

        #region Унарные функции (Sqr, Rev, Mdl, Cnr, Pwr, Root)
        public void PressSqr()
        {
            var x = Parse(DisplayText);
            var res = x.Square();
            DisplayText = res.ToString();
            if (pendingOperator != null) isNewEntry = false;
            // currentValue не меняем до нажатия "="
        }

        public void PressRev()
        {
            var x = Parse(DisplayText);
            try
            {
                var res = x.Reciprocal();
                DisplayText = res.ToString();
                if (pendingOperator != null) isNewEntry = false;
            }
            catch (Exception ex)
            {
                DisplayText = "Ошибка: " + ex.Message;
                // можно сбросить всё
            }
        }

        public double PressMdl()
        {
            var x = Parse(DisplayText);
            return x.Modulus();
        }

        public (double deg, double rad) PressCnr()
        {
            var x = Parse(DisplayText);
            double rad = x.ArgumentRadians();
            double deg = x.ArgumentDegrees();
            return (deg, rad);
        }

        public void PressPower(int n)
        {
            var x = Parse(DisplayText);
            var res = x.Power(n);
            DisplayText = res.ToString();
            if (pendingOperator != null) isNewEntry = false;
        }

        public ComplexNumber[] PressRoot(int n)
        {
            var x = Parse(DisplayText);
            var arr = x.Roots(n);
            if (arr.Length > 0)
            {
                DisplayText = arr[0].ToString(); // главный корень
            }
            if (pendingOperator != null) isNewEntry = false;
            return arr;
        }
        #endregion

        #region Память
        public void MemoryClear()
        {
            memory.Clear();
        }

        public void MemoryStore()
        {
            var val = Parse(DisplayText);
            memory.Store(val);
        }

        public void MemoryAdd()
        {
            var val = Parse(DisplayText);
            memory.Add(val);
        }

        public void MemoryRecall()
        {
            if (memory.HasValue)
            {
                var val = memory.Value;
                DisplayText = val.ToString();
                isNewEntry = false;
            }
        }
        #endregion

        // --- ВСПОМОГАТЕЛЬНЫЕ ПРИВАТНЫЕ МЕТОДЫ ---

        private void ApplyPendingOperator(ComplexNumber second)
        {
            // Применить pendingOperator к currentValue и second
            try
            {
                switch (pendingOperator)
                {
                    case "+": currentValue = currentValue + second; break;
                    case "-": currentValue = currentValue - second; break;
                    case "*": currentValue = currentValue * second; break;
                    case "/": currentValue = currentValue / second; break;
                }
            }
            catch (Exception ex)
            {
                DisplayText = "Ошибка: " + ex.Message;
                return;
            }
            // Отобразим результат
            DisplayText = currentValue.ToString();
        }

        private void ApplyLastOperator()
        {
            try
            {
                switch (lastOperator)
                {
                    case "+": currentValue = currentValue + lastOperand; break;
                    case "-": currentValue = currentValue - lastOperand; break;
                    case "*": currentValue = currentValue * lastOperand; break;
                    case "/": currentValue = currentValue / lastOperand; break;
                }
            }
            catch (Exception ex)
            {
                DisplayText = "Ошибка: " + ex.Message;
                return;
            }
            DisplayText = currentValue.ToString();
        }

        // Проверка точки в соответствующей части (реальной или мнимой)
        private bool HasDotInCurrentPart(string text)
        {
            // если нет "i*", значит вводим реальную часть
            int idx = text.IndexOf("i*");
            if (idx == -1)
            {
                // смотрим всю строку
                return text.Contains(".");
            }
            else
            {
                // после "i*"
                string imagPart = text.Substring(idx + 2);
                return imagPart.Contains(".");
            }
        }

        public void SetDisplayTextDirect(string raw)
        {
            // Если нужно просто записать строку:
            DisplayText = raw;
            isNewEntry = false;
            lastOpEquals = false;
        }

        // Парсинг строки -> ComplexNumber
        private ComplexNumber Parse(string s)
        {
            s = s.Trim();
            if (string.IsNullOrEmpty(s)) return new ComplexNumber(0, 0);

            int idx = s.IndexOf("i*");
            if (idx < 0)
            {
                double real;
                if (!double.TryParse(s, out real)) real = 0;
                return new ComplexNumber(real, 0);
            }
            // Есть i*
            string realPart = s.Substring(0, idx).Trim();
            string imagPart = s.Substring(idx + 2).Trim();

            double realVal = 0, imagVal = 0;
            if (string.IsNullOrEmpty(realPart) || realPart == "-")
            {
                if (realPart == "-") realVal = -0.0;
                else realVal = 0.0;
            }
            else
            {
                double.TryParse(realPart, out realVal);
            }

            if (string.IsNullOrEmpty(imagPart) || imagPart == "+")
            {
                imagVal = 1.0;
            }
            else if (imagPart == "-")
            {
                imagVal = -1.0;
            }
            else
            {
                double.TryParse(imagPart, out imagVal);
            }

            return new ComplexNumber(realVal, imagVal);
        }
    }
}
