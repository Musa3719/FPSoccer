using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMove : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("awake");
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start");
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-2f, 0f), 0, Random.Range(0f, 2f));
    }
}
