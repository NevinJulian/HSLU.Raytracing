using System;
using System.Collections.Generic;

namespace Common
{
    public class Cube : IRaycastable
    {
        public Vector3D Center { get; }
        public float Size { get; }
        public MyColor Color { get; }
        public float RotationAngle { get; }
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
            float halfSize = Size / 2f;

            // Create the 8 corners of the cube
            Vector3D[] corners = new Vector3D[8];
            corners[0] = new Vector3D(Center.X - halfSize, Center.Y - halfSize, Center.Z - halfSize); // front bottom left
            corners[1] = new Vector3D(Center.X + halfSize, Center.Y - halfSize, Center.Z - halfSize); // front bottom right
            corners[2] = new Vector3D(Center.X + halfSize, Center.Y + halfSize, Center.Z - halfSize); // front top right
            corners[3] = new Vector3D(Center.X - halfSize, Center.Y + halfSize, Center.Z - halfSize); // front top left
            corners[4] = new Vector3D(Center.X - halfSize, Center.Y - halfSize, Center.Z + halfSize); // back bottom left
            corners[5] = new Vector3D(Center.X + halfSize, Center.Y - halfSize, Center.Z + halfSize); // back bottom right
            corners[6] = new Vector3D(Center.X + halfSize, Center.Y + halfSize, Center.Z + halfSize); // back top right
            corners[7] = new Vector3D(Center.X - halfSize, Center.Y + halfSize, Center.Z + halfSize); // back top left

            // Apply rotation to all corners
            if (Math.Abs(RotationAngle) > 0.001f)
            {
                for (int i = 0; i < corners.Length; i++)
                {
                    corners[i] = RotateAroundYAxis(corners[i]);
                }
            }

            // Create triangles for all 6 faces (2 triangles per face)
            // Consistent counter-clockwise winding for all faces
            var triangles = new List<Triangle>();

            // Front face (negative Z)
            triangles.Add(new Triangle(corners[0], corners[1], corners[2], Color));
            triangles.Add(new Triangle(corners[0], corners[2], corners[3], Color));

            // Back face (positive Z)
            triangles.Add(new Triangle(corners[4], corners[7], corners[6], Color));
            triangles.Add(new Triangle(corners[4], corners[6], corners[5], Color));

            // Left face (negative X)
            triangles.Add(new Triangle(corners[0], corners[3], corners[7], Color));
            triangles.Add(new Triangle(corners[0], corners[7], corners[4], Color));

            // Right face (positive X)
            triangles.Add(new Triangle(corners[1], corners[5], corners[6], Color));
            triangles.Add(new Triangle(corners[1], corners[6], corners[2], Color));

            // Bottom face (negative Y)
            triangles.Add(new Triangle(corners[0], corners[4], corners[5], Color));
            triangles.Add(new Triangle(corners[0], corners[5], corners[1], Color));

            // Top face (positive Y)
            triangles.Add(new Triangle(corners[3], corners[2], corners[6], Color));
            triangles.Add(new Triangle(corners[3], corners[6], corners[7], Color));

            return triangles;
        }

        private Vector3D RotateAroundYAxis(Vector3D point)
        {
            // Convert angle to radians
            float angleRad = RotationAngle * (float)Math.PI / 180f;

            // Calculate sin and cos values
            float cosAngle = (float)Math.Cos(angleRad);
            float sinAngle = (float)Math.Sin(angleRad);

            // Translate point relative to cube center
            float relativeX = point.X - Center.X;
            float relativeY = point.Y - Center.Y;
            float relativeZ = point.Z - Center.Z;

            // Apply Y-axis rotation
            float rotatedX = relativeX * cosAngle - relativeZ * sinAngle;
            float rotatedZ = relativeX * sinAngle + relativeZ * cosAngle;

            // Translate back to world space
            return new Vector3D(
                rotatedX + Center.X,
                relativeY + Center.Y,
                rotatedZ + Center.Z
            );
        }

        public (bool hasHit, float intersectionDistance) Intersect(Ray ray)
        {
            bool hasHit = false;
            float closestDistance = float.MaxValue;

            // Check all triangle faces and find closest intersection
            foreach (var triangle in Faces)
            {
                var (triangleHit, distance) = triangle.Intersect(ray);

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
            // Find the triangle closest to intersection point
            Triangle closestTriangle = null;
            float minDistance = float.MaxValue;

            foreach (var triangle in Faces)
            {
                // Calculate distance to triangle (using centroid for simplicity)
                Vector3D centroid = (triangle.V1 + triangle.V2 + triangle.V3) * (1.0f / 3.0f);
                float distance = (centroid - intersectionPoint).Length;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTriangle = triangle;
                }
            }

            // Return normal of closest triangle
            return closestTriangle?.Normal ?? new Vector3D(0, 1, 0);
        }
    }
}