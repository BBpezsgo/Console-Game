using Win32;
using Win32.Common;
using Win32.LowLevel;

namespace ConsoleGame
{
    public class Camera
    {
        public const float fNear = 0.000001f;
        public const float fFar = 1000.0f;
        public const float fFov = 90.0f;
        public static readonly float fFovRad = 1.0f / MathF.Tan(fFov * 0.5f / 180f * MathF.PI);
        public const float CameraSpeed = 8f;
        const float MouseIntensity = 0.001f;

        public Vector3 CameraPosition;
        public Vector3 CameraLookDirection = new(0f, 0f, 1f);
        public float CameraYaw;
        public float CameraBruh;

        public Matrix4x4 ProjectionMatrix;
        public Matrix4x4 ViewMatrix;

        Matrix4x4 cameraMatrix;
        Matrix4x4 rotX;
        Matrix4x4 rotY;
        Matrix4x4 cameraRotationMatrix;

        static readonly Vector3 up = new(0f, 1f, 0f);

        public Camera()
        {
            ProjectionMatrix = Matrix4x4.Zero;
            ViewMatrix = Matrix4x4.Zero;
            cameraMatrix = Matrix4x4.Zero;
            rotX = Matrix4x4.Zero;
            rotY = Matrix4x4.Zero;
            cameraRotationMatrix = Matrix4x4.Zero;
        }

        public void HandleInput(bool lockMouse, ref Vector2Int mousePosition)
        {
            Vector2Int mouseDelta = (Vector2Int)Mouse.ScreenPosition - mousePosition;
            mousePosition = Mouse.ScreenPosition;

            if (Keyboard.IsKeyPressed(VirtualKeyCode.LEFT))
            { this.CameraPosition.X += Time.DeltaTime * CameraSpeed; }
            if (Keyboard.IsKeyPressed(VirtualKeyCode.RIGHT))
            { this.CameraPosition.X -= Time.DeltaTime * CameraSpeed; }

            if (Keyboard.IsKeyPressed(VirtualKeyCode.UP))
            { this.CameraPosition.Y += Time.DeltaTime * CameraSpeed; }
            if (Keyboard.IsKeyPressed(VirtualKeyCode.DOWN))
            { this.CameraPosition.Y -= Time.DeltaTime * CameraSpeed; }

            if (Keyboard.IsKeyPressed('W'))
            { this.CameraPosition += this.CameraLookDirection * CameraSpeed * Time.DeltaTime; }

            if (Keyboard.IsKeyPressed('S'))
            { this.CameraPosition -= this.CameraLookDirection * CameraSpeed * Time.DeltaTime; }

            if (Keyboard.IsKeyPressed('A'))
            { this.CameraPosition -= Vector3.Cross(this.CameraLookDirection.Normalized, new Vector3(0f, 1f, 0f)) * CameraSpeed * Time.DeltaTime; }

            if (Keyboard.IsKeyPressed('D'))
            { this.CameraPosition += Vector3.Cross(this.CameraLookDirection.Normalized, new Vector3(0f, 1f, 0f)) * CameraSpeed * Time.DeltaTime; }

            if (lockMouse)
            {
                this.CameraYaw += mouseDelta.X * MouseIntensity;
                this.CameraBruh += mouseDelta.Y * MouseIntensity;

                Mouse.ScreenPosition = new Win32.Common.Point(DisplayMetrics.Width / 2, DisplayMetrics.Height / 2);

                Vector2Int center = new(DisplayMetrics.Width / 2, DisplayMetrics.Height / 2);

                mousePosition = center;
            }
            else
            {
                if (Keyboard.IsKeyPressed('A'))
                { this.CameraYaw -= 2.0f * Time.DeltaTime; }

                if (Keyboard.IsKeyPressed('D'))
                { this.CameraYaw += 2.0f * Time.DeltaTime; }
            }
        }

        public void DoMath(SmallSize screenSize, out Matrix4x4 projectionMatrix, out Matrix4x4 viewMatrix)
        {
            Matrix4x4.MakeProjection(ref this.ProjectionMatrix, (float)screenSize.Height / (float)screenSize.Width, Camera.fFovRad, Camera.fFar, Camera.fNear);

            Vector3 target = new(0f, 0f, 1f);

            Matrix4x4.MakeRotationY(ref this.rotY, this.CameraYaw);
            Matrix4x4.MakeRotationX(ref this.rotX, this.CameraBruh);

            Matrix4x4.Multiply(ref this.cameraRotationMatrix, this.rotX, this.rotY);

            this.CameraLookDirection = target * cameraRotationMatrix;
            target = this.CameraPosition + this.CameraLookDirection;

            Matrix4x4.MakePointAt(ref this.cameraMatrix, this.CameraPosition, target, up);

            Matrix4x4.QuickInverse(ref this.ViewMatrix, cameraMatrix);

            projectionMatrix = this.ProjectionMatrix;
            viewMatrix = this.ViewMatrix;
        }
    }
}
