using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public class BoundingBox
    {
        public Vector3D Min { get; }
        public Vector3D Max { get; }

        public BoundingBox(Vector3D min, Vector3D max)
        {
            Min = min;
            Max = max;
        }

        public BoundingBox(List<Vector3D> points)
        {
            if (points.Count == 0)
                throw new ArgumentException("Cannot create bounding box from empty list");

            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

            foreach (var p in points)
            {
                minX = Math.Min(minX, p.X);
                minY = Math.Min(minY, p.Y);
                minZ = Math.Min(minZ, p.Z);

                maxX = Math.Max(maxX, p.X);
                maxY = Math.Max(maxY, p.Y);
                maxZ = Math.Max(maxZ, p.Z);
            }

            Min = new Vector3D(minX, minY, minZ);
            Max = new Vector3D(maxX, maxY, maxZ);
        }

        public static BoundingBox FromTriangles(List<Triangle> triangles)
        {
            List<Vector3D> points = new List<Vector3D>(triangles.Count * 3);
            foreach (var triangle in triangles)
            {
                points.Add(triangle.V1);
                points.Add(triangle.V2);
                points.Add(triangle.V3);
            }
            return new BoundingBox(points);
        }

        public bool Intersect(Ray ray)
        {
            // Compute intersection with all three slabs
            float tx1 = (Min.X - ray.Origin.X) / ray.Direction.X;
            float tx2 = (Max.X - ray.Origin.X) / ray.Direction.X;

            float tmin = Math.Min(tx1, tx2);
            float tmax = Math.Max(tx1, tx2);

            float ty1 = (Min.Y - ray.Origin.Y) / ray.Direction.Y;
            float ty2 = (Max.Y - ray.Origin.Y) / ray.Direction.Y;

            tmin = Math.Max(tmin, Math.Min(ty1, ty2));
            tmax = Math.Min(tmax, Math.Max(ty1, ty2));

            float tz1 = (Min.Z - ray.Origin.Z) / ray.Direction.Z;
            float tz2 = (Max.Z - ray.Origin.Z) / ray.Direction.Z;

            tmin = Math.Max(tmin, Math.Min(tz1, tz2));
            tmax = Math.Min(tmax, Math.Max(tz1, tz2));

            return tmax >= tmin && tmax > 0;
        }
    }

    public class BVHNode
    {
        public BoundingBox Box { get; }
        public BVHNode Left { get; }
        public BVHNode Right { get; }
        public List<Triangle> Triangles { get; }

        private const int MAX_TRIANGLES_PER_LEAF = 10;

        // Leaf node constructor
        public BVHNode(List<Triangle> triangles)
        {
            Triangles = triangles;
            Box = BoundingBox.FromTriangles(triangles);
            Left = null;
            Right = null;
        }

        // Internal node constructor
        public BVHNode(BVHNode left, BVHNode right)
        {
            Left = left;
            Right = right;
            Triangles = new List<Triangle>();

            // Compute bounding box that encompasses both children
            float minX = Math.Min(left.Box.Min.X, right.Box.Min.X);
            float minY = Math.Min(left.Box.Min.Y, right.Box.Min.Y);
            float minZ = Math.Min(left.Box.Min.Z, right.Box.Min.Z);

            float maxX = Math.Max(left.Box.Max.X, right.Box.Max.X);
            float maxY = Math.Max(left.Box.Max.Y, right.Box.Max.Y);
            float maxZ = Math.Max(left.Box.Max.Z, right.Box.Max.Z);

            Box = new BoundingBox(
                new Vector3D(minX, minY, minZ),
                new Vector3D(maxX, maxY, maxZ)
            );
        }

        // Recursive construction of BVH
        public static BVHNode Build(List<Triangle> triangles)
        {
            if (triangles.Count <= MAX_TRIANGLES_PER_LEAF)
            {
                return new BVHNode(triangles);
            }

            // Find the axis with the largest extent
            BoundingBox box = BoundingBox.FromTriangles(triangles);
            float xExtent = box.Max.X - box.Min.X;
            float yExtent = box.Max.Y - box.Min.Y;
            float zExtent = box.Max.Z - box.Min.Z;

            int axis = 0; // 0 = x, 1 = y, 2 = z
            if (yExtent > xExtent) axis = 1;
            if (zExtent > (axis == 0 ? xExtent : yExtent)) axis = 2;

            // Sort triangles based on their centroid along the chosen axis
            triangles.Sort((a, b) =>
            {
                Vector3D centroidA = (a.V1 + a.V2 + a.V3) * (1.0f / 3.0f);
                Vector3D centroidB = (b.V1 + b.V2 + b.V3) * (1.0f / 3.0f);

                float valueA = axis == 0 ? centroidA.X : (axis == 1 ? centroidA.Y : centroidA.Z);
                float valueB = axis == 0 ? centroidB.X : (axis == 1 ? centroidB.Y : centroidB.Z);

                return valueA.CompareTo(valueB);
            });

            // Split triangles into left and right groups
            int mid = triangles.Count / 2;
            List<Triangle> leftTriangles = triangles.GetRange(0, mid);
            List<Triangle> rightTriangles = triangles.GetRange(mid, triangles.Count - mid);

            // Recursively build left and right subtrees
            BVHNode left = Build(leftTriangles);
            BVHNode right = Build(rightTriangles);

            return new BVHNode(left, right);
        }

        // Intersection test
        public HitInfo? Intersect(Ray ray)
        {
            // First, check if the ray intersects the bounding box
            if (!Box.Intersect(ray))
                return null;

            // If this is a leaf node, check all triangles
            if (Left == null && Right == null)
            {
                HitInfo? closestHit = null;
                float closestDistance = float.MaxValue;

                foreach (Triangle triangle in Triangles)
                {
                    var (hasHit, distance) = triangle.Intersect(ray);
                    if (hasHit && distance > 0.001f && distance < closestDistance)
                    {
                        Vector3D hitPoint = ray.GetPointAt(distance);
                        Vector3D normal = triangle.GetNormal(hitPoint);
                        closestHit = new HitInfo(triangle, hitPoint, normal, distance);
                        closestDistance = distance;
                    }
                }

                return closestHit;
            }

            // Otherwise, check both children and return the closest hit
            HitInfo? leftHit = Left.Intersect(ray);
            HitInfo? rightHit = Right.Intersect(ray);

            if (leftHit == null) return rightHit;
            if (rightHit == null) return leftHit;

            // Return the closer of the two hits
            return leftHit.Distance < rightHit.Distance ? leftHit : rightHit;
        }
    }

    // Helper class to integrate BVH with your scene
    public class BVHAccelerator
    {
        private BVHNode root;
        private List<IRaycastable> nonTriangles;

        public BVHAccelerator(List<IRaycastable> objects)
        {
            // Separate triangles from other objects
            List<Triangle> triangles = new List<Triangle>();
            nonTriangles = new List<IRaycastable>();

            foreach (var obj in objects)
            {
                if (obj is Triangle triangle)
                {
                    triangles.Add(triangle);
                }
                else
                {
                    nonTriangles.Add(obj);
                }
            }

            // Build BVH for triangles if we have any
            if (triangles.Count > 0)
            {
                root = BVHNode.Build(triangles);
                Console.WriteLine($"Built BVH with {triangles.Count} triangles");
            }
        }

        public HitInfo? FindClosestIntersection(Ray ray)
        {
            HitInfo? closestHit = null;
            float closestDistance = float.MaxValue;

            // Check BVH for triangle intersections
            if (root != null)
            {
                closestHit = root.Intersect(ray);
                if (closestHit != null)
                {
                    closestDistance = closestHit.Distance;
                }
            }

            // Check other objects (spheres, planes, etc.)
            foreach (IRaycastable obj in nonTriangles)
            {
                var (hasHit, distance) = obj.Intersect(ray);
                if (hasHit && distance > 0.001f && distance < closestDistance)
                {
                    Vector3D hitPoint = ray.GetPointAt(distance);
                    Vector3D normal = obj.GetNormal(hitPoint);
                    closestHit = new HitInfo(obj, hitPoint, normal, distance);
                    closestDistance = distance;
                }
            }

            return closestHit;
        }
    }
}