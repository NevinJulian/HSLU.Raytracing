namespace Common
{
    public class HitInfo
    {
        public IRaycastable Object { get; }
        public Vector3D HitPoint { get; }
        public Vector3D Normal { get; }
        public float Distance { get; }
        public int ObjectId => Object.ObjectId;


        public HitInfo(IRaycastable obj, Vector3D hitPoint, Vector3D normal, float distance)
        {
            Object = obj;
            HitPoint = hitPoint;
            Normal = normal;
            Distance = distance;
        }
    }
}