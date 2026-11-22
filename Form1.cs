using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FlappyBird;

public partial class Form1 : Form
{
    // ── Game constants ──────────────────────────────────────────────
    const int GameWidth    = 480;
    const int GameHeight   = 640;
    const int BirdX        = 100;
    const int BirdSize     = 36;
    const int PipeWidth    = 70;
    const int PipeGap      = 160;
    const int PipeSpeed    = 4;
    const float Gravity    = 0.5f;
    const float JumpForce  = -9f;
    const int PipeInterval = 1600;   // ms between pipes

    // ── Game state ──────────────────────────────────────────────────
    enum State { Waiting, Playing, Dead }
    State _state = State.Waiting;

    float _birdY;
    float _birdVelocity;
    float _birdAngle;

    int   _score;
    int   _bestScore;

    readonly List<(int x, int topH)> _pipes = new();
    int _pipeTimer;

    // Wing flap animation
    int _flapFrame;
    int _flapTick;

    // Death animation
    bool  _deathBouncing;

    // Ground scroll
    int _groundOffset;

    // ── Timer ───────────────────────────────────────────────────────
    readonly System.Windows.Forms.Timer _timer = new() { Interval = 16 }; // ~60 fps

    // ── Colors / brushes ────────────────────────────────────────────
    static readonly Color SkyTop    = Color.FromArgb(0x4E, 0xC0, 0xFB);
    static readonly Color SkyBot    = Color.FromArgb(0xAD, 0xE8, 0xFD);
    static readonly Color GroundTop = Color.FromArgb(0xDE, 0xD8, 0x95);
    static readonly Color GroundBot = Color.FromArgb(0xC4, 0xB8, 0x60);

    // ── Constructor ─────────────────────────────────────────────────
    public Form1()
    {
        InitializeComponent();

        // Window setup
        Text            = "Flappy Bird";
        ClientSize      = new Size(GameWidth, GameHeight);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        DoubleBuffered  = true;
        BackColor       = Color.Black;

        // Key input – every key press causes jump
        KeyPreview = true;
        KeyDown   += OnKeyDown;

        // Mouse click causes jump
        MouseDown += OnMouseDown;

        // Wire paint & timer
        Paint       += OnPaint;
        _timer.Tick += OnTick;
        _timer.Start();

        ResetBird();
    }

    // ── Input handlers ───────────────────────────────────────────────
    void OnKeyDown(object? s, KeyEventArgs e)
    {
        // Block alt+F4 and other system combos from triggering jump
        if (e.Alt || e.Control) return;
        Jump();
    }

    void OnMouseDown(object? s, MouseEventArgs e) => Jump();

    void Jump()
    {
        switch (_state)
        {
            case State.Waiting:
                _state        = State.Playing;
                _birdVelocity = JumpForce;
                break;

            case State.Playing:
                _birdVelocity = JumpForce;
                _flapFrame    = 0;   // reset flap animation
                break;

            case State.Dead:
                // Restart only after death animation settles
                if (!_deathBouncing)
                    RestartGame();
                break;
        }
    }

    // ── Game logic ───────────────────────────────────────────────────
    void OnTick(object? s, EventArgs e)
    {
        if (_state == State.Playing)
            UpdatePlaying();
        else if (_state == State.Dead)
            UpdateDead();

        // Scroll ground always (even on waiting/dead for visual polish)
        _groundOffset = (_groundOffset + 2) % 48;

        Invalidate();
    }

    void ResetBird()
    {
        _birdY        = GameHeight / 2f - 60;
        _birdVelocity = 0;
        _birdAngle    = 0;
    }

    void RestartGame()
    {
        _pipes.Clear();
        _pipeTimer = 0;
        _score     = 0;
        _state     = State.Waiting;
        _deathBouncing = false;
        _groundOffset  = 0;
        ResetBird();
    }

