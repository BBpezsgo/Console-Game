using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGame
{
    public static class Mtl
    {
        public class Material
        {
            public Color AmbientColor;
            public Color DiffuseColor;
            public Color SpecularColor;
            public float SpecularExponent;
        }

        /// <exception cref="Exception"/>
        public static Dictionary<string, Material> LoadFile(string file)
        {
            if (!File.Exists(file)) throw new Exception($"Object file \"{file}\" not found");
            string text = File.ReadAllText(file, Encoding.ASCII);
            string[] lines = text.Replace('\r', '\n').Split('\n');

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

            string? currentMaterialName = null;
            Material? currentMaterial = null;

            Dictionary<string, Material> materials = new();

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
                    case "newmtl":
                        {
                            if (currentMaterialName != null && currentMaterial != null)
                            { materials.Add(currentMaterialName, currentMaterial); }

                            currentMaterialName = line;
                            currentMaterial = new Material();
                            break;
                        }
                    case "Ka":
                        {
                            if (currentMaterialName == null || currentMaterial == null)
                            { break; }

                            if (!Color.TryParse(line, out currentMaterial.AmbientColor))
                            { throw new Exception($"Failed to parse mtl color (at line {i})"); }

                            break;
                        }
                    case "Kd":
                        {
                            if (currentMaterialName == null || currentMaterial == null)
                            { break; }

                            if (!Color.TryParse(line, out currentMaterial.DiffuseColor))
                            { throw new Exception($"Failed to parse mtl color (at line {i})"); }

                            break;
                        }
                    case "Ks":
                        {
                            if (currentMaterialName == null || currentMaterial == null)
                            { break; }

                            if (!Color.TryParse(line, out currentMaterial.SpecularColor))
                            { throw new Exception($"Failed to parse mtl color (at line {i})"); }

                            break;
                        }
                    case "Ns":
                        {
                            if (currentMaterialName == null || currentMaterial == null)
                            { break; }

                            if (!float.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out currentMaterial.SpecularExponent))
                            { throw new Exception($"Failed to parse mtl float (at line {i})"); }

                            break;
                        }
                    default:
                        // Debug.WriteLine($"Unsupported mtl line kind \"{kind}\" (\"{line}\") (at line {i})");
                        break;
                }
            }

            if (currentMaterialName != null && currentMaterial != null)
            { materials.Add(currentMaterialName, currentMaterial); }

            return materials;
        }
    }
}
