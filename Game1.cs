using System;
using System.Collections.Generic;
using System.Drawing;

namespace Blindrun1
{
    public class Game1
    {
        public List<Point> Snake { get; private set; }
        public Point Food { get; private set; }
        public int Score { get; private set; }
        public bool IsOver { get; private set; }
        public bool IsPaused { get; set; }
        public HashSet<Point> Walls { get; private set; }
        public int Level { get; private set; } = 1;

        private int dirX = 1, dirY = 0;
        private int nextDirX = 1, nextDirY = 0;
        private int cols, rows;
        private Random rng = new Random();
        private int foodEaten = 0;

        public Game1(int cols, int rows)
        {
            this.cols = cols;
            this.rows = rows;
            Snake = new List<Point>();
            Walls = new HashSet<Point>();

            int cx = cols / 2;
            int cy = rows / 2;
            Snake.Add(new Point(cx, cy));
            Snake.Add(new Point(cx - 1, cy));
            Snake.Add(new Point(cx - 2, cy));

            GenerateWalls();
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

            // Стена по краю
            if (newHead.X < 0 || newHead.X >= cols ||
                newHead.Y < 0 || newHead.Y >= rows)
            {
                IsOver = true;
                return;
            }

            // Стенки-препятствия
            if (Walls.Contains(newHead))
            {
                IsOver = true;
                return;
            }

            // Сам себя
            foreach (var seg in Snake)
            {
                if (seg == newHead)
                {
                    IsOver = true;
                    return;
                }
            }

            Snake.Insert(0, newHead);

            if (newHead == Food)
            {
                Score += 10;
                foodEaten++;

                // Новый уровень каждые 3 съеденных еды
                if (foodEaten % 3 == 0)
                {
                    Level++;
                    GenerateWalls();
                }

                SpawnFood();
            }
            else
            {
                Snake.RemoveAt(Snake.Count - 1);
            }
        }

        private void GenerateWalls()
        {
            Walls.Clear();

            // Безопасная зона вокруг головы змейки
            var safeZone = new HashSet<Point>();
            if (Snake.Count > 0)
            {
                for (int dx = -4; dx <= 4; dx++)
                    for (int dy = -4; dy <= 4; dy++)
                        safeZone.Add(new Point(Snake[0].X + dx, Snake[0].Y + dy));
            }

            int pattern = (Level - 1) % 6; // 6 паттернов, потом цикл

            switch (pattern)
            {
                case 0: // Уровень 1 — крест посередине
                    AddCross(cols / 2, rows / 2, 6, safeZone);
                    break;

                case 1: // Уровень 2 — 4 блока по углам
                    AddBlock(5, 5, 4, 3, safeZone);
                    AddBlock(cols - 9, 5, 4, 3, safeZone);
                    AddBlock(5, rows - 8, 4, 3, safeZone);
                    AddBlock(cols - 9, rows - 8, 4, 3, safeZone);
                    break;

                case 2: // Уровень 3 — зигзаг горизонтальный
                    AddZigzagH(rows / 3, safeZone);
                    AddZigzagH(rows * 2 / 3, safeZone);
                    break;

                case 3: // Уровень 4 — внутренняя рамка с проходами
                    AddFrameWithGaps(safeZone);
                    break;

                case 4: // Уровень 5 — диагональные барьеры
                    AddDiagonalBarriers(safeZone);
                    break;

                case 5: // Уровень 6 — лабиринт из комнат
                    AddRoomMaze(safeZone);
                    break;
            }
        }

        private void AddCross(int cx, int cy, int halfLen, HashSet<Point> safe)
        {
            for (int i = -halfLen; i <= halfLen; i++)
            {
                TryAddWall(cx + i, cy, safe);
                TryAddWall(cx, cy + i, safe);
            }
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
                for (int dx = 0; dx < 4; dx++)
                    TryAddWall(x + dx, r, safe);
                up = !up;
            }
        }

        private void AddFrameWithGaps(HashSet<Point> safe)
        {
            int margin = 6;
            int gapSize = 3;

            // Верхняя горизонтальная линия с разрывами
            for (int x = margin; x < cols - margin; x++)
                if (!InGap(x, margin, margin, gapSize))
                    TryAddWall(x, margin, safe);

            // Нижняя
            for (int x = margin; x < cols - margin; x++)
                if (!InGap(x, margin, cols - margin, gapSize))
                    TryAddWall(x, rows - margin, safe);

            // Левая вертикальная
            for (int y = margin; y < rows - margin; y++)
                if (!InGap(y, margin, rows / 2, gapSize))
                    TryAddWall(margin, y, safe);

            // Правая вертикальная
            for (int y = margin; y < rows - margin; y++)
                if (!InGap(y, margin, rows / 2, gapSize))
                    TryAddWall(cols - margin, y, safe);
        }

        private bool InGap(int pos, int start, int center, int gapSize)
        {
            return Math.Abs(pos - center) <= gapSize / 2;
        }

        private void AddDiagonalBarriers(HashSet<Point> safe)
        {
            // Диагональ сверху-слева вниз
            for (int i = 0; i < 10; i++)
                TryAddWall(4 + i, 4 + i, safe);

            // Диагональ сверху-справа вниз
            for (int i = 0; i < 10; i++)
                TryAddWall(cols - 5 - i, 4 + i, safe);

            // Горизонталь посередине с разрывом
            for (int x = 3; x < cols - 3; x++)
                if (Math.Abs(x - cols / 2) > 3)
                    TryAddWall(x, rows / 2, safe);
        }

        private void AddRoomMaze(HashSet<Point> safe)
        {
            // Вертикальные перегородки с разрывами
            int[] vxPositions = { 8, 15, 21 };
            foreach (int vx in vxPositions)
            {
                int gapY = rng.Next(3, rows - 6);
                for (int y = 1; y < rows - 1; y++)
                    if (Math.Abs(y - gapY) > 2)
                        TryAddWall(vx, y, safe);
            }

            // Горизонтальные перегородки с разрывами
            int[] hyPositions = { 8, 18 };
            foreach (int hy in hyPositions)
            {
                int gapX = rng.Next(3, cols - 6);
                for (int x = 1; x < cols - 1; x++)
                    if (Math.Abs(x - gapX) > 2)
                        TryAddWall(x, hy, safe);
            }
        }

        private void TryAddWall(int x, int y, HashSet<Point> safe)
        {
            if (x < 1 || x >= cols - 1 || y < 1 || y >= rows - 1) return;
            var p = new Point(x, y);
            if (!safe.Contains(p))
                Walls.Add(p);
        }

        private void SpawnFood()
        {
            int attempts = 0;
            while (true)
            {
                var candidate = new Point(rng.Next(0, cols), rng.Next(0, rows));
                if (Walls.Contains(candidate)) continue;

                bool onSnake = false;
                foreach (var seg in Snake)
                    if (seg == candidate) { onSnake = true; break; }

                if (!onSnake)
                {
                    Food = candidate;
                    return;
                }

                if (++attempts > 10000) break; // защита от зависания
            }
        }
    }
}