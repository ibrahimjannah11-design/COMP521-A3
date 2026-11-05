namespace Sim   // DINE
{
    public enum CharacterState { Idle, WaitingBeforeOrder, WaitingForDelivery, SeekingPlant, InPlant }

    public class Character : Agent
    {
        public CharacterState State = CharacterState.Idle;
        public Order ActiveOrder;
        public ClimateRegion TargetPlant;
        public bool InPlant;
        float waitTimer;
        float plantCheckTimer;
        OrderManager orders; 
        ClimateManager climate;
        readonly HashSet<ClimateRegion> unreachable = new HashSet<ClimateRegion>();     // plants we couldnt find a path to

        public Character(SimConfig cfg, NavGrid grid, PathRequestManager paths, Stats stats, Vec2 pos)
            : base(cfg, grid, paths, stats, pos, cfg.agentRadius, false) { }


        public void Init(OrderManager orders, ClimateManager climate)
        {
            this.orders = orders;
            this.climate = climate;
            State = CharacterState.WaitingBeforeOrder;
            waitTimer = MyRandom.Range(cfg.waitMin, cfg.waitMax);
            plantCheckTimer = MyRandom.Range(0f, cfg.plantSeekInterval);       // so not every character checks on the same frame
        }


        public override void Tick(float dt)
        {
            base.Tick(dt);
           

            // there are 2 cases:
            // 1. We are waiting before placing an order
            // 2. We are waiting for a delivery

            // case 1
            if (State == CharacterState.WaitingBeforeOrder)
            {
                waitTimer -= dt;
                if (waitTimer <= 0)
                {
                    // Ask OrderManager to place an order
                
                    
                    
                    orders.PlaceOrder(this);
                    State = CharacterState.WaitingForDelivery;
                }
            }
            // case 2: timeout handled by order manager


            // plant seeking runs in both cases (we just keep waiting wherever we end up standing)

            // if the plant we were going to / standing in disappeared, forget about it
            if (TargetPlant != null && !climate.Regions.Contains(TargetPlant))
            {
                LeavePlant();
            }

            plantCheckTimer -= dt;
            if (plantCheckTimer <= 0)
            {
                plantCheckTimer = cfg.plantSeekInterval; // Reset the timer
                if (TargetPlant == null)
                {
                    TrySeekPlant();
                }
            }

        }


        // helper func for plant region (or technically anyregion but whatevr)
        static Vec2 Centroid(ClimateRegion r)
        {
            Vec2 sum = new Vec2(0, 0);
            foreach (var v in r.Shape.Vertices) sum = sum + v;
            return sum * (1f / r.Shape.Vertices.Length);
        }
            

        // Find closest plant region with capacity, thenfind path
        void TrySeekPlant()
        {
            unreachable.RemoveWhere(r => !climate.Regions.Contains(r));     // forget plants that expired

            ClimateRegion closestPlant = null;
            float closestDistance = float.MaxValue;

            foreach (var plant in climate.Regions)
            {
                if (plant.Type != ClimateType.Plant || !plant.HasCapacity || unreachable.Contains(plant)) continue;

                float distance = (Centroid(plant) - Position).SqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlant = plant;
                }
            }

            if (closestPlant == null) return;

            // Path to its interior and register occuptat (we take the slot now so 2 characters cant both claim the last one)
            closestPlant.Occupants.Add(this);
            TargetPlant = closestPlant;
            ClimateRegion target = closestPlant;

            
            RequestPath(c => target.Shape.Contains(grid.CellToWorld(c)), grid.WorldToCell(Centroid(target)), PathPriority.Normal, path =>
            {
                if (path == null)
                {
                    // no feasible path
                    target.Occupants.Remove(this);
                    unreachable.Add(target);
                    if (TargetPlant == target)
                    {
                        TargetPlant = null;
                        StopMoving();
                    }
                }
            });
        }

        // gives up the plant (it expired)   We stay where we are until the next plant check
        void LeavePlant()
        {
            TargetPlant = null;
            InPlant = false;
            StopMoving();
        }

        // we reached our goal
        protected override void OnArrived()
        {
            if (TargetPlant != null) InPlant = true;
        }

        // Called by OrderManager when the bot delivers
        public void OnOrderDelivered()
        {
            ActiveOrder = null;
            State = CharacterState.WaitingBeforeOrder;
            waitTimer = MyRandom.Range(cfg.waitMin, cfg.waitMax);       // wait 1-3s then order again
        }

        // Called by OrderManager on 10s timeout
        public void OnOrderCancelled()
        {
            ActiveOrder = null;
            State = CharacterState.WaitingBeforeOrder;
            waitTimer = MyRandom.Range(cfg.waitMin, cfg.waitMax);       // wait 1-3s then order again
        }
    }
}


