using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Ball : MonoBehaviour
{
    public static Ball instance;

    public Rigidbody rb;
    float distToGround;

    public bool goalHappened;

    public float falso;
    public Vector3 falsoAxis;

    public GameObject whoControlsBall;
    public List<GameObject> BallTriggeredPlayers;

    public int WhichTeamTouchedLast;

    private void Awake()
    {
        falso = 0;
        instance = this;
        whoControlsBall = null;
        rb = GetComponent<Rigidbody>();
        BallTriggeredPlayers = new List<GameObject>();
        distToGround = GetComponent<SphereCollider>().bounds.extents.y;
    }
    private void OnCollisionEnter(Collision collision)
    {
        falso = 0;
        //GameAudio.instance.PlayClip(GameAudio.instance.DribbleForward, transform.position, rb.velocity.magnitude/10f);
        

        if (collision.gameObject.CompareTag("Player"))
        {
            WhichTeamTouchedLast = int.Parse(collision.gameObject.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[0].ToString());
        }
        else if (collision.gameObject.CompareTag("Pole"))
        {
            GetComponent<PhotonView>().RPC("PlayClipPitchRPC", RpcTarget.All, GameAudio.instance.Pole.name, transform.position.x, transform.position.y, transform.position.z, Random.Range(1f, 1.5f)*Mathf.Clamp(rb.velocity.magnitude/25f,0.5f,1f));
        }
        else
        {
            GetComponent<PhotonView>().RPC("PlayClipPitchRPC", RpcTarget.All, GameAudio.instance.Kick.name, transform.position.x, transform.position.y, transform.position.z, Random.Range(0.8f, 1.2f));
        }
        
    }
    [PunRPC]
    public void TeamTakeGoalRPC(int team)
    {
        //MatchState.instance.isGameStoppedForEvent = true;
        int rand = Random.Range(0, 3);
        AudioClip clip = GameAudio.instance.Goal1;
        if (rand == 0)  clip = GameAudio.instance.Goal1;
        else if (rand == 1)  clip = GameAudio.instance.Goal2;
        else if (rand == 2)  clip = GameAudio.instance.Goal3;

        GameAudio.instance.PlayClip(clip, 1);
        GameAudio.instance.PlayClip(GameAudio.instance.GoalHit, Random.Range(0.75f, 1.2f));
        goalHappened = true;
        if (team == 1)
        {
            MatchState.instance.T2Score++;
            MatchState.instance.teamWhoTakeGoal = 1;
        }
        else if (team == 2)
        {
            MatchState.instance.T1Score++;
            MatchState.instance.teamWhoTakeGoal = 2;
        }
    }
    [PunRPC]
    public void PlayClipPitchRPC(string clipName, float x, float y, float z, float pitch)
    {
        var list = GameAudio.instance.audioList;
        AudioClip clip = null;
        foreach (var item in list)
        {
            if (item.name == clipName)
            {
                clip = item;
                break;
            }
        }
        if (clip != null)
            GameAudio.instance.PlayClip(clip, new Vector3(x, y, z), pitch);
    }

    private void Update()
    {
        if (MatchState.instance.isGameStoppedForEvent && rb.velocity.magnitude > 0.5f)
        {
            rb.velocity = new Vector3(0, 0, 0);
            transform.position = Vector3.zero + new Vector3(0, 0.5f, 0);
        }

        if(whoControlsBall!=null && BallTriggeredPlayers.Count == 0)
        {
            whoControlsBall.GetComponent<PlayerStates>().WhoControlToNullRPC();
        }
        if (whoControlsBall == null && BallTriggeredPlayers.Count > 0 && PhotonNetwork.IsMasterClient)
        {
            CheckPlayerTakeControlUnAttached();
        }
        
        if (whoControlsBall != null && whoControlsBall.GetComponent<PhotonView>().IsMine)
        {
            if(GetComponent<PhotonView>().Owner!= PhotonNetwork.LocalPlayer)
                GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
        }
    }
    private void FixedUpdate()
    {
        if (whoControlsBall == null && GetComponent<PhotonView>().IsMine)
        {
            if (falso > 33 * Time.fixedDeltaTime)
            {
                rb.velocity += falsoAxis * 33 * Time.fixedDeltaTime / 2f;
                falso -= 33 * Time.fixedDeltaTime;
            }
            else
            {
                falso = 0;
            }
        }

        if (GetComponent<PhotonView>().IsMine)
        {
            if (IsGrounded())
            {
                Vector3 vel = rb.velocity;
                float multiplier;
                if (vel.magnitude > 7.5f)
                {
                    multiplier = 0.995f;
                }
                else
                {
                    multiplier = 0.985f;
                }
                
                vel.x *= multiplier;
                vel.z *= multiplier;
                rb.velocity = vel;
            }
            else
            {
                Vector3 vel = rb.velocity;
                float multiplier = 0.997f;
                vel.x *= multiplier;
                vel.z *= multiplier;
                rb.velocity = vel;
            }
            
        }


        if (rb.velocity.magnitude < 0.2f && GetComponent<PhotonView>().IsMine)
        {
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.deltaTime * 3);
        }
        
    }
    bool IsGrounded() 
    {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }
    void CheckPlayerTakeControlUnAttached()
    {
        float minDistance = 1000;
        GameObject minDistancePlayer = null;
        foreach (var player in BallTriggeredPlayers)
        {
            if (player == null)//if player leaves
            {
                BallTriggeredPlayers.Remove(player);
                return;
            }
            var distance = (player.transform.position - transform.position).magnitude;

            bool canBeTakenByPlayer = CanBeTakenByPlayer(player);

            if (distance < minDistance && !player.GetComponent<PlayerStates>().letBallGo && canBeTakenByPlayer)
            {
                
                minDistance = distance;
                minDistancePlayer = player;
            }
        }
        if (minDistancePlayer != null)
        {
            GameObject.FindObjectOfType<PlayerStates>().view.RPC("WhoControlsBallRPC", RpcTarget.All, minDistancePlayer.GetComponent<PhotonView>().ViewID);
            WhichTeamTouchedLast = int.Parse(minDistancePlayer.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[0].ToString());
        }

    }
    bool CanBeTakenByPlayer(GameObject player)
    {
        if (Ball.instance.transform.position.y > 1.1f || Ball.instance.rb.velocity.magnitude > 60f || player.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[2].ToString()=="K")
        {
            return false;
        }
        return true;
    }
    
}
