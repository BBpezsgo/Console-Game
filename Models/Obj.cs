using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace ConsoleGame
{
    public class Obj
    {
        /// <exception cref="Exception"/>
        public static Mesh LoadFile(string file)
        {
            if (!File.Exists(file)) throw new Exception($"Object file \"{file}\" not found");
            string text = File.ReadAllText(file, Encoding.ASCII);
            string[] lines = text.Replace('\r', '\n').Split('\n');

            List<Vector3> vertices = new();
            List<Triangle> triangles = new();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0) continue;
                if (line.StartsWith('#')) continue;
                char kind = line[0];
                line = line[1..].Trim();

                switch (kind)
                {
                    case 'v':
                        {
                            string[] parts = line.Split(' ');
                            if (parts.Length != 3)
                            { throw new Exception($"Expected 3 values as a vertex (at line {i})"); }
                            Vector3 vertex = default;
                            if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vertex.X))
                            { throw new Exception($"Expected a number as a vertex X component (at line {i})"); }
                            if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vertex.Y))
                            { throw new Exception($"Expected a number as a vertex Y component (at line {i})"); }
                            if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out vertex.Z))
                            { throw new Exception($"Expected a number as a vertex Z component (at line {i})"); }
                            vertices.Add(vertex);
                            break;
                        }
                    case 'f':
                        {
                            string[] parts = line.Split(' ');
                            if (parts.Length != 3)
                            { throw new Exception($"Expected 3 values as a face (at line {i})"); }
                            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int a))
                            { throw new Exception($"Expected a number as a face vertex index (at line {i})"); }
                            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int b))
                            { throw new Exception($"Expected a number as a face vertex index (at line {i})"); }
                            if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int c))
                            { throw new Exception($"Expected a number as a face vertex index (at line {i})"); }

                            Vector3 va, vb, vc;

                            if (a <= 0 || a > vertices.Count)
                            { throw new Exception($"Vertex index A out of range (at line {i})"); }

                            if (b <= 0 || b > vertices.Count)
                            { throw new Exception($"Vertex index B out of range (at line {i})"); }

                            if (c <= 0 || c > vertices.Count)
                            { throw new Exception($"Vertex index C out of range (at line {i})"); }

                            va = vertices[a - 1];
                            vb = vertices[b - 1];
                            vc = vertices[c - 1];

                            triangles.Add(new Triangle(va, vb, vc));

                            break;
                        }
                    default:
                        Debug.WriteLine($"Unsupported obj line kind \'{kind}\' (at line {i})");
                        break;
                }
            }

            return new Mesh()
            {
                Triangles = triangles,
            };
        }
    }
}