    void UpdatePlaying()
    {
        // Physics
        _birdVelocity += Gravity;
        _birdY        += _birdVelocity;

        // Tilt angle: nose-down when falling, nose-up after jump
        float targetAngle = _birdVelocity < 0 ? -25f : Math.Min(_birdVelocity * 4f, 90f);
        _birdAngle += (targetAngle - _birdAngle) * 0.2f;

        // Flap animation
        _flapTick++;
        if (_flapTick >= 6) { _flapTick = 0; _flapFrame = (_flapFrame + 1) % 3; }

        // Pipe spawning
        _pipeTimer += 16;
        if (_pipeTimer >= PipeInterval)
        {
            _pipeTimer = 0;
            int topH = Random.Shared.Next(80, GameHeight - 80 - PipeGap - 80);
            _pipes.Add((GameWidth + PipeWidth, topH));
        }

        // Move & score pipes
        for (int i = _pipes.Count - 1; i >= 0; i--)
        {
            var (px, ph) = _pipes[i];
            px -= PipeSpeed;

            // Score when bird passes pipe centre
            if (px + PipeWidth < BirdX && px + PipeWidth + PipeSpeed >= BirdX)
            {
                _score++;
                if (_score > _bestScore) _bestScore = _score;
            }

            if (px + PipeWidth < 0)
                _pipes.RemoveAt(i);
            else
                _pipes[i] = (px, ph);
        }

        // Collision
        if (CheckCollision())
        {
            _state         = State.Dead;
            _deathBouncing = true;
        }
    }

    void UpdateDead()
    {
        if (!_deathBouncing) return;

        // Bird falls to ground
        _birdVelocity += Gravity * 1.5f;
        _birdY        += _birdVelocity;
        _birdAngle     = 90f;   // nose down when dead

        float groundY = GameHeight - 80 - BirdSize / 2f;
        if (_birdY >= groundY)
        {
            _birdY         = groundY;
            _deathBouncing = false;
        }
    }

    bool CheckCollision()
    {
        // Hit ceiling
        if (_birdY - BirdSize / 2f < 0) return true;

        // Hit ground
        if (_birdY + BirdSize / 2f > GameHeight - 80) return true;

        // Hit pipes
        var birdRect = new RectangleF(BirdX - BirdSize / 2f + 4,
                                       _birdY - BirdSize / 2f + 4,
                                       BirdSize - 8, BirdSize - 8);
        foreach (var (px, ph) in _pipes)
        {
            var topPipe = new RectangleF(px, 0, PipeWidth, ph);
            var botPipe = new RectangleF(px, ph + PipeGap, PipeWidth, GameHeight);
            if (birdRect.IntersectsWith(topPipe) || birdRect.IntersectsWith(botPipe))
                return true;
        }
        return false;
    }

    // ── Rendering ────────────────────────────────────────────────────
    void OnPaint(object? s, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode      = SmoothingMode.AntiAlias;
        g.InterpolationMode  = InterpolationMode.HighQualityBicubic;

        DrawBackground(g);
        DrawPipes(g);
        DrawGround(g);
        DrawBird(g);
        DrawHUD(g);

        if (_state == State.Waiting) DrawStartScreen(g);
        if (_state == State.Dead && !_deathBouncing) DrawGameOver(g);
    }

    void DrawBackground(Graphics g)
    {
        using var brush = new LinearGradientBrush(
            new Point(0, 0), new Point(0, GameHeight - 80),
            SkyTop, SkyBot);
        g.FillRectangle(brush, 0, 0, GameWidth, GameHeight - 80);

        // Clouds (static decorative)
        DrawCloud(g, 60,  80, 1.0f);
        DrawCloud(g, 220, 50, 0.7f);
        DrawCloud(g, 360, 100, 0.85f);
    }

    void DrawCloud(Graphics g, int cx, int cy, float scale)
    {
        using var b = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
        int r = (int)(30 * scale);
        g.FillEllipse(b, cx - r,     cy - r / 2, r * 2,     r);
        g.FillEllipse(b, cx - r / 2, cy - r,     (int)(r * 1.4f), (int)(r * 1.4f));
        g.FillEllipse(b, cx + r / 3, cy - r / 2, r * 2, r);
    }

    void DrawPipes(Graphics g)
    {
        foreach (var (px, ph) in _pipes)
        {
            DrawPipe(g, px, 0, PipeWidth, ph, false);
            DrawPipe(g, px, ph + PipeGap, PipeWidth, GameHeight - ph - PipeGap, true);
        }
    }

