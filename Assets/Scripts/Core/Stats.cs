using System.IO;
using System.Text;

namespace Sim
{
    //Counters used for the performance/num of requests part of the report.
    public class Stats
    {
        public int pathRequests, pathSuccesses, pathFailures, pathsDroppedStale;
        public int replans, fireEscapes;
        public int ordersPlaced, ordersDelivered, ordersCancelled;
        public long nodesExpanded;
        public double searchTimeMs, maxSearchTimeMs;
        public int maxQueueLength;

        public void RecordSearch(bool success, int nodes, double ms)
        {
            if (success) pathSuccesses++; else pathFailures++;
            nodesExpanded += nodes;
            searchTimeMs += ms;
            if (ms > maxSearchTimeMs) maxSearchTimeMs = ms;
        }

        public static string CsvHeader =>
            "n,seed,duration,requests,successes,failures,stale,replans,fireEscapes,ordersPlaced,ordersDelivered,ordersCancelled,nodesExpanded,searchMs,maxSearchMs,maxQueue";

        public string CsvRow(int n, int seed, float duration) =>
            $"{n},{seed},{duration},{pathRequests},{pathSuccesses},{pathFailures},{pathsDroppedStale},{replans},{fireEscapes},{ordersPlaced},{ordersDelivered},{ordersCancelled},{nodesExpanded},{searchTimeMs:F2},{maxSearchTimeMs:F2},{maxQueueLength}";

        public void AppendCsv(string file, int n, int seed, float duration)
        {
            bool exists = File.Exists(file);
            var sb = new StringBuilder();
            if (!exists) sb.AppendLine(CsvHeader);
            sb.AppendLine(CsvRow(n, seed, duration));
            File.AppendAllText(file, sb.ToString());
        }
    }
}
