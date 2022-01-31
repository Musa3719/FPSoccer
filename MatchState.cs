using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using DigitalRuby.RainMaker;
using UnityEngine.UI;

public class MatchState : MonoBehaviour
{

    public static MatchState instance;

    public GameObject normalCamera;
    public GameObject replayCamera;

    public bool replayMode;

    public GameObject AudioPrefab;
    public float soundVol;

    RainScript rainScript;

    public float secondPast;
    public float minutesPast;

    public bool ifStopCounter;
    public float stopCounter;

    public float gameSpeed;
    public string gameWeather;

    public int T1Score;
    public int T2Score;

    public bool isGameStoppedForEvent;
    public bool isSecondHalf;
    public bool isOverTime;
    public bool isGameFinished;

    public string player11;
    public string player12;
    public string player13;
    public string player21;
    public string player22;
    public string player23;
    public string player1K;
    public string player2K;

    public Transform player11StartPos;
    public Transform player12StartPos;
    public Transform player13StartPos;
    public Transform player21StartPos;
    public Transform player22StartPos;
    public Transform player23StartPos;
    public Transform player1KStartPos;
    public Transform player2KStartPos;

    public Transform goal1Transform;
    public Transform goal2Transform;

    public Slider SoundSlider;

    public int teamWhoTakeGoal;

    public List<Transform> SpecPositions;
    private void ArrangeWeather()
    {
        switch (gameWeather)
        {
            case "Rain":
                GameObject.Find("Weathers").transform.Find("Rain").gameObject.SetActive(true);
                GameObject.Find("Weathers").transform.Find("Rain").Find("Script").GetComponent<RainScript>().Camera = Camera.main;
                rainScript = GameObject.Find("Weathers").transform.Find("Rain").transform.Find("Script").gameObject.GetComponent<RainScript>();

                break;
            default:
                GameObject.Find("Weathers").transform.Find("Normal").gameObject.SetActive(true);
                break;
        }
    }
    
    public void SoundVolume(float value)
    {
        soundVol = value;
        PlayerPrefs.SetFloat("Sound", soundVol);
        GetComponent<AudioSource>().volume = soundVol / 8f;
    }
    
    private void Awake()
    {
        soundVol = PlayerPrefs.GetFloat("Sound");
        GetComponent<AudioSource>().volume = soundVol / 8f;
        SoundSlider.value = soundVol;
        stopCounter = 0;
        teamWhoTakeGoal = 0;
        isSecondHalf = false;
        isOverTime = false;
        replayMode = false;
        soundVol = PlayerPrefs.GetFloat("Sound");
        gameSpeed= (float)PhotonNetwork.CurrentRoom.CustomProperties["Speed"];
        gameWeather= (string)PhotonNetwork.CurrentRoom.CustomProperties["Weather"];
        ArrangeWeather();
        instance = this;
        isGameStoppedForEvent = false;
        secondPast = 0;
        minutesPast = 0;
        T1Score = 0;
        T2Score = 0;
        
    }
    private void Start()
    {
        ArrangeCamera();
        if (PhotonNetwork.IsMasterClient)
        {
            Invoke("StartKick", 1.5f);
        }
    }
    
