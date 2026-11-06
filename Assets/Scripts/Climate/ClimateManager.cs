using System;
using System.Collections.Generic;

namespace Sim       //DONE
{
    
    public class ClimateManager
    {
        public readonly List<ClimateRegion> Regions = new List<ClimateRegion>();
        public event Action<ClimateRegion> OnRegionAdded, OnRegionRemoved;
        readonly SimConfig cfg;

        public ClimateManager(SimConfig cfg) { this.cfg = cfg; }

        public bool Tick(float t)
        {
            //Returns true if any regions were added or removed, false otherwise
            bool changed = false;
            
            //  First remove expired regions
            for (int i = Regions.Count - 1; i >= 0; i--)
            {
                ClimateRegion region = Regions[i];
                region.TimeLeft -= t; 
                if (region.TimeLeft <= 0)
                {
                    // Remove the region
                    region.Occupants.Clear(); // Clear plant occupants
                    Regions.RemoveAt(i);
                    changed = true;
                    OnRegionRemoved?.Invoke(region);

                    
                }
            }

            // Then spawn new regions if needed
           
            while (Regions.Count < cfg.regionCount)        
            {
                var region = TrySpawn();
                if (region == null) break;
                Regions.Add(region);
                OnRegionAdded?.Invoke(region);
            
                changed = true;
            }
            return changed;
        }

        
        ClimateRegion TrySpawn()
        {
             // (FIXED) After certain number of tries, break to avoid infinite loop if we can't spawn more regions
            
            for (int attempt = 0; attempt < 10; attempt++)
            {
                bool overlap = false;
                // Randomly choose a climate type
                ClimateType type = (ClimateType)MyRandom.Range(1, 5); //
                // Generate a random convex polygon
                ConvexPolygon shape = PolygonFactory.Random(cfg);
                // Check for overlap with existing regions
                foreach (var r in Regions)
                {
                    if (r.Shape.Overlaps(shape))
                    {
                        overlap = true;
                        break; // Overlaps, reject this shape
                    }
                }

                if (overlap) continue;
                // If we reach here, the shape doesn't overlap with any existing region
                ClimateRegion newRegion = new ClimateRegion();
                newRegion.Shape = shape;
                newRegion.Type = type;
                newRegion.TimeLeft = MyRandom.Range(cfg.regionLifetimeMin, cfg.regionLifetimeMax);
                if (type == ClimateType.Plant)
                {
                    newRegion.Capacity = MyRandom.Range(cfg.plantCapacityMin, cfg.plantCapacityMax);
                }
                return newRegion;
            }

            return null;
        }

        public ClimateType TypeAt(Vec2 p)
        {
            foreach (var r in Regions) if (r.Shape.Bounds.Contains(p) && r.Shape.Contains(p)) return r.Type;
            return ClimateType.None;
        }
    }
}
