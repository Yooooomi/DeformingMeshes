using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GenerateConvex : MonoBehaviour {

    private Mesh mesh;
    private List<Vector3> originalVertices;
    public List<BoxCollider> existing = new List<BoxCollider>();

    private void GenerateEachColliders(List<Vector3> sizes)
    {
        for (int i = 0; i < originalVertices.Count; i++)
        {
            Vector3 size = sizes[i];
            for (int j = 0; j < 3; j++)
            {
                size[j] = Mathf.Abs(size[j]);
            }
            BoxCollider coll = gameObject.AddComponent<BoxCollider>();
            coll.center = originalVertices[i];
            coll.size = size;
            foreach (var c in existing)
            {
                Physics.IgnoreCollision(c, coll);
            }
            existing.Add(coll);
        }
    }

    private void GenerateColliders()
    {
        int offset = 1;
        int count = 0;
        for (int i = 0; i < originalVertices.Count - offset; i += offset)
        {
            Vector3 size = originalVertices[i + offset] - originalVertices[i];
            for (int j = 0; j < 3; j++)
            {
                size[j] = Mathf.Abs(size[j]);
            }

            Vector3 position = originalVertices[i] + ((originalVertices[i + offset] - originalVertices[i]) / 2);

            BoxCollider coll = gameObject.AddComponent<BoxCollider>();
            count++;
            coll.center = position;
            coll.size = size;
            foreach(var c in existing)
            {
                Physics.IgnoreCollision(c, coll);
            }
            existing.Add(coll);
            Debug.Log(size.sqrMagnitude);
        }
        Debug.Log("Added " + (count).ToString() + " Colliders");
    }

    private IEnumerator BouleDeNoel()
    {
        while (true)
        {
            for (int i = 0; i < originalVertices.Count - 1; i++)
            {
                Debug.DrawLine(originalVertices[i] + transform.position, originalVertices[i + 1] + transform.position, Color.white, 0.2f);
                yield return new WaitForSeconds(0.01f);
            }
        }
    }

    private List<Vector3> SortVertices()
    {
        List<Vector3> sizes = new List<Vector3>();

        for (int i = 0; i < originalVertices.Count; i++)
        {
            var list = originalVertices.OrderBy(s => (s - originalVertices[i]).sqrMagnitude);
            for (int j = 0; j < list.Count(); j++)
            {
                Vector3 tmp = originalVertices[i] - list.ElementAt(j);
                if (tmp.sqrMagnitude > 0.01f)
                {
                    sizes.Add(tmp);
                    break;
                }
            }
        }
        return sizes;
    }

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        originalVertices = new List<Vector3>(mesh.vertices);
        List<Vector3> sizes = SortVertices();
        GenerateEachColliders(sizes);
    }
}
