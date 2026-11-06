using UnityEngine;

namespace Sim
{
    
    public class HUD : MonoBehaviour
    {
        public GameManager manager;
        void OnGUI()            // despite this wasnt even required, this was the most fun thihng to cvode loll
        {
            if (manager == null)
                return;

            GUILayout.BeginArea(
                new Rect(10,10,300,400));

            GUILayout.Label(
                $"n = {manager.config.n}");

            GUILayout.Label(
                $"Requests = {manager.stats.pathRequests}");

            GUILayout.Label(
                $"Successes = {manager.stats.pathSuccesses}");

            GUILayout.Label(
                $"Failures = {manager.stats.pathFailures}");

            GUILayout.Label(
                $"Orders Delivered = {manager.stats.ordersDelivered}");

            GUILayout.Label(
                $"Orders Cancelled = {manager.stats.ordersCancelled}");
                

            

            GUILayout.EndArea();
        }
    }
}
