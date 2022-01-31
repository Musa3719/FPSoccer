using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System;
public class RoomController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private List<GameObject> PlayerSpaces;

    [SerializeField]
    private GameObject MessagePrefab;
    [SerializeField]
    private TMP_InputField MessageInput;
    private Queue<GameObject> MessageQueue;

    bool havePosition;
    string changePosOtherPlayerNick;

    public AudioClip RequestSound;
    public AudioClip AcceptedSound;
    public AudioClip RefusedSound;

    public float musicVol;
    public Slider MusicSlider;
    public AudioClip Button;
    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        GameObject.Find("Canvas").transform.Find("Reconnect").gameObject.SetActive(true);
    }
    public void Reconnect()
    {

        Debug.Log("Client status disconnected!", this);

        if (PhotonNetwork.ReconnectAndRejoin())
        {
            if (PhotonNetwork.CurrentRoom == null)
            {
                PhotonNetwork.LoadLevel(1);
            }
            Debug.Log("Successful reconnected and joined!", this);
            GameObject.Find("Canvas").transform.Find("Reconnect").gameObject.SetActive(false);
        }
    }
    public override void OnConnected()
    {
        GameObject.Find("Canvas").transform.Find("Reconnect").gameObject.SetActive(false);
    }

    private void Awake()
    {
        MessageQueue = new Queue<GameObject>();
        PhotonNetwork.CurrentRoom.IsOpen = true;
        PhotonNetwork.CurrentRoom.PlayerTtl = -1;
        Cursor.visible = true;
        changePosOtherPlayerNick = string.Empty;
        GameObject.Find("RoomName").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Name;

        havePosition = false;
        if (PhotonNetwork.IsMasterClient)
        {
            StartSpeed();
            ChangePosition(PlayerSpaces[0]);
            MakeNormalWeather();
        }

        MusicSlider.value = PlayerPrefs.GetFloat("Music");
    }
    public void MusicVolume(float value)
    {
        musicVol = value;
        PlayerPrefs.SetFloat("Music", musicVol);
        GameObject.Find("Music").GetComponent<AudioSource>().volume = musicVol / 4f;
        //GameObject.Find("Music").GetComponent<AudioSource>().volume = GameObject.Find("Music").GetComponent<AudioSource>().volume * music;
    }
    public void ButtonSound()
    {
        AudioSource.PlayClipAtPoint(Button, Camera.main.transform.position + Vector3.up * 3f, PlayerPrefs.GetFloat("Sound"));
    }
    private void StartSpeed()
    {
        GameObject.Find("5min").GetComponent<RectTransform>().anchoredPosition = new Vector2(GameObject.Find("5min").GetComponent<RectTransform>().anchoredPosition.x, -250f);
        GameObject.Find("10min").GetComponent<RectTransform>().anchoredPosition = new Vector2(GameObject.Find("10min").GetComponent<RectTransform>().anchoredPosition.x, -200f);
        GameObject.Find("15min").GetComponent<RectTransform>().anchoredPosition = new Vector2(GameObject.Find("15min").GetComponent<RectTransform>().anchoredPosition.x, -200f);

        ExitGames.Client.Photon.Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
        if (hash.ContainsKey("Speed"))
        {
            hash["Speed"] = 90f / 5f;
        }
        else
        {
            hash.Add("Speed", 90f / 5f);
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return)|| Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            if (MessageInput.text != string.Empty)
            {
                MessageSend(MessageInput.text);
                MessageInput.text = string.Empty;
                MessageInput.ActivateInputField();
            }
        }
    }
    public void MessageSend(string message)
    {
        GetComponent<PhotonView>().RPC("MessageSendRPC", RpcTarget.All, PhotonNetwork.NickName + ": " + message);
    }
    [PunRPC]
    public void MessageSendRPC(string message)
    {
        GameObject newMessage = Instantiate(MessagePrefab, GameObject.Find("Content").transform);
        newMessage.GetComponent<TextMeshProUGUI>().text = message;
        MessageQueue.Enqueue(newMessage);
        if (MessageQueue.Count > 18)
        {
            Destroy(MessageQueue.Dequeue());
        }
        
    }
    public void KickPlayer(Transform buttonTransform)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("KickPlayerRPC", NickToPlayer(buttonTransform.parent.gameObject.GetComponent<TextMeshProUGUI>().text));
        }
    }
    [PunRPC]
    public void KickPlayerRPC()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }


    public void SendChangePosRequest(string space)
    {
        if (!GameObject.Find("Canvas").transform.Find("ChangePosRequest").gameObject.activeInHierarchy)
        {

            GameObject.Find("Canvas").transform.Find("ChangePosRequest").gameObject.SetActive(true);
            

            string nick = string.Empty;
            if (space[0] == 1.ToString().ToCharArray()[0])
            {
                if (space[2] == 1.ToString().ToCharArray()[0])
                {
                    nick = GameObject.Find("Team1-1").GetComponent<TextMeshProUGUI>().text;
                }
                else if (space[2] == 2.ToString().ToCharArray()[0])
                {
                    nick = GameObject.Find("Team1-2").GetComponent<TextMeshProUGUI>().text;
                }
                else if (space[2] == 3.ToString().ToCharArray()[0])
                {
                    nick = GameObject.Find("Team1-3").GetComponent<TextMeshProUGUI>().text;
                }
                else if(space[2]== "K".ToCharArray()[0])
                {
                    nick = GameObject.Find("Team1-K").GetComponent<TextMeshProUGUI>().text;
                }
            }

            else if (space[0] == 2.ToString().ToCharArray()[0])
            {
                if (space[2] == 1.ToString().ToCharArray()[0])
                {
                    nick = GameObject.Find("Team2-1").GetComponent<TextMeshProUGUI>().text;
                }
                else if (space[2] == 2.ToString().ToCharArray()[0])
                {
                    nick = GameObject.Find("Team2-2").GetComponent<TextMeshProUGUI>().text;
                }
                else if (space[2] == 3.ToString().ToCharArray()[0])
                {
                    nick = GameObject.Find("Team2-3").GetComponent<TextMeshProUGUI>().text;
                }
                else if (space[2] == "K".ToCharArray()[0])
                {
                    nick = GameObject.Find("Team2-K").GetComponent<TextMeshProUGUI>().text;
                }
            }

            GetComponent<PhotonView>().RPC("CloseChangeButtonsRPC", RpcTarget.All, new string[] { PhotonNetwork.NickName, nick });

            Photon.Realtime.Player playerForRPC = null;
            playerForRPC = NickToPlayer(nick);
            changePosOtherPlayerNick = nick;
            GameObject.Find("Canvas").transform.Find("ChangePosRequest").GetComponent<TextMeshProUGUI>().text = "Change Request Sent To " + nick;
            GetComponent<PhotonView>().RPC("ChangePosRequestRPC", playerForRPC, PhotonNetwork.NickName);
            AudioSource.PlayClipAtPoint(RequestSound, Camera.main.transform.position + Vector3.up * 3f, PlayerPrefs.GetFloat("Sound"));
        }
        
    }
    [PunRPC]
    public void CloseChangeButtonsRPC(string nick1, string nick2)
    {
        foreach (var space in PlayerSpaces)
        {
            if(space.GetComponent<TextMeshProUGUI>().text==nick1 || space.GetComponent<TextMeshProUGUI>().text == nick2)
            {
                space.transform.Find("Button").gameObject.SetActive(false);
            }
        }
    }
    [PunRPC]
    public void OpenChangeButtonsRPC(string nick1, string nick2)
    {
        foreach (var space in PlayerSpaces)
        {
            if (space.GetComponent<TextMeshProUGUI>().text == nick1 || space.GetComponent<TextMeshProUGUI>().text == nick2)
            {
                space.transform.Find("Button").gameObject.SetActive(true);
            }
        }
    }
    Photon.Realtime.Player NickToPlayer(string nick)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.NickName == nick)
            {
                return player;
            }
        }
        return null;
    }
    [PunRPC]
    public void ChangePosRequestRPC(string nick)
    {
        AudioSource.PlayClipAtPoint(RequestSound, Camera.main.transform.position + Vector3.up * 3f, PlayerPrefs.GetFloat("Sound"));
        GameObject.Find("Canvas").transform.Find("ChangePosText").gameObject.SetActive(true);
        GameObject.Find("Canvas").transform.Find("ChangePosText").GetComponent<TextMeshProUGUI>().text = nick + " Want To Change Positions With You";
        changePosOtherPlayerNick = nick;
    }

    public void AcceptChange()
    {
        AudioSource.PlayClipAtPoint(AcceptedSound, Camera.main.transform.position + Vector3.up * 3f, PlayerPrefs.GetFloat("Sound"));
        GameObject.Find("Canvas").transform.Find("ChangePosText").gameObject.SetActive(false);
        GetComponent<PhotonView>().RPC("AcceptChangeRPC", NickToPlayer(changePosOtherPlayerNick));
        GetComponent<PhotonView>().RPC("OpenChangeButtonsRPC", RpcTarget.All, new string[] { PhotonNetwork.NickName, changePosOtherPlayerNick });
        Invoke("ColorAfterTime", 0.1f);
        
    }
    public void ColorAfterTime()
    {
        GetComponent<PhotonView>().RPC("ArrangeAllColorRPC", RpcTarget.All);
    }
    [PunRPC]
    public void ArrangeAllColorRPC()
    {
        foreach (var space in PlayerSpaces)
        {
            if (space.GetComponent<TextMeshProUGUI>().text == PhotonNetwork.NickName)
            {
                space.GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 1f);
            }
            else if (space.GetComponent<TextMeshProUGUI>().text == "Empty")
            {
                space.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1);
            }
            else
            {
                space.GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0.1f);
            }
        }
    }
    public void RefuseChange()
    {
        GameObject.Find("Canvas").transform.Find("ChangePosText").gameObject.SetActive(false);
        AudioSource.PlayClipAtPoint(RefusedSound, Camera.main.transform.position + Vector3.up * 3f, PlayerPrefs.GetFloat("Sound"));
        GetComponent<PhotonView>().RPC("RefuseChangeRPC", NickToPlayer(changePosOtherPlayerNick));
        GetComponent<PhotonView>().RPC("OpenChangeButtonsRPC", RpcTarget.All, new string[] { PhotonNetwork.NickName, changePosOtherPlayerNick });
    }
    [PunRPC]
    public void AcceptChangeRPC()
    {
        GameObject.Find("Canvas").transform.Find("ChangePosRequest").gameObject.SetActive(false);
        AudioSource.PlayClipAtPoint(AcceptedSound, Camera.main.transform.position + Vector3.up * 3f, PlayerPrefs.GetFloat("Sound"));
        ChangeTwoPlayerPos(PhotonNetwork.NickName, changePosOtherPlayerNick);
    }
    [PunRPC]
    public void RefuseChangeRPC()
    {
        GameObject.Find("Canvas").transform.Find("ChangePosRequest").gameObject.SetActive(false);
        AudioSource.PlayClipAtPoint(RefusedSound, Camera.main.transform.position + Vector3.up * 3f, PlayerPrefs.GetFloat("Sound"));
    }
    public void CancelChangeRequest()
    {
        GameObject.Find("Canvas").transform.Find("ChangePosRequest").gameObject.SetActive(false);
        GetComponent<PhotonView>().RPC("CancelRequestRPC", NickToPlayer(changePosOtherPlayerNick));
        GetComponent<PhotonView>().RPC("OpenChangeButtonsRPC", RpcTarget.All, new string[] { PhotonNetwork.NickName, changePosOtherPlayerNick });
    }
    [PunRPC]
    public void CancelRequestRPC()
    {
        GameObject.Find("Canvas").transform.Find("ChangePosText").gameObject.SetActive(false);
    }
    public void ChangeTwoPlayerPos(string p1, string p2)
    {
        GetComponent<PhotonView>().RPC("ChangeTwoPlayerPosRPC", RpcTarget.All,new string[] {p1, p2 });
    }
    [PunRPC]
    public void ChangeTwoPlayerPosRPC(string p1, string p2)
    {
        GameObject space1 = null;
        GameObject space2 = null;
        foreach (var space in PlayerSpaces)
        {
            if (space.GetComponent<TextMeshProUGUI>().text == p1)
            {
                space1 = space;
            }
            else if(space.GetComponent<TextMeshProUGUI>().text == p2)
            {
                space2 = space;
            }
        }

        if (space1.transform.Find("MasterClient").gameObject.GetComponent<Image>().enabled)
        {
            space1.transform.Find("MasterClient").gameObject.GetComponent<Image>().enabled = false;
            space2.transform.Find("MasterClient").gameObject.GetComponent<Image>().enabled = true;
        }
        else if (space2.transform.Find("MasterClient").gameObject.GetComponent<Image>().enabled)
        {
            space1.transform.Find("MasterClient").gameObject.GetComponent<Image>().enabled = true;
            space2.transform.Find("MasterClient").gameObject.GetComponent<Image>().enabled = false;
        }

        if(space1.transform.Find("KickButton").gameObject.activeInHierarchy && space2.transform.Find("KickButton").gameObject.activeInHierarchy)
        {
            //do nothing
        }
        else if ( space1.transform.Find("KickButton").gameObject.activeInHierarchy)
        {
            space1.transform.Find("KickButton").gameObject.SetActive(false);
            space2.transform.Find("KickButton").gameObject.SetActive(true);
        }
        else if (space2.transform.Find("KickButton").gameObject.activeInHierarchy)
        {
            space1.transform.Find("KickButton").gameObject.SetActive(true);
            space2.transform.Find("KickButton").gameObject.SetActive(false);
        }

        Color tempColor = space1.GetComponent<TextMeshProUGUI>().color;
        space1.GetComponent<TextMeshProUGUI>().color = space2.GetComponent<TextMeshProUGUI>().color;
        space2.GetComponent<TextMeshProUGUI>().color = tempColor;

        string tempText = space1.GetComponent<TextMeshProUGUI>().text;
        space1.GetComponent<TextMeshProUGUI>().text = space2.GetComponent<TextMeshProUGUI>().text;
        space2.GetComponent<TextMeshProUGUI>().text = tempText;

    }


    [PunRPC]
    public void ChangePosForOtherClients()
    {
        foreach (var playerSpace in PlayerSpaces)
        {
            if (playerSpace.GetComponent<TextMeshProUGUI>().text == "Empty")
            {
                ChangePosition(playerSpace);
                break;
            }
        }
    }
    [PunRPC]
    public void ClientKickButtonMovesRPC(string fromSpace ,string toSpace)
    {
        if (fromSpace[0] == 1.ToString().ToCharArray()[0])
        {
            if (fromSpace[2] == 1.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-1").transform.Find("KickButton").gameObject.SetActive(false);
            }
            else if (fromSpace[2] == 2.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-2").transform.Find("KickButton").gameObject.SetActive(false);
            }
            else if (fromSpace[2] == 3.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-3").transform.Find("KickButton").gameObject.SetActive(false);
            }
            else if (fromSpace[2] == "K".ToCharArray()[0])
            {
                GameObject.Find("Team1-K").transform.Find("KickButton").gameObject.SetActive(false);
            }
        }
        else if (fromSpace[0] == 2.ToString().ToCharArray()[0])
        {
            if (fromSpace[2] == 1.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-1").transform.Find("KickButton").gameObject.SetActive(false);
            }
            else if (fromSpace[2] == 2.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-2").transform.Find("KickButton").gameObject.SetActive(false);
            }
            else if (fromSpace[2] == 3.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-3").transform.Find("KickButton").gameObject.SetActive(false);
            }
            else if (fromSpace[2] == "K".ToCharArray()[0])
            {
                GameObject.Find("Team2-K").transform.Find("KickButton").gameObject.SetActive(false);
            }
        }

        if (toSpace[0] == 1.ToString().ToCharArray()[0])
        {
            if (toSpace[2] == 1.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-1").transform.Find("KickButton").gameObject.SetActive(true);
            }
            else if (toSpace[2] == 2.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-2").transform.Find("KickButton").gameObject.SetActive(true);
            }
            else if (toSpace[2] == 3.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-3").transform.Find("KickButton").gameObject.SetActive(true);
            }
            else if (toSpace[2] == "K".ToCharArray()[0])
            {
                GameObject.Find("Team1-K").transform.Find("KickButton").gameObject.SetActive(true);
            }
        }
        else if (toSpace[0] == 2.ToString().ToCharArray()[0])
        {
            if (toSpace[2] == 1.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-1").transform.Find("KickButton").gameObject.SetActive(true);
            }
            else if (toSpace[2] == 2.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-2").transform.Find("KickButton").gameObject.SetActive(true);
            }
            else if (toSpace[2] == 3.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-3").transform.Find("KickButton").gameObject.SetActive(true);
            }
            else if (toSpace[2] == "K".ToCharArray()[0])
            {
                GameObject.Find("Team2-K").transform.Find("KickButton").gameObject.SetActive(true);
            }
        }
    }

    public void Make5Min()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("ArrangeGameLenghtUI", RpcTarget.All, 5);
            ExitGames.Client.Photon.Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
            if (hash.ContainsKey("Speed"))
            {
                hash["Speed"] = 90f / 5f;
            }
            else
            {
                hash.Add("Speed", 90f / 5f);
            }
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }
    public void Make10Min()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("ArrangeGameLenghtUI", RpcTarget.All, 10);
            ExitGames.Client.Photon.Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
            if (hash.ContainsKey("Speed"))
            {
                hash["Speed"] = 90f / 10f;
            }
            else
            {
                hash.Add("Speed", 90f / 10f);
            }
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }
    public void Make15Min()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("ArrangeGameLenghtUI", RpcTarget.All, 15);
            ExitGames.Client.Photon.Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
            if (hash.ContainsKey("Speed"))
            {
                hash["Speed"] = 90f / 15f;
            }
            else
            {
                hash.Add("Speed", 90f / 15f);
            }
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }
    public void MakeRainWeather()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("ArrangeWeatherUI", RpcTarget.All, "Rain");
            ExitGames.Client.Photon.Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
            if (hash.ContainsKey("Weather"))
            {
                hash["Weather"] = "Rain";
            }
            else
            {
                hash.Add("Weather", "Rain");
            }
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }
    public void MakeNormalWeather()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("ArrangeWeatherUI", RpcTarget.All, "Normal");
            ExitGames.Client.Photon.Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
            if (hash.ContainsKey("Weather"))
            {
                hash["Weather"] = "Normal";
            }
            else
            {
                hash.Add("Weather", "Normal");
            }
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }
    [PunRPC]
    public void ArrangeWeatherUI(string weather)
    {
        Color selectedColor = Color.green;
        Color notSelectedColor = new Color(188f/255f, 238f/255f, 161f/255f);
        if (weather == "Normal")
        {
            GameObject.Find("Normal").GetComponent<Image>().color = selectedColor;
            GameObject.Find("Rain").GetComponent<Image>().color = notSelectedColor;
        }
        else if (weather == "Rain")
        {
            GameObject.Find("Normal").GetComponent<Image>().color = notSelectedColor;
            GameObject.Find("Rain").GetComponent<Image>().color = selectedColor;
        }
    }
    [PunRPC]
    public void ArrangeGameLenghtUI(int min)
    {
        if(min == 5)
        {
            GameObject.Find("5min").GetComponent<RectTransform>().anchoredPosition = new Vector2(GameObject.Find("5min").GetComponent<RectTransform>().anchoredPosition.x, -250f);
            GameObject.Find("10min").GetComponent<RectTransform>().anchoredPosition = new Vector2(GameObject.Find("10min").GetComponent<RectTransform>().anchoredPosition.x, -200f);
            GameObject.Find("15min").GetComponent<RectTransform>().anchoredPosition = new Vector2(GameObject.Find("15min").GetComponent<RectTransform>().anchoredPosition.x, -200f);
        }
        else if(min == 10)
        {
            GameObject.Find("5min").GetComponent<RectTransform>().anchoredPosition = new Vector2(GameObject.Find("5min").GetComponent<RectTransform>().anchoredPosition.x, -200f);
            GameObject.Find("10min").GetComponent<RectTransform>().anchoredPosition = new Vector2(GameObject.Find("10min").GetComponent<RectTransform>().anchoredPosition.x, -250f);
            GameObject.Find("15min").GetComponent<RectTransform>().anchoredPosition = new Vector2(GameObject.Find("15min").GetComponent<RectTransform>().anchoredPosition.x, -200f);
        }
        else if (min == 15)
        {
            GameObject.Find("5min").GetComponent<RectTransform>().anchoredPosition = new Vector2(GameObject.Find("5min").GetComponent<RectTransform>().anchoredPosition.x, -200f);
            GameObject.Find("10min").GetComponent<RectTransform>().anchoredPosition = new Vector2(GameObject.Find("10min").GetComponent<RectTransform>().anchoredPosition.x, -200f);
            GameObject.Find("15min").GetComponent<RectTransform>().anchoredPosition = new Vector2(GameObject.Find("15min").GetComponent<RectTransform>().anchoredPosition.x, -250f);
        }
    }

    public void ChangePosition(GameObject spaceGameobject)
    {
        if (spaceGameobject.GetComponent<TextMeshProUGUI>().text == PhotonNetwork.NickName)    return;//block your own pos
        

        if (spaceGameobject.GetComponent<TextMeshProUGUI>().text == "Empty")
        {

            

            if (PhotonNetwork.IsMasterClient)
            {
                GetComponent<PhotonView>().RPC("MasterClientImageCloseRPC", RpcTarget.All);
                GetComponent<PhotonView>().RPC("MasterClientImageRPC", RpcTarget.All, spaceGameobject.name.Substring(4));
                
            }
            else
            {
                foreach (var space in PlayerSpaces)
                {
                    if (space.GetComponent<TextMeshProUGUI>().text == PhotonNetwork.NickName)
                    {
                        GetComponent<PhotonView>().RPC("ClientKickButtonMovesRPC", RpcTarget.MasterClient, new string[] {space.name.Substring(4), spaceGameobject.name.Substring(4) });
                        break;
                    }
                }
                
            }


            //check if same nickname exist and add a number
            restart:
            var Players = PhotonNetwork.PlayerListOthers;
            foreach (var player in Players)
            {
                if ( player.NickName == PhotonNetwork.NickName)
                {
                    PhotonNetwork.NickName = PhotonNetwork.NickName + UnityEngine.Random.Range(0, 10).ToString();
                    goto restart;
                }
            }

            //if have pos before then remove your old pos
            if (havePosition)
            {
                foreach (var playerSpace in PlayerSpaces)
                {
                    if (playerSpace.GetComponent<TextMeshProUGUI>().text == PhotonNetwork.NickName)
                    {
                        playerSpace.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1);
                        GetComponent<PhotonView>().RPC("MakeEmptyRPC", RpcTarget.All, playerSpace.name.Substring(4));
                        break;
                    }
                }
            }

            havePosition = true;

            //add your name to space
            GetComponent<PhotonView>().RPC("MakeYourSpaceRPC", RpcTarget.All, new string[] { spaceGameobject.name.Substring(4), PhotonNetwork.NickName });
            spaceGameobject.GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 1);

            if(GameObject.Find("Canvas").transform.Find("ChangePosText").gameObject.activeInHierarchy)
            {
                RefuseChange();
            }
            if (GameObject.Find("Canvas").transform.Find("ChangePosRequest").gameObject.activeInHierarchy)
            {
                CancelChangeRequest();
            }

        }

        else
        {
            SendChangePosRequest(spaceGameobject.name.Substring(4));
            
        }
        
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("MessageSendRPC", RpcTarget.All, newPlayer.NickName + " Entered The Game");

            string[] nicknames = new string[PlayerSpaces.Count];
            for (int i = 0; i < nicknames.Length; i++)
            {
                nicknames[i] = PlayerSpaces[i].GetComponent<TextMeshProUGUI>().text;
            }
            GetComponent<PhotonView>().RPC("StartSpacesRPC", newPlayer, nicknames);
            GetComponent<PhotonView>().RPC("ChangePosForOtherClients", newPlayer);

            StartCoroutine(KickButtonAfterTime(newPlayer.NickName));

            GetComponent<PhotonView>().RPC("MasterClientImageCloseRPC", newPlayer);
            foreach (var space in PlayerSpaces)
            {
                if (space.GetComponent<TextMeshProUGUI>().text == PhotonNetwork.NickName)
                {
                    GetComponent<PhotonView>().RPC("MasterClientImageRPC", newPlayer, space.name.Substring(4));
                    break;
                }
            }
            
        }
        
    }
    IEnumerator KickButtonAfterTime(string nick)
    {
        yield return new WaitForSeconds(0.4f);
        foreach (var space in PlayerSpaces)
        {
            if (space.GetComponent<TextMeshProUGUI>().text == nick)
            {
                space.transform.Find("KickButton").gameObject.SetActive(true);
            }
        }
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("MessageSendRPC", RpcTarget.All, otherPlayer.NickName + " Left The Game...");
        }
        foreach (var space in PlayerSpaces)
        {
            if (space.GetComponent<TextMeshProUGUI>().text == otherPlayer.NickName)
            {
                space.GetComponent<TextMeshProUGUI>().text = "Empty";
                space.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1);

                //kick button close for master
                if (PhotonNetwork.IsMasterClient)
                {
                    space.transform.Find("KickButton").gameObject.SetActive(false);
                }

                //if master left then image of the new master is activated and old image is deactivated
                if (space.transform.Find("MasterClient").GetComponent<Image>().enabled)
                {
                    MasterClientLeft(space);
                }
            }
        }
    }
    void MasterClientLeft(GameObject space)
    {
        space.transform.Find("MasterClient").GetComponent<Image>().enabled = false;

        foreach (var space2 in PlayerSpaces)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                //enable the kick buttons
                if (space2.GetComponent<TextMeshProUGUI>().text == "Empty")
                {
                    space2.transform.Find("KickButton").gameObject.SetActive(false);
                }
                else if (space2.GetComponent<TextMeshProUGUI>().text != PhotonNetwork.NickName)
                {
                    space2.transform.Find("KickButton").gameObject.SetActive(true);
                }
            }


            //enable masterclient image
            if (space2.GetComponent<TextMeshProUGUI>().text == PhotonNetwork.MasterClient.NickName)
            {
                space2.transform.Find("MasterClient").GetComponent<Image>().enabled = true;
            }
        }
    }
    [PunRPC]
    public void MakeYourSpaceRPC(string str, string nick)
    {
        if (str[0] == 1.ToString().ToCharArray()[0])
        {
            if (str[2] == 1.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-1").GetComponent<TextMeshProUGUI>().text = nick;
                GameObject.Find("Team1-1").GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0.1f);
            }
            else if (str[2] == 2.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-2").GetComponent<TextMeshProUGUI>().text = nick;
                GameObject.Find("Team1-2").GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0.1f);
            }
            else if (str[2] == 3.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-3").GetComponent<TextMeshProUGUI>().text = nick;
                GameObject.Find("Team1-3").GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0.1f);
            }
            else if (str[2] == "K".ToCharArray()[0])
            {
                GameObject.Find("Team1-K").GetComponent<TextMeshProUGUI>().text = nick;
                GameObject.Find("Team1-K").GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0.1f);
            }
        }
        else if (str[0] == 2.ToString().ToCharArray()[0])
        {
            if (str[2] == 1.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-1").GetComponent<TextMeshProUGUI>().text = nick;
                GameObject.Find("Team2-1").GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0.1f);
            }
            else if (str[2] == 2.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-2").GetComponent<TextMeshProUGUI>().text = nick;
                GameObject.Find("Team2-2").GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0.1f);
            }
            else if (str[2] == 3.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-3").GetComponent<TextMeshProUGUI>().text = nick;
                GameObject.Find("Team2-3").GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0.1f);
            }
            else if (str[2] == "K".ToCharArray()[0])
            {
                GameObject.Find("Team2-K").GetComponent<TextMeshProUGUI>().text = nick;
                GameObject.Find("Team2-K").GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0.1f);
            }
        }

    }
    [PunRPC]
    public void MakeEmptyRPC(string str)
    {
        if (str[0] == 1.ToString().ToCharArray()[0])
        {
            if (str[2] == 1.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-1").GetComponent<TextMeshProUGUI>().text = "Empty";
                GameObject.Find("Team1-1").GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1);
            }
            else if (str[2] == 2.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-2").GetComponent<TextMeshProUGUI>().text = "Empty";
                GameObject.Find("Team1-2").GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1);
            }
            else if (str[2] == 3.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-3").GetComponent<TextMeshProUGUI>().text = "Empty";
                GameObject.Find("Team1-3").GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1);
            }
            else if (str[2] == "K".ToCharArray()[0])
            {
                GameObject.Find("Team1-K").GetComponent<TextMeshProUGUI>().text = "Empty";
                GameObject.Find("Team1-K").GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1);
            }
        }
        else if (str[0] == 2.ToString().ToCharArray()[0])
        {
            if (str[2] == 1.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-1").GetComponent<TextMeshProUGUI>().text = "Empty";
                GameObject.Find("Team2-1").GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1);
            }
            else if (str[2] == 2.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-2").GetComponent<TextMeshProUGUI>().text = "Empty";
                GameObject.Find("Team2-2").GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1);
            }
            else if (str[2] == 3.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-3").GetComponent<TextMeshProUGUI>().text = "Empty";
                GameObject.Find("Team2-3").GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1);
            }
            else if (str[2] == "K".ToCharArray()[0])
            {
                GameObject.Find("Team2-K").GetComponent<TextMeshProUGUI>().text = "Empty";
                GameObject.Find("Team2-K").GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1);
            }
        }
        
    }
    [PunRPC]
    public void MasterClientImageCloseRPC()
    {
        GameObject[] masterClients = GameObject.FindGameObjectsWithTag("MasterClient");
        foreach (var masterClient in masterClients)
        {
            masterClient.GetComponent<Image>().enabled = false;
        }
    }
    [PunRPC]
    public void MasterClientImageRPC(string str)
    {
        if (str[0] == 1.ToString().ToCharArray()[0])
        {
            if (str[2] == 1.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-1").transform.Find("MasterClient").GetComponent<Image>().enabled = true;
            }
            else if (str[2] == 2.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-2").transform.Find("MasterClient").GetComponent<Image>().enabled = true;
            }
            else if (str[2] == 3.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team1-3").transform.Find("MasterClient").GetComponent<Image>().enabled = true;
            }
            else if (str[2] == "K".ToCharArray()[0])
            {
                GameObject.Find("Team1-K").transform.Find("MasterClient").GetComponent<Image>().enabled = true;
            }
        }
        else if(str[0]==2.ToString().ToCharArray()[0])
        {
            if (str[2] == 1.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-1").transform.Find("MasterClient").GetComponent<Image>().enabled = true;
            }
            else if (str[2] == 2.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-2").transform.Find("MasterClient").GetComponent<Image>().enabled = true;
            }
            else if (str[2] == 3.ToString().ToCharArray()[0])
            {
                GameObject.Find("Team2-3").transform.Find("MasterClient").GetComponent<Image>().enabled = true;
            }
            else if (str[2] == "K".ToCharArray()[0])
            {
                GameObject.Find("Team2-K").transform.Find("MasterClient").GetComponent<Image>().enabled = true;
            }
        }
    }
    [PunRPC]
    public void StartSpacesRPC(string[] strings)
    {
        for (int i = 0; i < PlayerSpaces.Count; i++)
        {
            PlayerSpaces[i].GetComponent<TextMeshProUGUI>().text = strings[i];
            if (PlayerSpaces[i].GetComponent<TextMeshProUGUI>().text != "Empty")
            {
                PlayerSpaces[i].GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0.1f);
            }
            
        }
    }

    public void ToGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
            hash.Add("1-1", GameObject.Find("Team1-1").GetComponent<TextMeshProUGUI>().text);
            hash.Add("1-2", GameObject.Find("Team1-2").GetComponent<TextMeshProUGUI>().text);
            hash.Add("1-3", GameObject.Find("Team1-3").GetComponent<TextMeshProUGUI>().text);
            hash.Add("2-1", GameObject.Find("Team2-1").GetComponent<TextMeshProUGUI>().text);
            hash.Add("2-2", GameObject.Find("Team2-2").GetComponent<TextMeshProUGUI>().text);
            hash.Add("2-3", GameObject.Find("Team2-3").GetComponent<TextMeshProUGUI>().text);
            hash.Add("1-K", GameObject.Find("Team1-K").GetComponent<TextMeshProUGUI>().text);
            hash.Add("2-K", GameObject.Find("Team2-K").GetComponent<TextMeshProUGUI>().text);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
            StartCoroutine(StartGame());
            
        }
        
    }
    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.4f);
        GetComponent<PhotonView>().RPC("StartGameRPC", RpcTarget.All);
    }
    [PunRPC]
    public void StartGameRPC()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel("Game");
    }
}
