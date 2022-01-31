using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SlideChild : MonoBehaviour
{
    bool ballTouched;
    private void Awake()
    {
        Physics.IgnoreCollision(GetComponent<Collider>(), transform.parent.gameObject.GetComponent<Collider>());
    }
    private void OnEnable()
    {
        ballTouched = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!transform.parent.gameObject.GetComponent<PhotonView>().IsMine) return;
        if (other.gameObject.CompareTag("Ball"))//ball push and faul not allowed
        {
            Ball.instance.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
            ballTouched = true;
            transform.parent.gameObject.GetComponent<PhotonView>().RPC("WhoControlToNullRPC", RpcTarget.All);

            if (transform.parent.gameObject.GetComponent<PlayerStates>().rb.velocity.magnitude > 0.1f)
            {
                transform.parent.gameObject.GetComponent<PlayerStates>().view.RPC("PlayClipRPC", RpcTarget.All, GameAudio.instance.DribbleForward.name, transform.parent.position.x, transform.parent.position.y, transform.parent.position.z);
            }

            Vector3 distance = (Ball.instance.transform.position - transform.parent.position);
            distance.y = 0;
            distance = distance.normalized;
            Vector3 direction = transform.parent.forward - distance;
            Ball.instance.rb.velocity = (transform.parent.gameObject.GetComponent<PlayerStates>().rb.velocity.magnitude * transform.parent.forward * 2f + transform.parent.gameObject.GetComponent<PlayerStates>().rb.velocity.magnitude / 2f * direction) * 2 / 3f;

        }//faul and fall
        else if (other.gameObject.CompareTag("Player") && !ballTouched && !MatchState.instance.AreTheyInSameTeam(other.gameObject.GetComponent<PhotonView>().Owner.NickName, PhotonNetwork.NickName))
        {
            if (Mathf.Abs(other.gameObject.transform.position.x) < 100 && Mathf.Abs(other.gameObject.transform.position.z) < 156)//if inside stadium
            {
                other.GetComponent<PlayerStates>().view.RPC("PlayClipPitchGlobalRPC", RpcTarget.All, GameAudio.instance.Whistle.name, (30f - other.GetComponent<PlayerStates>().rb.velocity.magnitude) / 30f);
                transform.parent.gameObject.GetComponent<PhotonView>().RPC("MakeOtherFallRPC", other.gameObject.GetComponent<PhotonView>().Owner, other.gameObject.GetComponent<PhotonView>().ViewID);
                Faul(other.gameObject.GetComponent<PhotonView>().Owner.NickName, transform.parent.gameObject.GetComponent<PlayerStates>());
            }

        }//fall
        else if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<PlayerStates>().view.RPC("PlayClipPitchRPC", RpcTarget.All, GameAudio.instance.Fall.name, other.transform.position.x, other.transform.position.y, other.transform.position.z, Random.Range(0.85f, 1.15f));

            transform.parent.gameObject.GetComponent<PhotonView>().RPC("MakeOtherFallRPC", other.gameObject.GetComponent<PhotonView>().Owner, other.gameObject.GetComponent<PhotonView>().ViewID);
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E)) transform.parent.gameObject.GetComponent<PlayerStates>().view.RPC("RedCardRPC", RpcTarget.All, PhotonNetwork.NickName);
    }

    public void Faul(string nickForControlPlayer, PlayerStates stateScriptForFaulPlayer)
    {
        
        bool isPenalty = false;
        if (NickToTeam(nickForControlPlayer) == 1)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var item in players)
            {
                if (item.GetComponent<PhotonView>().Owner.NickName == nickForControlPlayer)
                {
                    if (item.transform.position.z > -156 && item.transform.position.z < -96 && item.transform.position.x < 73 && item.transform.position.x > -73)
                    {
                        isPenalty = true;
                    }
                }
            }
        }
        else if (NickToTeam(nickForControlPlayer) == 2)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var item in players)
            {
                if (item.GetComponent<PhotonView>().Owner.NickName == nickForControlPlayer)
                {
                    
                    if (item.transform.position.z < 156 && item.transform.position.z > 96 && item.transform.position.x < 73 && item.transform.position.x > -73)
                    {
                        isPenalty = true;
                    }
                }
            }
        }
        transform.parent.gameObject.GetComponent<PhotonView>().RPC("GameStopRPC", RpcTarget.All);
        bool isRedCard;
        bool isYellowCard = false;
        if (isPenalty)
        {
            if (stateScriptForFaulPlayer.isYellowCardGiven)
            {
                isRedCard = Random.Range(0, 100) < 80 ? true : false;
            }
            else
            {
                isRedCard = Random.Range(0, 100) < 8 ? true : false;
                isYellowCard = Random.Range(0, 100) < 70 && !isRedCard ? true : false;
            }
            transform.parent.gameObject.GetComponent<PlayerStates>().GetComponent<PlayerStates>().view.RPC("BallKinematicRPC", RpcTarget.All, false);
            transform.parent.gameObject.GetComponent<PlayerStates>().view.RPC("PenaltyUIRPC", RpcTarget.All);
            transform.parent.gameObject.GetComponent<PlayerStates>().view.RPC("PenaltyColliderRPC", RpcTarget.All, true);
            transform.parent.gameObject.GetComponent<PhotonView>().RPC("PenaltyCallRPC", NickToPlayer(nickForControlPlayer));
        }
        else
        {
            if (stateScriptForFaulPlayer.isYellowCardGiven)
            {
                isRedCard = Random.Range(0, 100) < 60 ? true : false;
            }
            else
            {
                isRedCard = Random.Range(0, 100) < 4 ? true : false;
                isYellowCard = Random.Range(0, 100) < 40 && !isRedCard ? true : false;
            }
            
            transform.parent.gameObject.GetComponent<PlayerStates>().view.RPC("FaulUIRPC", RpcTarget.All);
            transform.parent.gameObject.GetComponent<PlayerStates>().view.RPC("BallKinematicRPC", RpcTarget.All, false);
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var item in players)
            {
                if (item.GetComponent<PhotonView>().Owner.NickName == nickForControlPlayer)
                {
                    transform.parent.gameObject.GetComponent<PhotonView>().RPC("FaulRPC", RpcTarget.All, nickForControlPlayer, item.transform.position.x, item.transform.position.z);
                    break;
                }
            }
            
        }
        if (isRedCard)
        {
            transform.parent.gameObject.GetComponent<PlayerStates>().view.RPC("RedCardRPC", RpcTarget.All, stateScriptForFaulPlayer.GetComponent<PhotonView>().Owner.NickName);
        }
        else if (isYellowCard)
        {
            transform.parent.gameObject.GetComponent<PlayerStates>().view.RPC("YellowCardRPC", RpcTarget.All, stateScriptForFaulPlayer.GetComponent<PhotonView>().Owner.NickName);
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
    int NickToTeam(string nick)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().Owner.NickName == nick)
            {
                return int.Parse(item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[0].ToString());
            }
        }
        return 0;
    }
    int NickToTeamInside(string nick)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().Owner.NickName == nick)
            {
                return item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[2];
            }
        }
        return 0;
    }
}
