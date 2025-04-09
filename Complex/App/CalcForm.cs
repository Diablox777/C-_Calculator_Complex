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
        private Button hamburgerButton;
        private Panel sidePanel;
        private ContextMenuStrip editMenu;
        private ContextMenuStrip settingsMenu;
        private ContextMenuStrip helpMenu;

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
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Гамбургер-кнопка
            hamburgerButton = new Button();
            hamburgerButton.Text = "≡";
            hamburgerButton.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            hamburgerButton.Size = new Size(40, 40);
            hamburgerButton.Location = new Point(10, 10);
            hamburgerButton.Click += HamburgerButton_Click;
            this.Controls.Add(hamburgerButton);

            // Боковая панель
            sidePanel = new Panel();
            sidePanel.Size = new Size(60, 320);
            sidePanel.Location = new Point(-60, 50);
            sidePanel.BackColor = Color.FromArgb(40, 40, 40);
            sidePanel.Visible = false;
            CreateSidePanelButtons();
            this.Controls.Add(sidePanel);

            // Основные элементы
            txtDisplay = new TextBox();
            txtDisplay.Location = new Point(10, 60);
            txtDisplay.Width = 300;
            txtDisplay.ReadOnly = true;
            txtDisplay.TextAlign = HorizontalAlignment.Right;
            txtDisplay.Font = new Font("Times New Roman", 14f, FontStyle.Bold);
            txtDisplay.BackColor = Color.FromArgb(50, 50, 50);
            txtDisplay.ForeColor = Color.White;
            txtDisplay.BorderStyle = BorderStyle.Fixed3D;
            txtDisplay.Text = "0";
            this.Controls.Add(txtDisplay);

            lblMemory = new Label();
            lblMemory.Text = "M";
            lblMemory.Location = new Point(325, 65);
            lblMemory.Font = new Font("Times New Roman", 12f, FontStyle.Bold);
            lblMemory.ForeColor = Color.Red;
            lblMemory.Visible = false;
            this.Controls.Add(lblMemory);

            CreateButtons();
            CreateContextMenus();
        }

        private void CreateSidePanelButtons()
        {
            Button btnEdit = new Button();
            btnEdit.Text = "✏️";
            btnEdit.Font = new Font("Segoe UI Symbol", 12f, FontStyle.Bold);
            btnEdit.Size = new Size(40, 40);
            btnEdit.Location = new Point(10, 10);
            btnEdit.Click += (s, e) => editMenu.Show(btnEdit, new Point(40, 0));
            sidePanel.Controls.Add(btnEdit);

            Button btnSettings = new Button();
            btnSettings.Text = "⚙️";
            btnSettings.Font = new Font("Segoe UI Symbol", 12f, FontStyle.Bold);
            btnSettings.Size = new Size(40, 40);
            btnSettings.Location = new Point(10, 60);
            btnSettings.Click += (s, e) => settingsMenu.Show(btnSettings, new Point(40, 0));
            sidePanel.Controls.Add(btnSettings);

            Button btnHelp = new Button();
            btnHelp.Text = "❓";
            btnHelp.Font = new Font("Segoe UI Symbol", 12f, FontStyle.Bold);
            btnHelp.Size = new Size(40, 40);
            btnHelp.Location = new Point(10, 110);
            btnHelp.Click += (s, e) => helpMenu.Show(btnHelp, new Point(40, 0));
            sidePanel.Controls.Add(btnHelp);
        }

        private void CreateContextMenus()
        {
            // Меню Правка
            editMenu = new ContextMenuStrip();
            var miCopy = new ToolStripMenuItem("Копировать", null, OnMenuCopy_Click) 
            { 
                ShortcutKeys = Keys.Control | Keys.C 
            };
            var miPaste = new ToolStripMenuItem("Вставить", null, OnMenuPaste_Click) 
            { 
                ShortcutKeys = Keys.Control | Keys.V 
            };
            editMenu.Items.Add(miCopy);
            editMenu.Items.Add(miPaste);

            // Меню Настройка
            settingsMenu = new ContextMenuStrip();
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
            
            settingsMenu.Items.Add(miComplexFormat);
            settingsMenu.Items.Add(miRealFormat);

            // Меню Справка
            helpMenu = new ContextMenuStrip();
            var miAbout = new ToolStripMenuItem("О программе", null, OnMenuAbout_Click);
            helpMenu.Items.Add(miAbout);
        }

        private void HamburgerButton_Click(object sender, EventArgs e)
        {
            if (sidePanel.Visible)
            {
                sidePanel.Hide();
            }
            else
            {
                sidePanel.Show();
                sidePanel.Location = new Point(10, 50);
            }
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
            Button btn7 = MakeButton("7", x1 + 60, y1, Color.FromArgb(70, 70, 70), (s, e) => { calcCore.PressDigit("7"); UpdateDisplay(); });
            Button btn8 = MakeButton("8", x1 + 120, y1, Color.FromArgb(70, 70, 70), (s, e) => { calcCore.PressDigit("8"); UpdateDisplay(); });
            Button btn9 = MakeButton("9", x1 + 180, y1, Color.FromArgb(70, 70, 70), (s, e) => { calcCore.PressDigit("9"); UpdateDisplay(); });
            Button btnAdd = MakeButton("+", x1 + 240, y1, Color.FromArgb(60, 60, 60), (s, e) => { calcCore.PressOperator("+"); UpdateDisplay(); });
            Button btnMul = MakeButton("*", x1 + 300, y1, Color.FromArgb(60, 60, 60), (s, e) => { calcCore.PressOperator("*"); UpdateDisplay(); });
            int y2 = 190;
            Button btnMR = MakeButton("MR", x1, y2, Color.FromArgb(255, 215, 0), (s, e) => {
                calcCore.MemoryRecall();
                UpdateDisplay();
            });
            Button btn4 = MakeButton("4", x1 + 60, y2, Color.FromArgb(70, 70, 70), (s, e) => { calcCore.PressDigit("4"); UpdateDisplay(); });
            Button btn5 = MakeButton("5", x1 + 120, y2, Color.FromArgb(70, 70, 70), (s, e) => { calcCore.PressDigit("5"); UpdateDisplay(); });
            Button btn6 = MakeButton("6", x1 + 180, y2, Color.FromArgb(70, 70, 70), (s, e) => { calcCore.PressDigit("6"); UpdateDisplay(); });
            Button btnSub = MakeButton("-", x1 + 240, y2, Color.FromArgb(60, 60, 60), (s, e) => { calcCore.PressOperator("-"); UpdateDisplay(); });
            Button btnDiv = MakeButton("/", x1 + 300, y2, Color.FromArgb(60, 60, 60), (s, e) => { calcCore.PressOperator("/"); UpdateDisplay(); });
            int y3 = 240;
            Button btnMS = MakeButton("MS", x1, y3, Color.FromArgb(255, 215, 0), (s, e) => {
                calcCore.MemoryStore();
                UpdateDisplay();
            });
            Button btn1 = MakeButton("1", x1 + 60, y3, Color.FromArgb(70, 70, 70), (s, e) => { calcCore.PressDigit("1"); UpdateDisplay(); });
            Button btn2 = MakeButton("2", x1 + 120, y3, Color.FromArgb(70, 70, 70), (s, e) => { calcCore.PressDigit("2"); UpdateDisplay(); });
            Button btn3 = MakeButton("3", x1 + 180, y3, Color.FromArgb(70, 70, 70), (s, e) => { calcCore.PressDigit("3"); UpdateDisplay(); });
            Button btnI = MakeButton("i*", x1 + 240, y3, Color.FromArgb(100, 149, 237), (s, e) => { calcCore.PressI(); UpdateDisplay(); });
            Button btnToggleSign = MakeButton("±", x1 + 300, y3, Color.FromArgb(70, 70, 70), (s, e) =>
            {
                calcCore.PressToggleSign();
                UpdateDisplay();
            });
            int y4 = 290;
            Button btnMPlus = MakeButton("M+", x1, y4, Color.FromArgb(255, 215, 0), (s, e) => {
                calcCore.MemoryAdd();
                UpdateDisplay();
            });
            Button btn0 = MakeButton("0", x1 + 60, y4, Color.FromArgb(70, 70, 70), (s, e) => { calcCore.PressDigit("0"); UpdateDisplay(); });
            Button btnBksp = MakeButton("⌫", x1 + 120, y4, Color.FromArgb(70, 70, 70), (s, e) => { calcCore.PressBackspace(); UpdateDisplay(); });
            Button btnEq = MakeButton("=", x1 + 180, y4, Color.FromArgb(50, 205, 50), (s, e) => {
                calcCore.PressEquals();
                UpdateDisplay();
            });
            Button btnC = MakeButton("C", x1 + 240, y4, Color.FromArgb(90, 0, 0), (s, e) => {
                calcCore.PressClear();
                UpdateDisplay();
            });
            Button btnCE = MakeButton("CE", x1 + 300, y4, Color.FromArgb(90, 0, 0), (s, e) => {
                calcCore.PressClearEntry();
                UpdateDisplay();
            });
            this.Controls.AddRange(new Control[] {
                btnMC, btnMR, btnMS, btnMPlus,
                btn7, btn8, btn9, btnAdd, btnSub,
                btn4, btn5, btn6, btnMul, btnDiv,
                btn1, btn2, btn3, btnI, btnToggleSign,
                btn0, btnBksp, btnEq, btnC, btnCE,
                btnSqr, btnRev, btnMdl, btnCnr, btnPwr, btnRoot
            });
        }

        private Button MakeButton(string text, int x, int y, Color backColor, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Times New Roman", 12f, FontStyle.Bold);
            btn.Size = new Size(50, 40);
            btn.Location = new Point(x, y);
            btn.BackColor = Color.FromArgb(70, 70, 70);
            btn.ForeColor = Color.White;
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
                          "Разработчик: Тетяков И.С. ИП-111\n" +
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
                BackColor = Color.FromArgb(30, 30, 30)
            };
            Label lbl = new Label() { Left = 10, Top = 10, Text = text, AutoSize = true };
            TextBox txtInput = new TextBox()
            {
                Left = 10,
                Top = 35,
                Width = 300,
                Text = defVal,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            Button btnOk = new Button()
            {
                Text = "OK",
                Left = 160,
                Width = 70,
                Top = 70,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            Button btnCancel = new Button()
            {
                Text = "Cancel",
                Left = 240,
                Width = 70,
                Top = 70,
                BackColor = Color.FromArgb(90, 0, 0),
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