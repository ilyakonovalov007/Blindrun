using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Blindrun1
{
    public partial class Form1 : Form
    {
        private Game1 game;
        private System.Windows.Forms.Timer gameTimer;
        private const int Cols = 30;
        private const int Rows = 30;
        private int CellSize => Math.Min(
            ClientSize.Width / Cols,
            (ClientSize.Height - 50) / Rows);

        private bool isFullscreen = false;
        private FormWindowState prevWindowState;
        private FormBorderStyle prevBorderStyle;
        private Size prevSize;

        private Button btnRestart;
        private Button btnExit;

        private readonly Color BgColor = Color.FromArgb(15, 15, 25);
        private readonly Color GridColor = Color.FromArgb(25, 25, 40);
        private readonly Color HeadColor = Color.FromArgb(80, 220, 120);
        private readonly Color BodyColor = Color.FromArgb(40, 160, 80);
        private readonly Color FoodColor = Color.FromArgb(255, 80, 80);
        private readonly Color BorderColor = Color.FromArgb(60, 200, 100);
        private readonly Color TextColor = Color.FromArgb(200, 255, 200);

        public Form1()
        {
            InitializeComponent();

            this.Text = "🐍 Snake";
            this.ClientSize = new Size(800, 850);
            this.MinimumSize = new Size(400, 450);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = BgColor;
            this.DoubleBuffered = true;
            this.KeyPreview = true;  // форма всегда получает клавиши первой
            this.KeyDown += OnKeyDown;
            this.Resize += (s, e) => { UpdateButtonPositions(); this.Invalidate(); };

            btnRestart = new Button();
            btnRestart.Text = "▶  Заново";
            btnRestart.Font = new Font("Consolas", 14, FontStyle.Bold);
            btnRestart.ForeColor = Color.FromArgb(80, 220, 120);
            btnRestart.BackColor = Color.FromArgb(25, 40, 25);
            btnRestart.FlatStyle = FlatStyle.Flat;
            btnRestart.FlatAppearance.BorderColor = Color.FromArgb(60, 200, 100);
            btnRestart.FlatAppearance.BorderSize = 2;
            btnRestart.Size = new Size(160, 48);
            btnRestart.Visible = false;
            btnRestart.Cursor = Cursors.Hand;
            btnRestart.Click += (s, e) => { RestartGame(); this.Focus(); };
            this.Controls.Add(btnRestart);

            btnExit = new Button();
            btnExit.Text = "✕  Выйти";
            btnExit.Font = new Font("Consolas", 14, FontStyle.Bold);
            btnExit.ForeColor = Color.FromArgb(255, 80, 80);
            btnExit.BackColor = Color.FromArgb(40, 20, 20);
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderColor = Color.FromArgb(200, 60, 60);
            btnExit.FlatAppearance.BorderSize = 2;
            btnExit.Size = new Size(160, 48);
            btnExit.Visible = false;
            btnExit.Cursor = Cursors.Hand;
            btnExit.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnExit);

            game = new Game1(Cols, Rows);

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 150;
            gameTimer.Tick += OnTick;
            gameTimer.Start();
        }

        private void RestartGame()
        {
            game = new Game1(Cols, Rows);
            gameTimer.Interval = 150;
            btnRestart.Visible = false;
            btnExit.Visible = false;
            this.Invalidate();
        }

        private void UpdateButtonPositions()
        {
            int cs = CellSize;
            int gameHeight = Rows * cs;
            int gameWidth = Cols * cs;
            int offsetX = (ClientSize.Width - gameWidth) / 2;
            int cx = ClientSize.Width / 2;
            int cy = gameHeight / 2;

            btnRestart.Location = new Point(cx - 170, cy + 30);
            btnExit.Location = new Point(cx + 10, cy + 30);
        }

        private void ToggleFullscreen()
        {
            if (!isFullscreen)
            {
                prevWindowState = this.WindowState;
                prevBorderStyle = this.FormBorderStyle;
                prevSize = this.Size;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                isFullscreen = true;
            }
            else
            {
                this.FormBorderStyle = prevBorderStyle;
                this.WindowState = prevWindowState;
                this.Size = prevSize;
                isFullscreen = false;
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!game.IsOver && !game.IsPaused)
            {
                game.Update();
                gameTimer.Interval = Math.Max(60, 150 - game.Score * 2);
            }

            if (game.IsOver)
            {
                UpdateButtonPositions();
                btnRestart.Visible = true;
                btnExit.Visible = true;
            }

            this.Invalidate();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up: case Keys.W: game.SetDirection(0, -1); break;
                case Keys.Down: case Keys.S: game.SetDirection(0, 1); break;
                case Keys.Left: case Keys.A: game.SetDirection(-1, 0); break;
                case Keys.Right: case Keys.D: game.SetDirection(1, 0); break;
                case Keys.P:
                    if (!game.IsOver) game.IsPaused = !game.IsPaused;
                    break;
                case Keys.R:
                    if (game.IsOver) RestartGame();
                    break;
                case Keys.F11:
                    ToggleFullscreen();
                    break;
                case Keys.Escape:
                    if (isFullscreen) ToggleFullscreen();
                    else Application.Exit();
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int cs = CellSize;
            int gameWidth = Cols * cs;
            int gameHeight = Rows * cs;
            int offsetX = (ClientSize.Width - gameWidth) / 2;

            g.TranslateTransform(offsetX, 0);

            // Фон поля
            g.FillRectangle(new SolidBrush(BgColor), 0, 0, gameWidth, gameHeight);

            // Сетка
            using (var gridPen = new Pen(GridColor, 1))
            {
                for (int x = 0; x <= Cols; x++)
                    g.DrawLine(gridPen, x * cs, 0, x * cs, gameHeight);
                for (int y = 0; y <= Rows; y++)
                    g.DrawLine(gridPen, 0, y * cs, gameWidth, y * cs);
            }

            // Еда
            DrawCell(g, game.Food.X, game.Food.Y, FoodColor, true, cs);

            // Тело
            var snake = game.Snake;
            for (int i = snake.Count - 1; i >= 1; i--)
                DrawCell(g, snake[i].X, snake[i].Y, BodyColor, false, cs);

            // Голова
            if (snake.Count > 0)
                DrawCell(g, snake[0].X, snake[0].Y, HeadColor, false, cs);

            // Рамка
            using (var borderPen = new Pen(BorderColor, 2))
                g.DrawRectangle(borderPen, 1, 1, gameWidth - 2, gameHeight - 2);

            // HUD
            g.FillRectangle(new SolidBrush(Color.FromArgb(20, 20, 35)), 0, gameHeight, gameWidth, 50);
            using (var hudFont = new Font("Consolas", 13, FontStyle.Bold))
            using (var hudBrush = new SolidBrush(TextColor))
            using (var tipFont = new Font("Consolas", 9))
            using (var tipBrush = new SolidBrush(Color.FromArgb(100, 150, 100)))
            {
                g.DrawString($"Счёт: {game.Score}", hudFont, hudBrush, 10, gameHeight + 6);
                g.DrawString($"Длина: {snake.Count}", hudFont, hudBrush, 200, gameHeight + 6);
                g.DrawString("WASD/стрелки  P-пауза  F11-полный экран  ESC-выход",
                             tipFont, tipBrush, 10, gameHeight + 30);
            }

            // Game Over overlay
            if (game.IsOver)
                DrawGameOverOverlay(g, gameWidth, gameHeight);

            // Пауза
            if (game.IsPaused && !game.IsOver)
                DrawOverlay(g, "ПАУЗА", "Нажми P чтобы продолжить", gameWidth, gameHeight);

            g.ResetTransform();
        }

        private void DrawCell(Graphics g, int col, int row, Color color, bool isFood, int cs)
        {
            int x = col * cs + 2;
            int y = row * cs + 2;
            int s = cs - 4;
            if (s < 1) return;

            if (isFood)
            {
                using (var glowBrush = new SolidBrush(Color.FromArgb(60, 255, 100, 100)))
                    g.FillEllipse(glowBrush, x - 3, y - 3, s + 6, s + 6);
                using (var brush = new SolidBrush(color))
                    g.FillEllipse(brush, x, y, s, s);
            }
            else
            {
                int r = Math.Max(2, cs / 5);
                var path = new GraphicsPath();
                var rect = new Rectangle(x, y, s, s);
                path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
                path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
                path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
                path.CloseFigure();
                using (var brush = new SolidBrush(color))
                    g.FillPath(brush, path);
            }
        }

        private void DrawGameOverOverlay(Graphics g, int w, int h)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(170, 0, 0, 0)), 0, 0, w, h);

            int pw = 360, ph = 120;
            int px = (w - pw) / 2, py = h / 2 - ph / 2 - 50;

            using (var panelBrush = new SolidBrush(Color.FromArgb(230, 20, 30, 20)))
                g.FillRectangle(panelBrush, px, py, pw, ph);
            using (var panelPen = new Pen(BorderColor, 2))
                g.DrawRectangle(panelPen, px, py, pw, ph);

            using (var titleFont = new Font("Consolas", 26, FontStyle.Bold))
            using (var subFont = new Font("Consolas", 12))
            using (var titleBrush = new SolidBrush(FoodColor))
            using (var subBrush = new SolidBrush(TextColor))
            {
                var ts = g.MeasureString("GAME OVER", titleFont);
                g.DrawString("GAME OVER", titleFont, titleBrush,
                             px + (pw - ts.Width) / 2, py + 12);

                string sub = $"Счёт: {game.Score}   Длина: {game.Snake.Count}";
                var ss = g.MeasureString(sub, subFont);
                g.DrawString(sub, subFont, subBrush,
                             px + (pw - ss.Width) / 2, py + 75);
            }
        }

        private void DrawOverlay(Graphics g, string title, string subtitle, int w, int h)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(160, 0, 0, 0)), 0, 0, w, h);
            int pw = 360, ph = 100;
            int px = (w - pw) / 2, py = (h - ph) / 2;

            using (var panelBrush = new SolidBrush(Color.FromArgb(220, 20, 30, 20)))
                g.FillRectangle(panelBrush, px, py, pw, ph);
            using (var panelPen = new Pen(BorderColor, 2))
                g.DrawRectangle(panelPen, px, py, pw, ph);

            using (var titleFont = new Font("Consolas", 22, FontStyle.Bold))
            using (var subFont = new Font("Consolas", 11))
            using (var titleBrush = new SolidBrush(FoodColor))
            using (var subBrush = new SolidBrush(TextColor))
            {
                var ts = g.MeasureString(title, titleFont);
                g.DrawString(title, titleFont, titleBrush,
                             px + (pw - ts.Width) / 2, py + 12);
                var ss = g.MeasureString(subtitle, subFont);
                g.DrawString(subtitle, subFont, subBrush,
                             px + (pw - ss.Width) / 2, py + 62);
            }
        }
    }
}