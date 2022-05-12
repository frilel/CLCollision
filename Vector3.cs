/*===================================================================================================

    Author	:	Christian Kenneth Karl Lindberg
    E-Mail	:	ckkli@kth.se
    Brief	:	An introductory Vector class.
    Note	:	Written by the author as part of a course assignment at KTH University in Stockholm.

===================================================================================================*/
using System;

namespace CLCollision
{
    public class Vector3
    {
        // VARIABLES
        public float x;
        public float y;
        public float z;

        public Vector3 Zero { get => new Vector3(0.0f, 0.0f, 0.0f); private set {;} }
        public Vector3 Up { get => new Vector3(0.0f, 1.0f, 0.0f); private set {;} }

        // CONSTRUCTORS
        public Vector3() => x = y = z = 0.0f;

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

            if (magnitude < float.Epsilon)
                return;

            x /= magnitude;
            y /= magnitude;
            z /= magnitude;
        }

        public float Dot(Vector3 other) => (x * other.x) + (y * other.y) + (z * other.z);

        public Vector3 Cross(Vector3 other)
        {
            return new Vector3
            {
                x = (y * other.z) - (z * other.y),
                y = (z * other.x) - (x * other.z),
                z = (x * other.y) - (y * other.x)
            }; ;
        }


    }
}// namespace
