using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinueUI : MonoBehaviour
{
    private void OnEnable()
    {
        Invoke("Close", 3);
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
