namespace Common
{
    public class Light
    {
        public Vector3D Position { get; }
        public MyColor Color { get; }
        public float Intensity { get; }

        public Light(Vector3D position, MyColor color, float intensity)
        {
            Position = position;
            Color = color;
            Intensity = Math.Clamp(intensity, 0f, 1f); // Clamp intensity between 0 and 1
        }
    }
}