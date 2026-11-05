using System;

namespace Sim   //configs for testifnd awsignemnt
{
   
    [Serializable]
    public class SimConfig
    {
        // World
        public int n = 10;                    // number of characters = houses = bots
        public float arenaWidth = 100f;
        public float arenaHeight = 100f;
        public float cellSize = 1f;           // agent diameter
        public int seed = 12345;

        // Sizes (fixed)
        public float agentRadius = 0.4f;
        public float botRadius = 0.3f;
        public float houseSize = 3f;

        // Movement
        public float baseSpeed = 5f;

        // Climate
        public int regionCount = 10;
        public float regionLifetimeMin = 3f, regionLifetimeMax = 5f;
        public float regionRadiusMin = 4f, regionRadiusMax = 10f;
        public int plantCapacity = 3;
        public int plantCapacityMax = 5;
        public int plantCapacityMin = 1;
        public float fireSpeed = 0f, floodSpeed = 0.25f, desertSpeed = 0.5f, plantSpeed = 1.5f;

        // Orders
        public float pauseMin = 0.3f, pauseMax = 0.5f;
        public float waitMin = 1f, waitMax = 3f;
        public float orderTimeout = 10f;
        public float plantSeekInterval = 1f;

        // Pathfinding
        public int maxSearchesPerFrame = 8;
        public int maxNodesPerSearch = 20000;
        public float stuckTimeout = 1.5f;

        // Experiments
        public float runDuration = 60f;       // seconds for a timed run
    }
}
