using System;
using System.Collections.Generic;
using System.Drawing;

namespace Blindrun1
{
    public class Enemy
    {
        public Point Pos;
        private int dx, dy;
        private int stepTimer = 0;
        private int stepInterval; // тиков между шагами врага
        private Random rng;
        private int cols, rows;

        public Enemy(int x, int y, int cols, int rows, int interval, Random rng)
        {
            Pos = new Point(x, y);
            this.cols = cols;
            this.rows = rows;
            this.stepInterval = interval;
            this.rng = rng;
            PickDirection();
        }

        private void PickDirection()
        {
            int[][] dirs = { new[] { 1, 0 }, new[] { -1, 0 }, new[] { 0, 1 }, new[] { 0, -1 } };
            var d = dirs[rng.Next(4)];
            dx = d[0]; dy = d[1];
        }

        public void Update(HashSet<Point> walls)
        {
            stepTimer++;
            if (stepTimer < stepInterval) return;
            stepTimer = 0;

            Point next = new Point(Pos.X + dx, Pos.Y + dy);

            bool blocked = next.X < 1 || next.X >= cols - 1 ||
                           next.Y < 1 || next.Y >= rows - 1 ||
                           walls.Contains(next);

            if (blocked)
            {
                PickDirection();
                // Пробуем ещё раз в новом направлении
                next = new Point(Pos.X + dx, Pos.Y + dy);
                if (next.X < 1 || next.X >= cols - 1 ||
                    next.Y < 1 || next.Y >= rows - 1 ||
                    walls.Contains(next))
                    return; // стоим
            }

            Pos = next;
        }
    }

    public class Game1
    {
        public List<Point> Snake { get; private set; }
        public Point Food { get; private set; }
        public int Score { get; private set; }
        public bool IsOver { get; private set; }
        public bool IsPaused { get; set; }
        public HashSet<Point> Walls { get; private set; }
        public List<Enemy> Enemies { get; private set; }
        public int Level { get; private set; } = 1;

        private int dirX = 1, dirY = 0;
        private int nextDirX = 1, nextDirY = 0;
        private int cols, rows;
        private Random rng = new Random();
        private int foodEaten = 0;
        private Difficulty difficulty;

        // Параметры по сложности
        private int EnemyCount => difficulty == Difficulty.Easy ? 1
                                : difficulty == Difficulty.Medium ? 2 : 4;
        private int EnemyStepInterval => difficulty == Difficulty.Easy ? 4
                                       : difficulty == Difficulty.Medium ? 3 : 2;

        public Game1(int cols, int rows, Difficulty diff = Difficulty.Medium)
        {
            this.cols = cols;
            this.rows = rows;
            this.difficulty = diff;

            Snake = new List<Point>();
            Walls = new HashSet<Point>();
            Enemies = new List<Enemy>();

            int cx = cols / 2;
            int cy = rows / 2;
            Snake.Add(new Point(cx, cy));
            Snake.Add(new Point(cx - 1, cy));
            Snake.Add(new Point(cx - 2, cy));

            GenerateWalls();
            SpawnEnemies();
            SpawnFood();
        }

        public void SetDirection(int dx, int dy)
        {
            if (dx == -dirX && dy == -dirY) return;
            nextDirX = dx;
            nextDirY = dy;
        }

        public void Update()
        {
            if (IsOver || IsPaused) return;
            dirX = nextDirX;
            dirY = nextDirY;

            Point head = Snake[0];
            Point newHead = new Point(head.X + dirX, head.Y + dirY);

            if (newHead.X < 0 || newHead.X >= cols ||
                newHead.Y < 0 || newHead.Y >= rows)
            { IsOver = true; return; }

            if (Walls.Contains(newHead))
            { IsOver = true; return; }

            foreach (var seg in Snake)
                if (seg == newHead) { IsOver = true; return; }

            Snake.Insert(0, newHead);

            if (newHead == Food)
            {
                Score += 10;
                foodEaten++;
                if (foodEaten % 3 == 0)
                {
                    Level++;
                    GenerateWalls();
                    SpawnEnemies();
                }
                SpawnFood();
            }
            else
            {
                Snake.RemoveAt(Snake.Count - 1);
            }

            // Обновляем врагов
            foreach (var enemy in Enemies)
                enemy.Update(Walls);

            // Проверяем столкновение с врагами
            foreach (var enemy in Enemies)
                if (enemy.Pos == Snake[0]) { IsOver = true; return; }
        }

        private void SpawnEnemies()
        {
            Enemies.Clear();
            var safeZone = new HashSet<Point>();
            if (Snake.Count > 0)
                for (int dx2 = -6; dx2 <= 6; dx2++)
                    for (int dy2 = -6; dy2 <= 6; dy2++)
                        safeZone.Add(new Point(Snake[0].X + dx2, Snake[0].Y + dy2));

            int attempts = 0;
            while (Enemies.Count < EnemyCount && attempts < 1000)
            {
                attempts++;
                int x = rng.Next(2, cols - 2);
                int y = rng.Next(2, rows - 2);
                var p = new Point(x, y);
                if (Walls.Contains(p) || safeZone.Contains(p)) continue;
                Enemies.Add(new Enemy(x, y, cols, rows, EnemyStepInterval, rng));
            }
        }

