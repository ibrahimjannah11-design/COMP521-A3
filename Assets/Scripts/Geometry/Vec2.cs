using System;

namespace Sim
{
    // Custom 2d struct to not use Unity, might change back to Unity's later !!
    public struct Vec2
    {
        public float x, y;
        public Vec2(float x, float y) { this.x = x; this.y = y; }

        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.x + b.x, a.y + b.y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.x - b.x, a.y - b.y);
        public static Vec2 operator *(Vec2 a, float s) => new Vec2(a.x * s, a.y * s);
        public float Magnitude => (float)Math.Sqrt(x * x + y * y);
        public float SqrMagnitude => x * x + y * y;
        public static float Distance(Vec2 a, Vec2 b) => (a - b).Magnitude;
        public static float Dot(Vec2 a, Vec2 b) => a.x * b.x + a.y * b.y;
        public Vec2 Normalized { get { float m = Magnitude; return m > 1e-6f ? new Vec2(x / m, y / m) : new Vec2(0, 0); } }
        public override string ToString() => $"({x:F2}, {y:F2})";

#if UNITY_5_3_OR_NEWER
        public static implicit operator Vec2(UnityEngine.Vector2 v) => new Vec2(v.x, v.y);
        public static implicit operator UnityEngine.Vector2(Vec2 v) => new UnityEngine.Vector2(v.x, v.y);
        /// <summary>Convert to a world position on the ground plane (Unity x/z).</summary>
        public UnityEngine.Vector3 ToWorld(float height = 0f) => new UnityEngine.Vector3(x, height, y);
#endif
    }
}
