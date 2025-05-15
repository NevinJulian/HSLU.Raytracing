namespace Common
{
    public class Material
    {
        public MaterialType Type { get; }
        public MyColor Ambient { get; }
        public MyColor Diffuse { get; }
        public MyColor Specular { get; }
        public float Shininess { get; }
        public float Reflectivity { get; }
        public float Transparency { get; }

        public Material(MaterialType type, MyColor ambient, MyColor diffuse, MyColor specular,
                        float shininess, float reflectivity, float transparency = 0.0f)
        {
            Type = type;
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
            Shininess = shininess;
            Reflectivity = Math.Clamp(reflectivity, 0f, 1f);
            Transparency = Math.Clamp(transparency, 0f, 1f);
        }

        public static Material Create(MaterialType type, float reflectivity, float transparency = 0.0f)
        {
            return type switch
            {
                MaterialType.EMERALD => new Material(type,
                    new MyColor((int)(0.0215f * 255), (int)(0.1745f * 255), (int)(0.0215f * 255)),
                    new MyColor((int)(0.07568f * 255), (int)(0.61424f * 255), (int)(0.07568f * 255)),
                    new MyColor((int)(0.633f * 255), (int)(0.727811f * 255), (int)(0.633f * 255)),
                    0.6f, reflectivity, transparency),

                MaterialType.JADE => new Material(type,
                    new MyColor((int)(0.135f * 255), (int)(0.2225f * 255), (int)(0.1575f * 255)),
                    new MyColor((int)(0.54f * 255), (int)(0.89f * 255), (int)(0.63f * 255)),
                    new MyColor((int)(0.316228f * 255), (int)(0.316228f * 255), (int)(0.316228f * 255)),
                    0.1f, reflectivity, transparency),

                MaterialType.RUBY => new Material(type,
                    new MyColor((int)(0.1745f * 255), (int)(0.01175f * 255), (int)(0.01175f * 255)),
                    new MyColor((int)(0.61424f * 255), (int)(0.04136f * 255), (int)(0.04136f * 255)),
                    new MyColor((int)(0.727811f * 255), (int)(0.626959f * 255), (int)(0.626959f * 255)),
                    0.6f, reflectivity, transparency),

                MaterialType.GOLD => new Material(type,
                    new MyColor((int)(0.24725f * 255), (int)(0.1995f * 255), (int)(0.0745f * 255)),
                    new MyColor((int)(0.75164f * 255), (int)(0.60648f * 255), (int)(0.22648f * 255)),
                    new MyColor((int)(0.628281f * 255), (int)(0.555802f * 255), (int)(0.366065f * 255)),
                    0.4f, reflectivity, transparency),

                MaterialType.RED_PLASTIC => new Material(type,
                    new MyColor(0, 0, 0),
                    new MyColor((int)(0.5f * 255), 0, 0),
                    new MyColor((int)(0.7f * 255), (int)(0.6f * 255), (int)(0.6f * 255)),
                    0.25f, reflectivity, transparency),

                _ => new Material(type,
                    new MyColor((int)(0.05f * 255), (int)(0.05f * 255), (int)(0.05f * 255)),
                    new MyColor((int)(0.5f * 255), (int)(0.5f * 255), (int)(0.5f * 255)),
                    new MyColor((int)(0.7f * 255), (int)(0.7f * 255), (int)(0.7f * 255)),
                    0.078125f, reflectivity, transparency)
            };
        }
    }
}