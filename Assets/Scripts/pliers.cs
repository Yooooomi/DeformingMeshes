using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pliers : MonoBehaviour
{

    public GameObject plierLeft;
    public GameObject plierRight;

    // Use this for initialization
    void Start()
    {

    }

    private bool isNull(Vector3 v)
    {
        for (int i = 0; i < 3; i++)
        {
            if (v[i] != 0f) return false;
        }
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = Vector3.zero;
        Vector3 specialDir = Vector3.zero;
        Vector3 rotate =Vector3.zero;
        if (Input.GetMouseButton(0))
        {
            specialDir += new Vector3(1, 0);
        }
        else if (Input.GetMouseButton(1))
        {
            specialDir -= new Vector3(1, 0);
        }

        float upDown = Input.GetAxis("Fire1");
        if (upDown != 0f) dir += new Vector3(0, upDown > 0f ? 1 : -1, 0);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f) rotate = new Vector3(0, 0, scroll > 0f ? -4: 4);

        float vertic = Input.GetAxis("Vertical");
        if (vertic != 0f) dir += new Vector3(0, 0, vertic < 0f ? -1 : 1);

        float hor = Input.GetAxis("Horizontal");
        if (hor != 0f) dir += new Vector3(hor > 0f ? 1 : -1, 0);

        if (isNull(dir) && isNull(specialDir) && isNull(rotate)) return;

        dir *= Time.deltaTime;
        specialDir *= Time.deltaTime;
        plierLeft.transform.Translate(dir + specialDir, Space.World);
        plierRight.transform.Translate(dir - specialDir, Space.World);
        plierLeft.transform.Rotate(rotate);
        plierRight.transform.Rotate(-rotate);   
    }
}