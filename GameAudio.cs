using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudio : MonoBehaviour
{
    public static GameAudio instance;
    public List<AudioClip> audioList;
    private void Awake()
    {
        instance = this;
        audioList = new List<AudioClip>();
        audioList.Add(DribbleForward);
        audioList.Add(DribbleHimself);
        audioList.Add(Kick);
        audioList.Add(Whistle);
        audioList.Add(Fall);
        audioList.Add(UnSuccessfull);
        audioList.Add(Successfull);
        audioList.Add(Goal1);
        audioList.Add(Goal2);
        audioList.Add(Goal3);
        audioList.Add(GoalHit);
        audioList.Add(Pole);
        audioList.Add(Slide);
        audioList.Add(Wind);
        audioList.Add(Button);
        audioList.Add(Walk);
    }

    public AudioClip DribbleForward;
    public AudioClip DribbleHimself;
    public AudioClip Kick;
    public AudioClip Whistle;
    public AudioClip Fall;
    public AudioClip UnSuccessfull;
    public AudioClip Successfull;
    public AudioClip Goal1;
    public AudioClip Goal2;
    public AudioClip Goal3;
    public AudioClip GoalHit;
    public AudioClip Pole;
    public AudioClip Slide;
    public AudioClip Wind;
    public AudioClip Button;
    public AudioClip Walk;

    public void PlayClip(AudioClip clip, Vector3 position)
    {
        GameObject AudioPrefab = MatchState.instance.AudioPrefab;
        GameObject audio = Instantiate(AudioPrefab, position, Quaternion.identity);
        audio.GetComponent<AudioSource>().clip = clip;
        audio.GetComponent<AudioSource>().volume = MatchState.instance.soundVol;
        audio.GetComponent<AudioSource>().Play();
        Destroy(audio, 2);
    }
    public void PlayClip(AudioClip clip, Vector3 position, float pitch)
    {
        GameObject AudioPrefab = MatchState.instance.AudioPrefab;
        GameObject audio = Instantiate(AudioPrefab, position, Quaternion.identity);
        audio.GetComponent<AudioSource>().clip = clip;
        audio.GetComponent<AudioSource>().volume = MatchState.instance.soundVol;
        if (clip.name == "Walk") audio.GetComponent<AudioSource>().volume = MatchState.instance.soundVol * 0.7f;
        pitch = Random.Range(pitch * 85f / 100f, pitch * 115f / 100f);
        pitch = Mathf.Clamp(pitch, 0.75f, 2.5f);
        audio.GetComponent<AudioSource>().pitch = pitch;
        audio.GetComponent<AudioSource>().Play();
        Destroy(audio, 2);
    }
    
    public void PlayClip(AudioClip clip, float pitch)
    {
        GameObject AudioPrefab = MatchState.instance.AudioPrefab;
        GameObject audio = Instantiate(AudioPrefab, Camera.main.transform.position + Vector3.up * 3f, Quaternion.identity);
        audio.GetComponent<AudioSource>().clip = clip;
        audio.GetComponent<AudioSource>().volume = MatchState.instance.soundVol;
        pitch = Random.Range(pitch * 85f / 100f, pitch * 115f / 100f);
        pitch = Mathf.Clamp(pitch, 0.75f, 2.5f);
        audio.GetComponent<AudioSource>().pitch = pitch;
        audio.GetComponent<AudioSource>().Play();
        Destroy(audio, 2);
    }
    public void ButtonSound()
    {
        AudioSource.PlayClipAtPoint(Button, Camera.main.transform.position + Vector3.up * 3f, PlayerPrefs.GetFloat("Sound"));
    }
}
