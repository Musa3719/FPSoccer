using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{

    public TMP_InputField CreateInput;
    public TMP_InputField JoinInput;
    private byte maxPlayersForRoom;
    public override void OnDisconnected(DisconnectCause cause)
    {
        GameObject.Find("Canvas").transform.Find("Reconnect").gameObject.SetActive(true);
    }
    public void Reconnect()
    {


        Debug.Log("Client status disconnected!", this);

        if (PhotonNetwork.Reconnect() && PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Successful reconnected!", this);
            GameObject.Find("Canvas").transform.Find("Reconnect").gameObject.SetActive(false);
        }
    }
    public override void OnConnected()
    {
        GameObject.Find("Canvas").transform.Find("Reconnect").gameObject.SetActive(false);
    }
    private void Awake()
    {
        
        maxPlayersForRoom = 6;
        InvokeRepeating("RefreshRoomAndPlayerCount", 0, 1);
        if (PlayerPrefs.GetString("Nick") != string.Empty)
        {
            ChangeNickname(PlayerPrefs.GetString("Nick"));
        }
        else
        {
            GameObject.Find("Nickname").transform.Find("Text Area").transform.Find("Placeholder").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.NickName;
        }
        
    }
    
    public void ChangeNickname(string Nick)
    {
        if (Nick != string.Empty)
        {
            PhotonNetwork.NickName = Nick;
            GameObject.Find("Nickname").transform.Find("Text Area").transform.Find("Placeholder").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.NickName;
            PlayerPrefs.SetString("Nick", Nick);
        }
    }
    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnected || CreateInput.text==string.Empty)    return;


        RoomOptions RO = new RoomOptions();
        RO.EmptyRoomTtl = 0;
        RO.MaxPlayers = maxPlayersForRoom;

        //if has password
        if (GameObject.Find("IsPrivate").GetComponent<Toggle>().isOn)
        {
            RO.IsVisible = false;
        }

        PhotonNetwork.CreateRoom(CreateInput.text, RO);

    }
    
    public void JoinRoom()
    {
        if (!PhotonNetwork.IsConnected || JoinInput.text == string.Empty) return;

        PhotonNetwork.JoinRoom(JoinInput.text);
    }
    public void JoinRandomRoom()
    {
        if (!PhotonNetwork.IsConnected) return;
        PhotonNetwork.JoinRandomRoom(null, maxPlayersForRoom);
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Room");
    }
    public void RefreshRoomAndPlayerCount()
    {
        GameObject.Find("RoomCount").GetComponent<TextMeshProUGUI>().text = "Active Rooms: " + PhotonNetwork.CountOfRooms;
        GameObject.Find("PlayerCount").GetComponent<TextMeshProUGUI>().text = "Active Players: " + PhotonNetwork.CountOfPlayers;
    }
    public void ExitGame()
    {
        Application.Quit();
    }

}
