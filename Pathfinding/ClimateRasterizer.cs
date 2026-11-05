using System;
using System.Collections.Generic;

namespace Sim   //DONE
{
    public static class ClimateRasterizer
    {
        // This class labels each cell grid to its appropriate region based on shaep 
        public static void Apply(NavGrid grid, List<ClimateRegion> regions)
        {
            grid.ClearTerrain();

            // For each region, loop over cells 
            foreach (var region in regions)
            {
                Rect2D b = region.Shape.Bounds;

                // Cell range covered by the polygon's bounding box, clamped to the grid
                GridPos min = grid.WorldToCell(new Vec2(b.MinX, b.MinY));
                GridPos max = grid.WorldToCell(new Vec2(b.MaxX, b.MaxY));
                int minX = Math.Max(min.x, 0);
                int minY = Math.Max(min.y, 0);
                int maxX = Math.Min(max.x, grid.Width - 1);
                int maxY = Math.Min(max.y, grid.Height - 1);


                // Label cells with centers in polygon

                for(int x = minX; x<= maxX; x++)
                {
                    for(int y = minY; y<= maxY; y++)
                    {
                        GridPos cell = new GridPos(x, y);
                        if (region.Shape.Contains(grid.CellToWorld(cell)))
                        {
                            grid.SetTerrain(cell, region.Type);
                        }
                    }
                }
            }
        }
    }
}
