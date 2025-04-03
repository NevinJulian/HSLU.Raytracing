using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IRaycastable
    {
        (bool hasHit, float intersectionDistance) Intersect(Ray ray);

        Vector3D GetNormal(Vector3D intersectionPoint);

        MyColor Color { get; }
    }
}
