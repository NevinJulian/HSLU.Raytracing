using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;

namespace Common
{
    public class ObjModelImporter
    {
        private class ObjData
        {
            public List<Vector3D> Vertices { get; set; } = new List<Vector3D>();
            public List<Vector3D> Normals { get; set; } = new List<Vector3D>();
            public List<Vector2D> TexCoords { get; set; } = new List<Vector2D>();
            public List<Triangle> Triangles { get; set; } = new List<Triangle>();
        }

        public List<Triangle> ImportObj(string filePath, Material material, Vector3D position, float scale, Vector3D rotation)
        {
            ObjData objData = new ObjData();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                        continue;

                    string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2)
                        continue;

                    string command = parts[0].ToLower();

                    if (command == "v" && parts.Length >= 4)
                    {
                        if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                            float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                        {
                            objData.Vertices.Add(new Vector3D(x, y, z));
                        }
                    }

                    else if (command == "vn" && parts.Length >= 4)
                    {
                        if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                            float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                        {
                            objData.Normals.Add(new Vector3D(x, y, z).Normalize());
                        }
                    }

                    else if (command == "vt" && parts.Length >= 3)
                    {
                        if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float u) &&
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
                        {
                            objData.TexCoords.Add(new Vector2D(u, v));
                        }
                    }

                    else if (command == "f" && parts.Length >= 4)
                    {
                        if (parts.Length == 4)
                        {
                            int[] v1 = ParseFaceIndices(parts[1]);
                            int[] v2 = ParseFaceIndices(parts[2]);
                            int[] v3 = ParseFaceIndices(parts[3]);

                            if (v1[0] > 0 && v2[0] > 0 && v3[0] > 0)
                            {
                                Vector3D vertex1 = objData.Vertices[v1[0] - 1];
                                Vector3D vertex2 = objData.Vertices[v2[0] - 1];
                                Vector3D vertex3 = objData.Vertices[v3[0] - 1];

                                Triangle triangle = new Triangle(vertex1, vertex2, vertex3, material);
                                objData.Triangles.Add(triangle);
                            }
                        }

                        else if (parts.Length > 4)
                        {
                            List<int[]> vertexIndices = new List<int[]>();
                            for (int i = 1; i < parts.Length; i++)
                            {
                                vertexIndices.Add(ParseFaceIndices(parts[i]));
                            }

                            for (int i = 1; i < vertexIndices.Count - 1; i++)
                            {
                                int[] v1 = vertexIndices[0];
                                int[] v2 = vertexIndices[i];
                                int[] v3 = vertexIndices[i + 1];

                                if (v1[0] > 0 && v2[0] > 0 && v3[0] > 0)
                                {
                                    Vector3D vertex1 = objData.Vertices[v1[0] - 1];
                                    Vector3D vertex2 = objData.Vertices[v2[0] - 1];
                                    Vector3D vertex3 = objData.Vertices[v3[0] - 1];

                                    Triangle triangle = new Triangle(vertex1, vertex2, vertex3, material);
                                    objData.Triangles.Add(triangle);
                                }
                            }
                        }
                    }
                }
            }

            List<Triangle> transformedTriangles = new List<Triangle>();

            float rotX = rotation.X * MathF.PI / 180f;
            float rotY = rotation.Y * MathF.PI / 180f;
            float rotZ = rotation.Z * MathF.PI / 180f;

            foreach (var triangle in objData.Triangles)
            {
                Vector3D v1 = TransformVertex(triangle.V1, scale, rotX, rotY, rotZ, position);
                Vector3D v2 = TransformVertex(triangle.V2, scale, rotX, rotY, rotZ, position);
                Vector3D v3 = TransformVertex(triangle.V3, scale, rotX, rotY, rotZ, position);

                Triangle transformedTriangle = new Triangle(v1, v2, v3, triangle.Material);
                transformedTriangles.Add(transformedTriangle);
            }

            return transformedTriangles;
        }

        private int[] ParseFaceIndices(string indexString)
        {
            string[] indices = indexString.Split('/');
            int[] result = new int[3] { -1, -1, -1 };

            if (indices.Length >= 1 && !string.IsNullOrEmpty(indices[0]))
                int.TryParse(indices[0], out result[0]);

            if (indices.Length >= 2 && !string.IsNullOrEmpty(indices[1]))
                int.TryParse(indices[1], out result[1]);

            if (indices.Length >= 3 && !string.IsNullOrEmpty(indices[2]))
                int.TryParse(indices[2], out result[2]);

            return result;
        }

        private Vector3D TransformVertex(Vector3D vertex, float scale, float rotX, float rotY, float rotZ, Vector3D position)
        {
            Vector3D scaled = new Vector3D(
                vertex.X * scale,
                vertex.Y * scale,
                vertex.Z * scale
            );

            float y1 = scaled.Y * MathF.Cos(rotX) - scaled.Z * MathF.Sin(rotX);
            float z1 = scaled.Y * MathF.Sin(rotX) + scaled.Z * MathF.Cos(rotX);

            float x2 = scaled.X * MathF.Cos(rotY) + z1 * MathF.Sin(rotY);
            float z2 = -scaled.X * MathF.Sin(rotY) + z1 * MathF.Cos(rotY);

            float x3 = x2 * MathF.Cos(rotZ) - y1 * MathF.Sin(rotZ);
            float y3 = x2 * MathF.Sin(rotZ) + y1 * MathF.Cos(rotZ);

            return new Vector3D(
                x3 + position.X,
                y3 + position.Y,
                z2 + position.Z
            );
        }
    }
}