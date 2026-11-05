using UnityEngine;

namespace Sim   //NOT DONE !!
{
    public class ClimateRegionView : MonoBehaviour
    {
        public ClimateRegion Region { get; private set; }

        public void Init(ClimateRegion region, Material mat)
        {
            Region = region;
            // TODO: build a Mesh from region.Shape.Vertices (triangle fan), y = 0.02
            // TODO: MeshFilter + MeshRenderer with `mat` (blue/grey/green/red by Type)
        }
    }
}
