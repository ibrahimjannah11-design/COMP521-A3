using System.Collections.Generic;
using UnityEngine;

namespace Sim   //NOT DONE -- SETUP IN UNITY
{

    public class GameManager : MonoBehaviour
    {
        public SimConfig config = new SimConfig();

        [Header("Prefabs")]
        public GameObject characterPrefab, housePrefab, botPrefab;

        [Header("Run control")]
        public bool paused;
        [Range(0.1f, 10f)] public float timeScale = 1f;

        public Stats stats { get; private set; }
        NavGrid grid;
        ClimateManager climate;
        PathRequestManager paths;
        OrderManager orders;
        List<House> houses;
        List<Character> characters;
        List<DeliveryBot> bots;
        float elapsed;

        void Start() { Restart(); }

        /// <summary>Tear down and rebuild the world (used by HUD for changing n).</summary>
        public void Restart()
        {
            // TODO: destroy old views/GameObjects
            Rng.Seed(config.seed);
            stats = new Stats();
            elapsed = 0f;

            // TODO: grid = new NavGrid(config);
            // TODO: paths = new PathRequestManager(config, grid, stats);
            // TODO: var spawner = new WorldSpawner(config, grid); spawner.Spawn(paths, stats, out houses, out characters, out bots);
            // TODO: climate = new ClimateManager(config);
            // TODO: orders = new OrderManager(config, houses, characters, bots, paths, stats);
            // TODO: create GameObjects from prefabs and attach AgentView to each agent
            // TODO: subscribe climate.OnRegionAdded/Removed to create/destroy ClimateRegionView
        }

        void Update()
        {
            if (paused) return;
            float dt = Time.deltaTime * timeScale;
            elapsed += dt;

            // Order matters. Keep it explicit.
            // 1. climate.Tick(dt);                          expire + spawn regions
            // 2. ClimateRasterizer.Apply(grid, climate.Regions);   update terrain layer
            // 3. invalidate paths hit by new fire; flag agents standing in fire
            // 4. orders.Tick(dt);                           new orders, timeouts, bot assignment
            // 5. paths.Process();                           run up to K searches
            // 6. foreach agent: Tick(dt)
            // 7. grid.RefreshAgentLayer(characters, bots);

            // if (elapsed >= config.runDuration) { stats.AppendCsv(...); }
        }
    }
}
