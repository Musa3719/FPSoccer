using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Texture2D cursor;

    // Start is called before the first frame update
    void Start()
    {
        //Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
        PhotonNetwork.GameVersion = "0.0.0";
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.NickName = "Rakun" + Random.Range(0, 1000);
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene(1);
    }
    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        StartCoroutine(MainReconnect());
    }
    private IEnumerator MainReconnect()
    {
        while (PhotonNetwork.NetworkingClient.LoadBalancingPeer.PeerState != ExitGames.Client.Photon.PeerStateValue.Disconnected)
        {
            Debug.Log("Waiting for client to be fully disconnected..", this);

            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log("Client is disconnected!", this);

        if (PhotonNetwork.Reconnect())
        {
            Debug.Log("Successful reconnected!", this);
        }
        else
        {
            Debug.Log("Reconnect Failed!", this);
        }
    }


}
