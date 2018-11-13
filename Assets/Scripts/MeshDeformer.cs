using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformer : MonoBehaviour
{
    public float springForce = 20.0f; //Ressort
    public float damping = 1.0f; //Dureté du ressort

    private Mesh deformedMesh;

    private Vector3[] originalVertices;
    private Vector3[] deformedVertices;
    private Vector3[] vertexVelocities;

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
}
