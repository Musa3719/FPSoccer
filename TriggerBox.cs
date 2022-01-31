using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBox : MonoBehaviour
{
    private PlayerStates parentPlayer;
    private void Awake()
    {
        parentPlayer = transform.parent.gameObject.GetComponent<PlayerStates>();
    }
    private void OnTriggerEnter(Collider other)
    {
        
        if (parentPlayer.view.IsMine && other.gameObject.CompareTag("Ball"))
        {
            if (parentPlayer.TeamNumber.ToCharArray()[2].ToString() == "K") return;
            parentPlayer.view.RPC("ListAddRPC", RpcTarget.All, parentPlayer.view.ViewID);

        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (parentPlayer.playerStateMachine is ControlGameStop) return;
        if (parentPlayer.view.IsMine && other.gameObject.CompareTag("Ball"))
        {
            parentPlayer.view.RPC("ListRemoveRPC", RpcTarget.All, parentPlayer.view.ViewID);
            if (Ball.instance.whoControlsBall == parentPlayer.gameObject)
            {
                if (parentPlayer.TeamNumber.ToCharArray()[2].ToString() == "K") return;
                Debug.Log(parentPlayer.TeamNumber);
                parentPlayer.playerStateMachine = new Walk();
                parentPlayer.view.RPC("WhoControlToNullRPC", RpcTarget.All);
            }

        }

    }
}
