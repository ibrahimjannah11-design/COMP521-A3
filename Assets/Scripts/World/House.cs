using System.Collections.Generic;

namespace Sim
{
    public class House
    {
        public Vec2 Position;
        public Rect2D Footprint;

        public House(Vec2 pos, float size)
        {
            Position = pos;
            Footprint = Rect2D.FromCenter(pos, size / 2f, size / 2f);
        }


        public bool IsAdjacentCell(NavGrid grid, GridPos c)
        // returns twrue if c is adjacent to house
        {
            Vec2 center = grid.CellToWorld(c);
            return !Footprint.Contains(center) && Footprint.Inflate(grid.CellSize).Contains(center);
        }
    }
}
