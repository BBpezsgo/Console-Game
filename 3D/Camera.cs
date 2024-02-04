using System.Numerics;
using System.Runtime.Versioning;
using Win32;
using Win32.Common;
using Win32.LowLevel;

namespace ConsoleGame
{
    public class Camera
    {
        const float Near = 0.000001f;
        const float Far = 1000.0f;
        const float Fov = 90.0f;
        static readonly float FovRad = 1.0f / MathF.Tan(Fov * 0.5f / 180f * MathF.PI);
        const float Speed = 8f;
        const float MouseSensitivity = 0.001f;

        public Vector3 Position;
        public float Yaw;
        public float Bruh;
        public Vector3 LookDirection => _lookDirection;

        public Matrix4x4 ProjectionMatrix => _projectionMatrix;
        public Matrix4x4 ViewMatrix => _viewMatrix;

        Vector3 _lookDirection = new(0f, 0f, 1f);

        Matrix4x4 _projectionMatrix;
        Matrix4x4 _viewMatrix;

        Matrix4x4 _cameraMatrix;
        Matrix4x4 _rotX;
        Matrix4x4 _rotY;
        Matrix4x4 _RotationMatrix;

        [SupportedOSPlatform("windows")]
        public void HandleInput(bool lockMouse, ref Vector2Int mousePosition)
        {
            Vector2Int mouseDelta = (Vector2Int)Mouse.ScreenPosition - mousePosition;
            mousePosition = Mouse.ScreenPosition;

            if (Keyboard.IsKeyPressed(VirtualKeyCode.LEFT))
            { Position.X += Time.DeltaTime * Speed; }
            if (Keyboard.IsKeyPressed(VirtualKeyCode.RIGHT))
            { Position.X -= Time.DeltaTime * Speed; }

            if (Keyboard.IsKeyPressed(VirtualKeyCode.UP))
            { Position.Y += Time.DeltaTime * Speed; }
            if (Keyboard.IsKeyPressed(VirtualKeyCode.DOWN))
            { Position.Y -= Time.DeltaTime * Speed; }

            if (Keyboard.IsKeyPressed('W'))
            { Position += _lookDirection * Speed * Time.DeltaTime; }

            if (Keyboard.IsKeyPressed('S'))
            { Position -= _lookDirection * Speed * Time.DeltaTime; }

            if (Keyboard.IsKeyPressed('A'))
            { Position -= Vector3.Cross(Vector3.Normalize(_lookDirection), new Vector3(0f, 1f, 0f)) * Speed * Time.DeltaTime; }

            if (Keyboard.IsKeyPressed('D'))
            { Position += Vector3.Cross(Vector3.Normalize(_lookDirection), new Vector3(0f, 1f, 0f)) * Speed * Time.DeltaTime; }

            if (lockMouse)
            {
                Yaw += mouseDelta.X * MouseSensitivity;
                Bruh += mouseDelta.Y * MouseSensitivity;

                Mouse.ScreenPosition = new Point(DisplayMetrics.Width / 2, DisplayMetrics.Height / 2);

                Vector2Int center = new(DisplayMetrics.Width / 2, DisplayMetrics.Height / 2);

                mousePosition = center;
            }
            else
            {
                if (Keyboard.IsKeyPressed('A'))
                { Yaw -= 2.0f * Time.DeltaTime; }

                if (Keyboard.IsKeyPressed('D'))
                { Yaw += 2.0f * Time.DeltaTime; }
            }
        }

        public void DoMath(SmallSize screenSize)
        {
            Matrix.MakeProjection(ref _projectionMatrix, (float)screenSize.Height / (float)screenSize.Width, FovRad, Far, Near);

            Vector3 target = new(0f, 0f, 1f);

            _rotY = Matrix4x4.CreateRotationY(-Yaw);
            _rotX = Matrix4x4.CreateRotationX(Bruh);

            _RotationMatrix = Matrix4x4.Multiply(_rotX, _rotY);

            _lookDirection = Matrix.Multiply(target.To4(), in _RotationMatrix).To3();
            target = Position + _lookDirection;

            Matrix.MakePointAt(ref _cameraMatrix, Position, target, Vector3.UnitY);

            Matrix.QuickInverse(ref _viewMatrix, _cameraMatrix);
        }
    }
}