    void DrawPipe(Graphics g, int x, int y, int w, int h, bool isBottom)
    {
        // Body
        using var bodyBrush = new LinearGradientBrush(
            new Point(x, 0), new Point(x + w, 0),
            Color.FromArgb(0x56, 0xC0, 0x2B),
            Color.FromArgb(0x2D, 0x8A, 0x0F));
        bodyBrush.WrapMode = WrapMode.TileFlipX;
        g.FillRectangle(bodyBrush, x + 5, y, w - 10, h);

        // Cap
        int capH = 26, capX = x, capW = w;
        int capY = isBottom ? y : y + h - capH;
        using var capBrush = new LinearGradientBrush(
            new Point(capX, 0), new Point(capX + capW, 0),
            Color.FromArgb(0x72, 0xD9, 0x37),
            Color.FromArgb(0x36, 0xA3, 0x18));
        capBrush.WrapMode = WrapMode.TileFlipX;
        g.FillRectangle(capBrush, capX, capY, capW, capH);

        // Highlight
        using var hBrush = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
        g.FillRectangle(hBrush, x + 8, y, 8, h);
    }

    void DrawGround(Graphics g)
    {
        int groundY = GameHeight - 80;

        // Green strip
        using var gBrush = new LinearGradientBrush(
            new Point(0, groundY), new Point(0, groundY + 20),
            Color.FromArgb(0x72, 0xC0, 0x2B),
            Color.FromArgb(0x56, 0x9A, 0x1A));
        g.FillRectangle(gBrush, 0, groundY, GameWidth, 20);

        // Dirt
        using var dBrush = new LinearGradientBrush(
            new Point(0, groundY + 20), new Point(0, GameHeight),
            GroundTop, GroundBot);
        g.FillRectangle(dBrush, 0, groundY + 20, GameWidth, 60);

        // Scrolling dashes
        using var linePen = new Pen(Color.FromArgb(80, 0, 0, 0), 2);
        for (int x = -_groundOffset; x < GameWidth; x += 48)
            g.DrawLine(linePen, x, groundY + 10, x + 24, groundY + 10);
    }

    void DrawBird(Graphics g)
    {
        float cx = BirdX, cy = _birdY;

        g.TranslateTransform(cx, cy);
        g.RotateTransform(_birdAngle);

        // Shadow
        using var shadow = new SolidBrush(Color.FromArgb(40, 0, 0, 0));
        g.FillEllipse(shadow, -BirdSize / 2 + 4, -BirdSize / 2 + 4, BirdSize, BirdSize);

        // Body
        using var bodyBrush = new LinearGradientBrush(
            new PointF(-BirdSize / 2f, -BirdSize / 2f),
            new PointF(BirdSize / 2f, BirdSize / 2f),
            Color.FromArgb(0xFF, 0xDD, 0x00),
            Color.FromArgb(0xFF, 0xAA, 0x00));
        g.FillEllipse(bodyBrush, -BirdSize / 2, -BirdSize / 2, BirdSize, BirdSize);

        // Wing (3-frame flap: up / mid / down)
        int[] wingYOffsets = { -10, -4, 4 };
        int wingY = wingYOffsets[_flapFrame];
        using var wingBrush = new SolidBrush(Color.FromArgb(0xFF, 0xBB, 0x00));
        g.FillEllipse(wingBrush, -4, wingY - 6, 18, 12);

        // White eye patch
        using var whiteBrush = new SolidBrush(Color.White);
        g.FillEllipse(whiteBrush, 4, -16, 16, 14);

        // Pupil
        using var pupilBrush = new SolidBrush(Color.Black);
        g.FillEllipse(pupilBrush, 10, -13, 7, 7);

        // Cheek blush
        using var blushBrush = new SolidBrush(Color.FromArgb(100, 255, 100, 80));
        g.FillEllipse(blushBrush, 6, -4, 10, 7);

        // Beak
        using var beakBrush = new SolidBrush(Color.FromArgb(0xFF, 0x77, 0x00));
        var beakPts = new PointF[] { new(14, -5), new(26, -1), new(14, 4) };
        g.FillPolygon(beakBrush, beakPts);

        g.ResetTransform();
    }

    void DrawHUD(Graphics g)
    {
        if (_state == State.Waiting) return;

        // Score
        string scoreText = _score.ToString();
        using var font = new Font("Arial", 42, FontStyle.Bold, GraphicsUnit.Pixel);

        // Shadow
        using var shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
        g.DrawString(scoreText, font, shadowBrush, GameWidth / 2f - 2 + 2, 60 + 2,
            new StringFormat { Alignment = StringAlignment.Center });

        // White text
        using var whiteBrush = new SolidBrush(Color.White);
        g.DrawString(scoreText, font, whiteBrush, GameWidth / 2f - 2, 60,
            new StringFormat { Alignment = StringAlignment.Center });
    }

