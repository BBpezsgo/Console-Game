using System.Numerics;

namespace ConsoleGame
{
    public readonly struct Matrix4x4
    {
        readonly float[][] V;

        public Matrix4x4(float[][] v)
        {
            V = v;
        }

        public static Matrix4x4 Zero => new(new float[][] {
            new float[] { 0f, 0f, 0f, 0f, },
            new float[] { 0f, 0f, 0f, 0f, },
            new float[] { 0f, 0f, 0f, 0f, },
            new float[] { 0f, 0f, 0f, 0f, },
        });

        public static Matrix4x4 Identity => new(new float[][] {
            new float[] { 1f, 1f, 1f, 1f, },
            new float[] { 1f, 1f, 1f, 1f, },
            new float[] { 1f, 1f, 1f, 1f, },
            new float[] { 1f, 1f, 1f, 1f, },
        });

        public float this[int x, int y]
        {
            get => V[x][y];
            readonly set => V[x][y] = value;
        }

        public static Vector4 operator *(Vector4 v, Matrix4x4 m)
        {
            Vector4 result = default;

            result.X = v.X * m[0, 0] + v.Y * m[1, 0] + v.Z * m[2, 0] + v.W * m[3, 0];
            result.Y = v.X * m[0, 1] + v.Y * m[1, 1] + v.Z * m[2, 1] + v.W * m[3, 1];
            result.Z = v.X * m[0, 2] + v.Y * m[1, 2] + v.Z * m[2, 2] + v.W * m[3, 2];
            result.W = v.X * m[0, 3] + v.Y * m[1, 3] + v.Z * m[2, 3] + v.W * m[3, 3];

            return result;
        }

        public void Clear()
        {
            Array.Clear(V[0]);
            Array.Clear(V[1]);
            Array.Clear(V[2]);
            Array.Clear(V[3]);
        }

        public static void MakeRotationX(ref Matrix4x4 matrix, float theta)
        {
            matrix[0, 0] = 1;
            matrix[1, 1] = MathF.Cos(theta);
            matrix[1, 2] = MathF.Sin(theta);
            matrix[2, 1] = -MathF.Sin(theta);
            matrix[2, 2] = MathF.Cos(theta);
            matrix[3, 3] = 1;
        }
        public static Matrix4x4 MakeRotationX(float theta)
        {
            Matrix4x4 result = Matrix4x4.Zero;
            MakeRotationX(ref result, theta);
            return result;
        }

        public static void MakeRotationY(ref Matrix4x4 matrix, float theta)
        {
            matrix[0, 0] = MathF.Cos(theta);
            matrix[0, 2] = MathF.Sin(theta);
            matrix[2, 0] = -MathF.Sin(theta);
            matrix[1, 1] = 1.0f;
            matrix[2, 2] = MathF.Cos(theta);
            matrix[3, 3] = 1.0f;
        }
        public static Matrix4x4 MakeRotationY(float theta)
        {
            Matrix4x4 result = Matrix4x4.Zero;
            MakeRotationY(ref result, theta);
            return result;
        }

        public static void MakeRotationZ(ref Matrix4x4 matrix, float theta)
        {
            matrix[0, 0] = MathF.Cos(theta);
            matrix[0, 1] = MathF.Sin(theta);
            matrix[1, 0] = -MathF.Sin(theta);
            matrix[1, 1] = MathF.Cos(theta);
            matrix[2, 2] = 1;
            matrix[3, 3] = 1;
        }
        public static Matrix4x4 MakeRotationZ(float theta)
        {
            Matrix4x4 result = Matrix4x4.Zero;
            MakeRotationZ(ref result, theta);
            return result;
        }

        public static void MakeProjection(ref Matrix4x4 matrix, float aspectRatio, float fovRad, float far, float near)
        {
            matrix[0, 0] = aspectRatio * fovRad;
            matrix[1, 1] = fovRad;
            matrix[2, 2] = far / (far - near);
            matrix[3, 2] = (-far * near) / (far - near);
            matrix[2, 3] = 1f;
            matrix[3, 3] = 0f;
        }
        public static Matrix4x4 MakeProjection(float aspectRatio, float fovRad, float far, float near)
        {
            Matrix4x4 result = Matrix4x4.Zero;
            MakeProjection(ref result, aspectRatio, fovRad, far, near);
            return result;
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
        public static Matrix4x4 MakeProjection(Vector3 v)
        {
            Matrix4x4 result = Matrix4x4.Zero;
            MakeProjection(ref result, v);
            return result;
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
        public static Matrix4x4 MakeTransition(float x, float y, float z)
        {
            Matrix4x4 result = Matrix4x4.Zero;
            MakeTransition(ref result, x, y, z);
            return result;
        }

        public static void MakePointAt(ref Matrix4x4 matrix, Vector3 pos, Vector3 target, Vector3 up)
        {
            Vector3 newForward = Vector3.Normalize(target - pos);

            Vector3 a = newForward * Vector3.Dot(up, newForward);
            Vector3 newUp = up - a;
            newUp = Vector3.Normalize(newUp);

            Vector3 newRight = Vector3.Cross(newUp, newForward);

            matrix[0, 0] = newRight.X;
            matrix[0, 1] = newRight.Y;
            matrix[0, 2] = newRight.Z;
            matrix[0, 3] = 0.0f;
            
            matrix[1, 0] = newUp.X; 
            matrix[1, 1] = newUp.Y; 
            matrix[1, 2] = newUp.Z; 
            matrix[1, 3] = 0.0f;
            
            matrix[2, 0] = newForward.X; 
            matrix[2, 1] = newForward.Y; 
            matrix[2, 2] = newForward.Z;
            matrix[2, 3] = 0.0f;
            
            matrix[3, 0] = pos.X;
            matrix[3, 1] = pos.Y;
            matrix[3, 2] = pos.Z;
            matrix[3, 3] = 1.0f;
        }
        public static Matrix4x4 MakePointAt(Vector3 pos, Vector3 target, Vector3 up)
        {
            Matrix4x4 result = Matrix4x4.Zero;
            MakePointAt(ref result, pos, target, up);
            return result;
        }

        /// <summary>
        /// <b>Only for Rotation/Translation matrices!</b>
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static void QuickInverse(ref Matrix4x4 result, Matrix4x4 m)
        {
            result.Clear();
            result[0, 0] = m[0, 0]; result[0, 1] = m[1, 0]; result[0, 2] = m[2, 0]; result[0, 3] = 0.0f;
            result[1, 0] = m[0, 1]; result[1, 1] = m[1, 1]; result[1, 2] = m[2, 1]; result[1, 3] = 0.0f;
            result[2, 0] = m[0, 2]; result[2, 1] = m[1, 2]; result[2, 2] = m[2, 2]; result[2, 3] = 0.0f;
            result[3, 0] = -(m[3, 0] * result[0, 0] + m[3, 1] * result[1, 0] + m[3, 2] * result[2, 0]);
            result[3, 1] = -(m[3, 0] * result[0, 1] + m[3, 1] * result[1, 1] + m[3, 2] * result[2, 1]);
            result[3, 2] = -(m[3, 0] * result[0, 2] + m[3, 1] * result[1, 2] + m[3, 2] * result[2, 2]);
            result[3, 3] = 1.0f;
        }

        /// <summary>
        /// <b>Only for Rotation/Translation matrices!</b>
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Matrix4x4 QuickInverse(Matrix4x4 m)
        {
            Matrix4x4 matrix = Matrix4x4.Zero;
            Matrix4x4.QuickInverse(ref matrix, m);
            return matrix;
        }

        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            Matrix4x4 matrix = Matrix4x4.Zero;
            Matrix4x4.Multiply(ref matrix, a, b);
            return matrix;
        }

        public static void Multiply(ref Matrix4x4 result, Matrix4x4 a, Matrix4x4 b)
        {
            for (int c = 0; c < 4; c++)
            {
                for (int r = 0; r < 4; r++)
                {
                    result[r, c] = a[r, 0] * b[0, c] + a[r, 1] * b[1, c] + a[r, 2] * b[2, c] + a[r, 3] * b[3, c];
                }
            }
        }
    }
}
