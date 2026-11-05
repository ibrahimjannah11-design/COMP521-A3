using System.Collections.Generic;

namespace Sim
{
    public class Path
    {
        public List<Vec2> Waypoints = new List<Vec2>();
        public int Index;                                   // next waypoint to reach
        public bool Finished => Index >= Waypoints.Count;
        public Vec2 Current => Waypoints[Index];


        public bool IsStillValid(NavGrid grid, bool isBot, GridPos selfCell, int lookahead = 8)
        {
            for (int i = Index; i < Waypoints.Count && i < Index + lookahead; i++)
            {
                GridPos cell = grid.WorldToCell(Waypoints[i]);
                bool self = (cell.x == selfCell.x && cell.y == selfCell.y);
                if (!grid.IsWalkable(cell, isBot, ignoreAgents: self))      // Added to fix bug where path can invalidate itself if the agent is standing on a cell that becomes blocked
                {
                    return false;
                }
            }
            return true;
        }
    }
}
