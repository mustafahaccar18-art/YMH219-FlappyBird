using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FlappyBird
{
    public enum GameState { Menu, Playing, Dead, Paused }

    public class GameForm : Form
    {
        // ── Layout ───────────────────────────────────────────────
        private const int ScreenWidth  = 480;
        private const int ScreenHeight = 640;
        private const int GroundY      = 560;

        // ── Game constants ───────────────────────────────────────
        private const int   PipeSpawnInterval = 90;   // ticks between pipes
        private const int   GapMin            = 130;
        private const int   GapMax            = 170;
        private const int   GapYMin           = 80;
        private const float BaseSpeed         = 3.0f;
        private const float SpeedIncrement    = 0.0004f; // per tick

        // ── State ────────────────────────────────────────────────
        private GameState _state = GameState.Menu;
        private Bird _bird = null!;
        private List<Pipe> _pipes = new();
        private Background _bg = null!;

        private int   _score;
        private int   _bestScore;
        private int   _tick;
        private float _speed;
        private int   _elapsedSeconds;
        private int   _ticksThisSecond;

        // ── Rendering ────────────────────────────────────────────
        private readonly Timer _timer = new();
        private Bitmap _backBuffer = null!;
        private Graphics _bbg = null!;

        // ── Flash effect ─────────────────────────────────────────
        private int _flashFrames;

        // ── Fonts & brushes ──────────────────────────────────────
        private readonly Font _scoreFont    = new Font("Arial", 36, FontStyle.Bold, GraphicsUnit.Pixel);
        private readonly Font _bigFont      = new Font("Arial", 52, FontStyle.Bold, GraphicsUnit.Pixel);
        private readonly Font _medFont      = new Font("Arial", 22, FontStyle.Bold, GraphicsUnit.Pixel);
        private readonly Font _smallFont    = new Font("Arial", 16, FontStyle.Regular, GraphicsUnit.Pixel);

        public GameForm()
        {
            Text            = "Flappy Bird";
            ClientSize      = new Size(ScreenWidth, ScreenHeight);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            DoubleBuffered  = true;
            BackColor       = Color.Black;
            StartPosition   = FormStartPosition.CenterScreen;

            InitBuffers();
            _bg = new Background(ScreenWidth, GroundY);
            ResetGame();

            _timer.Interval = 16; // ~60 fps
            _timer.Tick += OnTick;
            _timer.Start();

            KeyDown += OnKeyDown;
            MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) HandleFlap(); };
        }

        private void InitBuffers()
        {
            _backBuffer?.Dispose();
            _bbg?.Dispose();
            _backBuffer = new Bitmap(ScreenWidth, ScreenHeight);
            _bbg = Graphics.FromImage(_backBuffer);
            _bbg.SmoothingMode = SmoothingMode.AntiAlias;
        }

        private void ResetGame()
        {
            _bird            = new Bird(100, GroundY / 2f - 20);
            _pipes           = new List<Pipe>();
            _score           = 0;
            _tick            = 0;
            _speed           = BaseSpeed;
            _elapsedSeconds  = 0;
            _ticksThisSecond = 0;
        }

        // ── Input ────────────────────────────────────────────────
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { Close(); return; }
            if (e.KeyCode == Keys.P && _state == GameState.Playing)  { _state = GameState.Paused; return; }
            if (e.KeyCode == Keys.P && _state == GameState.Paused)   { _state = GameState.Playing; return; }
            HandleFlap();
        }

        private void HandleFlap()
        {
            switch (_state)
            {
                case GameState.Menu:
                    _state = GameState.Playing;
                    _bird.Flap();
                    break;
                case GameState.Playing:
                    _bird.Flap();
                    break;
                case GameState.Dead:
                    _state = GameState.Menu;
                    ResetGame();
                    break;
                case GameState.Paused:
                    _state = GameState.Playing;
                    break;
            }
        }

        // ── Game loop ────────────────────────────────────────────
        private void OnTick(object? sender, EventArgs e)
        {
            if (_state == GameState.Playing)
                Update();

            Render();
            Invalidate();
        }

        private new void Update()
        {
            _tick++;
            _speed += SpeedIncrement;

            // Timer: count elapsed seconds (timer fires at ~60 fps)
            _ticksThisSecond++;
            if (_ticksThisSecond >= 60)
            {
                _ticksThisSecond = 0;
                _elapsedSeconds++;
            }

            _bird.Update();
            _bg.Update(_speed);

            // Spawn pipes
            if (_tick % PipeSpawnInterval == 0)
            {
                var rng = new Random();
                int gapH = rng.Next(GapMin, GapMax);
                int gapY = rng.Next(GapYMin, GroundY - gapH - 40);
                _pipes.Add(new Pipe(ScreenWidth + 10, gapY, gapH));
            }

            // Update pipes & score
            foreach (var pipe in _pipes)
            {
                pipe.Update(_speed);
                if (!pipe.Passed && pipe.X + Pipe.Width < _bird.X)
                {
                    pipe.Passed = true;
                    _score++;
                    if (_score > _bestScore) _bestScore = _score;
                }
            }
            _pipes.RemoveAll(p => p.IsOffScreen);

            // Collision
            if (CheckCollision())
            {
                _flashFrames = 8;
                _state = GameState.Dead;
            }

            if (_flashFrames > 0) _flashFrames--;
        }

        private bool CheckCollision()
        {
            // Ground / ceiling
            if (_bird.Y + Bird.Height >= GroundY || _bird.Y < 0) return true;

            var birdRect = _bird.Bounds;
            foreach (var pipe in _pipes)
            {
                if (birdRect.IntersectsWith(pipe.TopBounds) ||
                    birdRect.IntersectsWith(pipe.BottomBounds))
                    return true;
            }
            return false;
        }

        // ── Rendering ────────────────────────────────────────────
        private void Render()
        {
            _bg.Draw(_bbg, ScreenHeight);

            foreach (var pipe in _pipes)
                pipe.Draw(_bbg, GroundY);

            _bird.Draw(_bbg);

            // HUD overlay
            switch (_state)
            {
                case GameState.Menu:    DrawMenu(); break;
                case GameState.Playing: DrawHUD();  break;
                case GameState.Dead:    DrawDead();  DrawHUD(); break;
                case GameState.Paused:  DrawPaused(); DrawHUD(); break;
            }

            // Flash on death
            if (_flashFrames > 0)
            {
                int alpha = _flashFrames * 30;
                using var flashBrush = new SolidBrush(Color.FromArgb(Math.Min(alpha, 255), 255, 255, 255));
                _bbg.FillRectangle(flashBrush, 0, 0, ScreenWidth, ScreenHeight);
            }
        }

        private void DrawHUD()
        {
            // Score (centered top)
            DrawTextWithShadow(_bbg, _score.ToString(), _scoreFont,
                Color.Black, Color.FromArgb(80, 255, 255, 255),
                ScreenWidth / 2f, 50, centered: true);

            // Timer (top-right, right-aligned)
            int mins = _elapsedSeconds / 60;
            int secs = _elapsedSeconds % 60;
            string timerText = $"{mins:D2}:{secs:D2}";
            SizeF timerSize = _bbg.MeasureString(timerText, _medFont);
            DrawTextWithShadow(_bbg, timerText, _medFont,
                Color.Black, Color.FromArgb(80, 255, 255, 255),
                ScreenWidth - timerSize.Width - 8f, 14, centered: false);
        }

        private void DrawMenu()
        {
            // Dim overlay
            using var dimBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
            _bbg.FillRectangle(dimBrush, 0, 0, ScreenWidth, ScreenHeight);

            DrawTextWithShadow(_bbg, "FLAPPY BIRD", _bigFont,
                Color.Black, Color.FromArgb(150, 255, 255, 255),
                ScreenWidth / 2f, 160, centered: true);

            DrawTextWithShadow(_bbg, "Press SPACE or click to start", _medFont,
                Color.Black, Color.FromArgb(120, 255, 255, 255),
                ScreenWidth / 2f, 280, centered: true);

            DrawTextWithShadow(_bbg, $"Best: {_bestScore}", _medFont,
                Color.Black, Color.FromArgb(120, 255, 255, 255),
                ScreenWidth / 2f, 330, centered: true);

            DrawTextWithShadow(_bbg, "P = Pause   ESC = Quit", _smallFont,
                Color.Black, Color.FromArgb(100, 255, 255, 255),
                ScreenWidth / 2f, 390, centered: true);
        }

        private void DrawDead()
        {
            using var dimBrush = new SolidBrush(Color.FromArgb(140, 0, 0, 0));
            _bbg.FillRectangle(dimBrush, 0, 0, ScreenWidth, ScreenHeight);

            DrawTextWithShadow(_bbg, "GAME OVER", _bigFont,
                Color.Black, Color.FromArgb(150, 255, 255, 255),
                ScreenWidth / 2f, 190, centered: true);

            DrawPanel(_bbg, ScreenWidth / 2 - 140, 270, 280, 140);

            DrawTextWithShadow(_bbg, $"Score : {_score}", _medFont,
                Color.Black, Color.FromArgb(80, 255, 255, 255), ScreenWidth / 2f, 285, centered: true);
            DrawTextWithShadow(_bbg, $"Best  : {_bestScore}", _medFont,
                Color.Black, Color.FromArgb(80, 255, 255, 255), ScreenWidth / 2f, 318, centered: true);

            int mins = _elapsedSeconds / 60;
            int secs = _elapsedSeconds % 60;
            DrawTextWithShadow(_bbg, $"Time  : {mins:D2}:{secs:D2}", _medFont,
                Color.Black, Color.FromArgb(80, 255, 255, 255), ScreenWidth / 2f, 351, centered: true);

            DrawTextWithShadow(_bbg, "Press SPACE to retry", _medFont,
                Color.Black, Color.FromArgb(120, 255, 255, 255),
                ScreenWidth / 2f, 435, centered: true);
        }

        private void DrawPaused()
        {
            using var dimBrush = new SolidBrush(Color.FromArgb(120, 0, 0, 0));
            _bbg.FillRectangle(dimBrush, 0, 0, ScreenWidth, ScreenHeight);

            DrawTextWithShadow(_bbg, "PAUSED", _bigFont,
                Color.Black, Color.FromArgb(120, 255, 255, 255),
                ScreenWidth / 2f, 220, centered: true);
            DrawTextWithShadow(_bbg, "Press P to resume", _medFont,
                Color.Black, Color.FromArgb(80, 255, 255, 255),
                ScreenWidth / 2f, 310, centered: true);
        }

        // ── Helpers ──────────────────────────────────────────────
        private static void DrawPanel(Graphics g, int x, int y, int w, int h)
        {
            using var panelBrush = new SolidBrush(Color.FromArgb(160, 30, 30, 30));
            using var panelPen   = new Pen(Color.FromArgb(200, 200, 200), 2);
            g.FillRoundedRectangle(panelBrush, x, y, w, h, 14);
            g.DrawRoundedRectangle(panelPen, x, y, w, h, 14);
        }

        private static void DrawTextWithShadow(Graphics g, string text, Font font,
            Color color, Color shadow, float x, float y, bool centered = false)
        {
            SizeF size = g.MeasureString(text, font);
            float dx = centered ? x - size.Width / 2f : x;
            float dy = y;

            using var shadowBrush = new SolidBrush(shadow);
            g.DrawString(text, font, shadowBrush, dx + 2, dy + 2);

            using var textBrush = new SolidBrush(color);
            g.DrawString(text, font, textBrush, dx, dy);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(_backBuffer, 0, 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
                _bbg.Dispose();
                _backBuffer.Dispose();
                _scoreFont.Dispose();
                _bigFont.Dispose();
                _medFont.Dispose();
                _smallFont.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Extension methods for rounded rectangles
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, int x, int y, int w, int h, int radius)
        {
            using var path = RoundedRect(x, y, w, h, radius);
            g.FillPath(brush, path);
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, int x, int y, int w, int h, int radius)
        {
            using var path = RoundedRect(x, y, w, h, radius);
            g.DrawPath(pen, path);
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(int x, int y, int w, int h, int r)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
