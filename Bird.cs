using System;
using System.Drawing;

namespace FlappyBird
{
    public class Bird
    {
        public float X { get; private set; }
        public float Y { get; set; }
        public float Velocity { get; set; }

        public const int Width = 36;
        public const int Height = 26;

        private const float Gravity = 0.5f;
        private const float JumpForce = -9f;
        private const float MaxFallSpeed = 12f;

        private float _angle;    // visual tilt angle
        private float _flapTimer;

        public RectangleF Bounds => new RectangleF(X + 4, Y + 4, Width - 8, Height - 8);

        public Bird(float x, float y)
        {
            X = x;
            Y = y;
        }

        public void Flap()
        {
            Velocity = JumpForce;
            _flapTimer = 0.3f;
        }

        public void Update()
        {
            Velocity = Math.Min(Velocity + Gravity, MaxFallSpeed);
            Y += Velocity;

            // tilt based on velocity
            float targetAngle = Velocity < 0 ? -25f : Math.Min(Velocity * 5f, 90f);
            _angle += (targetAngle - _angle) * 0.2f;

            if (_flapTimer > 0) _flapTimer -= 0.05f;
        }

        public void Draw(Graphics g)
        {
            g.TranslateTransform(X + Width / 2f, Y + Height / 2f);
            g.RotateTransform(_angle);

            // Body
            using var bodyBrush = new SolidBrush(Color.FromArgb(255, 230, 50));
            g.FillEllipse(bodyBrush, -Width / 2, -Height / 2, Width, Height);

            // Wing (flaps when recently jumped)
            float wingY = _flapTimer > 0 ? -6 : 2;
            using var wingBrush = new SolidBrush(Color.FromArgb(255, 190, 20));
            g.FillEllipse(wingBrush, -4, (int)wingY, 14, 8);

            // Eye
            using var eyeWhite = new SolidBrush(Color.White);
            g.FillEllipse(eyeWhite, 6, -8, 12, 12);
            using var eyePupil = new SolidBrush(Color.Black);
            g.FillEllipse(eyePupil, 10, -5, 6, 6);

            // Beak
            using var beakBrush = new SolidBrush(Color.FromArgb(255, 140, 0));
            Point[] beak = { new Point(14, -2), new Point(22, 1), new Point(14, 5) };
            g.FillPolygon(beakBrush, beak);

            // Cheek blush
            using var blushBrush = new SolidBrush(Color.FromArgb(100, 255, 100, 100));
            g.FillEllipse(blushBrush, 4, 2, 8, 6);

            g.ResetTransform();
        }
    }
}
