using Win32;

namespace ConsoleGame
{
    public class Camera
    {
        public const float fNear = 0.00001f;
        public const float fFar = 1000.0f;
        public const float fFov = 90.0f;
        public static readonly float fFovRad = 1.0f / MathF.Tan(fFov * 0.5f / 180f * MathF.PI);
        public const float CameraSpeed = 8f;
        const float MouseIntensity = 0.001f;

        public Vector3 CameraPosition;
        public Vector3 CameraLookDirection = new(0f, 0f, 1f);
        public float CameraYaw;
        public float CameraBruh;

        public void HandleInput(bool lockMouse, ref VectorInt mousePosition)
        {
            VectorInt mouseDelta = Win32.Utilities.Mouse.Position - mousePosition;
            mousePosition = Win32.Utilities.Mouse.Position;

            if (Keyboard.IsKeyPressed(VirtualKeyCodes.LEFT))
            { this.CameraPosition.X += Time.DeltaTime * CameraSpeed; }
            if (Keyboard.IsKeyPressed(VirtualKeyCodes.RIGHT))
            { this.CameraPosition.X -= Time.DeltaTime * CameraSpeed; }

            if (Keyboard.IsKeyPressed(VirtualKeyCodes.UP))
            { this.CameraPosition.Y += Time.DeltaTime * CameraSpeed; }
            if (Keyboard.IsKeyPressed(VirtualKeyCodes.DOWN))
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

                Win32.Utilities.Mouse.Position = new Point(Game.width / 2, Game.height / 2);

                VectorInt center = new(Game.width / 2, Game.height / 2);

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
    }
}