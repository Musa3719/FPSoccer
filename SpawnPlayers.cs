using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    

    public GameObject playerPrefab;
    
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;
    public void Reconnect()
    {
        StartCoroutine(MainReconnect());
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0, 2.5f, 0), Quaternion.identity);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        
        GameObject.Find("UI").transform.Find("Reconnect").gameObject.SetActive(true);
    }
    private IEnumerator MainReconnect()
    {
        while (PhotonNetwork.NetworkingClient.LoadBalancingPeer.PeerState != ExitGames.Client.Photon.PeerStateValue.Disconnected)
        {
            Debug.Log("Waiting for client to be fully disconnected..", this);

            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log("Client is disconnected!", this);

        PhotonNetwork.ReconnectAndRejoin();
    }
    

    private void Start()
    {
        Cursor.visible = false;
        Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), 0.5f, Random.Range(minZ, maxZ));
        PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);
    }
    private void Update()
    {
        if (PhotonNetwork.IsConnected && GameObject.Find("UI").transform.Find("Reconnect").gameObject.activeSelf)
        {
            GameObject.Find("UI").transform.Find("Reconnect").gameObject.SetActive(false);
        }
        
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().Owner == otherPlayer)
            {
                Destroy(item);
            }
        }
    }
    



}
