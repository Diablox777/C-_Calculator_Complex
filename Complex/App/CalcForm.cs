﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace ComplexCalcSeparated
{
    public partial class CalcForm : Form
    {
        private CalculatorCore calcCore;  // «Ядро» логики

        private TextBox txtDisplay;
        private Label lblMemory;
        private MenuStrip menuStrip;

        public CalcForm()
        {
            InitializeComponent();
            // Создаём объект логики
            calcCore = new CalculatorCore();

            // Изначальное состояние
            txtDisplay.Text = calcCore.DisplayText;
            lblMemory.Visible = calcCore.MemoryHasValue;
        }

        private void InitializeComponent()
        {
            this.Text = "Complex Calculator";
            this.Size = new Size(650, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true;
            this.KeyPress += CalcForm_KeyPress;

            // Меню
            menuStrip = new MenuStrip();
            var menuEdit = new ToolStripMenuItem("Правка");
            var miCopy = new ToolStripMenuItem("Копировать", null, OnMenuCopy_Click) { ShortcutKeys = (Keys.Control | Keys.C) };
            var miPaste = new ToolStripMenuItem("Вставить", null, OnMenuPaste_Click) { ShortcutKeys = (Keys.Control | Keys.V) };
            menuEdit.DropDownItems.Add(miCopy);
            menuEdit.DropDownItems.Add(miPaste);

            var menuHelp = new ToolStripMenuItem("Справка");
            var miAbout = new ToolStripMenuItem("О программе", null, OnMenuAbout_Click);

            menuHelp.DropDownItems.Add(miAbout);
            menuStrip.Items.Add(menuEdit);
            menuStrip.Items.Add(menuHelp);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // Поле дисплея
            txtDisplay = new TextBox();
            txtDisplay.Location = new Point(10, 40);
            txtDisplay.Width = 580;
            txtDisplay.ReadOnly = true;
            txtDisplay.TextAlign = HorizontalAlignment.Right;
            txtDisplay.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            txtDisplay.Text = "0";
            this.Controls.Add(txtDisplay);

            // Индикатор памяти
            lblMemory = new Label();
            lblMemory.Text = "M";
            lblMemory.Location = new Point(600, 45);
            lblMemory.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblMemory.Visible = false;
            this.Controls.Add(lblMemory);

            // Теперь создаём кнопки:
            // Память (MC, MR, MS, M+)
            int x0 = 10, y0 = 90;
            Button btnMC = MakeButton("MC", x0, y0, (s, e) => {
                calcCore.MemoryClear();
                UpdateDisplay();
            });
            Button btnMR = MakeButton("MR", x0 + 60, y0, (s, e) => {
                calcCore.MemoryRecall();
                UpdateDisplay();
            });
            Button btnMS = MakeButton("MS", x0 + 120, y0, (s, e) => {
                calcCore.MemoryStore();
                UpdateDisplay();
            });
            Button btnMPlus = MakeButton("M+", x0 + 180, y0, (s, e) => {
                calcCore.MemoryAdd();
                UpdateDisplay();
            });

            // Строка 1: 7,8,9, +, -
            int x1 = 10, y1 = 140;
            Button btn7 = MakeButton("7", x1, y1, (s, e) => { calcCore.PressDigit("7"); UpdateDisplay(); });
            Button btn8 = MakeButton("8", x1 + 60, y1, (s, e) => { calcCore.PressDigit("8"); UpdateDisplay(); });
            Button btn9 = MakeButton("9", x1 + 120, y1, (s, e) => { calcCore.PressDigit("9"); UpdateDisplay(); });
            Button btnAdd = MakeButton("+", x1 + 180, y1, (s, e) => { calcCore.PressOperator("+"); UpdateDisplay(); });
            Button btnSub = MakeButton("-", x1 + 240, y1, (s, e) => { calcCore.PressOperator("-"); UpdateDisplay(); });

            // Строка 2: 4,5,6, *, /
            int y2 = 190;
            Button btn4 = MakeButton("4", x1, y2, (s, e) => { calcCore.PressDigit("4"); UpdateDisplay(); });
            Button btn5 = MakeButton("5", x1 + 60, y2, (s, e) => { calcCore.PressDigit("5"); UpdateDisplay(); });
            Button btn6 = MakeButton("6", x1 + 120, y2, (s, e) => { calcCore.PressDigit("6"); UpdateDisplay(); });
            Button btnMul = MakeButton("*", x1 + 180, y2, (s, e) => { calcCore.PressOperator("*"); UpdateDisplay(); });
            Button btnDiv = MakeButton("/", x1 + 240, y2, (s, e) => { calcCore.PressOperator("/"); UpdateDisplay(); });

            // Строка 3: 1,2,3, ., i
            int y3 = 240;
            Button btn1 = MakeButton("1", x1, y3, (s, e) => { calcCore.PressDigit("1"); UpdateDisplay(); });
            Button btn2 = MakeButton("2", x1 + 60, y3, (s, e) => { calcCore.PressDigit("2"); UpdateDisplay(); });
            Button btn3 = MakeButton("3", x1 + 120, y3, (s, e) => { calcCore.PressDigit("3"); UpdateDisplay(); });
            Button btnDot = MakeButton(".", x1 + 180, y3, (s, e) => { calcCore.PressDigit("."); UpdateDisplay(); });
            Button btnI = MakeButton("i", x1 + 240, y3, (s, e) => { calcCore.PressI(); UpdateDisplay(); });

            // Строка 4: 0, Backspace, =, C
            int y4 = 290;
            Button btn0 = MakeButton("0", x1, y4, (s, e) => { calcCore.PressDigit("0"); UpdateDisplay(); });
            Button btnBksp = MakeButton("←", x1 + 60, y4, (s, e) => { calcCore.PressBackspace(); UpdateDisplay(); });
            Button btnEq = MakeButton("=", x1 + 120, y4, (s, e) => {
                calcCore.PressEquals();
                UpdateDisplay();
            });
            Button btnC = MakeButton("C", x1 + 180, y4, (s, e) => {
                calcCore.PressClear();
                UpdateDisplay();
            });

            // Строка 5: функции (Sqr, Rev, Mdl, Cnr, Pwr, Root)
            int y5 = 340;
            Button btnSqr = MakeButton("Sqr", x1, y5, (s, e) => {
                calcCore.PressSqr();
                UpdateDisplay();
            });
            Button btnRev = MakeButton("Rev", x1 + 60, y5, (s, e) => {
                calcCore.PressRev();
                UpdateDisplay();
            });
            Button btnMdl = MakeButton("Mdl", x1 + 120, y5, (s, e) => {
                double m = calcCore.PressMdl();
                // Покажем модуль во всплывающем окне
                MessageBox.Show($"|{txtDisplay.Text}| = {m:F4}", "Модуль");
            });
            Button btnCnr = MakeButton("Cnr", x1 + 180, y5, (s, e) => {
                var (deg, rad) = calcCore.PressCnr();
                MessageBox.Show($"arg({txtDisplay.Text}) = {deg:F2}° ({rad:F3} rad)", "Аргумент");
            });
            Button btnPwr = MakeButton("Pwr", x1 + 240, y5, (s, e) => {
                // Запрашиваем степень (можно делать InputBox-форму)
                string input = Prompt("Введите степень", "Pwr", "2");
                if (int.TryParse(input, out int n))
                {
                    calcCore.PressPower(n);
                    UpdateDisplay();
                }
            });

            // Строка 6: Root (расположим рядом)
            int y6 = 390;
            Button btnRoot = MakeButton("Root", x1, y6, (s, e) => {
                string input = Prompt("Степень корня (целое >0)", "Root", "2");
                if (int.TryParse(input, out int n) && n > 0)
                {
                    var allRoots = calcCore.PressRoot(n);
                    UpdateDisplay();
                    if (allRoots.Length > 0)
                    {
                        string msg = "";
                        for (int k = 0; k < allRoots.Length; k++)
                            msg += $"k={k}: {allRoots[k]}\n";
                        MessageBox.Show(msg, $"Корни степени {n}");
                    }
                }
            });

            this.Controls.AddRange(new Control[] {
                btnMC, btnMR, btnMS, btnMPlus,
                btn7, btn8, btn9, btnAdd, btnSub,
                btn4, btn5, btn6, btnMul, btnDiv,
                btn1, btn2, btn3, btnDot, btnI,
                btn0, btnBksp, btnEq, btnC,
                btnSqr, btnRev, btnMdl, btnCnr, btnPwr, btnRoot
            });
        }

        // Утилита для создания кнопок
        private Button MakeButton(string text, int x, int y, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 12f);
            btn.Size = new Size(50, 40);
            btn.Location = new Point(x, y);
            btn.Click += onClick;
            return btn;
        }

        // Обновить дисплей и индикатор памяти
        private void UpdateDisplay()
        {
            txtDisplay.Text = calcCore.DisplayText;
            lblMemory.Visible = calcCore.MemoryHasValue;
        }

        // Меню: Копировать, Вставить, О программе
        private void OnMenuCopy_Click(object sender, EventArgs e)
        {
            string txt = txtDisplay.Text;
            if (!string.IsNullOrEmpty(txt))
                Clipboard.SetText(txt);
        }

        private void OnMenuPaste_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string clip = Clipboard.GetText();
                // вызывем специальный метод SetDisplayTextDirect
                calcCore.SetDisplayTextDirect(clip);
                UpdateDisplay();
            }
        }

        private void OnMenuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Калькулятор комплексных чисел\n" +
                            "Автор: Тетяков Илья Сергеевич.\n" +
                            "Вариант 20",
                            "О программе");
        }

        // Обработчик ввода с клавиатуры
        private void CalcForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;
            if (ch >= '0' && ch <= '9')
            {
                calcCore.PressDigit(ch.ToString());
                UpdateDisplay();
                e.Handled = true;
            }
            else if (ch == '.' || ch == ',')
            {
                calcCore.PressDigit(".");
                UpdateDisplay();
                e.Handled = true;
            }
            else if (ch == 'i' || ch == 'и')
            {
                calcCore.PressI();
                UpdateDisplay();
                e.Handled = true;
            }
            else if (ch == '+' || ch == '-' || ch == '*' || ch == '/')
            {
                calcCore.PressOperator(ch.ToString());
                UpdateDisplay();
                e.Handled = true;
            }
            else if (ch == '=' || ch == '\r')
            {
                calcCore.PressEquals();
                UpdateDisplay();
                e.Handled = true;
            }
            // Backspace = ASCII 8
            else if ((int)ch == 8)
            {
                calcCore.PressBackspace();
                UpdateDisplay();
                e.Handled = true;
            }
            // Esc = 27
            else if ((int)ch == 27)
            {
                calcCore.PressClear();
                UpdateDisplay();
                e.Handled = true;
            }
        }

        // Простейший метод Prompt (вместо InputBox)
        private string Prompt(string text, string title, string defVal = "")
        {
            Form prompt = new Form()
            {
                Width = 350,
                Height = 150,
                Text = title,
                StartPosition = FormStartPosition.CenterParent
            };
            Label lbl = new Label() { Left = 10, Top = 10, Text = text };
            lbl.AutoSize = true;
            TextBox txtInput = new TextBox() { Left = 10, Top = 35, Width = 300, Text = defVal };
            Button btnOk = new Button() { Text = "OK", Left = 160, Width = 70, Top = 70 };
            Button btnCancel = new Button() { Text = "Cancel", Left = 240, Width = 70, Top = 70 };

            btnOk.Click += (s, e) => { prompt.DialogResult = DialogResult.OK; prompt.Close(); };
            btnCancel.Click += (s, e) => { prompt.DialogResult = DialogResult.Cancel; prompt.Close(); };

            prompt.Controls.Add(lbl);
            prompt.Controls.Add(txtInput);
            prompt.Controls.Add(btnOk);
            prompt.Controls.Add(btnCancel);
            prompt.AcceptButton = btnOk;
            prompt.CancelButton = btnCancel;

            return prompt.ShowDialog() == DialogResult.OK ? txtInput.Text : null;
        }
    }
}
