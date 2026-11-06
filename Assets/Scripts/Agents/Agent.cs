using System;
namespace Sim   //DONE 
{
    //Base class for anything that paths and moves (aka Character, DeliveryBot)
    public abstract class Agent
    {
        public Vec2 Position;
        public float Radius;
        public bool IsBot;
        public Path CurrentPath;
        public bool PathPending;            // a request is in the queue
        protected SimConfig cfg;
        protected NavGrid grid;
        protected PathRequestManager paths;
        protected Stats stats;
        float stuckTimer;
        bool escaping;
        Vec2 lastCheckPos;

        protected Agent(SimConfig cfg, NavGrid grid, PathRequestManager paths, Stats stats, Vec2 pos, float radius, bool isBot)
        {
            this.cfg = cfg; this.grid = grid; this.paths = paths; this.stats = stats;
            Position = pos; Radius = radius; IsBot = isBot;
        }

        // Variables needed for pathfinding
        Func<GridPos,bool> goalTest;
         GridPos goalHeuristic; 
         PathPriority goalPriority; 
         Action<Path> goalOnDone;
         PathRequest pendingRequest;

        public virtual void Tick(float dt)
        {
            // 1. if standing in fire: request FireEscape path (highest priority)
            // 2. follow CurrentPath (FollowPath)
            // 3. stuck detection\

            //bool esaping = false;    NOT NEEDEDF ANYMORE
            if (InFire() && !escaping)           // If in fire, find path to safety (highest priotity)
            {
                 escaping = true;

                RequestPath(
                grid.IsSafe,
                grid.WorldToCell(Position),
                PathPriority.FireEscape,
                p =>
                {
                    escaping = false;
                    CurrentPath = p;
                });
            }
            else if (CurrentPath != null)       // If we have a current path, follow it
            {
                FollowPath(dt);
            }

            // Check if we're stuck (no progress for cfg.stuckTimeout seconds)
            CheckStuck(dt);
        }

        // Helper function to create and enquue requests (replace copypasted code marked w !!)
        void Enqueue(Func<GridPos,bool> isGoal, GridPos target, PathPriority pri, Action<Path> cb)
        {
            if (pendingRequest != null)
            {
                pendingRequest.Cancelled = true;
            }
            var req = new PathRequest { 
                Requester = this, StartWorld = Position, IsGoal = isGoal,
                                        HeuristicTarget = target, Priority = pri
            };
            req.OnComplete = p => { 
                if (req.Cancelled) return; 
                pendingRequest = null;
                PathPending = false; cb(p); };
            pendingRequest = req; PathPending = true;
            paths.Enqueue(req);
        }

        // called after a*
        void HandleResult(Path p, Action<Path> onDone)
        {
            if (p == null)
            {
                OnPathFailed();
            }
            else if (p.Waypoints.Count == 0)        // we were already standing at the goal
            {
                OnArrived();
            }
            else
            {
                CurrentPath = p;
                stuckTimer = 0f;
                lastCheckPos = Position;
            }
            onDone?.Invoke(p);              // whatever extra thing the caller wanted to do with the result
        }

        // finds new path
        protected void Repath()     
        {
            if (PathPending || escaping || goalTest == null) return;   
            stats.replans++;
            Enqueue(goalTest, goalHeuristic, goalPriority, p => HandleResult(p, goalOnDone));
        }

        protected void FollowPath(float dt) // (FIXED) rework this (no while so it dfoesnt telepot)
        {

            // adding safeg8uard tp avopid those woeird errors
            if (CurrentPath == null){
                return;
            }
            if (CurrentPath.Finished)
            {
                CurrentPath = null;
                OnArrived();
                return;
            }

            GridPos here = grid.WorldToCell(Position);

            if (!CurrentPath.IsStillValid(grid, IsBot, here))
            {
                // Request a new path if the current one is no longer valid
                CurrentPath = null;
                if (escaping)
                {
                    escaping = false;
                    if (!InFire()) Repath();
                }
                else
                {
                    Repath();
                }
                return;
            }

            
            float mult = InFire() ? 1f : grid.SpeedMultiplier(here, IsBot);
            float step = cfg.baseSpeed * mult * dt;

            var target = CurrentPath.Current;
            var toTarget = target - Position;
            float dist = toTarget.Magnitude;

            if (dist <= step) // close enough to reach the waypoint this frame
            {
                Position = target;              // snap to it so we never overshoot
                CurrentPath.Index++;
                if (CurrentPath.Finished)
                {
                        CurrentPath = null;
                        OnArrived();
                        return;
                }
            }
            else
            {
                Position = Position + toTarget * (step / dist);
            }
        
        }

        //build a pathrequst and enqueue it
        protected void RequestPath(System.Func<GridPos, bool> isGoal, GridPos heuristicTarget, PathPriority pri,
                                   System.Action<Path> onDone)
        {
          
            // Now create a new path request and enqueue it
            goalTest = isGoal; 
            goalHeuristic = heuristicTarget; 
            goalPriority = pri; 
            goalOnDone = onDone;
            Enqueue(isGoal, heuristicTarget, pri, p => HandleResult(p, onDone));


        }

        protected bool InFire() => grid.TerrainAt(grid.WorldToCell(Position)) == ClimateType.Fire;

        //if no progress for cfg.stuckTimeout seconds, repath (maybe with small random jitter)
        protected void CheckStuck(float dt)
        {
            if (CurrentPath != null)
            {
                // If we have a current path, check if we're making progress
                var target = CurrentPath.Current;
                if ((Position - target).Magnitude<0.1f) // threshold to consider "close enough"
                {
                    stuckTimer = 0f; // Reset timer if we're close to the target
                }
                else
                {
                    stuckTimer += dt;
                    if (stuckTimer > cfg.stuckTimeout)
                    {
                        // request a new path
                        Repath();
                        stats.replans++;
                        stuckTimer = 0f; // Reset timer after requesting a new path
                    }
                }
            }

        }
        
        protected virtual void OnArrived() { }

        protected virtual void OnPathFailed()
        {
            CurrentPath = null;
        }

        protected virtual void StopMoving()
        {
            CurrentPath = null;
        }
    }

}
