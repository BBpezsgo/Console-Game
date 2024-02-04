using System.Numerics;

namespace ConsoleGame
{
    public static class Matrix
    {
        public static void MakeProjection(ref Matrix4x4 matrix, float aspectRatio, float fovRad, float far, float near)
        {
            matrix[0, 0] = aspectRatio * fovRad;
            matrix[1, 1] = fovRad;
            matrix[2, 2] = far / (far - near);
            matrix[3, 2] = (-far * near) / (far - near);
            matrix[2, 3] = 1f;
            matrix[3, 3] = 0f;
        }

        public static void MakeProjection(ref Matrix4x4 matrix, Vector3 v)
        {
            matrix[0, 0] = 1f;
            matrix[1, 1] = 1f;
            matrix[2, 2] = 1f;
            matrix[3, 3] = 1f;
            matrix[3, 0] = v.X;
            matrix[3, 1] = v.Y;
            matrix[3, 2] = v.Z;
        }

        public static void MakeTransition(ref Matrix4x4 matrix, float x, float y, float z)
        {
            matrix[0, 0] = 1f;
            matrix[1, 1] = 1f;
            matrix[2, 2] = 1f;
            matrix[3, 3] = 1f;
            matrix[3, 0] = x;
            matrix[3, 1] = y;
            matrix[3, 2] = z;
        }

        public static void MakePointAt(ref Matrix4x4 matrix, Vector3 position, Vector3 target, Vector3 up)
        {
            Vector3 newForward = Vector3.Normalize(target - position);

            Vector3 a = newForward * Vector3.Dot(up, newForward);
            Vector3 newUp = Vector3.Normalize(up - a);

            Vector3 newRight = Vector3.Cross(newUp, newForward);

            matrix[0, 0] = newRight.X;
            matrix[0, 1] = newRight.Y;
            matrix[0, 2] = newRight.Z;
            matrix[0, 3] = 0f;

            matrix[1, 0] = newUp.X;
            matrix[1, 1] = newUp.Y;
            matrix[1, 2] = newUp.Z;
            matrix[1, 3] = 0f;

            matrix[2, 0] = newForward.X;
            matrix[2, 1] = newForward.Y;
            matrix[2, 2] = newForward.Z;
            matrix[2, 3] = 0f;

            matrix[3, 0] = position.X;
            matrix[3, 1] = position.Y;
            matrix[3, 2] = position.Z;
            matrix[3, 3] = 1f;
        }

        /// <summary>
        /// <b>Only for Rotation/Translation matrices!</b>
        /// </summary>
        public static void QuickInverse(ref Matrix4x4 result, Matrix4x4 m)
        {
            result = default;
            result[0, 0] = m[0, 0]; result[0, 1] = m[1, 0]; result[0, 2] = m[2, 0]; result[0, 3] = 0.0f;
            result[1, 0] = m[0, 1]; result[1, 1] = m[1, 1]; result[1, 2] = m[2, 1]; result[1, 3] = 0.0f;
            result[2, 0] = m[0, 2]; result[2, 1] = m[1, 2]; result[2, 2] = m[2, 2]; result[2, 3] = 0.0f;
            result[3, 0] = -(m[3, 0] * result[0, 0] + m[3, 1] * result[1, 0] + m[3, 2] * result[2, 0]);
            result[3, 1] = -(m[3, 0] * result[0, 1] + m[3, 1] * result[1, 1] + m[3, 2] * result[2, 1]);
            result[3, 2] = -(m[3, 0] * result[0, 2] + m[3, 1] * result[1, 2] + m[3, 2] * result[2, 2]);
            result[3, 3] = 1.0f;
        }

        public static Vector4 Multiply(Vector4 v, in Matrix4x4 m)
        {
            Vector4 result = default;

            result.X = v.X * m[0, 0] + v.Y * m[1, 0] + v.Z * m[2, 0] + v.W * m[3, 0];
            result.Y = v.X * m[0, 1] + v.Y * m[1, 1] + v.Z * m[2, 1] + v.W * m[3, 1];
            result.Z = v.X * m[0, 2] + v.Y * m[1, 2] + v.Z * m[2, 2] + v.W * m[3, 2];
            result.W = v.X * m[0, 3] + v.Y * m[1, 3] + v.Z * m[2, 3] + v.W * m[3, 3];

            return result;
        }
    }
}
