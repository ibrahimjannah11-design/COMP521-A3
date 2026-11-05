using UnityEngine;

namespace Sim   //NOT DONE
{
    
    [RequireComponent(typeof(Camera))]
    public class CameraFitter : MonoBehaviour
    {
        public GameManager manager;
        void Start()
        {
            var cam = GetComponent<Camera>();
            var cfg = manager.config;
            cam.orthographic = true;
            cam.transform.position = new Vector3(cfg.arenaWidth / 2f, 100f, cfg.arenaHeight / 2f);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            // TODO: set cam.orthographicSize so the arena fits both width and height (account for cam.aspect)
        }
    }
}
