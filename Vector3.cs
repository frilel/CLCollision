/*===================================================================================================

    Author	:	Christian Kenneth Karl Lindberg
    E-Mail	:	ckkli@kth.se
    Brief	:	An introductory 3D Vector struct, written for practice. Since we're already using
                Unity components we could just use Unity's Vector3 struct.
    Note	:	Written by the author as part of a course project at KTH University in Stockholm.

===================================================================================================*/
using System;

namespace clCollision
{
    [Serializable]
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3 Zero     { get => new Vector3(0f, 0f, 0f); private set {; } }
        public Vector3 One      { get => new Vector3(1f, 1f, 1f); private set {; } }
        public Vector3 Up       { get => new Vector3(0f, 1f, 0f); private set {; } }
        public Vector3 Down     { get => new Vector3(0f, -1f, 0f); private set {; } }
        public Vector3 Right    { get => new Vector3(1f, 0f, 0f); private set {; } }
        public Vector3 Left     { get => new Vector3(-1f, 0f, 0f); private set {; } }


        // CONSTRUCTORS
        //public Vector3() => x = y = z = 0f;
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vector3(Vector3 start, Vector3 end)
        {
            x = end.x - start.x;
            y = end.y - start.y;
            z = end.z - start.z;
        }

        // OPERATORS
        public static Vector3 operator +(Vector3 a) => a;
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.x, -a.y, -a.z);
        public static Vector3 operator +(Vector3 a, Vector3 b)
            => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b)
            => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 a, Vector3 b)
            => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector3 operator *(Vector3 a, float scalar)
            => new Vector3(a.x * scalar, a.y * scalar, a.z * scalar);
        public static Vector3 operator *(float scalar, Vector3 a)
            => new Vector3(a.x * scalar, a.y * scalar, a.z * scalar);
        public static Vector3 operator /(Vector3 a, Vector3 b)
            => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

        // OPERATORS WITH UNITY VECTOR3
        public static Vector3 operator +(UnityEngine.Vector3 a, Vector3 b)
            => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(UnityEngine.Vector3 a, Vector3 b)
            => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(UnityEngine.Vector3 a, Vector3 b)
            => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector3 operator /(UnityEngine.Vector3 a, Vector3 b)
        => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

        public static Vector3 operator +(Vector3 a, UnityEngine.Vector3 b)
            => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, UnityEngine.Vector3 b)
            => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 a, UnityEngine.Vector3 b)
            => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector3 operator /(Vector3 a, UnityEngine.Vector3 b)
            => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

        // OVERRIDES
        public override string ToString() => $"({x}, {y}, {z})";

        // FUNCTIONS
        public double GetMagnitude() => Math.Sqrt((x * x) + (y * y) + (z * z));
        public double GetSqrMagnitude() => (x * x) + (y * y) + (z * z);
        
        public void SetMagnitude(float magnitude)
        {
            Normalize();
            x *= magnitude;
            y *= magnitude;
            z *= magnitude;
        }

        public void Normalize()
        {
            float magnitude = (float)GetMagnitude();

            if (magnitude <= 0.0f)
                return; // avoid division by zero

            x /= magnitude;
            y /= magnitude;
            z /= magnitude;
        }

        public float Dot(Vector3 other) => (x * other.x) + (y * other.y) + (z * other.z);

        public Vector3 Cross(Vector3 other) => new Vector3
        {
            x = (y * other.z) - (z * other.y),
            y = (z * other.x) - (x * other.z),
            z = (x * other.y) - (y * other.x)
        };

        public static UnityEngine.Vector3 ToUnity(Vector3 other) => new UnityEngine.Vector3(other.x, other.y, other.z);

        public static Vector3 ToVector3(UnityEngine.Vector3 other) => new Vector3(other.x, other.y, other.z);

    }//Vector3
}//namespace
