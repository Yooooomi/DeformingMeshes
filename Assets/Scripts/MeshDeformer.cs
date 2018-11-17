using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformer : MonoBehaviour
{
    public float springForce = 20.0f; //Ressort
    public float damping = 1.0f; //Dureté du ressort

    public float strangerObjectForce = 3.0f;

    private Mesh deformedMesh;
    private MeshCollider coll;

    private Vector3[] originalVertices;
    private Vector3[] deformedVertices;
    private Vector3[] vertexVelocities;

    private List<GameObject> watchInside = new List<GameObject>();

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
        coll = GetComponent<MeshCollider>();

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
            ExecuteForce(point);
        }
    }

    private void UpdateVertices() {
        for (int i = 0; i < deformedVertices.Length; i++) {
            Vector3 force = vertexVelocities[i];
            Vector3 displacement = deformedVertices[i] - originalVertices[i];

            force -= displacement * springForce * Time.deltaTime;
            force *= 1 - damping * Time.deltaTime;
            deformedVertices[i] += force * Time.deltaTime;
            vertexVelocities[i] = force;
        }
        deformedMesh.vertices = deformedVertices;
        deformedMesh.RecalculateNormals();
        coll.sharedMesh = deformedMesh;
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
        float ratio = (deformedVertices[i]).sqrMagnitude / (originalVertices[i]).sqrMagnitude;
        if (ratio > 1)
            ratio = 1;
        attenuatedForce *= ratio;
        float velocity = attenuatedForce * Time.deltaTime;

        vertexVelocities[i] += pointToVertex.normalized * velocity;
    }

    void ExecuteForce(Vector3 point)
    {
        AddDeform(point, 15.0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (watchInside.Contains(collision.gameObject))
        {
            watchInside.Remove(collision.gameObject);
        }
        Vector3 point = collision.contacts[0].point - (collision.contacts[0].normal * 0.5f);
        ExecuteForce(point);
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("EXIT");
        bool isInList = watchInside.Contains(collision.gameObject);
        bool isIn = coll.bounds.Contains(collision.collider.gameObject.transform.position);

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
        Vector3 point = collision.contacts[0].point - (collision.contacts[0].normal * 0.5f);
        AddDeform(point, collision.relativeVelocity.magnitude * collision.rigidbody.mass);
    }
}
