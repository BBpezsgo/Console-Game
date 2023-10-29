namespace ConsoleGame
{
    public class Material
    {
        public Color AmbientColor;
        public Color DiffuseColor;
        public Color SpecularColor;
        public float SpecularExponent;
        public float Alpha;

        public Color EmissionColor;
        public bool IsEmissive => EmissionColor.Intensity > float.Epsilon;

        public Material()
        {
            AmbientColor = Color.White * .3f;
            DiffuseColor = Color.White;
            SpecularColor = Color.Black;
            SpecularExponent = 0f;
            Alpha = 1f;
            EmissionColor = Color.Black;
        }

        public Material Normalize()
        {
            AmbientColor.Clamp();
            DiffuseColor.Clamp();
            SpecularColor.Clamp();

            SpecularExponent = Math.Clamp(SpecularExponent, 0f, 1000f);
            Alpha = Math.Clamp(Alpha, 0f, 1f);
            
            EmissionColor.Clamp();

            return this;
        }
    }
}
