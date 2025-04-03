namespace Common
{
    public class Cube : IRaycastable
    {
        public Vector3D Center { get; }
        public float Size { get; } // Length of one side
        public MyColor Color { get; }
        public float RotationAngle { get; } // Rotation angle in degrees
        private List<Triangle> Faces { get; }

        public Cube(Vector3D center, float size, MyColor color, float rotationAngle = 0f)
        {
            Center = center;
            Size = size;
            Color = color;
            RotationAngle = rotationAngle;
            Faces = CreateFaces();
        }

        private List<Triangle> CreateFaces()
        {
            // Half the size to get distance from center to face
            float halfSize = Size / 2f;

            // Create 8 corners of the cube (unrotated)
            List<Vector3D> corners = new List<Vector3D>
            {
                new Vector3D(Center.X - halfSize, Center.Y - halfSize, Center.Z - halfSize), // 0
                new Vector3D(Center.X + halfSize, Center.Y - halfSize, Center.Z - halfSize), // 1
                new Vector3D(Center.X + halfSize, Center.Y + halfSize, Center.Z - halfSize), // 2
                new Vector3D(Center.X - halfSize, Center.Y + halfSize, Center.Z - halfSize), // 3
                new Vector3D(Center.X - halfSize, Center.Y - halfSize, Center.Z + halfSize), // 4
                new Vector3D(Center.X + halfSize, Center.Y - halfSize, Center.Z + halfSize), // 5
                new Vector3D(Center.X + halfSize, Center.Y + halfSize, Center.Z + halfSize), // 6
                new Vector3D(Center.X - halfSize, Center.Y + halfSize, Center.Z + halfSize)  // 7
            };

            // Apply rotation to each corner
            if (RotationAngle != 0)
            {
                for (int i = 0; i < corners.Count; i++)
                {
                    corners[i] = RotatePoint(corners[i], Center, RotationAngle);
                }
            }

            // Create 12 triangles (2 for each face of the cube)
            var triangles = new List<Triangle>
            {
                // Front face (facing negative Z)
                new Triangle(corners[0], corners[1], corners[2], Color),
                new Triangle(corners[0], corners[2], corners[3], Color),
                
                // Back face (facing positive Z)
                new Triangle(corners[4], corners[6], corners[5], Color),
                new Triangle(corners[4], corners[7], corners[6], Color),
                
                // Left face (facing negative X)
                new Triangle(corners[0], corners[3], corners[7], Color),
                new Triangle(corners[0], corners[7], corners[4], Color),
                
                // Right face (facing positive X)
                new Triangle(corners[1], corners[5], corners[6], Color),
                new Triangle(corners[1], corners[6], corners[2], Color),
                
                // Bottom face (facing negative Y)
                new Triangle(corners[0], corners[4], corners[5], Color),
                new Triangle(corners[0], corners[5], corners[1], Color),
                
                // Top face (facing positive Y)
                new Triangle(corners[3], corners[2], corners[6], Color),
                new Triangle(corners[3], corners[6], corners[7], Color)
            };

            return triangles;
        }

        // Helper method to rotate a point around a center point
        private Vector3D RotatePoint(Vector3D point, Vector3D center, float angleDegrees)
        {
            // Convert angle to radians
            float angleRadians = angleDegrees * (float)Math.PI / 180f;

            // Translate point to origin
            float x = point.X - center.X;
            float y = point.Y - center.Y;
            float z = point.Z - center.Z;

            // Rotate around Y-axis (vertical axis)
            float cosA = (float)Math.Cos(angleRadians);
            float sinA = (float)Math.Sin(angleRadians);

            float newX = x * cosA - z * sinA;
            float newZ = x * sinA + z * cosA;

            // Also add a moderate tilt around X-axis (15 degrees) for better visibility
            float tiltAngle = 15f * (float)Math.PI / 180f;
            float cosT = (float)Math.Cos(tiltAngle);
            float sinT = (float)Math.Sin(tiltAngle);

            float finalY = y * cosT - newZ * sinT;
            float finalZ = y * sinT + newZ * cosT;

            // Translate back
            return new Vector3D(
                newX + center.X,
                finalY + center.Y,
                finalZ + center.Z
            );
        }

        public (bool hasHit, float intersectionDistance) Intersect(Ray ray)
        {
            bool hasHit = false;
            float closestDistance = float.MaxValue;

            // Check intersection with each face
            foreach (var triangle in Faces)
            {
                var (triangleHit, distance) = triangle.Intersect(ray);

                // Use a tiny epsilon to avoid precision issues
                if (triangleHit && distance > 0.0001f && distance < closestDistance)
                {
                    hasHit = true;
                    closestDistance = distance;
                }
            }

            return (hasHit, closestDistance);
        }

        public Vector3D GetNormal(Vector3D intersectionPoint)
        {
            // Find which triangle was hit
            Triangle closestTriangle = null;
            float minDistance = float.MaxValue;

            foreach (var triangle in Faces)
            {
                // Calculate the distance to the triangle's plane
                float distance = Math.Abs(triangle.Normal.Dot(intersectionPoint - triangle.V1));

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTriangle = triangle;
                }
            }

            // Return the normal of the closest triangle
            if (closestTriangle != null)
            {
                return closestTriangle.Normal;
            }

            // Fallback (shouldn't happen)
            return new Vector3D(0, 0, 1);
        }
    }
}