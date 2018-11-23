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
        if (!Input.GetMouseButtonDown(0)) return;

        Ray inputRay = cam.ScreenPointToRay(Input.mousePosition);

        GameObject bulletObj = Instantiate(bullet, inputRay.origin, Quaternion.identity);
        Rigidbody rb = bulletObj.GetComponent<Rigidbody>();

        rb.AddForce(inputRay.direction * 5.0f, ForceMode.Impulse);
    }
}
