namespace Common;

public class Sphere : IRaycastable
{
    public Vector3D Center { get; }
    public MyColor Color { get; }
    public int Radius { get; }

    public Sphere(Vector3D center, int radius, MyColor color)
    {
        Center = center;
        Radius = radius;
        Color = color;
    }

    // Method to maintain backward compatibility with existing code
    public bool IsInSphere(Vector2D point)
    {
        var dx = point.X - Center.X;
        var dy = point.Y - Center.Y;
        return (dx * dx + dy * dy) <= Math.Pow(Radius, 2);
    }

    // New method for IRaycastable interface
    public (bool hasHit, float intersectionDistance) Intersect(Ray ray)
    {
        // Vector from ray origin to sphere center (equivalent to 'v' in professor's code)
        Vector3D oc = ray.Origin - Center;

        // Quadratic equation coefficients (a, b, c in professor's code)
        float a = ray.Direction.Dot(ray.Direction);
        float b = 2.0f * oc.Dot(ray.Direction);
        float c = oc.Dot(oc) - Radius * Radius;

        // Calculate discriminant
        float discriminant = b * b - 4 * a * c;

        // No intersection if discriminant is negative
        if (discriminant < 0)
            return (false, float.MaxValue);

        // Calculate the two intersection points
        float sqrt = (float)Math.Sqrt(discriminant);
        float t1 = (-b - sqrt) / (2 * a);
        float t2 = (-b + sqrt) / (2 * a);

        // Return the closest positive intersection
        if (t1 > 0)
            return (true, t1);
        else if (t2 > 0)
            return (true, t2);
        else
            return (false, float.MaxValue);
    }

    // Calculate normal at intersection point
    public Vector3D GetNormal(Vector3D intersectionPoint)
    {
        // Normal is the vector from center to intersection point, normalized
        return (intersectionPoint - Center).Normalize();
    }
}