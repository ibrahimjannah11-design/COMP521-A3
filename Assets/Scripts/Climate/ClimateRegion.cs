using System.Collections.Generic;

namespace Sim
{
    public class ClimateRegion
    {
        public ConvexPolygon Shape;
        public ClimateType Type;
        public float TimeLeft;
        public int Capacity;                         // plants only
        public List<Character> Occupants = new List<Character>();

        public bool HasCapacity => Type == ClimateType.Plant && Occupants.Count < Capacity;
    }
}
