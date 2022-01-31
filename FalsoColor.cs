using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FalsoColor : MonoBehaviour
{
    float lastXPos;
    Image image;
    // Start is called before the first frame update
    void Start()
    {
        lastXPos = transform.position.x;
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (lastXPos != transform.position.x)
        {
            float absX = Mathf.Abs(GetComponent<RectTransform>().anchoredPosition.x);
            image.color = new Color(absX * 2f / 100f, 2 - (absX * 2f / 100f), 0);
            lastXPos = transform.position.x;
        }
    }
}
