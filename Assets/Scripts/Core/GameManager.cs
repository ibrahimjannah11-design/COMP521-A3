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
        List<GameObject> climateObjects = new List<GameObject>();
        float elapsed;

        void Start() { Restart(); }

        // restarts scene (lol obviouslys)
        public void Restart()
        {

            //(FIXED) old scene not deleting?\
            foreach (var v in FindObjectsOfType<AgentView>())
            {
                Destroy(v.gameObject);
            }

            MyRandom.Seed(config.seed);

            stats = new Stats();
            elapsed = 0f;

            grid = new NavGrid(config);

            paths = new PathRequestManager(
                config,
                grid,
                stats);

            WorldSpawner spawner =
                new WorldSpawner(config, grid);

            spawner.Spawn(
                paths,
                stats,
                out houses,
                out characters,
                out bots);

            climate = new ClimateManager(config);

            orders = new OrderManager(
                config,
                houses,
                characters,
                bots,
                paths,
                stats);

            foreach (var c in characters)
            {
                c.Init(orders, climate);

                if (characterPrefab != null)
                {
                    GameObject go =
                        Instantiate(characterPrefab);

                    var view =
                        go.AddComponent<AgentView>();

                    view.Agent = c;
                }
            }

            foreach (var b in bots)
            {
                if (botPrefab != null)
                {
                    GameObject go =
                        Instantiate(botPrefab);

                    var view =
                        go.AddComponent<AgentView>();

                    view.Agent = b;
                }
            }

            foreach (var h in houses)
            {
                if (housePrefab != null)
                {
                    Vector3 pos =
                        h.Position.ToWorld(0.5f);

                    Instantiate(
                        housePrefab,
                        pos,
                        Quaternion.identity);
                }
            }
        }

        void Update()
        {
           if (paused){
                return;
           }

            float dt = Time.deltaTime * timeScale;

            elapsed += dt;

            bool changed = climate.Tick(dt);

            if (changed)
            {
                ClimateRasterizer.Apply(
                    grid,
                    climate.Regions);

                foreach(var go in climateObjects)
                    Destroy(go);

                climateObjects.Clear();

                foreach(var region in climate.Regions)
                {
                    GameObject go =
                        new GameObject("ClimateRegion");

                    climateObjects.Add(go);

                    var view =
                        go.AddComponent<ClimateRegionView>();

                    Material mat =
                        new Material(
                            Shader.Find("Universal Render Pipeline/Unlit"));

                    switch(region.Type)
                    {
                        case ClimateType.Fire:
                            mat.color = new Color(1f, 0f, 0f, 0.25f);
                            break;

                        case ClimateType.Flood:
                            mat.color = new Color(0f, 0f, 1f, 0.25f);
                            break;

                        case ClimateType.Desert:
                            mat.color = new Color(0.5f, 0.5f, 0.5f, 0.25f);
                            break;

                        case ClimateType.Plant:
                            mat.color = new Color(0f, 1f, 0f, 0.025f);
                            break;
                    }

                    view.Init(region, mat);
                }
            }

            orders.Tick(dt);

            paths.Process();

            foreach (var c in characters)
            {
                c.Tick(dt);
            }

            foreach (var b in bots)
            {
                b.Tick(dt);
            }

            grid.RefreshAgentLayer(
                characters,
                bots);

            if (elapsed >= config.runDuration)
            {
                paused = true;

                stats.AppendCsv(
                    "results.csv",
                    config.n,
                    config.seed,
                    elapsed);
            }
        }
    }
}
