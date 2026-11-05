using System.Collections.Generic;

namespace Sim
{
    //Queues abd sevrves path requests
    public class PathRequestManager
    {
        readonly SimConfig cfg;
        readonly NavGrid grid;
        readonly Stats stats;
        readonly AStar astar;
        readonly List<PathRequest> queue = new List<PathRequest>();   // or one MinHeap per priority

        public PathRequestManager(SimConfig cfg, NavGrid grid, Stats stats)
        {
            this.cfg = cfg; this.grid = grid; this.stats = stats;
            astar = new AStar(grid);
        }

        public void Enqueue(PathRequest req)
        {
            
            queue.Add(req);
            stats.maxQueueLength = Mathf.Max(stats.maxQueueLength, queue.Count);
            stats.pathRequests++;
        }

         public void Enqueue(PathRequest req)
        {
            queue.Add(req);
            stats.maxQueueLength = Math.Max(stats.maxQueueLength, queue.Count);
            stats.pathRequests++;
        }

        public void Process()
        // wow throwback to comp310 (kinda)
        {

            int searches = 0;
            while (searches < cfg.maxSearchesPerFrame)
            {
                // First remove cancelled requests (these dont use up any of the search budget)
                for (int i = queue.Count - 1; i >= 0; i--)
                {
                    if (queue[i].Cancelled)
                    {
                        stats.pathsDroppedStale++;              // skip cancelled requests
                        queue.RemoveAt(i);
                    }
                }
                // Then check if there are any requests left in the queue
                if (queue.Count == 0) return;

                // is so pick highets priority 
                int best = 0;                                         // highest priority, then oldest
                for (int i = 1; i < queue.Count; i++)
                {
                                    
                    if (queue[i].Priority > queue[best].Priority) {
                        best = i;
                    }
                }
                PathRequest req = queue[best];
                queue.RemoveAt(best);

                // now process
                var sw = System.Diagnostics.Stopwatch.StartNew();
                Path path = astar.FindPath(grid.WorldToCell(req.Requester.Position), req.IsGoal, req.HeuristicTarget, req.Requester.IsBot, cfg.maxNodesPerSearch);
                sw.Stop();

                // thi s is for recording stats
                stats.RecordSearch(path != null, astar.LastNodesExpanded, sw.Elapsed.TotalMilliseconds);
                searches++;
                req.OnComplete?.Invoke(path);
            }
        }
                    
        
    }
}
