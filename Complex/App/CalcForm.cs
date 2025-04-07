﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace ComplexCalcSeparated
{
    public partial class CalcForm : Form
    {
        private CalculatorCore calcCore;

        private TextBox txtDisplay;
        private Label lblMemory;
        private MenuStrip menuStrip;
        private ToolStripMenuItem menuSettings;

        public CalcForm()
        {
            InitializeComponent();
            calcCore = new CalculatorCore();

            txtDisplay.Text = calcCore.DisplayText;
            lblMemory.Visible = calcCore.MemoryHasValue;
        }

        private void InitializeComponent()
        {
            this.Text = "Complex Calculator";
            this.Size = new Size(385, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true;
            this.KeyPress += CalcForm_KeyPress;
            this.BackColor = Color.FromArgb(240, 240, 240);

            menuStrip = new MenuStrip();
            menuStrip.BackColor = Color.FromArgb(70, 130, 180);

            var menuEdit = new ToolStripMenuItem("Правка");
            var miCopy = new ToolStripMenuItem("Копировать", null, OnMenuCopy_Click) { ShortcutKeys = Keys.Control | Keys.C };
            var miPaste = new ToolStripMenuItem("Вставить", null, OnMenuPaste_Click) { ShortcutKeys = Keys.Control | Keys.V };
            menuEdit.DropDownItems.Add(miCopy);
            menuEdit.DropDownItems.Add(miPaste);
            menuStrip.Items.Add(menuEdit);

            menuSettings = new ToolStripMenuItem("Настройка");

            var miComplexFormat = new ToolStripMenuItem("Комплексный формат");
            var miRealFormat = new ToolStripMenuItem("Действительный формат");

            miComplexFormat.Click += (s, e) =>
            {
                calcCore.DisplayFormat = DisplayFormat.Complex;
                UpdateDisplay();
                miComplexFormat.Checked = true;
                miRealFormat.Checked = false;
            };
            miComplexFormat.Checked = true;

            miRealFormat.Click += (s, e) =>
            {
                calcCore.DisplayFormat = DisplayFormat.Real;
                UpdateDisplay();
                miComplexFormat.Checked = false;
                miRealFormat.Checked = true;
            };
            miRealFormat.Checked = false;

            menuSettings.DropDownItems.Add(miComplexFormat);
            menuSettings.DropDownItems.Add(miRealFormat);
            menuStrip.Items.Add(menuSettings);

            var menuHelp = new ToolStripMenuItem("Справка");
            var miAbout = new ToolStripMenuItem("О программе", null, OnMenuAbout_Click);
            menuHelp.DropDownItems.Add(miAbout);
            menuStrip.Items.Add(menuHelp);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            txtDisplay = new TextBox();
            txtDisplay.Location = new Point(10, 40);
            txtDisplay.Width = 300;
            txtDisplay.ReadOnly = true;
            txtDisplay.TextAlign = HorizontalAlignment.Right;
            txtDisplay.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            txtDisplay.BackColor = Color.FromArgb(255, 255, 230);
            txtDisplay.ForeColor = Color.FromArgb(70, 70, 70);
            txtDisplay.BorderStyle = BorderStyle.Fixed3D;
            txtDisplay.Text = "0";
            this.Controls.Add(txtDisplay);

            lblMemory = new Label();
            lblMemory.Text = "M";
            lblMemory.Location = new Point(325, 45);
            lblMemory.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblMemory.ForeColor = Color.FromArgb(70, 130, 180);
            lblMemory.Visible = false;
            this.Controls.Add(lblMemory);

            CreateButtons();
        }

        private void CreateButtons()
        {
            int x0 = 10, y0 = 90;
            Button btnSqr = MakeButton("x²", x0, y0, Color.FromArgb(100, 149, 237), (s, e) => {
                calcCore.PressSqr();
                UpdateDisplay();
            });
            Button btnRev = MakeButton("1/x", x0 + 60, y0, Color.FromArgb(100, 149, 237), (s, e) => {
                calcCore.PressRev();
                UpdateDisplay();
            });
            Button btnMdl = MakeButton("|x|", x0 + 120, y0, Color.FromArgb(100, 149, 237), (s, e) => {
                double m = calcCore.PressMdl();
                MessageBox.Show($"|{txtDisplay.Text}| = {m:F4}", "Модуль");
            });
            Button btnCnr = MakeButton("x°", x0 + 180, y0, Color.FromArgb(100, 149, 237), (s, e) => {
                var (deg, rad) = calcCore.PressCnr();
                MessageBox.Show($"arg({txtDisplay.Text}) = {deg:F2}° ({rad:F3} rad)", "Аргумент");
            });
            Button btnPwr = MakeButton("xʸ", x0 + 240, y0, Color.FromArgb(100, 149, 237), (s, e) => {
                string input = Prompt("Введите степень", "Pwr", "2");
                if (int.TryParse(input, out int n))
                {
                    calcCore.PressPower(n);
                    UpdateDisplay();
                }
            });
            Button btnRoot = MakeButton("√x", x0 + 300, y0, Color.FromArgb(100, 149, 237), (s, e) => {
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

            int x1 = 10, y1 = 140;
            Button btnMC = MakeButton("MC", x1, y1, Color.FromArgb(255, 215, 0), (s, e) => {
                calcCore.MemoryClear();
                UpdateDisplay();
            });
            Button btn7 = MakeButton("7", x1 + 60, y1, Color.FromArgb(245, 245, 245), (s, e) => { calcCore.PressDigit("7"); UpdateDisplay(); });
            Button btn8 = MakeButton("8", x1 + 120, y1, Color.FromArgb(245, 245, 245), (s, e) => { calcCore.PressDigit("8"); UpdateDisplay(); });
            Button btn9 = MakeButton("9", x1 + 180, y1, Color.FromArgb(245, 245, 245), (s, e) => { calcCore.PressDigit("9"); UpdateDisplay(); });
            Button btnAdd = MakeButton("+", x1 + 240, y1, Color.FromArgb(255, 165, 0), (s, e) => { calcCore.PressOperator("+"); UpdateDisplay(); });
            Button btnMul = MakeButton("*", x1 + 300, y1, Color.FromArgb(255, 165, 0), (s, e) => { calcCore.PressOperator("*"); UpdateDisplay(); });

            int y2 = 190;
            Button btnMR = MakeButton("MR", x1, y2, Color.FromArgb(255, 215, 0), (s, e) => {
                calcCore.MemoryRecall();
                UpdateDisplay();
            });
            Button btn4 = MakeButton("4", x1 + 60, y2, Color.FromArgb(245, 245, 245), (s, e) => { calcCore.PressDigit("4"); UpdateDisplay(); });
            Button btn5 = MakeButton("5", x1 + 120, y2, Color.FromArgb(245, 245, 245), (s, e) => { calcCore.PressDigit("5"); UpdateDisplay(); });
            Button btn6 = MakeButton("6", x1 + 180, y2, Color.FromArgb(245, 245, 245), (s, e) => { calcCore.PressDigit("6"); UpdateDisplay(); });
            Button btnSub = MakeButton("-", x1 + 240, y2, Color.FromArgb(255, 165, 0), (s, e) => { calcCore.PressOperator("-"); UpdateDisplay(); });
            Button btnDiv = MakeButton("/", x1 + 300, y2, Color.FromArgb(255, 165, 0), (s, e) => { calcCore.PressOperator("/"); UpdateDisplay(); });

            int y3 = 240;
            Button btnMS = MakeButton("MS", x1, y3, Color.FromArgb(255, 215, 0), (s, e) => {
                calcCore.MemoryStore();
                UpdateDisplay();
            });
            Button btn1 = MakeButton("1", x1 + 60, y3, Color.FromArgb(245, 245, 245), (s, e) => { calcCore.PressDigit("1"); UpdateDisplay(); });
            Button btn2 = MakeButton("2", x1 + 120, y3, Color.FromArgb(245, 245, 245), (s, e) => { calcCore.PressDigit("2"); UpdateDisplay(); });
            Button btn3 = MakeButton("3", x1 + 180, y3, Color.FromArgb(245, 245, 245), (s, e) => { calcCore.PressDigit("3"); UpdateDisplay(); });
            Button btnI = MakeButton("i*", x1 + 240, y3, Color.FromArgb(100, 149, 237), (s, e) => { calcCore.PressI(); UpdateDisplay(); });

            int y4 = 290;
            Button btnMPlus = MakeButton("M+", x1, y4, Color.FromArgb(255, 215, 0), (s, e) => {
                calcCore.MemoryAdd();
                UpdateDisplay();
            });
            Button btn0 = MakeButton("0", x1 + 60, y4, Color.FromArgb(245, 245, 245), (s, e) => { calcCore.PressDigit("0"); UpdateDisplay(); });
            Button btnBksp = MakeButton("⌫", x1 + 120, y4, Color.FromArgb(220, 220, 220), (s, e) => { calcCore.PressBackspace(); UpdateDisplay(); });
            Button btnEq = MakeButton("=", x1 + 180, y4, Color.FromArgb(50, 205, 50), (s, e) => {
                calcCore.PressEquals();
                UpdateDisplay();
            });
            Button btnC = MakeButton("C", x1 + 240, y4, Color.FromArgb(220, 20, 60), (s, e) => {
                calcCore.PressClear();
                UpdateDisplay();
            });
            Button btnCE = MakeButton("CE", x1 + 300, y4, Color.FromArgb(220, 20, 60), (s, e) => {
                calcCore.PressClearEntry();
                UpdateDisplay();
            });

            this.Controls.AddRange(new Control[] {
                btnMC, btnMR, btnMS, btnMPlus,
                btn7, btn8, btn9, btnAdd, btnSub,
                btn4, btn5, btn6, btnMul, btnDiv,
                btn1, btn2, btn3, btnI,
                btn0, btnBksp, btnEq, btnC, btnCE,
                btnSqr, btnRev, btnMdl, btnCnr, btnPwr, btnRoot
            });
        }

        private Button MakeButton(string text, int x, int y, Color backColor, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 12f);
            btn.Size = new Size(50, 40);
            btn.Location = new Point(x, y);
            btn.BackColor = backColor;
            btn.ForeColor = Color.Black;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.Gray;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                Math.Min(255, backColor.R + 30),
                Math.Min(255, backColor.G + 30),
                Math.Min(255, backColor.B + 30));
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(
                Math.Max(0, backColor.R - 30),
                Math.Max(0, backColor.G - 30),
                Math.Max(0, backColor.B - 30));
            btn.Click += onClick;
            return btn;
        }

        private void UpdateDisplay()
        {
            txtDisplay.Text = calcCore.DisplayText;
            lblMemory.Visible = calcCore.MemoryHasValue;
        }

        private void OnMenuCopy_Click(object sender, EventArgs e)
        {
            string txt = calcCore.DisplayText;
            if (!string.IsNullOrEmpty(txt))
                Clipboard.SetText(txt);
        }

        private void OnMenuPaste_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string clip = Clipboard.GetText();
                calcCore.SetDisplayTextDirect(clip);
                UpdateDisplay();
            }
        }

        private void OnMenuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Калькулятор комплексных чисел\n" +
                          "Разработчик: \n" +
                          "Вариант 20",
                          "О программе");
        }

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
            else if ((int)ch == 8)
            {
                calcCore.PressBackspace();
                UpdateDisplay();
                e.Handled = true;
            }
            else if ((int)ch == 27)
            {
                calcCore.PressClear();
                UpdateDisplay();
                e.Handled = true;
            }
            else if ((int)ch == 46)
            {
                calcCore.PressClearEntry();
                UpdateDisplay();
                e.Handled = true;
            }
        }

        private string Prompt(string text, string title, string defVal = "")
        {
            Form prompt = new Form()
            {
                Width = 350,
                Height = 150,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            Label lbl = new Label() { Left = 10, Top = 10, Text = text, AutoSize = true };
            TextBox txtInput = new TextBox()
            {
                Left = 10,
                Top = 35,
                Width = 300,
                Text = defVal,
                BackColor = Color.FromArgb(255, 255, 230)
            };
            Button btnOk = new Button()
            {
                Text = "OK",
                Left = 160,
                Width = 70,
                Top = 70,
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            Button btnCancel = new Button()
            {
                Text = "Cancel",
                Left = 240,
                Width = 70,
                Top = 70,
                BackColor = Color.FromArgb(220, 20, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

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