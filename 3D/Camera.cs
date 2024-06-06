using System.Runtime.Versioning;

namespace ConsoleGame;

public class Camera
{
    const float Near = 0.000001f;
    const float Far = 1000.0f;
    const float Fov = 45.0f;
    static readonly float FovRad = 1.0f / MathF.Tan(Fov * 0.5f / 180f * MathF.PI);
    const float Speed = 8f;
    const float MouseSensitivity = 0.001f;

    public Vector3 Position;
    public float Yaw;
    public float Bruh;
    public Vector3 LookDirection => _lookDirection;

    public ref readonly Maths.Matrix4x4 ProjectionMatrix => ref _projectionMatrix;
    public ref readonly Maths.Matrix4x4 ViewMatrix => ref _viewMatrix;

    Vector3 _lookDirection = new(0f, 0f, 1f);

    Maths.Matrix4x4 _projectionMatrix;
    Maths.Matrix4x4 _viewMatrix;

    Maths.Matrix4x4 _cameraMatrix;
    Maths.Matrix4x4 _rotX;
    Maths.Matrix4x4 _rotY;
    Maths.Matrix4x4 _RotationMatrix;

    [SupportedOSPlatform("windows")]
    public void HandleInput(bool lockMouse, ref Vector2Int mousePosition)
    {
        Vector2Int mouseDelta = (Vector2Int)Mouse.ScreenPosition - mousePosition;
        mousePosition = Mouse.ScreenPosition;

        if (ConsoleKeyboard.IsKeyPressed(VirtualKeyCode.Left))
        { Position.X += Time.DeltaTime * Speed; }
        if (ConsoleKeyboard.IsKeyPressed(VirtualKeyCode.Right))
        { Position.X -= Time.DeltaTime * Speed; }

        if (ConsoleKeyboard.IsKeyPressed(VirtualKeyCode.Up))
        { Position.Y += Time.DeltaTime * Speed; }
        if (ConsoleKeyboard.IsKeyPressed(VirtualKeyCode.Down))
        { Position.Y -= Time.DeltaTime * Speed; }

        if (ConsoleKeyboard.IsKeyPressed('W'))
        { Position += _lookDirection * Speed * Time.DeltaTime; }

        if (ConsoleKeyboard.IsKeyPressed('S'))
        { Position -= _lookDirection * Speed * Time.DeltaTime; }

        if (ConsoleKeyboard.IsKeyPressed('A'))
        { Position -= Vector3.Cross(Vector3.Normalize(_lookDirection), new Vector3(0f, 1f, 0f)) * Speed * Time.DeltaTime; }

        if (ConsoleKeyboard.IsKeyPressed('D'))
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
            if (ConsoleKeyboard.IsKeyPressed('A'))
            { Yaw -= 2.0f * Time.DeltaTime; }

            if (ConsoleKeyboard.IsKeyPressed('D'))
            { Yaw += 2.0f * Time.DeltaTime; }
        }
    }

    public void DoMath(Size screenSize)
    {
        Matrix.MakeProjection(ref _projectionMatrix, (float)screenSize.Height / (float)screenSize.Width, FovRad, Far, Near);

        Vector3 target = new(0f, 0f, 1f);

        _rotY = Maths.Matrix4x4.CreateRotationY(-Yaw);
        _rotX = Maths.Matrix4x4.CreateRotationX(Bruh);

        _RotationMatrix = _rotX * _rotY;

        _lookDirection = Matrix.Multiply(target.To4(), in _RotationMatrix).To3();
        target = Position + _lookDirection;

        Matrix.MakePointAt(ref _cameraMatrix, Position, target, Vector3.UnitY);

        Matrix.QuickInverse(ref _viewMatrix, _cameraMatrix);
    }
}
