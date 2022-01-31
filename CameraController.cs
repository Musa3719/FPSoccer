using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class CameraController : MonoBehaviour
{
    Rigidbody rb;
    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        if (!GetComponentInParent<PhotonView>().IsMine)
        {
            gameObject.SetActive(false);
        }
        rb = transform.parent.gameObject.GetComponent<Rigidbody>();
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        cam.fieldOfView = Mathf.Clamp(80 + rb.velocity.magnitude / 2f, 90, 110);
    }
}