    public void StartKick()
    {
        secondPast = 0;
        Kickoff(1);
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine)
            {
                player.GetComponent<PhotonView>().RPC("OpenPanelRPC", RpcTarget.All);
            }
        }
        
    }
    
    public void ArrangeCamera()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().IsMine)
            {
                normalCamera = item.transform.Find("Main Camera").gameObject;
                break;
            }
        }
    } 
    public bool AreTheyInSameTeam(string nick1, string nick2)
    {
        int n1 = 0;
        int n2 = 0;

        if(nick1== player11 || nick1 == player12 || nick1== player13 || nick1 == player1K)
        {
            n1 = 1;
        }
        else
        {
            n1 = 2;
        }

        if (nick2 == player11 || nick2 == player12 || nick2 == player13 || nick2==player1K)
        {
            n2 = 1;
        }
        else
        {
            n2 = 2;
        }

        if (n1 == n2)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    void CheckBallOut()
    {
        if (Mathf.Abs(Ball.instance.transform.position.x) > 112 || Mathf.Abs(Ball.instance.transform.position.z) > 185 || Mathf.Abs(Ball.instance.transform.position.y) > 100)
        {

            if (Ball.instance.WhichTeamTouchedLast == 1)
            {
                Kickoff(2);
            }
            else if (Ball.instance.WhichTeamTouchedLast == 2)
            {
                Kickoff(1);
            }
        }


    }
    // Update is called once per frame
    void Update()
    {
        if (Ball.instance.whoControlsBall != null)
        {
            if (Ball.instance.whoControlsBall.GetPhotonView().IsMine)
            {
                CheckBallOut();
            }
        }
        
        else if (PhotonNetwork.IsMasterClient && !replayMode)
        {
            CheckBallOut();
        }
        ArrangeTimeForMatch();
       /* if (replayMode && !replayCamera.activeInHierarchy)
        {
            replayCamera.SetActive(true);
            normalCamera.SetActive(false);
        }
        else if(!replayMode && !normalCamera.activeInHierarchy)
        {
            replayCamera.SetActive(false);
            normalCamera.SetActive(true);
        }*/

        if (ifStopCounter && PhotonNetwork.IsMasterClient)
        {
            stopCounter += Time.deltaTime;
            if (stopCounter > 20)
            {
                var players = GameObject.FindGameObjectsWithTag("Player");
                foreach (var item in players)
                {
                    if (item.GetComponent<PhotonView>().IsMine)
                    {
                        item.GetComponent<PlayerStates>().view.RPC("ToWalkRPC", RpcTarget.All);
                        item.GetComponent<PlayerStates>().view.RPC("GameContinueRPC", RpcTarget.All);
                        item.GetComponent<PlayerStates>().view.RPC("WhoControlToNullRPC", RpcTarget.All);
                        item.GetComponent<PlayerStates>().view.RPC("ContinueUIRPC", RpcTarget.All);
                    }
                }
            }
        }

        if (gameWeather == "Rain")
        {
            rainScript.RainIntensity += Random.Range(-0.5f, 0.5f) * Time.deltaTime;
        }

        
    }
    void ArrangeTimeForMatch()
    {
        if (!isGameStoppedForEvent && PhotonNetwork.IsMasterClient)
        {
            secondPast += Time.deltaTime * gameSpeed;//gameSpeed ==1 means realtime
            if (secondPast >= 60)
            {
                secondPast -= 60;
                minutesPast += 1;
            }
            if (!isSecondHalf && minutesPast >= 45)
            {
                isSecondHalf = true;
                Kickoff(2);
            }
            else if (!isOverTime && minutesPast >= 90)
            {
                if (PhotonNetwork.IsMasterClient && T1Score==T2Score)// eðer beraberlik varsa uzatmalara git
                {
                    isOverTime = true;
                    Kickoff(1);
                }
                else if(PhotonNetwork.IsMasterClient)
                {
                    FinishGame();
                }
            }
            
            else if (minutesPast >= 120)
            {
                FinishGame();
            }
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var item in players)
            {
                if (item.GetComponent<PhotonView>().IsMine)
                {
                    item.GetComponent<PlayerStates>().view.RPC("TimeRPC", RpcTarget.Others, minutesPast, secondPast);
                }
            }
        }
        
    }
    
    public void Kickoff(int team)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().IsMine)
            {
                Ball.instance.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                Ball.instance.transform.position = new Vector3(0f, 0.5f, 0f);
                Ball.instance.rb.velocity = Vector3.zero;
                item.GetComponent<PlayerStates>().view.RPC("PlayClipPitchGlobalRPC", RpcTarget.All, GameAudio.instance.Whistle.name, (30f - item.GetComponent<PlayerStates>().rb.velocity.magnitude) / 30f);
                item.GetComponent<PlayerStates>().view.RPC("BallTransformToNullRPC", RpcTarget.All);
                item.GetComponent<PlayerStates>().view.RPC("BallKinematicRPC", RpcTarget.All, false);
                item.GetComponent<PlayerStates>().view.RPC("KickOffUIRPC", RpcTarget.All);
                item.GetComponent<PlayerStates>().view.RPC("ToWaitRPC", RpcTarget.All);
                item.GetComponent<PlayerStates>().view.RPC("GameStopRPC", RpcTarget.All);
                item.GetComponent<PlayerStates>().view.RPC("KickoffPositionsRPC", RpcTarget.All, team);
            }
        }
    }
    
    void FinishGame()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine)
            {
                player.GetComponent<PhotonView>().RPC("FinishGameRPC", RpcTarget.All, T1Score, T2Score);
            }
        }
    }
    public void MuteChat(bool isMuted)
    {
        if (isMuted)
        {
            GameObject.Find("UI").transform.Find("ChatParent").Find("ChatInput").gameObject.SetActive(false);
            GameObject.Find("UI").transform.Find("ChatParent").Find("Chat").gameObject.SetActive(false);
        }
        else
        {
            GameObject.Find("UI").transform.Find("ChatParent").Find("ChatInput").gameObject.SetActive(true);
            GameObject.Find("UI").transform.Find("ChatParent").Find("Chat").gameObject.SetActive(true);
        }
    }
    
}