    void DrawStartScreen(Graphics g)
    {
        // Semi overlay
        using var overlay = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
        g.FillRectangle(overlay, 0, 0, GameWidth, GameHeight);

        // Title
        using var titleFont = new Font("Arial", 54, FontStyle.Bold, GraphicsUnit.Pixel);
        DrawShadowString(g, "Flappy Bird", titleFont, Color.FromArgb(0xFF, 0xDD, 0x00),
                         GameWidth / 2f, 200, StringAlignment.Center);

        // Subtitle
        using var subFont = new Font("Arial", 22, FontStyle.Regular, GraphicsUnit.Pixel);
        DrawShadowString(g, "Press any key or click to start", subFont, Color.White,
                         GameWidth / 2f, 290, StringAlignment.Center);

        // Instruction icons
        using var smallFont = new Font("Arial", 17, FontStyle.Regular, GraphicsUnit.Pixel);
        DrawShadowString(g, "⌨  Any key  |  🖱  Click", smallFont,
                         Color.FromArgb(0xCC, 0xFF, 0xCC), GameWidth / 2f, 340, StringAlignment.Center);

        // Best score
        if (_bestScore > 0)
        {
            DrawShadowString(g, $"Best: {_bestScore}", subFont, Color.FromArgb(0xFF, 0xAA, 0x00),
                             GameWidth / 2f, 390, StringAlignment.Center);
        }
    }

    void DrawGameOver(Graphics g)
    {
        // Semi overlay
        using var overlay = new SolidBrush(Color.FromArgb(140, 0, 0, 0));
        g.FillRoundedRectangle(overlay, GameWidth / 2 - 160, GameHeight / 2 - 130, 320, 260, 20);

        using var panelBrush = new SolidBrush(Color.FromArgb(220, 255, 255, 255));
        g.FillRoundedRectangle(panelBrush, GameWidth / 2 - 158, GameHeight / 2 - 128, 316, 256, 18);

        using var titleFont = new Font("Arial", 36, FontStyle.Bold, GraphicsUnit.Pixel);
        DrawShadowString(g, "Game Over", titleFont, Color.FromArgb(0xCC, 0x22, 0x22),
                         GameWidth / 2f, GameHeight / 2f - 110, StringAlignment.Center);

        using var scoreFont = new Font("Arial", 26, FontStyle.Bold, GraphicsUnit.Pixel);
        DrawShadowString(g, $"Score: {_score}", scoreFont, Color.Black,
                         GameWidth / 2f, GameHeight / 2f - 55, StringAlignment.Center);

        DrawShadowString(g, $"Best:  {_bestScore}", scoreFont, Color.FromArgb(0xFF, 0x88, 0x00),
                         GameWidth / 2f, GameHeight / 2f - 15, StringAlignment.Center);

        // Medal
        string medal = _score >= 30 ? "🥇" : _score >= 20 ? "🥈" : _score >= 10 ? "🥉" : "🕹";
        using var medalFont = new Font("Segoe UI Emoji", 44, FontStyle.Regular, GraphicsUnit.Pixel);
        DrawShadowString(g, medal, medalFont, Color.White,
                         GameWidth / 2f, GameHeight / 2f + 25, StringAlignment.Center);

        using var subFont = new Font("Arial", 18, FontStyle.Regular, GraphicsUnit.Pixel);
        DrawShadowString(g, "Any key / click to retry", subFont, Color.FromArgb(0x44, 0x44, 0x44),
                         GameWidth / 2f, GameHeight / 2f + 95, StringAlignment.Center);
    }

    void DrawShadowString(Graphics g, string text, Font font, Color color,
                           float x, float y, StringAlignment align)
    {
        var sf = new StringFormat { Alignment = align, LineAlignment = StringAlignment.Near };
        using var shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
        g.DrawString(text, font, shadowBrush, x + 2, y + 2, sf);
        using var brush = new SolidBrush(color);
        g.DrawString(text, font, brush, x, y, sf);
    }
}

// Extension for rounded rectangles
static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics g, Brush brush,
        int x, int y, int width, int height, int radius)
    {
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
        path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
        path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        g.FillPath(brush, path);
    }
}
