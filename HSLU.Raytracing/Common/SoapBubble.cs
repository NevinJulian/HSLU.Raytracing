using System;
using Common;

namespace Common
{
    public class SoapBubble : IRaycastable
    {
        public Vector3D Center { get; }
        public MyColor Color { get; }
        public Material Material { get; }
        public float Radius { get; }
        public int ObjectId { get; set; }

        private readonly float age;
        private readonly float uniquenessFactor;
        private readonly float filmVariation;

        private readonly int bubbleId;
        private static int nextBubbleId = 0;

        private readonly float minThickness;
        private readonly float maxThickness;
        private readonly float drainageStrength;

        public SoapBubble(Vector3D center, float radius, Material material, float age = 0.5f,
                          float uniquenessFactor = 0.8f, float filmVariation = 0.7f)
        {
            Center = center;
            Radius = radius;
            Material = material;
            Color = material.Diffuse;
            this.age = age;
            this.uniquenessFactor = uniquenessFactor;
            this.filmVariation = filmVariation;
            this.bubbleId = nextBubbleId++;

            Random random = new Random(bubbleId * 1000 + 42);

            minThickness = 100.0f + (float)random.NextDouble() * 100.0f;
            maxThickness = 500.0f + (float)random.NextDouble() * 300.0f;
            drainageStrength = 0.3f + (float)random.NextDouble() * 0.7f;
        }

        public (bool hasHit, float intersectionDistance) Intersect(Ray ray)
        {
            Vector3D oc = ray.Origin - Center;
            float a = ray.Direction.Dot(ray.Direction);
            float b = 2.0f * oc.Dot(ray.Direction);
            float c = oc.Dot(oc) - Radius * Radius;

            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
                return (false, float.MaxValue);

            float sqrt = MathF.Sqrt(discriminant);
            float t1 = (-b - sqrt) / (2 * a);
            float t2 = (-b + sqrt) / (2 * a);

            if (t1 > 0.0001f)
                return (true, t1);
            else if (t2 > 0.0001f)
                return (true, t2);
            else
                return (false, float.MaxValue);
        }

        public Vector3D GetNormal(Vector3D intersectionPoint)
        {
            return (intersectionPoint - Center).Normalize();
        }

        public MyColor GetInterferenceColor(Vector3D intersectionPoint, Vector3D viewDirection)
        {
            Vector3D normal = GetNormal(intersectionPoint);
            float cosIncidence = MathF.Abs(normal.Dot(viewDirection));
            float thickness = CalculateFilmThickness((intersectionPoint - Center).Normalize());

            return CalculateSpectralBubbleColor(thickness, cosIncidence);
        }

        private float CalculateFilmThickness(Vector3D localPoint)
        {
            float height = 0.5f + 0.5f * localPoint.Y;
            float gravityFactor = MathF.Pow(1.0f - height, 1.0f + age * 2.0f);
            float baseThickness = minThickness + (maxThickness - minThickness) * gravityFactor * drainageStrength;

            float noise = 0.5f + 0.5f * MathF.Sin(localPoint.X * 17 + localPoint.Y * 13 + localPoint.Z * 19);
            float variation = noise * filmVariation * 40f;

            float finalThickness = baseThickness + variation;
            return Math.Clamp(finalThickness, minThickness * 0.5f, maxThickness * 1.5f);
        }

        private MyColor CalculateSpectralBubbleColor(float thickness, float cosIncidence)
        {
            float refractiveIndex = 1.33f;
            float opticalPath = 2.0f * refractiveIndex * thickness * cosIncidence;

            float wavelengthStart = 380f;
            float wavelengthEnd = 780f;
            int numSamples = 25;

            float rSum = 0, gSum = 0, bSum = 0;

            for (int i = 0; i < numSamples; i++)
            {
                float wavelength = wavelengthStart + (wavelengthEnd - wavelengthStart) * i / (numSamples - 1);
                float phase = 2 * MathF.PI * opticalPath / wavelength;
                float intensity = 0.5f + 0.5f * MathF.Cos(phase);

                MyColor baseColor = WavelengthToRGB(wavelength);
                rSum += baseColor.R * intensity;
                gSum += baseColor.G * intensity;
                bSum += baseColor.B * intensity;
            }

            rSum /= numSamples;
            gSum /= numSamples;
            bSum /= numSamples;

            return new MyColor(
                (int)Math.Clamp(rSum, 0, 255),
                (int)Math.Clamp(gSum, 0, 255),
                (int)Math.Clamp(bSum, 0, 255)
            );
        }

        private MyColor WavelengthToRGB(float wavelength)
        {
            float R = 0, G = 0, B = 0;

            if (wavelength >= 380 && wavelength <= 440)
            {
                R = -(wavelength - 440) / (440 - 380);
                G = 0;
                B = 1;
            }
            else if (wavelength <= 490)
            {
                R = 0;
                G = (wavelength - 440) / (490 - 440);
                B = 1;
            }
            else if (wavelength <= 510)
            {
                R = 0;
                G = 1;
                B = -(wavelength - 510) / (510 - 490);
            }
            else if (wavelength <= 580)
            {
                R = (wavelength - 510) / (580 - 510);
                G = 1;
                B = 0;
            }
            else if (wavelength <= 645)
            {
                R = 1;
                G = -(wavelength - 645) / (645 - 580);
                B = 0;
            }
            else if (wavelength <= 780)
            {
                R = 1;
                G = 0;
                B = 0;
            }

            float factor = 1.0f;
            if (wavelength >= 380 && wavelength < 420)
                factor = 0.3f + 0.7f * (wavelength - 380) / (420 - 380);
            else if (wavelength > 700 && wavelength <= 780)
                factor = 0.3f + 0.7f * (780 - wavelength) / (780 - 700);

            R = (float)Math.Clamp(R * factor, 0.0, 1.0);
            G = (float)Math.Clamp(G * factor, 0.0, 1.0);
            B = (float)Math.Clamp(B * factor, 0.0, 1.0);

            return new MyColor((int)(R * 255), (int)(G * 255), (int)(B * 255));
        }
    }
}
