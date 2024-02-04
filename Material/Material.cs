using Win32.Gdi32;

namespace ConsoleGame
{
    public class Material
    {
        public ColorF AmbientColor;
        public ColorF DiffuseColor;
        public ColorF SpecularColor;
        public float SpecularExponent;
        public float Alpha;

        public ColorF EmissionColor;
        public bool IsEmissive => EmissionColor.Intensity > float.Epsilon;

        public Material()
        {
            AmbientColor = ColorF.White * .3f;
            DiffuseColor = ColorF.White;
            SpecularColor = ColorF.Black;
            SpecularExponent = 0f;
            Alpha = 1f;
            EmissionColor = ColorF.Black;
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
