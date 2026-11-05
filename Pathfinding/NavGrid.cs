using System;
using System.Collections.Generic;

namespace Sim
{
    public struct GridPos : IEquatable<GridPos>
    {
        public int x, y;
        public GridPos(int x, int y) { this.x = x; this.y = y; }
        public bool Equals(GridPos o) => x == o.x && y == o.y;
        public override bool Equals(object o) => o is GridPos g && Equals(g);
        public override int GetHashCode() => x * 73856093 ^ y * 19349663;
        public override string ToString() => $"[{x},{y}]";
    }

    
    // Uniform 2D grid w 3 layers:static (houses), climate (terrain type), agents (dynamic blockers).
    
    public class NavGrid
    {
        public readonly int Width, Height;
        public readonly float CellSize;
        public readonly SimConfig Cfg;

        bool[] staticBlocked;       // houses
        ClimateType[] terrain;      // from rasterized regions
        short[] agentCount;         // number of agents occupying each cell

        public NavGrid(SimConfig cfg)
        {
            Cfg = cfg;
            CellSize = cfg.cellSize;
            Width = (int)Math.Ceiling(cfg.arenaWidth / CellSize);
            Height = (int)Math.Ceiling(cfg.arenaHeight / CellSize);
            staticBlocked = new bool[Width * Height];
            terrain = new ClimateType[Width * Height];
            agentCount = new short[Width * Height];
        }

        int Idx(GridPos p) => p.y * Width + p.x;
        public bool InBounds(GridPos p) => p.x >= 0 && p.y >= 0 && p.x < Width && p.y < Height;
        public GridPos WorldToCell(Vec2 w) => new GridPos((int)(w.x / CellSize), (int)(w.y / CellSize));
        public Vec2 CellToWorld(GridPos p) => new Vec2((p.x + 0.5f) * CellSize, (p.y + 0.5f) * CellSize);

        public void SetStaticBlocked(GridPos p, bool v) { if (InBounds(p)) staticBlocked[Idx(p)] = v; }
        public void SetTerrain(GridPos p, ClimateType t) { if (InBounds(p)) terrain[Idx(p)] = t; }
        public ClimateType TerrainAt(GridPos p) => terrain[Idx(p)];
        public void ClearTerrain() => Array.Clear(terrain, 0, terrain.Length);

        public float SpeedMultiplier(GridPos p, bool isBot) => terrain[Idx(p)].SpeedMultiplier(isBot, Cfg);

        
        public bool IsWalkable(GridPos p, bool isBot, bool ignoreAgents = false)
        {
            if (!InBounds(p)) return false;
            int i = Idx(p);
            if (staticBlocked[i]) return false;
            if (terrain[i] == ClimateType.Fire) return false;
            if (!ignoreAgents && agentCount[i] > 0) return false;
            return true;
        }

        
        //This function updates the agentCount array to reflect the current positions of characters and delivery bots on the grid. 
        // Count and not bool to allow 2 agents briefly shar ecell
        public void RefreshAgentLayer(IEnumerable<Character> chars, IEnumerable<DeliveryBot> bots)
        {
            
            // First clear the agentCount array
            Array.Clear(agentCount, 0, agentCount.Length);
          
            // Then increment the count for each character's position
            foreach (var c in chars)
            {
                GridPos cellPos = WorldToCell(c.Position);
                if (InBounds(cellPos))
                {
                    agentCount[Idx(cellPos)]++;
                }
            }

            // Then increment the count for each bot's position
            foreach (var b in bots)
            {
                GridPos cellPos = WorldToCell(b.Position);
                if (InBounds(cellPos))
                {
                    agentCount[Idx(cellPos)]++;
                }
            }
        }

        //This function returns neighbors of given cell, needed for A* impleentation
        public  List<GridPos> Neighbors(GridPos p)
        {
            List<GridPos> neighbors = new List<GridPos>();
            for (int x = -1; x<= 1; x++)
            {
                for (int y = -1; y<=1; y++)
                {
                    if (y == 0 && x == 0)       // skip self
                    {
                        continue;
                    }

                    var neighbor = new GridPos(p.x+x, p.y + y);
                    if (!InBounds(neighbor))           // out of bounds
                    {
                        continue;
                    }

                    bool diagonal = x != 0 && y != 0;
                    if (diagonal)
                    {
                        // Check if diagonal move is possible (no corner cutting)

                        var side1 = new GridPos(p.x+x,p.y);
                        var side2 = new GridPos(p.x,p.y+y);

                        if (!IsWalkable(side1, false, true) || !IsWalkable(side2, false, true))
                        {
                            continue;           // not possible
                        }

                    }

                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }
        
    }
}
