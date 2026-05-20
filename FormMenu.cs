using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Blindrun1
{
    public class FormMenu : Form
    {
        private readonly Color BgColor = Color.FromArgb(12, 8, 18);
        private readonly Color AccentColor = Color.FromArgb(160, 255, 130);
        private readonly Color RedColor = Color.FromArgb(180, 40, 40);

        private Button btnPlay;
        private Button btnExit;

        // Простая анимация — мигающий курсор в заголовке
        private System.Windows.Forms.Timer animTimer;
        private bool cursorVisible = true;

        public FormMenu()
        {
            this.Text = "Blindrun";
            this.ClientSize = new Size(480, 360);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BgColor;
            this.DoubleBuffered = true;

            // Кнопка Играть
            btnPlay = new Button();
            btnPlay.Text = "▶   ИГРАТЬ";
            btnPlay.Font = new Font("Consolas", 16, FontStyle.Bold);
            btnPlay.ForeColor = AccentColor;
            btnPlay.BackColor = Color.FromArgb(20, 40, 15);
            btnPlay.FlatStyle = FlatStyle.Flat;
            btnPlay.FlatAppearance.BorderColor = Color.FromArgb(80, 160, 60);
            btnPlay.FlatAppearance.BorderSize = 2;
            btnPlay.Size = new Size(220, 56);
            btnPlay.Location = new Point((ClientSize.Width - 220) / 2, 210);
            btnPlay.Cursor = Cursors.Hand;
            btnPlay.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(btnPlay);

            // Кнопка Выйти
            btnExit = new Button();
            btnExit.Text = "✕   ВЫЙТИ";
            btnExit.Font = new Font("Consolas", 14, FontStyle.Bold);
            btnExit.ForeColor = Color.FromArgb(130, 100, 100);
            btnExit.BackColor = Color.FromArgb(18, 10, 10);
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderColor = Color.FromArgb(80, 40, 40);
            btnExit.FlatAppearance.BorderSize = 2;
            btnExit.Size = new Size(220, 48);
            btnExit.Location = new Point((ClientSize.Width - 220) / 2, 284);
            btnExit.Cursor = Cursors.Hand;
            btnExit.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnExit);

            // Таймер анимации курсора
            animTimer = new System.Windows.Forms.Timer();
            animTimer.Interval = 500;
            animTimer.Tick += (s, e) =>
            {
                cursorVisible = !cursorVisible;
                this.Invalidate();
            };
            animTimer.Start();

            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else if (e.KeyCode == Keys.Escape)
                    Application.Exit();
            };
            this.KeyPreview = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            int w = ClientSize.Width;

            // Декоративная рамка
            using (var pen = new Pen(Color.FromArgb(60, 30, 30), 2))
                g.DrawRectangle(pen, 10, 10, w - 20, ClientSize.Height - 20);

            using (var pen = new Pen(Color.FromArgb(30, 60, 20), 1))
                g.DrawRectangle(pen, 14, 14, w - 28, ClientSize.Height - 28);

            // Заголовок — змейка ASCII
            string snake = ">===[{  BLINDRUN  }]===<";
            using (var font = new Font("Consolas", 11, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(100, 80, 80)))
            {
                var sz = g.MeasureString(snake, font);
                g.DrawString(snake, font, brush, (w - sz.Width) / 2, 30);
            }

            // Главный заголовок
            using (var font = new Font("Consolas", 38, FontStyle.Bold))
            {
                string title = "🐍 BLINDRUN";
                var sz = g.MeasureString(title, font);

                // Тень
                using (var shadowBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                    g.DrawString(title, font, shadowBrush, (w - sz.Width) / 2 + 3, 58);

                // Основной текст
                using (var brush = new SolidBrush(AccentColor))
                    g.DrawString(title, font, brush, (w - sz.Width) / 2, 55);
            }

            // Подзаголовок
            string sub = "Выживи в темноте";
            using (var font = new Font("Consolas", 11, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(140, 100, 100)))
            {
                var sz = g.MeasureString(sub, font);
                g.DrawString(sub, font, brush, (w - sz.Width) / 2, 120);
            }

            // Мигающая подсказка
            string hint = cursorVisible ? "[ ENTER или нажми ИГРАТЬ ]" : "                          ";
            using (var font = new Font("Consolas", 9))
            using (var brush = new SolidBrush(Color.FromArgb(80, 130, 60)))
            {
                var sz = g.MeasureString(hint, font);
                g.DrawString(hint, font, brush, (w - sz.Width) / 2, 158);
            }

            // Управление
            string controls = "WASD / ←↑↓→  •  P — пауза  •  F11 — полный экран";
            using (var font = new Font("Consolas", 8))
            using (var brush = new SolidBrush(Color.FromArgb(55, 45, 55)))
            {
                var sz = g.MeasureString(controls, font);
                g.DrawString(controls, font, brush, (w - sz.Width) / 2, 182);
            }
        }
    }
}