using System;

namespace ComplexCalcSeparated
{
    public enum DisplayFormat
    {
        Complex,
        Real
    }

    public class CalculatorCore
    {
        private ComplexNumber currentValue = new ComplexNumber(0, 0);
        private ComplexNumber lastOperand = new ComplexNumber(0, 0);
        private string pendingOperator = null;
        private bool isNewEntry = true;
        private bool lastOpEquals = false;
        private string lastOperator = null;
        private DisplayFormat displayFormat = DisplayFormat.Complex;

        private Memory<ComplexNumber> memory = new Memory<ComplexNumber>();
        public bool MemoryHasValue => memory.HasValue;
        public DisplayFormat DisplayFormat
        {
            get => displayFormat;
            set
            {
                displayFormat = value;
                UpdateDisplayText();
            }
        }

        public string DisplayText { get; set; } = "0";

        public CalculatorCore() { }

        #region Нажатие цифр и символов (0..9, ., i)
        public void PressDigit(string digit)
        {
            if (isNewEntry)
            {
                DisplayText = (digit == ".") ? "0" : "";
                isNewEntry = false;
            }

            if (digit == ".")
            {
                if (HasDotInCurrentPart(DisplayText))
                {
                    return;
                }
            }

            DisplayText += digit;
        }

        public void PressI()
        {
            if (DisplayText.Contains("i*")) return;

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
            if (lastOpEquals)
            {
                lastOpEquals = false;
            }

            if (pendingOperator != null)
            {
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
                if (lastOpEquals && !string.IsNullOrEmpty(lastOperator))
                {
                    ApplyLastOperator();
                }
                return;
            }

            ComplexNumber second;
            if (isNewEntry)
            {
                second = currentValue;
            }
            else
            {
                second = Parse(DisplayText);
            }
            lastOperand = second;
            ApplyPendingOperator(second);

            pendingOperator = null;
            lastOpEquals = true;
            isNewEntry = true;
        }
        #endregion

        #region Переключение знака (Toggle Sign)
        public void PressToggleSign()
        {
            int idx = DisplayText.IndexOf("i*");
            if (idx != -1)
            {ода мнимой части.
                string realPart = DisplayText.Substring(0, idx).Trim();
                string imagPart = DisplayText.Substring(idx + 2).Trim();
                if (string.IsNullOrEmpty(imagPart))
                {
                    imagPart = "-";
                }
                else if (imagPart.StartsWith("-"))
                {
                    imagPart = imagPart.Substring(1).TrimStart();
                }
                else
                {
                    imagPart = "-" + imagPart;
                }
                DisplayText = $"{realPart} i* {imagPart}";
            }
            else
            {
                // Режим ввода действительной части.
                string trimmed = DisplayText.Trim();
                if (trimmed.StartsWith("-"))
                {
                    trimmed = trimmed.Substring(1);
                }
                else
                {
                    trimmed = "-" + trimmed;
                }
                DisplayText = trimmed;
            }
        }
        #endregion

        #region Очистка
        public void PressClearEntry()
        {
            DisplayText = "0";
            isNewEntry = true;
        }
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
            if (isNewEntry) return;
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
            DisplayText = res.ToString(displayFormat);
            if (pendingOperator != null) isNewEntry = false;
        }

        public void PressRev()
        {
            var x = Parse(DisplayText);
            try
            {
                var res = x.Reciprocal();
                DisplayText = res.ToString(displayFormat);
                if (pendingOperator != null) isNewEntry = false;
            }
            catch (Exception ex)
            {
                DisplayText = "Ошибка: " + ex.Message;
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
            DisplayText = res.ToString(displayFormat);
            if (pendingOperator != null) isNewEntry = false;
        }

        public ComplexNumber[] PressRoot(int n)
        {
            var x = Parse(DisplayText);
            var arr = x.Roots(n);
            if (arr.Length > 0)
            {
                DisplayText = arr[0].ToString(displayFormat);
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
                DisplayText = val.ToString(displayFormat);
                isNewEntry = false;
            }
        }
        #endregion

        private void ApplyPendingOperator(ComplexNumber second)
        {
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
            DisplayText = currentValue.ToString(displayFormat);
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
            DisplayText = currentValue.ToString(displayFormat);
        }

        private void UpdateDisplayText()
        {
            DisplayText = Parse(DisplayText).ToString(displayFormat);
        }

        private bool HasDotInCurrentPart(string text)
        {
            int idx = text.IndexOf("i*");
            if (idx == -1)
            {
                return text.Contains(".");
            }
            else
            {
                string imagPart = text.Substring(idx + 2);
                return imagPart.Contains(".");
            }
        }

        public void SetDisplayTextDirect(string raw)
        {
            DisplayText = raw;
            isNewEntry = false;
            lastOpEquals = false;
        }

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