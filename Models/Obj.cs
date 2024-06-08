using System.Globalization;
using System.Text;

namespace ConsoleGame;

public static class Obj
{
    /// <exception cref="FileNotFoundException"/>
    /// <exception cref="ParsingException"/>
    public static Mesh LoadFile(string file)
    {
        if (!File.Exists(file)) throw new FileNotFoundException($"Object file \"{file}\" not found");
        string text = File.ReadAllText(file, Encoding.ASCII);
        string[] lines = text.Replace('\r', '\n').Split('\n');

        List<Vector3> vertices = new();
        List<Vector2> texs = new();

        List<Triangle3Ex> triangles = new();

        Dictionary<string, Material> materials = new();

        // bool smoothShading = false; // Not supported :(

        static bool TryParseIndexes(string text, out int[] indexes)
        {
            text = text.Trim();
            string[] parts = text.Split('/');
            indexes = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                if (part.Length == 0)
                { indexes[i] = -1; }
                else if (!int.TryParse(part, out indexes[i]))
                { return false; }
            }
            return true;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith('#')) continue;
            if (!line.Contains(' ')) continue;
            string kind = line.Split(' ')[0];
            line = line[(kind.Length + 1)..].Trim();

            switch (kind)
            {
                case "v":
                    {
                        if (!Vector.TryParse(line, out Vector3 vertex))
                        { throw new ParsingException($"Failed to parse {nameof(Vector3)} (at line {i})"); }

                        vertices.Add(vertex);
                        break;
                    }
                case "vt":
                    {
                        if (!Vector.TryParse(line, out Vector2 tex))
                        { throw new ParsingException($"Failed to parse {nameof(Vector2)} (at line {i})"); }

                        texs.Add(tex);
                        break;
                    }
                case "f":
                case "vn":
                case "usemtl": break;
                case "mtllib":
                    {
                        string? dir = Path.GetDirectoryName(file);
                        if (dir == null)
                        {
                            Debug.WriteLine("Bruh");
                            break;
                        }
                        string mtlFile = Path.Combine(dir, line);
                        foreach (KeyValuePair<string, Material> newMaterial in Mtl.LoadFile(mtlFile))
                        {
                            materials[newMaterial.Key] = newMaterial.Value;
                        }
                        break;
                    }
                case "s":
                    {
                        // switch (line)
                        // {
                        //     case "1": smoothShading = true; break;
                        //     case "0": smoothShading = false; break;
                        //     case "on": smoothShading = true; break;
                        //     case "off": smoothShading = false; break;
                        //     default: break;
                        // }
                        break;
                    }
                case "l": break;
                case "o": break;
                case "g": break;
                default:
                    Debug.WriteLine($"Unsupported obj line kind \"{kind}\" (\"{line}\") (at line {i})");
                    break;
            }
        }

        int currentMaterialI = -1;
        List<Material> usedMaterials = new();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith('#')) continue;
            if (!line.Contains(' ')) continue;
            string kind = line.Split(' ')[0];
            line = line[(kind.Length + 1)..].Trim();

            if (kind == "usemtl")
            {
                if (!materials.TryGetValue(line, out Material? currentMaterial))
                { currentMaterialI = -1; }
                else
                {
                    currentMaterialI = usedMaterials.IndexOf(currentMaterial);
                    if (currentMaterialI == -1)
                    {
                        usedMaterials.Add(currentMaterial);
                        currentMaterialI = usedMaterials.Count - 1;
                    }
                }
                continue;
            }

            if (kind != "f") continue;

            string[] parts = line.Split(' ');
            if (parts.Length != 3)
            { continue; }
            // { throw new ParsingException($"Expected 3 values as a face (at line {i})"); }

            Triangle3Ex newTri;

            if (!TryParseIndexes(parts[0], out int[] partA))
            {
                _ = TryParseIndexes(parts[0], out _);
                throw new ParsingException($"Failed to parse indexes (at line {i})");
            }

            if (!TryParseIndexes(parts[1], out int[] partB))
            {
                _ = TryParseIndexes(parts[1], out _);
                throw new ParsingException($"Failed to parse indexes (at line {i})");
            }

            if (!TryParseIndexes(parts[2], out int[] partC))
            {
                _ = TryParseIndexes(parts[2], out _);
                throw new ParsingException($"Failed to parse indexes (at line {i})");
            }

            if (partA.Length != partB.Length || partA.Length != partC.Length)
            { throw new ParsingException($"Inconsistent face informations (at line {i})"); }

            if (partA.Length == 3)
            {
                Vector3 va, vb, vc;

                if (partA[0] <= 0 || partA[0] > vertices.Count)
                { throw new ParsingException($"Vertex index A out of range (at line {i})"); }

                if (partB[0] <= 0 || partB[0] > vertices.Count)
                { throw new ParsingException($"Vertex index B out of range (at line {i})"); }

                if (partC[0] <= 0 || partC[0] > vertices.Count)
                { throw new ParsingException($"Vertex index C out of range (at line {i})"); }

                if (partA[1] != -1 && (partA[1] <= 0 || partA[1] > texs.Count))
                { throw new ParsingException($"Texture coord index A out of range (at line {i})"); }

                if (partB[1] != -1 && (partC[1] <= 0 || partB[1] > texs.Count))
                { throw new ParsingException($"Texture coord index B out of range (at line {i})"); }

                if (partC[1] != -1 && (partC[1] <= 0 || partC[1] > texs.Count))
                { throw new ParsingException($"Texture coord index C out of range (at line {i})"); }

                va = vertices[partA[0] - 1];
                vb = vertices[partB[0] - 1];
                vc = vertices[partC[0] - 1];

                if (partA[1] != -1 && partB[1] != -1 && partC[1] != -1)
                {
                    newTri = new Triangle3Ex(va, vb, vc)
                    {
                        TexA = texs[partA[1] - 1],
                        TexB = texs[partB[1] - 1],
                        TexC = texs[partC[1] - 1],
                    };
                }
                else
                {
                    newTri = new Triangle3Ex(va, vb, vc);
                }
            }
            else if (partA.Length == 2)
            {
                if (!int.TryParse(parts[0].Split('/')[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int a))
                { throw new ParsingException($"Expected a number as a face vertex index (at line {i})"); }

                if (!int.TryParse(parts[1].Split('/')[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int b))
                { throw new ParsingException($"Expected a number as a face vertex index (at line {i})"); }

                if (!int.TryParse(parts[2].Split('/')[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int c))
                { throw new ParsingException($"Expected a number as a face vertex index (at line {i})"); }

                if (!int.TryParse(parts[0].Split('/')[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int ta))
                { throw new ParsingException($"Expected a number as a texture coord index (at line {i})"); }

                if (!int.TryParse(parts[1].Split('/')[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int tb))
                { throw new ParsingException($"Expected a number as a texture coord index (at line {i})"); }

                if (!int.TryParse(parts[2].Split('/')[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int tc))
                { throw new ParsingException($"Expected a number as a texture coord index (at line {i})"); }

                Vector3 va, vb, vc;

                if (a <= 0 || a > vertices.Count)
                { throw new ParsingException($"Vertex index A out of range (at line {i})"); }

                if (b <= 0 || b > vertices.Count)
                { throw new ParsingException($"Vertex index B out of range (at line {i})"); }

                if (c <= 0 || c > vertices.Count)
                { throw new ParsingException($"Vertex index C out of range (at line {i})"); }

                if (ta <= 0 || ta > texs.Count)
                { throw new ParsingException($"Texture coord index A out of range (at line {i})"); }

                if (tb <= 0 || tb > texs.Count)
                { throw new ParsingException($"Texture coord index B out of range (at line {i})"); }

                if (tc <= 0 || tc > texs.Count)
                { throw new ParsingException($"Texture coord index C out of range (at line {i})"); }

                va = vertices[a - 1];
                vb = vertices[b - 1];
                vc = vertices[c - 1];

                newTri = new Triangle3Ex(va, vb, vc)
                {
                    TexA = texs[ta - 1],
                    TexB = texs[tb - 1],
                    TexC = texs[tc - 1],
                };
            }
            else if (partA.Length == 1)
            {
                if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int a))
                { throw new ParsingException($"Expected a number as a face vertex index (at line {i})"); }

                if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int b))
                { throw new ParsingException($"Expected a number as a face vertex index (at line {i})"); }

                if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int c))
                { throw new ParsingException($"Expected a number as a face vertex index (at line {i})"); }

                Vector3 va, vb, vc;

                if (a <= 0 || a > vertices.Count)
                { throw new ParsingException($"Vertex index A out of range (at line {i})"); }

                if (b <= 0 || b > vertices.Count)
                { throw new ParsingException($"Vertex index B out of range (at line {i})"); }

                if (c <= 0 || c > vertices.Count)
                { throw new ParsingException($"Vertex index C out of range (at line {i})"); }

                va = vertices[a - 1];
                vb = vertices[b - 1];
                vc = vertices[c - 1];

                newTri = new Triangle3Ex(va, vb, vc);
            }
            else
            { continue; }

            newTri.MaterialIndex = currentMaterialI;

            triangles.Add(newTri);
        }

        Mesh result = new(triangles.ToArray())
        { Materials = usedMaterials.ToArray() };

        if (usedMaterials.Count == 0)
        { result.SetMaterial(new Material()); }

        return result;
    }
}
