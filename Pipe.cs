using System;
using System.Drawing;

namespace FlappyBird
{
    public class Pipe
    {
        public float X { get; set; }
        public int GapY { get; }       // top of the gap
        public int GapHeight { get; }  // height of the gap
        public bool Passed { get; set; }

        public const int Width = 60;

        private static readonly Color PipeColor = Color.FromArgb(100, 200, 50);
        private static readonly Color PipeDark  = Color.FromArgb(60,  150, 20);
        private static readonly Color PipeLight = Color.FromArgb(140, 230, 80);
        private const int CapHeight = 20;
        private const int CapOverhang = 5;

        public Pipe(float x, int gapY, int gapHeight)
        {
            X = x;
            GapY = gapY;
            GapHeight = gapHeight;
        }

        // Collision rectangles (slightly inset for forgiveness)
        public RectangleF TopBounds    => new RectangleF(X + 3, 0, Width - 6, GapY);
        public RectangleF BottomBounds => new RectangleF(X + 3, GapY + GapHeight, Width - 6, 2000);

        public void Update(float speed) => X -= speed;

        public bool IsOffScreen => X + Width < 0;

        public void Draw(Graphics g, int screenHeight)
        {
            int topPipeBottom = GapY;
            int bottomPipeTop = GapY + GapHeight;

            DrawPipeSegment(g, (int)X, 0, topPipeBottom, true, screenHeight);
            DrawPipeSegment(g, (int)X, bottomPipeTop, screenHeight - bottomPipeTop, false, screenHeight);
        }

        private void DrawPipeSegment(Graphics g, int x, int y, int height, bool isTop, int screenH)
        {
            if (height <= 0) return;

            // Main body
            using var bodyBrush = new SolidBrush(PipeColor);
            g.FillRectangle(bodyBrush, x + CapOverhang, y, Width - CapOverhang * 2, height);

            // Shading
            using var lightBrush = new SolidBrush(PipeLight);
            g.FillRectangle(lightBrush, x + CapOverhang, y, 8, height);
            using var darkBrush = new SolidBrush(PipeDark);
            g.FillRectangle(darkBrush, x + Width - CapOverhang - 8, y, 8, height);

            // Cap
            int capY = isTop ? y + height - CapHeight : y;
            using var capBrush = new SolidBrush(PipeColor);
            g.FillRectangle(capBrush, x, capY, Width, CapHeight);
            using var capLightBrush = new SolidBrush(PipeLight);
            g.FillRectangle(capLightBrush, x, capY, 10, CapHeight);
            using var capDarkBrush = new SolidBrush(PipeDark);
            g.FillRectangle(capDarkBrush, x + Width - 10, capY, 10, CapHeight);
            using var edgePen = new Pen(PipeDark, 2);
            g.DrawRectangle(edgePen, x, capY, Width, CapHeight);
        }
    }
}
