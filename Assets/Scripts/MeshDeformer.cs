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

    private void UpdateVertices() {
        for (int i = 0; i < deformedVertices.Length; i++) {
            Vector3 force = vertexVelocities[i];
            Vector3 displacement = deformedVertices[i] - originalVertices[i];

            force *= getAttenuation(deformedVertices[i], originalVertices[i]);
            force -= displacement * springForce * Time.deltaTime;
            force *= 1 - damping * Time.deltaTime;
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
        Debug.Log(collision.relativeVelocity.sqrMagnitude);
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
