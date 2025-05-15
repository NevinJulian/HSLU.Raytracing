namespace Common
{
    public class TransparentCube : IRaycastable
    {
        public Vector3D Center { get; }
        public float Size { get; }
        public MyColor Color { get; }
        public Material Material { get; }
        public int ObjectId { get; set; }

        private readonly float RotationX;
        private readonly float RotationY;
        private readonly float RotationZ;
        private readonly List<Triangle> Triangles;

        public TransparentCube(Vector3D center, float size, Material material,
                              float rotationX = 0f, float rotationY = 0f, float rotationZ = 0f)
        {
            Center = center;
            Size = size;
            Material = material;
            Color = material.Diffuse;
            RotationX = rotationX * MathF.PI / 180f; // Convert to radians
            RotationY = rotationY * MathF.PI / 180f;
            RotationZ = rotationZ * MathF.PI / 180f;
            Triangles = CreateTriangles();
        }

        private List<Triangle> CreateTriangles()
        {
            List<Triangle> result = new(12); // A cube has 12 triangles (2 per face)

            float hs = Size / 2.0f;

            Vector3D[] vertices = new Vector3D[8];
            vertices[0] = new Vector3D(-hs, -hs, -hs); // bottom-left-back
            vertices[1] = new Vector3D(hs, -hs, -hs);  // bottom-right-back
            vertices[2] = new Vector3D(hs, hs, -hs);   // top-right-back
            vertices[3] = new Vector3D(-hs, hs, -hs);  // top-left-back
            vertices[4] = new Vector3D(-hs, -hs, hs);  // bottom-left-front
            vertices[5] = new Vector3D(hs, -hs, hs);   // bottom-right-front
            vertices[6] = new Vector3D(hs, hs, hs);    // top-right-front
            vertices[7] = new Vector3D(-hs, hs, hs);   // top-left-front

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = RotateVertex(vertices[i]);
                vertices[i] = new Vector3D(
                    vertices[i].X + Center.X,
                    vertices[i].Y + Center.Y,
                    vertices[i].Z + Center.Z
                );
            }

            var purpleMaterial = new Material(
                MaterialType.RED_PLASTIC,
                new MyColor(40, 0, 40),       // Ambient
                new MyColor(180, 30, 180),    // Purple
                Material.Specular,            // Same specular as base
                Material.Shininess,           // Same shininess
                Material.Reflectivity,        // Same reflectivity
                Material.Transparency         // Same transparency
            );

            var yellowMaterial = new Material(
                MaterialType.YELLOW_PLASTIC,
                new MyColor(40, 40, 0),       // Ambient
                new MyColor(200, 200, 30),    // Yellow
                Material.Specular,            // Same specular as base
                Material.Shininess,           // Same shininess
                Material.Reflectivity,        // Same reflectivity
                Material.Transparency         // Same transparency
            );

            var cyanMaterial = new Material(
                MaterialType.CYAN_PLASTIC,
                new MyColor(0, 40, 40),       // Ambient
                new MyColor(30, 180, 180),    // Cyan
                Material.Specular,            // Same specular as base
                Material.Shininess,           // Same shininess
                Material.Reflectivity,        // Same reflectivity
                Material.Transparency         // Same transparency
            );

            // Create a base transparent material
            var baseMaterial = Material;

            // Front face (base color)
            result.Add(new Triangle(vertices[4], vertices[5], vertices[6], baseMaterial));
            result.Add(new Triangle(vertices[4], vertices[6], vertices[7], baseMaterial));

            // Back face (cyan)
            result.Add(new Triangle(vertices[1], vertices[0], vertices[3], cyanMaterial));
            result.Add(new Triangle(vertices[1], vertices[3], vertices[2], cyanMaterial));

            // Left face (purple)
            result.Add(new Triangle(vertices[0], vertices[4], vertices[7], purpleMaterial));
            result.Add(new Triangle(vertices[0], vertices[7], vertices[3], purpleMaterial));

            // Right face (yellow)
            result.Add(new Triangle(vertices[5], vertices[1], vertices[2], yellowMaterial));
            result.Add(new Triangle(vertices[5], vertices[2], vertices[6], yellowMaterial));

            // Top face (cyan)
            result.Add(new Triangle(vertices[7], vertices[6], vertices[2], cyanMaterial));
            result.Add(new Triangle(vertices[7], vertices[2], vertices[3], cyanMaterial));

            // Bottom face (base color)
            result.Add(new Triangle(vertices[0], vertices[1], vertices[5], baseMaterial));
            result.Add(new Triangle(vertices[0], vertices[5], vertices[4], baseMaterial));

            foreach (var triangle in result)
            {
                triangle.ParentId = this.ObjectId;
            }

            return result;
        }

        private Vector3D RotateVertex(Vector3D v)
        {
            float y1 = v.Y * MathF.Cos(RotationX) - v.Z * MathF.Sin(RotationX);
            float z1 = v.Y * MathF.Sin(RotationX) + v.Z * MathF.Cos(RotationX);

            float x2 = v.X * MathF.Cos(RotationY) + z1 * MathF.Sin(RotationY);
            float z2 = -v.X * MathF.Sin(RotationY) + z1 * MathF.Cos(RotationY);

            float x3 = x2 * MathF.Cos(RotationZ) - y1 * MathF.Sin(RotationZ);
            float y3 = x2 * MathF.Sin(RotationZ) + y1 * MathF.Cos(RotationZ);

            return new Vector3D(x3, y3, z2);
        }

        public (bool hasHit, float intersectionDistance) Intersect(Ray ray)
        {
            bool hasHit = false;
            float closestDistance = float.MaxValue;

            foreach (Triangle triangle in Triangles)
            {
                var (triangleHit, distance) = triangle.Intersect(ray);
                if (triangleHit && distance > 0.001f && distance < closestDistance)
                {
                    hasHit = true;
                    closestDistance = distance;
                }
            }

            return (hasHit, closestDistance);
        }

        public Vector3D GetNormal(Vector3D intersectionPoint)
        {
            Triangle? closestTriangle = null;
            float minDistance = float.MaxValue;

            foreach (var triangle in Triangles)
            {
                float distance = Math.Abs(triangle.Normal.Dot(intersectionPoint - triangle.V1));

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTriangle = triangle;
                }
            }

            return closestTriangle?.Normal.Normalize() ?? new Vector3D(0, 1, 0);
        }
    }
}