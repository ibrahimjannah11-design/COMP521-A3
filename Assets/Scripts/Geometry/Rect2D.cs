namespace Sim
{
    public struct Rect2D
    {
        public float MinX, MinY, MaxX, MaxY;
        public Rect2D(float minX, float minY, float maxX, float maxY) { MinX = minX; MinY = minY; MaxX = maxX; MaxY = maxY; }
        public static Rect2D FromCenter(Vec2 c, float halfW, float halfH) => new Rect2D(c.x - halfW, c.y - halfH, c.x + halfW, c.y + halfH);
        public Vec2 Center => new Vec2((MinX + MaxX) * 0.5f, (MinY + MaxY) * 0.5f);
        public bool Contains(Vec2 p) => p.x >= MinX && p.x <= MaxX && p.y >= MinY && p.y <= MaxY;
        public bool Overlaps(Rect2D o) => MinX < o.MaxX && MaxX > o.MinX && MinY < o.MaxY && MaxY > o.MinY;
        public Rect2D Inflate(float m) => new Rect2D(MinX - m, MinY - m, MaxX + m, MaxY + m);
    }
}
