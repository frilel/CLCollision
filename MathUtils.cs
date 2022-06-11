/*===================================================================================================

    Author	:	Christian Kenneth Karl Lindberg
    E-Mail	:	ckkli@kth.se
    Brief	:	An introductory class of Math Utilites, mainly for this project.
    Note	:	Written by the author as part of a course project at KTH University in Stockholm.

===================================================================================================*/

namespace clCollision
{
    public static class MathUtils
    {
        public static float Dot(Vector3 a, Vector3 b) => (a.x*b.x) + (a.y*b.y) + (a.z*b.z);

        public static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3
        {
            x = (a.y* b.z) - (a.z* b.y),
            y = (a.z* b.x) - (a.x * b.z),
            z = (a.x * b.y) - (a.y * b.x)
        };

        public static Vector3 Normalize(Vector3 a)
        {
            float magnitude = (float)a.GetMagnitude();

            if (magnitude <= 0.0f)
                return a; // avoid division by zero

            a.x /= magnitude;
            a.y /= magnitude;
            a.z /= magnitude;

            return a;
        }

        public static UnityEngine.Vector3 ToUnityVec(this Vector3 other) 
            => new UnityEngine.Vector3(other.x, other.y, other.z);

        public static Vector3 ToCLVec(this UnityEngine.Vector3 other) 
            => new Vector3(other.x, other.y, other.z);
    }
}// namespace
