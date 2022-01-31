using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayCamera : MonoBehaviour
{
    private GameObject ballObj;
    private Vector3 distance;
    private Vector3 ballVel;
    private Vector3 lastBallPos;
    private Vector3 lastVel;
    private void Awake()
    {
        
        ballVel = Vector3.zero;
        distance = new Vector3(0, 3f, -7f);
        ballObj = Ball.instance.gameObject;
        lastBallPos = ballObj.transform.position;
    }
    private void OnEnable()
    {
        lastVel = Vector3.zero;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ballVel = ballObj.transform.position - lastBallPos;
        ballVel.y = 0;
        Vector3 newPos = Vector3.zero;
        if (ballVel.magnitude < 0.1f)
        {
            if (lastVel == Vector3.zero)
            {
                newPos = ballObj.transform.position + new Vector3(0, 6, -10);
            }
            else
            {
                newPos = ballObj.transform.position + (-lastVel.normalized * 10f) + new Vector3(0, 6, 0);
            }
        }
        else
        {
            newPos = ballObj.transform.position + (-ballVel.normalized * 10f) + new Vector3(0, 6, 0);
        }

        if (ballVel.magnitude != 0)
        {
            lastVel = ballVel;
        }

        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 2f);


        var lookPos = ballObj.transform.position - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 3);
        lastBallPos = ballObj.transform.position;
    }
    public Vector2 Rotate(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
}
