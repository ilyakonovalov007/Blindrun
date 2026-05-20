using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Media;
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

        // Туман
        private float[,] fogMap = new float[Cols, Rows];
        private const float FogSpeed = 0.0015f;
        private const float FogLightRadius = 7.5f;

        // Мерцание еды
        private int foodPulse = 0;
        private int foodPulseDir = 1;

        // Флэш смерти
        private int deathFlash = 0;
        private bool deathSoundPlayed = false;

        // Флэш нового уровня
        private int levelFlash = 0;
        private int prevLevel = 1;

        // Звук сбора
        private int prevScore = 0;

        // Цвета
        private readonly Color BgColor = Color.FromArgb(12, 8, 18);
        private readonly Color GridColor = Color.FromArgb(30, 20, 40);
        private readonly Color HeadColor = Color.FromArgb(160, 255, 130);
        private readonly Color BodyColor = Color.FromArgb(80, 190, 90);
        private readonly Color FoodColor = Color.FromArgb(255, 50, 50);
        private readonly Color BorderColor = Color.FromArgb(120, 30, 30);
        private readonly Color TextColor = Color.FromArgb(210, 170, 170);
        private readonly Color FogColor = Color.FromArgb(12, 8, 18);
        private readonly Color WallColor = Color.FromArgb(160, 40, 30);
        private readonly Color WallDarkColor = Color.FromArgb(80, 15, 10);
        private readonly Color WallLightColor = Color.FromArgb(200, 70, 50);

        public Form1()
        {
            InitializeComponent();

            this.Text = "🐍 Blindrun";
            this.ClientSize = new Size(800, 850);
            this.MinimumSize = new Size(400, 450);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = BgColor;
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.KeyDown += OnKeyDown;
            this.Resize += (s, e) => { UpdateButtonPositions(); this.Invalidate(); };

            btnRestart = new Button();
            btnRestart.Text = "▶  Заново";
            btnRestart.Font = new Font("Consolas", 14, FontStyle.Bold);
            btnRestart.ForeColor = Color.FromArgb(180, 60, 60);
            btnRestart.BackColor = Color.FromArgb(30, 10, 10);
            btnRestart.FlatStyle = FlatStyle.Flat;
            btnRestart.FlatAppearance.BorderColor = Color.FromArgb(120, 20, 20);
            btnRestart.FlatAppearance.BorderSize = 2;
            btnRestart.Size = new Size(160, 48);
            btnRestart.Visible = false;
            btnRestart.Cursor = Cursors.Hand;
            btnRestart.Click += (s, e) => { RestartGame(); this.Focus(); };
            this.Controls.Add(btnRestart);

            btnExit = new Button();
            btnExit.Text = "✕  Выйти";
            btnExit.Font = new Font("Consolas", 14, FontStyle.Bold);
            btnExit.ForeColor = Color.FromArgb(100, 100, 100);
            btnExit.BackColor = Color.FromArgb(15, 15, 15);
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            btnExit.FlatAppearance.BorderSize = 2;
            btnExit.Size = new Size(160, 48);
            btnExit.Visible = false;
            btnExit.Cursor = Cursors.Hand;
            btnExit.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnExit);

            game = new Game1(Cols, Rows);
            InitFog();

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 150;
            gameTimer.Tick += OnTick;
            gameTimer.Start();
        }

        private void InitFog()
        {
            for (int x = 0; x < Cols; x++)
                for (int y = 0; y < Rows; y++)
                    fogMap[x, y] = 0f;
        }

        private void RestartGame()
        {
            game = new Game1(Cols, Rows);
            gameTimer.Interval = 150;
            deathFlash = 0;
            deathSoundPlayed = false;
            levelFlash = 0;
            prevLevel = 1;
            prevScore = 0;
            InitFog();
            btnRestart.Visible = false;
            btnExit.Visible = false;
            this.Invalidate();
        }

        private void UpdateButtonPositions()
        {
            int cs = CellSize;
            int gameHeight = Rows * cs;
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

        private void UpdateFog()
        {
            var snake = game.Snake;
            if (snake.Count == 0) return;
            var head = snake[0];

            for (int x = 0; x < Cols; x++)
                for (int y = 0; y < Rows; y++)
                    fogMap[x, y] = Math.Min(1f, fogMap[x, y] + FogSpeed);

            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    float dist = (float)Math.Sqrt(
                        (x - head.X) * (x - head.X) +
                        (y - head.Y) * (y - head.Y));

                    if (dist < FogLightRadius)
                    {
                        float light = 1f - (dist / FogLightRadius);
                        light = light * light;
                        fogMap[x, y] = Math.Max(0f, fogMap[x, y] - light * 0.9f);
                    }
                }
            }

            fogMap[game.Food.X, game.Food.Y] = Math.Min(fogMap[game.Food.X, game.Food.Y], 0.3f);
            for (int dx = -2; dx <= 2; dx++)
                for (int dy = -2; dy <= 2; dy++)
                {
                    int fx = game.Food.X + dx;
                    int fy = game.Food.Y + dy;
                    if (fx >= 0 && fx < Cols && fy >= 0 && fy < Rows)
                    {
                        float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                        float maxFog = 0.3f + dist * 0.15f;
                        fogMap[fx, fy] = Math.Min(fogMap[fx, fy], maxFog);
                    }
                }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!game.IsOver && !game.IsPaused)
            {
                game.Update();
                gameTimer.Interval = Math.Max(60, 150 - game.Score * 2);
                UpdateFog();

                // Звук сбора черепа
                if (game.Score > prevScore)
                {
                    prevScore = game.Score;
                    PlayPickupSound();
                }

                foodPulse += foodPulseDir * 3;
                if (foodPulse >= 40 || foodPulse <= 0) foodPulseDir = -foodPulseDir;

                // Детектируем смену уровня
                if (game.Level != prevLevel)
                {
                    prevLevel = game.Level;
                    levelFlash = 40;
                    InitFog();
                }
            }

            if (levelFlash > 0) levelFlash--;

            if (game.IsOver)
            {
                if (!deathSoundPlayed)
                {
                    deathSoundPlayed = true;
                    PlayDeathSound();
                }

                if (deathFlash < 20) deathFlash++;
                UpdateButtonPositions();
                btnRestart.Visible = true;
                btnExit.Visible = true;
            }

            this.Invalidate();
        }

        // ── Звуки ────────────────────────────────────────────────────────────

        private void PlayDeathSound()
        {
            try
            {
                using (var ms = new MemoryStream(GenerateDeathWav()))
                using (var player = new SoundPlayer(ms))
                    player.Play();
            }
            catch { }
        }

        private void PlayPickupSound()
        {
            try
            {
                using (var ms = new MemoryStream(GeneratePickupWav()))
                using (var player = new SoundPlayer(ms))
                    player.Play();
            }
            catch { }
        }

        /// Три нисходящих тона — эффект смерти
        private byte[] GenerateDeathWav()
        {
            var tones = new (double freq, double dur)[]
            {
                (440, 0.13),
                (330, 0.13),
                (200, 0.22),
            };
            return BuildWav(tones, 28000);
        }

        /// Два восходящих тона — приятный "дзинь" при сборе
        private byte[] GeneratePickupWav()
        {
            var tones = new (double freq, double dur)[]
            {
                (440, 0.07),
                (660, 0.10),
            };
            return BuildWav(tones, 22000);
        }

        /// Универсальный генератор WAV (PCM 16-bit, 44100 Hz, моно)
        private byte[] BuildWav((double freq, double dur)[] tones, short amplitude)
        {
            const int sampleRate = 44100;

            int totalSamples = 0;
            foreach (var t in tones)
                totalSamples += (int)(sampleRate * t.dur);

            short[] samples = new short[totalSamples];
            int pos = 0;

            foreach (var (freq, dur) in tones)
            {
                int count = (int)(sampleRate * dur);
                for (int i = 0; i < count; i++)
                {
                    double t = (double)i / sampleRate;
                    double envelope = 1.0 - (double)i / count;
                    samples[pos++] = (short)(Math.Sin(2 * Math.PI * freq * t) * envelope * amplitude);
                }
            }

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                int dataSize = samples.Length * 2;
                bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                bw.Write(36 + dataSize);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                bw.Write(16);
                bw.Write((short)1);       // PCM
                bw.Write((short)1);       // моно
                bw.Write(sampleRate);
                bw.Write(sampleRate * 2); // byteRate
                bw.Write((short)2);       // blockAlign
                bw.Write((short)16);      // bitsPerSample
                bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                bw.Write(dataSize);
                foreach (var s in samples)
                    bw.Write(s);
                return ms.ToArray();
            }
        }

        // ── Ввод ─────────────────────────────────────────────────────────────

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

        // ── Отрисовка ────────────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            int cs = CellSize;
            int gameWidth = Cols * cs;
            int gameHeight = Rows * cs;
            int offsetX = (ClientSize.Width - gameWidth) / 2;

            g.TranslateTransform(offsetX, 0);

            // Фон
            g.FillRectangle(new SolidBrush(BgColor), 0, 0, gameWidth, gameHeight);

            // Сетка
            using (var gridPen = new Pen(GridColor, 1))
            {
                for (int x = 0; x <= Cols; x++)
                    g.DrawLine(gridPen, x * cs, 0, x * cs, gameHeight);
                for (int y = 0; y <= Rows; y++)
                    g.DrawLine(gridPen, 0, y * cs, gameWidth, y * cs);
            }

            DrawWalls(g, cs);

            var snake = game.Snake;
            for (int i = snake.Count - 1; i >= 1; i--)
            {
                float fog = fogMap[snake[i].X, snake[i].Y];
                if (fog < 0.98f)
                {
                    int alpha = (int)(255 * (1f - fog));
                    DrawSnakeCell(g, snake[i].X, snake[i].Y,
                        Color.FromArgb(alpha, BodyColor), cs);
                }
            }

            if (snake.Count > 0)
            {
                float fog = fogMap[snake[0].X, snake[0].Y];
                int alpha = (int)(255 * (1f - fog));
                DrawSnakeCell(g, snake[0].X, snake[0].Y,
                    Color.FromArgb(Math.Max(alpha, 180), HeadColor), cs);
            }

            {
                float fog = fogMap[game.Food.X, game.Food.Y];
                int alpha = Math.Max(120, (int)(255 * (1f - fog)));
                DrawFood(g, game.Food.X, game.Food.Y, alpha, cs);
            }

            DrawFogLayer(g, cs, gameWidth, gameHeight);

            if (game.IsOver && deathFlash < 15)
            {
                int flashAlpha = (int)(180 * (1f - deathFlash / 15f));
                g.FillRectangle(new SolidBrush(Color.FromArgb(flashAlpha, 180, 0, 0)),
                                0, 0, gameWidth, gameHeight);
            }

            if (levelFlash > 0)
            {
                int flashAlpha = (int)(120 * (levelFlash / 40f));
                g.FillRectangle(new SolidBrush(Color.FromArgb(flashAlpha, 30, 60, 180)),
                                0, 0, gameWidth, gameHeight);
            }

            using (var borderPen = new Pen(BorderColor, 3))
                g.DrawRectangle(borderPen, 1, 1, gameWidth - 2, gameHeight - 2);

            // HUD
            g.FillRectangle(new SolidBrush(Color.FromArgb(20, 8, 8)), 0, gameHeight, gameWidth, 50);
            using (var hudFont = new Font("Consolas", 13, FontStyle.Bold))
            using (var hudBrush = new SolidBrush(TextColor))
            using (var tipFont = new Font("Consolas", 9))
            using (var tipBrush = new SolidBrush(Color.FromArgb(80, 60, 60)))
            using (var lvlBrush = new SolidBrush(Color.FromArgb(120, 140, 220)))
            {
                g.DrawString($"Счёт: {game.Score}", hudFont, hudBrush, 10, gameHeight + 6);
                g.DrawString($"Длина: {snake.Count}", hudFont, hudBrush, 200, gameHeight + 6);
                g.DrawString($"Уровень: {game.Level}", hudFont, lvlBrush, 380, gameHeight + 6);
                g.DrawString("WASD/стрелки  P-пауза  F11-полный экран  ESC-выход",
                             tipFont, tipBrush, 10, gameHeight + 30);
            }

            if (levelFlash > 20 && !game.IsOver)
                DrawOverlay(g, $"— УРОВЕНЬ {game.Level} —", GetLevelDescription(game.Level), gameWidth, gameHeight);

            if (game.IsOver)
                DrawGameOverOverlay(g, gameWidth, gameHeight);

            if (game.IsPaused && !game.IsOver)
                DrawOverlay(g, "— ПАУЗА —", "Нажми P чтобы продолжить", gameWidth, gameHeight);

            g.ResetTransform();
        }

        private string GetLevelDescription(int level)
        {
            switch ((level - 1) % 6)
            {
                case 0: return "Крест посередине";
                case 1: return "Угловые блоки";
                case 2: return "Зигзаги";
                case 3: return "Рамка с проходами";
                case 4: return "Диагональные барьеры";
                case 5: return "Лабиринт";
                default: return "";
            }
        }

        private void DrawWalls(Graphics g, int cs)
        {
            foreach (var wall in game.Walls)
            {
                float fog = fogMap[wall.X, wall.Y];
                if (fog >= 0.98f) continue;

                int alpha = (int)(255 * (1f - fog));
                int x = wall.X * cs;
                int y = wall.Y * cs;
                int s = cs;

                using (var fillBrush = new SolidBrush(Color.FromArgb(alpha, WallColor)))
                    g.FillRectangle(fillBrush, x + 1, y + 1, s - 2, s - 2);

                using (var lightPen = new Pen(Color.FromArgb(alpha, WallLightColor), 1))
                {
                    g.DrawLine(lightPen, x + 1, y + 1, x + s - 2, y + 1);
                    g.DrawLine(lightPen, x + 1, y + 1, x + 1, y + s - 2);
                }

                using (var darkPen = new Pen(Color.FromArgb(alpha, WallDarkColor), 1))
                {
                    g.DrawLine(darkPen, x + 1, y + s - 2, x + s - 2, y + s - 2);
                    g.DrawLine(darkPen, x + s - 2, y + 1, x + s - 2, y + s - 2);
                }

                if (cs >= 12)
                {
                    using (var brickBrush = new SolidBrush(Color.FromArgb(alpha / 3, WallDarkColor)))
                    {
                        int mid = s / 2;
                        g.FillRectangle(brickBrush, x + 2, y + mid, s - 4, 1);
                    }
                }
            }
        }

        private void DrawSnakeCell(Graphics g, int col, int row, Color color, int cs)
        {
            int x = col * cs + 2;
            int y = row * cs + 2;
            int s = cs - 4;
            if (s < 1) return;

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

        private void DrawFood(Graphics g, int col, int row, int alpha, int cs)
        {
            int x = col * cs;
            int y = row * cs;
            int s = cs;
            if (s < 4) return;

            int glowAlpha = (int)(alpha * 0.35f) + foodPulse;
            glowAlpha = Math.Min(255, glowAlpha);
            using (var glowBrush = new SolidBrush(Color.FromArgb(glowAlpha, 180, 0, 0)))
                g.FillEllipse(glowBrush, x - 4, y - 4, s + 8, s + 8);

            using (var font = new Font("Segoe UI Emoji", cs * 0.55f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(Color.FromArgb(alpha, 220, 60, 60)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString("💀", font, brush, new RectangleF(x, y, s, s), sf);
            }
        }

        private void DrawFogLayer(Graphics g, int cs, int gameWidth, int gameHeight)
        {
            for (int x = 0; x < Cols; x++)
                for (int y = 0; y < Rows; y++)
                {
                    float fog = fogMap[x, y];
                    if (fog < 0.02f) continue;
                    int alpha = (int)(fog * 255);
                    using (var fogBrush = new SolidBrush(Color.FromArgb(alpha, FogColor)))
                        g.FillRectangle(fogBrush, x * cs, y * cs, cs, cs);
                }

            int vSize = (int)(Cols * cs * 0.22f);
            DrawVignette(g, gameWidth, gameHeight, vSize);
        }

        private void DrawVignette(Graphics g, int w, int h, int size)
        {
            using (var br = new LinearGradientBrush(new Rectangle(0, 0, size, h),
                Color.FromArgb(200, BgColor), Color.Transparent, 0f))
                g.FillRectangle(br, 0, 0, size, h);

            using (var br = new LinearGradientBrush(new Rectangle(w - size, 0, size, h),
                Color.Transparent, Color.FromArgb(200, BgColor), 0f))
                g.FillRectangle(br, w - size, 0, size, h);

            using (var br = new LinearGradientBrush(new Rectangle(0, 0, w, size),
                Color.FromArgb(200, BgColor), Color.Transparent, 90f))
                g.FillRectangle(br, 0, 0, w, size);

            using (var br = new LinearGradientBrush(new Rectangle(0, h - size, w, size),
                Color.Transparent, Color.FromArgb(200, BgColor), 90f))
                g.FillRectangle(br, 0, h - size, w, size);
        }

        private void DrawGameOverOverlay(Graphics g, int w, int h)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 5, 0, 0)), 0, 0, w, h);

            int pw = 380, ph = 130;
            int px = (w - pw) / 2, py = h / 2 - ph / 2 - 50;

            using (var panelBrush = new SolidBrush(Color.FromArgb(240, 15, 5, 5)))
                g.FillRectangle(panelBrush, px, py, pw, ph);
            using (var panelPen = new Pen(Color.FromArgb(150, 20, 20), 2))
                g.DrawRectangle(panelPen, px, py, pw, ph);

            using (var titleFont = new Font("Consolas", 26, FontStyle.Bold))
            using (var subFont = new Font("Consolas", 12))
            using (var titleBrush = new SolidBrush(Color.FromArgb(200, 30, 30)))
            using (var subBrush = new SolidBrush(Color.FromArgb(150, 120, 120)))
            {
                var ts = g.MeasureString("💀  GAME OVER  💀", titleFont);
                g.DrawString("💀  GAME OVER  💀", titleFont, titleBrush,
                             px + (pw - ts.Width) / 2, py + 12);

                string sub = $"Счёт: {game.Score}   Длина: {game.Snake.Count}   Уровень: {game.Level}";
                var ss = g.MeasureString(sub, subFont);
                g.DrawString(sub, subFont, subBrush,
                             px + (pw - ss.Width) / 2, py + 82);
            }
        }

        private void DrawOverlay(Graphics g, string title, string subtitle, int w, int h)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(160, 0, 0, 0)), 0, 0, w, h);
            int pw = 360, ph = 100;
            int px = (w - pw) / 2, py = (h - ph) / 2;

            using (var panelBrush = new SolidBrush(Color.FromArgb(220, 15, 10, 15)))
                g.FillRectangle(panelBrush, px, py, pw, ph);
            using (var panelPen = new Pen(Color.FromArgb(80, 30, 80), 2))
                g.DrawRectangle(panelPen, px, py, pw, ph);

            using (var titleFont = new Font("Consolas", 22, FontStyle.Bold))
            using (var subFont = new Font("Consolas", 11))
            using (var titleBrush = new SolidBrush(Color.FromArgb(160, 100, 160)))
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