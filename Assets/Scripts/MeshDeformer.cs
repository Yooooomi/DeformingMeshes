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

        originalVertices = deformedMesh.vertices;
        colliders = GetComponent<GenerateConvex>().existing;
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

    /*private void UpdateVertices() {
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
    }*/

    private void UpdateVertices()
    {
        for (int i = 0; i < deformedVertices.Length; i++)
        {
            Vector3 force = vertexVelocities[i];
            Vector3 displacement = deformedVertices[i] - originalVertices[i];
            force -= displacement * springForce * Time.deltaTime;
            force *= 1 - damping * Time.deltaTime;
            deformedVertices[i] += force * Time.deltaTime;
            vertexVelocities[i] = force;
        }
        deformedMesh.vertices = deformedVertices;
        deformedMesh.RecalculateNormals();
        for (int i = 0; i < colliders.Count; i++)
        {
            colliders[i].center = deformedVertices[i];
        }
    }

    private float clamp(float value, float max)
    {
        if (value > max) return max;
        return value;
    }

    private float dist(Vector3 v)
    {
        return Vector3.Distance(v, Vector3.zero);
    }

    private float getAttenuation(Vector3 currentVertice, Vector3 currentVerticeVelocity, Vector3 originalVertice, Vector3 dir, float force)
    {
        float deformAmount = dist(currentVertice - originalVertice) / dist(originalVertice);
        deformAmount = clamp(Mathf.Abs(deformAmount), 1);
        float ratio = deformAmount;
        ratio = Mathf.Exp(-4 * ratio * ratio);
        //float deformAmountPlanned = (currentVertice + dir * force * ratio).sqrMagnitude / (originalVertice).sqrMagnitude;
        //float deformAmountPlanned = dist(((currentVertice + dir * force * ratio) - originalVertice) - (originalVertice)) / dist((originalVertice));    
        float deformAmountPlanned = dist((currentVertice + currentVerticeVelocity + dir * force * Time.deltaTime * ratio) - originalVertice) / dist(originalVertice);
        if (deformAmountPlanned > 0.9f)
        {
            ratio = ratio / deformAmountPlanned;
        }
        return ratio;
    }

    void AddForceToVertex(int i, Vector3 point, Vector3 dir, float force)
    {
        Vector3 pointToVertex = deformedVertices[i] - point;
        float ratioAttenuation = getAttenuation(deformedVertices[i], vertexVelocities[i], originalVertices[i], dir, force);
        force *= ratioAttenuation;
        force = force / (1f + pointToVertex.sqrMagnitude); // force des cotes

        force *= Time.deltaTime;
        vertexVelocities[i] += dir * force;
    }

    public void AddDeform(Vector3 point, Vector3 dir, float force)
    {
        point = transform.InverseTransformPoint(point);
        for (int i = 0; i < deformedVertices.Length; i++)
        {
            AddForceToVertex(i, point, dir, force);
        }
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
        //Debug.DrawLine(point, point + collision.relativeVelocity, Color.red, 1.0f);
        if (force == 0) force = 1000;
        //Debug.Log(force);
        //Debug.Log(collision.relativeVelocity.normalized);
        AddDeform(point, collision.relativeVelocity.normalized, force);
    }

    private void OnCollisionEnter(Collision collision)
    {
        handleCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        handleCollision(collision);
    }
}
