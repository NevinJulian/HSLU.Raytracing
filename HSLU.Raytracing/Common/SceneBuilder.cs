namespace Common
{
    public class SceneBuilder
    {
        private readonly Scene scene;

        public SceneBuilder()
        {
            scene = new Scene();
        }

        public SceneBuilder AddSphere(Vector3D center, float radius, MaterialType material, float reflectivity)
        {
            scene.AddObject(new Sphere(center, radius, Material.Create(material, reflectivity)));
            return this;
        }

        public SceneBuilder AddRotatedCube(Vector3D center, float size, MaterialType material, float reflectivity,
                                       float rotationX, float rotationY, float rotationZ)
        {
            scene.AddObject(new RotatedCube(
                center, size, Material.Create(material, reflectivity),
                rotationX, rotationY, rotationZ
            ));
            return this;
        }

        public SceneBuilder AddLight(Vector3D position, MyColor color, float intensity)
        {
            scene.AddLight(new Light(position, color, intensity));
            return this;
        }

        public Scene Build()
        {
            return scene;
        }
    }
}