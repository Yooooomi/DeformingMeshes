using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformer : MonoBehaviour
{
    public float springForce = 20.0f; //Ressort
    public float damping = 1.0f; //Dureté du ressort

    public float offsetForce = 0.5f;

    private Mesh deformedMesh;

    private Vector3[] originalVertices;
    private Vector3[] deformedVertices;
    private Vector3[] vertexVelocities;

    private List<GameObject> watchInside = new List<GameObject>();
    private List<BoxCollider> colliders;
    Vector3 tmp = Vector3.zero;

    public void ChangeSpring(float value) {
        springForce = value;
    }

    public void ChangeDamping(float value) {
        damping = value;
    }

    // Use this for initialization
    void Start()
    {
        deformedMesh = GetComponent<MeshFilter>().mesh;
        colliders = GetComponent<GenerateConvex>().existing;

        originalVertices = deformedMesh.vertices;
        deformedVertices = new Vector3[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            deformedVertices[i] = originalVertices[i];
        }
        vertexVelocities = new Vector3[originalVertices.Length];
    }

    void Update() {
        Debug.Log(tmp);
        UpdateVertices();
        foreach(var i in watchInside)
        {
            Vector3 point = i.transform.position;
            //ExecuteForce(point);
        }
    }


    
    private float getAttenuation(Vector3 deformedVertice, Vector3 originalVertice)
    {
        float ratio = 1 - (deformedVertice).sqrMagnitude / (originalVertice).sqrMagnitude;
        ratio = Mathf.Exp(-4 * ratio * ratio);
        return ratio;
    }

    private float computeDamping(Vector3 deformedVertice, Vector3 originalVertice)
    {
        float ratio = ((deformedVertice).sqrMagnitude / (originalVertice).sqrMagnitude) - 1;
        ratio = Mathf.Abs(ratio) + 0.1f;
        ratio *= damping;
        return ratio;
        //old  return force * (1 - damping * Time.deltaTime);
    }

    private bool isBeeingMoreDeformed(Vector3 deformedVetice, Vector3 originalVertice, Vector3 force)
    {
        float diff = originalVertice.magnitude - deformedVetice.magnitude;
        if ((force.magnitude > 0 && diff > 0) || (force.magnitude < 0 && diff < 0))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void UpdateVertices() {
        for (int i = 0; i < deformedVertices.Length; i++) {
            Vector3 force = vertexVelocities[i];
            Vector3 displacement = deformedVertices[i] - originalVertices[i];

            if (isBeeingMoreDeformed(deformedVertices[i], originalVertices[i], force))
            {
                force *= getAttenuation(deformedVertices[i], originalVertices[i]);            
            }
            else
            {
                force *= computeDamping(deformedVertices[i], originalVertices[i]);
            }
            force -= displacement * springForce * Time.deltaTime;
            tmp = force;
            deformedVertices[i] += force * Time.deltaTime;
            vertexVelocities[i] = force;
        }
        deformedMesh.vertices = deformedVertices;
        deformedMesh.RecalculateNormals();
        for(int i = 0; i < colliders.Count; i++)
        {
            colliders[i].center = deformedVertices[i];
        }
    }

    public void AddDeform(Vector3 point, float force)
    {
        point = transform.InverseTransformPoint(point);
        for (int i = 0; i < deformedVertices.Length; i++)
        {
            AddForceToVertex(i, point, force);
        }
    }

    void AddForceToVertex(int i, Vector3 point, float force)
    {
        Vector3 pointToVertex = deformedVertices[i] - point;
        float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);

        float velocity = attenuatedForce * Time.deltaTime;
        vertexVelocities[i] += pointToVertex.normalized * velocity;

    }

    void ExecuteForce(Vector3 point)
    {
        Debug.Log("Depractaed call to executeForce");
        //AddDeform(point, 15.0f);
    }

    private void handleCollision(Collision collision)
    {
        Vector3 point = collision.contacts[0].point - (collision.contacts[0].normal * offsetForce);
        float force = 1;
        if (collision.rigidbody)
        {
            force = collision.rigidbody.mass;
        }
        force *= collision.relativeVelocity.sqrMagnitude;
        AddDeform(point, force);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (watchInside.Contains(collision.gameObject))
        {
            watchInside.Remove(collision.gameObject);
        }
        handleCollision(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        bool isInList = watchInside.Contains(collision.gameObject);
        bool isIn = false; //coll.bounds.Contains(collision.collider.gameObject.transform.position);

        if (!isInList && isIn)
        {
            watchInside.Add(collision.gameObject);
        }
        else if (isInList && !isIn)
        {
            watchInside.Remove(collision.gameObject);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        handleCollision(collision);
    }
}
