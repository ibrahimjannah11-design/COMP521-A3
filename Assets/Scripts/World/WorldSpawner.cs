using System.Collections.Generic;
using System;
namespace Sim
{
    //Places n houses, n characters, n bots with no initial overlap 
    public class WorldSpawner
    {
        readonly SimConfig cfg;
        readonly NavGrid grid;
        const int MaxAttempts = 20000;      // so a too-small arrea gives an error instead of freezing forever
        public WorldSpawner(SimConfig cfg, NavGrid grid) { this.cfg = cfg; this.grid = grid; }

        public void Spawn(PathRequestManager paths, Stats stats,
                          out List<House> houses, out List<Character> characters, out List<DeliveryBot> bots)
        {
            houses = new List<House>(); characters = new List<Character>(); bots = new List<DeliveryBot>();
    
    

            // Forst lets spawn houses
            float half = cfg.houseSize / 2f;
            for (int i = 0; i < cfg.n; i++)
            {
               House house = null;
                for (int attempt = 0; attempt < MaxAttempts && house == null; attempt++)
                {
                    // Randomly generate a position for the house (not too close to the edge of the arena)
                    Vec2 housePos = new Vec2(MyRandom.Range(half + 1, cfg.arenaWidth - half - 1), MyRandom.Range(half + 1, cfg.arenaHeight - half - 1));
                    House candidate = new House(housePos, cfg.houseSize);

                    // Check if it overlaps with another house (we keep a gap of 2 so there is room to walk between them)
                    bool validPos = true;
                    foreach (var other in houses)
                    {
                        if (candidate.Footprint.Inflate(2f).Overlaps(other.Footprint))
                        {
                            validPos = false;
                            break;
                        }
                    }
                    if (validPos) house = candidate;
                }
                if (house == null) throw new InvalidOperationException("Cannot place house " + i + ": arena is too small for n = " + cfg.n);

                houses.Add(house);
                // Mark every cell the house covers as blocked in the NavGrid
                BlockFootprint(house);
            }

            // Now lets spawn characters
            var taken = new List<(Vec2 pos, float radius)>();
            for (int i = 0; i < cfg.n; i++)
            {
                Vec2 charPos = FindFreePosition(houses, taken, cfg.agentRadius);
                taken.Add((charPos, cfg.agentRadius));
                Character character = new Character(cfg, grid, paths, stats, charPos);
                characters.Add(character);
            }

            // Now lets spawn bots
            for (int i = 0; i < cfg.n; i++)
            {
               Vec2 botPos = FindFreePosition(houses, taken, cfg.botRadius);
                taken.Add((botPos, cfg.botRadius));
                DeliveryBot bot = new DeliveryBot(cfg, grid, paths, stats, botPos);
                bots.Add(bot);
            
            }
        }



        // marks all the grid cells covered by the house as blocked 
        void BlockFootprint(House house)
        {
            GridPos min = grid.WorldToCell(new Vec2(house.Footprint.MinX, house.Footprint.MinY));
            GridPos max = grid.WorldToCell(new Vec2(house.Footprint.MaxX, house.Footprint.MaxY));
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    GridPos cell = new GridPos(x, y);
                    if (house.Footprint.Contains(grid.CellToWorld(cell)))
                    {
                        grid.SetStaticBlocked(cell, true);
                    }
                }
            }
        }

        // helper function (before i had a wqhile loop that was pretty inefectef this is betetr)
        Vec2 FindFreePosition(List<House> houses, List<(Vec2 pos, float radius)> taken, float radius)
        {
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                Vec2 pos = new Vec2(MyRandom.Range(1f, cfg.arenaWidth - 1f), MyRandom.Range(1f, cfg.arenaHeight - 1f));
                bool validPos = true;

                // Check if the position overlaps with any house
                foreach (var house in houses)
                {
                    if (house.Footprint.Inflate(radius + grid.CellSize).Contains(pos))
                    {
                        validPos = false;
                        break;
                    }
                }

                // Check if the position overlaps with any cgar or bot
                if (validPos)
                {
                    foreach (var t in taken)
                    {
                        if ((pos - t.pos).Magnitude < radius + t.radius + 0.5f)
                        {
                            validPos = false;
                            break;
                        }
                    }
                }

                if (validPos) return pos;
            }
            throw new InvalidOperationException("Cannot place agent: arena is too crowded for n = " + cfg.n);
        }
    }
}