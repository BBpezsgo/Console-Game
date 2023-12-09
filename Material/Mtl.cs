using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace ConsoleGame
{
    public static class Mtl
    {
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="ParsingException"/>
        public static Dictionary<string, Material> LoadFile(string file)
        {
            if (!File.Exists(file)) throw new FileNotFoundException($"Object file \"{file}\" not found");
            string text = File.ReadAllText(file, Encoding.ASCII);
            string[] lines = text.Replace('\r', '\n').Split('\n');

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

                if (kind == "newmtl")
                {
                    if (currentMaterialName != null && currentMaterial != null)
                    { materials.Add(currentMaterialName, currentMaterial); }

                    currentMaterialName = line;
                    currentMaterial = new Material();
                    continue;
                }

                if (currentMaterialName == null || currentMaterial == null)
                { throw new ParsingException($"No material made (at line {i})"); }

                switch (kind)
                {
                    #region Basic

                    case "Ka":
                        {
                            if (!Color.TryParse(line, CultureInfo.InvariantCulture, out currentMaterial.AmbientColor))
                            { throw new ParsingException($"Failed to parse mtl color (at line {i})"); }

                            break;
                        }
                    case "Kd":
                        {
                            if (!Color.TryParse(line, CultureInfo.InvariantCulture, out currentMaterial.DiffuseColor))
                            { throw new ParsingException($"Failed to parse mtl color (at line {i})"); }

                            break;
                        }
                    case "Ks":
                        {
                            if (!Color.TryParse(line, CultureInfo.InvariantCulture, out currentMaterial.SpecularColor))
                            { throw new ParsingException($"Failed to parse mtl color (at line {i})"); }

                            break;
                        }
                    case "Ns":
                        {
                            if (!float.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out currentMaterial.SpecularExponent))
                            { throw new ParsingException($"Failed to parse mtl float (at line {i})"); }

                            break;
                        }

                    case "d":
                        {
                            if (!float.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out currentMaterial.Alpha))
                            { throw new ParsingException($"Failed to parse mtl float (at line {i})"); }

                            break;
                        }

                    case "Tr":
                        {
                            if (!float.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out currentMaterial.Alpha))
                            { throw new ParsingException($"Failed to parse mtl float (at line {i})"); }

                            currentMaterial.Alpha = 1f - currentMaterial.Alpha;

                            break;
                        }

                    #endregion

                    #region Physically-based

                    case "Ke":
                    case "map_Ke":
                        {
                            if (!Color.TryParse(line, CultureInfo.InvariantCulture, out currentMaterial.EmissionColor))
                            { throw new ParsingException($"Failed to parse mtl color (at line {i})"); }

                            break;
                        }

                    case "illum":

                    case "Ni":

                    case "Pr":
                    case "map_Pr":

                    case "Pm":
                    case "map_Pm":

                    case "Ps":
                    case "map_Ps":

                    case "Pc":

                    case "Pcr":

                    case "aniso":

                    case "anisor":

                    case "norm":

                    #endregion

                    default:
                        Debug.WriteLine($"Unsupported mtl line kind \"{kind}\" (\"{line}\") (at line {i})");
                        break;
                }
            }

            if (currentMaterialName != null && currentMaterial != null)
            { materials.Add(currentMaterialName, currentMaterial); }

            return materials;
        }
    }
}
