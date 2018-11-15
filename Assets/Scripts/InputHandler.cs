using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour {

	public Camera cam;
    public GameObject bullet;

	public float forceOffset = 1;
	public float force = 10;

	// Use this for initialization
	void Start () {
		
	}
	
	public void changeForceOffset(float value) {
		forceOffset = value;
	}

	public void changeForce(float value) {
		force = value;
	}

    private void Update()
    {
        if (Input.GetMouseButton(1)) OldUpdate();
        if (!Input.GetMouseButtonDown(0)) return;

        Ray inputRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        GameObject bulletObj = Instantiate(bullet, inputRay.origin, Quaternion.identity);
        Rigidbody rb = bulletObj.GetComponent<Rigidbody>();

        rb.AddForce(inputRay.direction * 5.0f, ForceMode.Impulse);
    }

    // Update is called once per frame
    void OldUpdate () {
		Ray inputRay = cam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast(inputRay, out hit)) {
			MeshDeformer md = hit.collider.gameObject.GetComponent<MeshDeformer>();

			if (md) {
				Vector3 point = hit.point;

				point += hit.normal * forceOffset;
				md.AddDeform(point, force);
			}
		}
	}
}
