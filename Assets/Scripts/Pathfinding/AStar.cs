using System;
using System.Collections.Generic;

namespace Sim   //DONE
{
    //OK this is the big one !!!
    // I picked A* just bc its the most common alg i think
    
    public class AStar
    {
        readonly NavGrid grid;
        readonly MinHeap<GridPos> open = new MinHeap<GridPos>();
      
        readonly float[] gCost;
        readonly int[] parent, visitStamp, closedStamp;
        private int searchId = 0;
        private int width;
        private int height;


        public int LastNodesExpanded { get; private set; }

        public AStar(NavGrid grid) { 
            this.grid = grid; 
            width = grid.Width;
            height = grid.Height;
            
            int totalCells = width * height;
            gCost = new float[totalCells];
            parent = new int[totalCells];
            visitStamp = new int[totalCells];
            closedStamp = new int[totalCells];
        }

        int ToIndex(GridPos p) => p.y * width + p.x;
        GridPos FromIndex(int idx) => new GridPos(idx % width, idx / width);

       
        public Path FindPath(GridPos start, Func<GridPos, bool> isGoal, GridPos heuristicTarget,
                             bool isBot, int maxNodes)
        {
            LastNodesExpanded = 0;

            // Not walkable or out of bounds, return null
            if (!grid.InBounds(start) || !grid.InBounds(heuristicTarget))
            {
                return null;
            }

            searchId++;
            if (searchId == int.MaxValue) // wrap around
            {
                Array.Clear(visitStamp, 0, visitStamp.Length);
                Array.Clear(closedStamp, 0, closedStamp.Length);
                searchId = 1;
            }

            open.Clear();

            int startIdx = ToIndex(start);
            gCost[startIdx] = 0;
            parent[startIdx] = -1;
            visitStamp[startIdx] = searchId;
            open.Push(start, Heuristic(start, heuristicTarget, isBot));

            
            while (open.Count > 0)
            {
                GridPos current = open.Pop(out _);     // (FIXED) changed from float currentFCost to _ to avoid unused warning (which sometimes caused errors idk why)

                int currentIdx = ToIndex(current);
                if (closedStamp[currentIdx] == searchId)
                {
                    continue; // Skip if already closed
                }

                closedStamp[currentIdx] = searchId;  

                if (isGoal(current))
                {
                    return ReconstructPath(currentIdx);
                }

                if (++LastNodesExpanded >= maxNodes)
                {
                    return null;
                }
                


                foreach (GridPos neighbor in grid.Neighbors(current))
                {
                    if (!grid.IsWalkable(neighbor, isBot))
                    {
                        continue;
                    }

                    int neighborIdx = ToIndex(neighbor);
                    if (closedStamp[neighborIdx] == searchId)
                    {
                        continue; // Skip if already closed
                    }

                    // !! (FIXED) formula needs reworking
                    bool isDiagonal = neighbor.x != current.x && neighbor.y != current.y;
                    float step = (isDiagonal ? (float)Math.Sqrt(2) : 1f) * grid.CellSize / grid.SpeedMultiplier(neighbor, isBot);
                    float tentativeGCost = gCost[currentIdx] + step;

                    


                    if (visitStamp[neighborIdx] == searchId && tentativeGCost >= gCost[neighborIdx])    // might be inverted (FIXED, it was before)
                    {
                       continue; // Not a better path
                    }

                    gCost[neighborIdx] = tentativeGCost;
                    parent[neighborIdx] = currentIdx;
                    visitStamp[neighborIdx] = searchId;
                    float fCost = tentativeGCost + Heuristic(neighbor, heuristicTarget, isBot);
                    open.Push(neighbor, fCost);
                }

            }

            return null;
        }

        float Heuristic(GridPos a, GridPos b, bool isBot)
        {
            float x = Math.Abs(a.x - b.x);
            float y = Math.Abs(a.y - b.y);

            float min = Math.Min(x, y);
            float max = Math.Max(x, y);

            float bestSpeed = isBot ? 1f : grid.Cfg.plantSpeed; 
            float octileDistance = (float)(Math.Sqrt(2) * min + (max - min)) * grid.CellSize / bestSpeed;
            return octileDistance;
        }

        private Path ReconstructPath(int endIdx)
        {
            var cells = new List<int>();
            for (int i = endIdx; i != -1; i = parent[i])
            {
                cells.Add(i);
            }
            cells.Reverse();

            var path = new Path();
            for (int i = 1; i < cells.Count; i++)
            {
                path.Waypoints.Add(grid.CellToWorld(FromIndex(cells[i])));
            }
            return path;
        }
    }
}