        // --- Всё остальное как было (GenerateWalls и ниже) ---

        private void GenerateWalls()
        {
            Walls.Clear();
            var safeZone = new HashSet<Point>();
            if (Snake.Count > 0)
                for (int dx2 = -4; dx2 <= 4; dx2++)
                    for (int dy2 = -4; dy2 <= 4; dy2++)
                        safeZone.Add(new Point(Snake[0].X + dx2, Snake[0].Y + dy2));

            int pattern = (Level - 1) % 6;
            switch (pattern)
            {
                case 0: AddCross(cols / 2, rows / 2, 6, safeZone); break;
                case 1:
                    AddBlock(5, 5, 4, 3, safeZone); AddBlock(cols - 9, 5, 4, 3, safeZone);
                    AddBlock(5, rows - 8, 4, 3, safeZone); AddBlock(cols - 9, rows - 8, 4, 3, safeZone);
                    break;
                case 2: AddZigzagH(rows / 3, safeZone); AddZigzagH(rows * 2 / 3, safeZone); break;
                case 3: AddFrameWithGaps(safeZone); break;
                case 4: AddDiagonalBarriers(safeZone); break;
                case 5: AddRoomMaze(safeZone); break;
            }
        }

        private void AddCross(int cx, int cy, int halfLen, HashSet<Point> safe)
        {
            for (int i = -halfLen; i <= halfLen; i++)
            { TryAddWall(cx + i, cy, safe); TryAddWall(cx, cy + i, safe); }
        }

        private void AddBlock(int x, int y, int w, int h, HashSet<Point> safe)
        {
            for (int i = x; i < x + w; i++)
                for (int j = y; j < y + h; j++)
                    TryAddWall(i, j, safe);
        }

        private void AddZigzagH(int row, HashSet<Point> safe)
        {
            bool up = true;
            for (int x = 2; x < cols - 2; x += 4)
            {
                int r = up ? row - 1 : row + 1;
                for (int dx2 = 0; dx2 < 4; dx2++) TryAddWall(x + dx2, r, safe);
                up = !up;
            }
        }

        private void AddFrameWithGaps(HashSet<Point> safe)
        {
            int margin = 6, gapSize = 3;
            for (int x = margin; x < cols - margin; x++)
                if (!InGap(x, margin, margin, gapSize)) TryAddWall(x, margin, safe);
            for (int x = margin; x < cols - margin; x++)
                if (!InGap(x, margin, cols - margin, gapSize)) TryAddWall(x, rows - margin, safe);
            for (int y = margin; y < rows - margin; y++)
                if (!InGap(y, margin, rows / 2, gapSize)) TryAddWall(margin, y, safe);
            for (int y = margin; y < rows - margin; y++)
                if (!InGap(y, margin, rows / 2, gapSize)) TryAddWall(cols - margin, y, safe);
        }

        private bool InGap(int pos, int start, int center, int gapSize) =>
            Math.Abs(pos - center) <= gapSize / 2;

        private void AddDiagonalBarriers(HashSet<Point> safe)
        {
            for (int i = 0; i < 10; i++) TryAddWall(4 + i, 4 + i, safe);
            for (int i = 0; i < 10; i++) TryAddWall(cols - 5 - i, 4 + i, safe);
            for (int x = 3; x < cols - 3; x++)
                if (Math.Abs(x - cols / 2) > 3) TryAddWall(x, rows / 2, safe);
        }

        private void AddRoomMaze(HashSet<Point> safe)
        {
            int[] vxPositions = { 8, 15, 21 };
            foreach (int vx in vxPositions)
            {
                int gapY = rng.Next(3, rows - 6);
                for (int y = 1; y < rows - 1; y++)
                    if (Math.Abs(y - gapY) > 2) TryAddWall(vx, y, safe);
            }
            int[] hyPositions = { 8, 18 };
            foreach (int hy in hyPositions)
            {
                int gapX = rng.Next(3, cols - 6);
                for (int x = 1; x < cols - 1; x++)
                    if (Math.Abs(x - gapX) > 2) TryAddWall(x, hy, safe);
            }
        }

        private void TryAddWall(int x, int y, HashSet<Point> safe)
        {
            if (x < 1 || x >= cols - 1 || y < 1 || y >= rows - 1) return;
            var p = new Point(x, y);
            if (!safe.Contains(p)) Walls.Add(p);
        }

        private void SpawnFood()
        {
            int attempts = 0;
            while (true)
            {
                var candidate = new Point(rng.Next(0, cols), rng.Next(0, rows));
                if (Walls.Contains(candidate)) continue;
                bool onSnake = false;
                foreach (var seg in Snake) if (seg == candidate) { onSnake = true; break; }
                if (!onSnake) { Food = candidate; return; }
                if (++attempts > 10000) break;
            }
        }
    }
}