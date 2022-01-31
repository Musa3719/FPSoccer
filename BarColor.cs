using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarColor : MonoBehaviour
{
    Image image;
    float lastFillAmount;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        lastFillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (image.fillAmount != lastFillAmount)
        {
            image.color = new Color(image.fillAmount * 2f, 2 - (image.fillAmount * 2), 0, 180f/255f);
            lastFillAmount = image.fillAmount;
        }
        
    }
}
