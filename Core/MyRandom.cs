using System;

namespace Sim   //DONE
{
    // Custom random number generator to not use Unity, might change back to Unity's later !!
    public static class MyRandom
    {
        static Random r = new Random(0);
        public static void Seed(int seed) { r = new Random(seed); }
        public static float Range(float min, float max) => min + (float)r.NextDouble() * (max - min);
        public static int Range(int minInclusive, int maxExclusive) => r.Next(minInclusive, maxExclusive);
        public static Vec2 PointIn(Rect2D rect) => new Vec2(Range(rect.MinX, rect.MaxX), Range(rect.MinY, rect.MaxY));
    }
}
