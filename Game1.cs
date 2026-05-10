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

        private int dirX = 1, dirY = 0;
        private int nextDirX = 1, nextDirY = 0;
        private int cols, rows;
        private Random rng = new Random();

        public Game1(int cols, int rows)
        {
            this.cols = cols;
            this.rows = rows;

            Snake = new List<Point>();
            int cx = cols / 2;
            int cy = rows / 2;
            Snake.Add(new Point(cx, cy));
            Snake.Add(new Point(cx - 1, cy));
            Snake.Add(new Point(cx - 2, cy));

            SpawnFood();
        }

        public void SetDirection(int dx, int dy)
        {
            // Запрещаем разворот на 180°
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

            // Стена
            if (newHead.X < 0 || newHead.X >= cols ||
                newHead.Y < 0 || newHead.Y >= rows)
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
                SpawnFood();
                // Хвост остаётся — змейка растёт
            }
            else
            {
                Snake.RemoveAt(Snake.Count - 1);
            }
        }

        private void SpawnFood()
        {
            while (true)
            {
                var candidate = new Point(rng.Next(0, cols), rng.Next(0, rows));
                bool onSnake = false;
                foreach (var seg in Snake)
                    if (seg == candidate) { onSnake = true; break; }
                if (!onSnake)
                {
                    Food = candidate;
                    return;
                }
            }
        }
    }
}
