using UnityEngine;

namespace Sim
{
    
    public class AgentView : MonoBehaviour
    {
        public Agent Agent;
        public float height = 0.5f;
        void LateUpdate()
        {
            if (Agent == null) return;
            transform.position = Agent.Position.ToWorld(height);
            
        }
    }
}
