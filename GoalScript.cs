using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GoalScript : MonoBehaviour
{
    public int team;
    
    private bool invoked;
    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient && !invoked && !Ball.instance.goalHappened && !MatchState.instance.isGameStoppedForEvent && !MatchState.instance.replayMode && other.CompareTag("Ball"))
        {
            if (team == 1)
            {
                Ball.instance.GetComponent<PhotonView>().RPC("TeamTakeGoalRPC", RpcTarget.All, 1);
                Invoke("KickOff1", 2);
            }
            else if (team == 2)
            {
                Ball.instance.GetComponent<PhotonView>().RPC("TeamTakeGoalRPC", RpcTarget.All, 2);
                Invoke("KickOff2", 2);
            }
            //Invoke("Replay", 2);
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var item in players)
            {
                if (item.GetComponent<PhotonView>().IsMine)
                {
                    item.GetComponent<PlayerStates>().view.RPC("GoalUIRPC", RpcTarget.All);
                }
            }

            invoked = true;
            
        }
    }
    public void Replay()
    {
        invoked = false;
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            item.GetComponent<ReplaySystem>().StartPlayBack();
        }
        Ball.instance.GetComponent<ReplaySystem>().StartPlayBack();

    }
    public void KickOff1()
    {
        MatchState.instance.Kickoff(1);
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetPhotonView().IsMine)
                item.GetComponent<PlayerStates>().view.RPC("GoalHappenedRPC", RpcTarget.All, false);
        }
        invoked = false;
    }
    public void KickOff2()
    {
        MatchState.instance.Kickoff(2);
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetPhotonView().IsMine)
                item.GetComponent<PlayerStates>().view.RPC("GoalHappenedRPC", RpcTarget.All, false);
        }
        invoked = false;
    }
}
