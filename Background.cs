using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FlappyBird
{
    public class Background
    {
        private float _groundOffset;
        private float _cloudOffset1, _cloudOffset2;

        private readonly List<(float x, float y, float size)> _clouds1 = new();
        private readonly List<(float x, float y, float size)> _clouds2 = new();

        private readonly int _screenWidth;
        private readonly int _groundY;

        public Background(int screenWidth, int groundY)
        {
            _screenWidth = screenWidth;
            _groundY = groundY;

            var rng = new Random(42);
            for (int i = 0; i < 6; i++)
            {
                _clouds1.Add((rng.Next(0, screenWidth), rng.Next(40, 140), rng.Next(40, 80)));
                _clouds2.Add((rng.Next(0, screenWidth), rng.Next(60, 180), rng.Next(25, 50)));
            }
        }

        public void Update(float pipeSpeed)
        {
            _groundOffset = (_groundOffset + pipeSpeed) % 24;
            _cloudOffset1 = (_cloudOffset1 + pipeSpeed * 0.3f) % _screenWidth;
            _cloudOffset2 = (_cloudOffset2 + pipeSpeed * 0.15f) % _screenWidth;
        }

        public void Draw(Graphics g, int screenHeight)
        {
            // Sky gradient
            using var skyBrush = new LinearGradientBrush(
                new Rectangle(0, 0, _screenWidth, _groundY),
                Color.FromArgb(100, 200, 255),
                Color.FromArgb(170, 230, 255),
                LinearGradientMode.Vertical);
            g.FillRectangle(skyBrush, 0, 0, _screenWidth, _groundY);

            // Far clouds (slow)
            foreach (var c in _clouds2)
            {
                float cx = (c.x - _cloudOffset2 + _screenWidth * 2) % (_screenWidth + 100) - 50;
                DrawCloud(g, cx, c.y, c.size, Color.FromArgb(220, 240, 255));
            }

            // Near clouds (faster)
            foreach (var c in _clouds1)
            {
                float cx = (c.x - _cloudOffset1 + _screenWidth * 2) % (_screenWidth + 120) - 60;
                DrawCloud(g, cx, c.y, c.size, Color.White);
            }

            // Ground
            int groundH = screenHeight - _groundY;
            using var dirtBrush = new SolidBrush(Color.FromArgb(220, 180, 100));
            g.FillRectangle(dirtBrush, 0, _groundY, _screenWidth, groundH);

            // Grass strip
            using var grassBrush = new SolidBrush(Color.FromArgb(100, 190, 50));
            g.FillRectangle(grassBrush, 0, _groundY, _screenWidth, 18);

            // Ground lines
            using var linePen = new Pen(Color.FromArgb(180, 140, 70), 1);
            for (float lx = -_groundOffset; lx < _screenWidth; lx += 24)
                g.DrawLine(linePen, lx, _groundY + 18, lx + 12, screenHeight);
        }

        private static void DrawCloud(Graphics g, float x, float y, float size, Color color)
        {
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, x, y, size, size * 0.6f);
            g.FillEllipse(brush, x + size * 0.3f, y - size * 0.2f, size * 0.7f, size * 0.6f);
            g.FillEllipse(brush, x + size * 0.6f, y, size * 0.5f, size * 0.5f);
        }
    }
}
