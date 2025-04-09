namespace Common
{
    public class RotatedCube : IRaycastable
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

        public RotatedCube(Vector3D center, float size, MyColor color, float rotationX = 0f, float rotationY = 0f, float rotationZ = 0f,
                       MaterialType materialType = MaterialType.WHITE_PLASTIC, float reflectivity = 0f)
        {
            Center = center;
            Size = size;
            Color = color;
            Material = Common.Material.Create(materialType, reflectivity);
            RotationX = rotationX * MathF.PI / 180f; // Convert to radians
            RotationY = rotationY * MathF.PI / 180f;
            RotationZ = rotationZ * MathF.PI / 180f;
            Triangles = CreateTriangles();
        }

        public RotatedCube(Vector3D center, float size, Material material, float rotationX = 0f, float rotationY = 0f, float rotationZ = 0f)
        {
            Center = center;
            Size = size;
            Material = material;
            Color = material.Diffuse; // Use diffuse color as the main color
            RotationX = rotationX * MathF.PI / 180f; // Convert to radians
            RotationY = rotationY * MathF.PI / 180f;
            RotationZ = rotationZ * MathF.PI / 180f;
            Triangles = CreateTriangles();
        }

        private List<Triangle> CreateTriangles()
        {
            List<Triangle> result = new(12); // A cube has 12 triangles (2 per face)

            // Calculate half-size for vertex positions
            float hs = Size / 2.0f;

            // Define the 8 vertices of the cube (before rotation)
            Vector3D[] vertices = new Vector3D[8];
            vertices[0] = new Vector3D(-hs, -hs, -hs); // bottom-left-back
            vertices[1] = new Vector3D(hs, -hs, -hs);  // bottom-right-back
            vertices[2] = new Vector3D(hs, hs, -hs);   // top-right-back
            vertices[3] = new Vector3D(-hs, hs, -hs);  // top-left-back
            vertices[4] = new Vector3D(-hs, -hs, hs);  // bottom-left-front
            vertices[5] = new Vector3D(hs, -hs, hs);   // bottom-right-front
            vertices[6] = new Vector3D(hs, hs, hs);    // top-right-front
            vertices[7] = new Vector3D(-hs, hs, hs);   // top-left-front

            // Apply rotations and translation to all vertices
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = RotateVertex(vertices[i]);
                vertices[i] = new Vector3D(
                    vertices[i].X + Center.X,
                    vertices[i].Y + Center.Y,
                    vertices[i].Z + Center.Z
                );
            }

            // Front face
            result.Add(new Triangle(vertices[4], vertices[5], vertices[6], Material));
            result.Add(new Triangle(vertices[4], vertices[6], vertices[7], Material));

            // Back face
            result.Add(new Triangle(vertices[1], vertices[0], vertices[3], Material));
            result.Add(new Triangle(vertices[1], vertices[3], vertices[2], Material));

            // Left face
            result.Add(new Triangle(vertices[0], vertices[4], vertices[7], Material));
            result.Add(new Triangle(vertices[0], vertices[7], vertices[3], Material));

            // Right face
            result.Add(new Triangle(vertices[5], vertices[1], vertices[2], Material));
            result.Add(new Triangle(vertices[5], vertices[2], vertices[6], Material));

            // Top face
            result.Add(new Triangle(vertices[7], vertices[6], vertices[2], Material));
            result.Add(new Triangle(vertices[7], vertices[2], vertices[3], Material));

            // Bottom face
            result.Add(new Triangle(vertices[0], vertices[1], vertices[5], Material));
            result.Add(new Triangle(vertices[0], vertices[5], vertices[4], Material));

            foreach (var triangle in result)
            {
                triangle.ParentId = this.ObjectId;
            }

            for (int i = 0; i < result.Count; i++)
            {
                // Make sure triangle normal points outward from cube center
                Vector3D centroid = (result[i].V1 + result[i].V2 + result[i].V3) * (1.0f / 3.0f);
                Vector3D toCenter = Center - centroid;

                // If normal points toward center, flip it
                if (result[i].Normal.Dot(toCenter) > 0)
                {
                    // Swap two vertices to flip the normal
                    Vector3D temp = result[i].V2;
                    result[i] = new Triangle(result[i].V1, result[i].V3, temp, result[i].Material);
                }
            }

            return result;
        }

        private Vector3D RotateVertex(Vector3D v)
        {
            // Apply X rotation
            float y1 = v.Y * MathF.Cos(RotationX) - v.Z * MathF.Sin(RotationX);
            float z1 = v.Y * MathF.Sin(RotationX) + v.Z * MathF.Cos(RotationX);

            // Apply Y rotation
            float x2 = v.X * MathF.Cos(RotationY) + z1 * MathF.Sin(RotationY);
            float z2 = -v.X * MathF.Sin(RotationY) + z1 * MathF.Cos(RotationY);

            // Apply Z rotation
            float x3 = x2 * MathF.Cos(RotationZ) - y1 * MathF.Sin(RotationZ);
            float y3 = x2 * MathF.Sin(RotationZ) + y1 * MathF.Cos(RotationZ);

            return new Vector3D(x3, y3, z2);
        }

        public (bool hasHit, float intersectionDistance) Intersect(Ray ray)
        {
            bool hasHit = false;
            float closestDistance = float.MaxValue;

            // Check intersection with all triangles
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
            // Find the triangle closest to intersection point
            Triangle? closestTriangle = null;
            float minDistance = float.MaxValue;

            foreach (var triangle in Triangles)
            {
                // Calculate distance to triangle plane
                float distance = Math.Abs(triangle.Normal.Dot(intersectionPoint - triangle.V1));

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTriangle = triangle;
                }
            }

            // Return normal of closest triangle, ensuring it's properly normalized
            return closestTriangle?.Normal.Normalize() ?? new Vector3D(0, 1, 0);
        }
    }
}