namespace Common
{
    public class LightSource
    {
        public Vector3D Position { get; }
        public float Intensity { get; }
        public MyColor Color { get; }

        public LightSource(Vector3D position, float intensity, MyColor color)
        {
            Position = position;
            Intensity = intensity;
            Color = color;
        }
    }
}
