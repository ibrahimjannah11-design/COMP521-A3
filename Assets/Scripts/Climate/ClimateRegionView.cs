using UnityEngine;

namespace Sim   //NOT DONE !!
{
    public class ClimateRegionView : MonoBehaviour
    {
        public ClimateRegion Region { get; private set; }

        public void Init(ClimateRegion region, Material mat)
        {

            // FOR DENBUGGING remove later !!
            Debug.Log($"Region spawned: {region.Type}");
            Region = region;

            var mf = gameObject.GetComponent<MeshFilter>();
            if (mf == null) mf = gameObject.AddComponent<MeshFilter>();

            var mr = gameObject.GetComponent<MeshRenderer>();
            if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();

            mr.material = mat;

            Vector3[] verts = new Vector3[region.Shape.Vertices.Length];

            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = new Vector3(
                    region.Shape.Vertices[i].x,
                    -2f,
                    region.Shape.Vertices[i].y);
            }

            int triCount = verts.Length - 2;
            int[] tris = new int[triCount * 3];

            for (int i = 0; i < triCount; i++)
            {
                tris[i * 3 + 0] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = i + 2;        // reversed order so that regions will be visible hopefullyyy
                //tris[i * 3 + 1] = i + 1;
                
            }

            Mesh mesh = new Mesh();
            mesh.vertices = verts;
            mesh.triangles = tris;
            //mesh.RecalculateNormals();
            //maybe this will fix upside down thing

            Vector3[] normals = new Vector3[verts.Length];

            for(int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.up;
            }

            mesh.normals = normals;
            mesh.RecalculateBounds();

            mf.mesh = mesh;

        }
    }
}
