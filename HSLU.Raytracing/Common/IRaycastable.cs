namespace Common
{
    public interface IRaycastable
    {
        (bool hasHit, float intersectionDistance) Intersect(Ray ray);
        Vector3D GetNormal(Vector3D intersectionPoint);
        MyColor Color { get; }
        Material Material { get; } // Added to match Java's material system
        int ObjectId { get; set; }
    }
}