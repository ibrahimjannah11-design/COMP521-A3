using System;

namespace Sim
{
    public enum PathPriority { Low = 0, Normal = 1, FireEscape = 2 }

    public class PathRequest
    {
        public Agent Requester;
        public Vec2 StartWorld;
        public Func<GridPos, bool> IsGoal;
        public GridPos HeuristicTarget;
        public PathPriority Priority;
        public float TimeCreated;
        public bool Cancelled;                 // set when the order is cancelled / requester no longer cares
        public Action<Path> OnComplete;        // null path leads to failure
    }
}
