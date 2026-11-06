using System;

namespace Sim
{
    public static class PolygonFactory
    {
        
        public static ConvexPolygon Regular(Vec2 center, float radius, int sides, float rotation)
        {
            var verts = new Vec2[sides];
            for (int i = 0; i < sides; i++)
            {
                float a = rotation + i * 2f * (float)Math.PI / sides;
                verts[i] = new Vec2(center.x + radius * (float)Math.Cos(a), center.y + radius * (float)Math.Sin(a));
            }
            return new ConvexPolygon(verts);
        }


        public static ConvexPolygon Random(SimConfig cfg)
        {
            int sides = MyRandom.Range(3, 7);
            float radius = MyRandom.Range(cfg.regionRadiusMin, cfg.regionRadiusMax);
            float rot = MyRandom.Range(0f, 6.2831853f);
            var center = new Vec2(MyRandom.Range(0f, cfg.arenaWidth), MyRandom.Range(0f, cfg.arenaHeight));
            return Regular(center, radius, sides, rot);
        }
    }
}
