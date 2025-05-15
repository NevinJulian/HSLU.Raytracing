namespace Common
{
    public class Room
    {
        public static void AddRoom(OptimizedScene scene, Vector3D center, float width, float height, float depth, Material material)
        {
            float halfWidth = width / 2;
            float halfHeight = height / 2;
            float halfDepth = depth / 2;

            // Floor
            scene.AddObject(new PlaneObject(
                new Vector3D(center.X, center.Y - halfHeight, center.Z),
                new Vector3D(0, 1, 0),
                material
            ));

            // Ceiling
            scene.AddObject(new PlaneObject(
                new Vector3D(center.X, center.Y + halfHeight, center.Z),
                new Vector3D(0, -1, 0),
                material
            ));

            // Back wall
            scene.AddObject(new PlaneObject(
                new Vector3D(center.X, center.Y, center.Z + halfDepth),
                new Vector3D(0, 0, -1),
                material
            ));

            // Front wall (usually not visible unless you're inside the room)
            scene.AddObject(new PlaneObject(
                new Vector3D(center.X, center.Y, center.Z - halfDepth),
                new Vector3D(0, 0, 1),
                material
            ));

            // Left wall
            scene.AddObject(new PlaneObject(
                new Vector3D(center.X - halfWidth, center.Y, center.Z),
                new Vector3D(1, 0, 0),
                material
            ));

            // Right wall
            scene.AddObject(new PlaneObject(
                new Vector3D(center.X + halfWidth, center.Y, center.Z),
                new Vector3D(-1, 0, 0),
                material
            ));
        }
    }
}