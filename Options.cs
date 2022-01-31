using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public float sound;
    public float music;
    public Slider Sound;
    public AudioClip Button;
    private void Awake()
    {
        Time.timeScale = 1;
        sound = PlayerPrefs.GetFloat("Sound");
        music = PlayerPrefs.GetFloat("Music");
        if (sound == 0)
        {
            sound = 0.5f;
            PlayerPrefs.SetFloat("Sound", 0.5f);
        }
        Sound.value = sound;

        if (music == 0)
        {
            music = 0.5f;
            PlayerPrefs.SetFloat("Music", 0.5f);
        }
        

        Cursor.visible = true;
    }
    public void SoundVolume(float value)
    {
        sound = value;
        PlayerPrefs.SetFloat("Sound", sound);
    }
   
    public void ButtonSound()
    {
        AudioSource.PlayClipAtPoint(Button, Camera.main.transform.position + Vector3.up * 3f, PlayerPrefs.GetFloat("Sound"));
    }
    public void OpenControls()
    {
        GameObject.Find("Canvas").transform.Find("ControlsScreen").gameObject.SetActive(true);
    }
    public void CloseControls()
    {
        GameObject.Find("Canvas").transform.Find("ControlsScreen").gameObject.SetActive(false);
    }
}
