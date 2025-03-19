namespace Common
{
    public class Triangle
    {
        public Vector3D V1 { get; }
        public Vector3D V2 { get; }
        public Vector3D V3 { get; }
        public Vector3D Normal { get; }

        public Triangle(Vector3D v1, Vector3D v2, Vector3D v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;

            // Calculate normal using cross product
            Vector3D edge1 = new Vector3D(V2.X - V1.X, V2.Y - V1.Y, V2.Z - V1.Z);
            Vector3D edge2 = new Vector3D(V3.X - V1.X, V3.Y - V1.Y, V3.Z - V1.Z);

            // Calculate the cross product
            Vector3D crossProduct = edge1.Cross(edge2);

            // Normalize
            Normal = new Vector3D(
                crossProduct.X / crossProduct.Length,
                crossProduct.Y / crossProduct.Length,
                crossProduct.Z / crossProduct.Length
            );
        }

        public (bool hit, Vector3D intersection) IntersectRay(Vector3D rayOrigin, Vector3D rayDirection)
        {
            // Using formula from the slides - ray-plane intersection
            // Check if ray and plane are parallel
            float dotProduct = Normal.Dot(rayDirection);

            // If close to zero, ray is parallel to the plane
            if (Math.Abs(dotProduct) < 1e-6)
            {
                return (false, new Vector3D(0, 0, 0));
            }

            // Calculate t (lambda) using the formula from slide 8
            Vector3D toPoint = new Vector3D(V1.X - rayOrigin.X, V1.Y - rayOrigin.Y, V1.Z - rayOrigin.Z);
            float t = Normal.Dot(toPoint) / dotProduct;

            // If t is negative, intersection is behind the ray origin
            if (t < 0)
            {
                return (false, new Vector3D(0, 0, 0));
            }

            // Calculate intersection point
            Vector3D intersection = new Vector3D(
                rayOrigin.X + t * rayDirection.X,
                rayOrigin.Y + t * rayDirection.Y,
                rayOrigin.Z + t * rayDirection.Z
            );

            // Check if intersection point is inside the triangle using barycentric coordinates
            bool insideTriangle = IsPointInTriangle(intersection, V1, V2, V3);

            return (insideTriangle, intersection);
        }

        private bool IsPointInTriangle(Vector3D p, Vector3D a, Vector3D b, Vector3D c)
        {
            // Using method 1 from slide 9 (barycentric coordinates)
            // Calculate vectors from point to vertices
            Vector3D v0 = c - a;
            Vector3D v1 = b - a;
            Vector3D v2 = p - a;

            // Compute dot products
            float dot00 = v0.Dot(v0);
            float dot01 = v0.Dot(v1);
            float dot02 = v0.Dot(v2);
            float dot11 = v1.Dot(v1);
            float dot12 = v1.Dot(v2);

            // Compute barycentric coordinates
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // Check if point is inside triangle
            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }
    }

    public class Cube
    {
        public Vector3D Center { get; }
        public float Size { get; } // Length of one side
        public MyColor Color { get; }
        private List<Triangle> Faces { get; }
        public float RotationAngle { get; } // Rotation angle in degrees

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
            Vector3D[] corners = new Vector3D[8];
            corners[0] = new Vector3D(Center.X - halfSize, Center.Y - halfSize, Center.Z - halfSize);
            corners[1] = new Vector3D(Center.X + halfSize, Center.Y - halfSize, Center.Z - halfSize);
            corners[2] = new Vector3D(Center.X + halfSize, Center.Y + halfSize, Center.Z - halfSize);
            corners[3] = new Vector3D(Center.X - halfSize, Center.Y + halfSize, Center.Z - halfSize);
            corners[4] = new Vector3D(Center.X - halfSize, Center.Y - halfSize, Center.Z + halfSize);
            corners[5] = new Vector3D(Center.X + halfSize, Center.Y - halfSize, Center.Z + halfSize);
            corners[6] = new Vector3D(Center.X + halfSize, Center.Y + halfSize, Center.Z + halfSize);
            corners[7] = new Vector3D(Center.X - halfSize, Center.Y + halfSize, Center.Z + halfSize);

            // Apply rotation to each corner
            if (RotationAngle != 0)
            {
                for (int i = 0; i < corners.Length; i++)
                {
                    corners[i] = RotatePoint(corners[i], Center, RotationAngle);
                }
            }

            // Create 12 triangles (2 for each face of the cube)
            var triangles = new List<Triangle>();

            // Front face
            triangles.Add(new Triangle(corners[0], corners[1], corners[2]));
            triangles.Add(new Triangle(corners[0], corners[2], corners[3]));

            // Back face
            triangles.Add(new Triangle(corners[4], corners[6], corners[5]));
            triangles.Add(new Triangle(corners[4], corners[7], corners[6]));

            // Left face
            triangles.Add(new Triangle(corners[0], corners[3], corners[7]));
            triangles.Add(new Triangle(corners[0], corners[7], corners[4]));

            // Right face
            triangles.Add(new Triangle(corners[1], corners[5], corners[6]));
            triangles.Add(new Triangle(corners[1], corners[6], corners[2]));

            // Bottom face
            triangles.Add(new Triangle(corners[0], corners[4], corners[5]));
            triangles.Add(new Triangle(corners[0], corners[5], corners[1]));

            // Top face
            triangles.Add(new Triangle(corners[3], corners[2], corners[6]));
            triangles.Add(new Triangle(corners[3], corners[6], corners[7]));

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

            // Rotate around X-axis (horizontal axis) - adding a slight tilt
            float tiltAngle = 15f * (float)Math.PI / 180f; // 15 degrees tilt
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

        public (bool hit, float depth, Vector3D normal) IntersectRay(Vector2D pixel)
        {
            bool hit = false;
            float maxDepth = float.NegativeInfinity;
            Vector3D hitNormal = new Vector3D(0, 0, 0);

            // Create a ray from the pixel into the scene
            Vector3D rayOrigin = new Vector3D(pixel.X, pixel.Y, 0);
            Vector3D rayDirection = new Vector3D(0, 0, 1); // Looking along the z-axis

            foreach (var triangle in Faces)
            {
                var (triangleHit, intersectionPoint) = triangle.IntersectRay(rayOrigin, rayDirection);

                if (triangleHit)
                {
                    float depth = intersectionPoint.Z;
                    if (depth > maxDepth)
                    {
                        hit = true;
                        maxDepth = depth;
                        hitNormal = triangle.Normal;
                    }
                }
            }

            return (hit, maxDepth, hitNormal);
        }
    }
}