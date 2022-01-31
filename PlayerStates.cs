using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Animations.Rigging;

public class PlayerStates : MonoBehaviour
{
    private IPlayerStateMachine stateInsideForKeeper;
    public IPlayerStateMachine playerStateMachine { get => stateInsideForKeeper; set { if (stateInsideForKeeper is KeeperHoldingBall) { 
        view.RPC("KeeperColliderRPC", RpcTarget.All, false);
            }
            stateInsideForKeeper = value; } }
    public string TeamNumber;
    public bool isYellowCardGiven;
    public bool isRedCardGiven;

    public Transform HandTransformGK;

    public Rigidbody rb;
    public PhotonView view;
    public Collider collider;
    public Animator animController;

    public bool letBallGo;

    public float cameraSpeed;
    public float moveSpeed;


    public float stamina;
    public bool canRun;

    public bool canSteal;
    public float stealBar;

    public float dribbleCounter;

    public float KickPower;
    public float KickHeight;
    public float KickMouseX;
    public float KickMouseY;
    public float KickFalso;

    public float jumpPower;
    public float throwPower;
    public float ballThrowedCounter;

    public Vector3 slideDirection;
    public float slideTime;

    public float fallTimer;

    public Vector3 ControlStopPosition;
    public Vector3 ControlStopBallPosition;

    public GameObject nameUI;
    public GameObject Hands;
    public GameObject LeftHand;

    public bool controlGiven;
    public ChainIKConstraint chainIK;
    public float chainIKCounter;

    [SerializeField]
    GameObject foot;
    [SerializeField]
    GameObject upperLeg;
    public float WalkCounter;

    private void Awake()
    {
        chainIKCounter = 0;
        chainIK = GetComponentInChildren<ChainIKConstraint>();
        chainIK.weight = 0;
        controlGiven = false;
        nameUI = transform.Find("NameCanvas").Find("NamePrefab").gameObject;
        Physics.IgnoreCollision(GetComponent<Collider>(), transform.Find("GameStopCollider").GetComponent<SphereCollider>(), true);
        fallTimer = 0;
        slideDirection = Vector3.zero;
        collider = GetComponent<Collider>();
        animController = GetComponent<Animator>();
        canSteal = true;
        stealBar = 0;
        dribbleCounter = 0;
        KickHeight = 0;
        KickPower = 0;
        KickMouseX = 0;
        KickMouseY = 0;
        KickFalso = 0;
        canRun = true;
        stamina = 100;
        letBallGo = false;
        cameraSpeed = 30;
        moveSpeed = 10;
        
        rb = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
        if (view.IsMine)
        {
            var renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var item in renderers)
            {
                if(!item.CompareTag("Legs"))
                    item.enabled = false;
            }
            StartTeams();
            if (TeamNumber[2] == "K".ToCharArray()[0])
            {
                playerStateMachine = new Walk();
                view.RPC("OpenHandsRPC", RpcTarget.All, PhotonNetwork.NickName);
                UIManager.instance.transform.Find("Stamina1").gameObject.SetActive(false);
                UIManager.instance.transform.Find("Stamina2").gameObject.SetActive(false);
                UIManager.instance.transform.Find("StaminaCover").gameObject.SetActive(false);
            }
        }
        

    }
    [PunRPC]
    public void OpenHandsRPC(string name)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if(item.GetComponent<PhotonView>().Owner.NickName == name)
            {
                foreach(Transform bone in item.GetComponent<PlayerStates>().Hands.transform)
                {
                    bone.GetComponent<TwoBoneIKConstraint>().weight = 1;
                }
            }
        }
        
    }
    private void Start()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            item.GetComponent<PlayerStates>().nameUI.GetComponent<TextMeshProUGUI>().text = item.GetComponent<PhotonView>().Owner.NickName;
            if(item.GetComponent<PhotonView>().IsMine)
            {
                item.GetComponent<PlayerStates>().nameUI.GetComponent<TextMeshProUGUI>().text = "";
                if(item.GetComponent<PlayerStates>().TeamNumber[2] == "K".ToCharArray()[0])
                {
                    item.transform.Find("Main Camera").gameObject.SetActive(false);
                    item.transform.Find("GKCam").gameObject.SetActive(true);
                }
                
            }
            if (MatchState.instance.AreTheyInSameTeam(item.GetComponent<PhotonView>().Owner.NickName, PhotonNetwork.NickName))
            {
                item.GetComponent<PlayerStates>().nameUI.GetComponent<TextMeshProUGUI>().color = Color.green;
            }
            else
            {
                item.GetComponent<PlayerStates>().nameUI.GetComponent<TextMeshProUGUI>().color = Color.red;
            }
        }
        
    }
    public void Fall()
    {
        if (Ball.instance.whoControlsBall == gameObject)
        {
            view.RPC("WhoControlToNullRPC", RpcTarget.All);
        }
        playerStateMachine = new Fall();
        fallTimer = 1.2f;
    }
    [PunRPC]
    public void KickoffPositionsRPC(int team)
    {
        Physics.IgnoreLayerCollision(7, 9, false);
        controlGiven = false;
        Ball.instance.GetComponent<Rigidbody>().position = Vector3.zero + new Vector3(0, 0.5f, 0);
        Ball.instance.rb.velocity = Vector3.zero;
        Ball.instance.rb.angularVelocity = Vector3.zero;
        Ball.instance.rb.isKinematic = false;
        var players = GameObject.FindGameObjectsWithTag("Player");
        //Debug.Log(players.Length);
        float yRotation = 0;
        if (team == 1)
        {
            yRotation = 0;
        }
        else if(team==2)
        {
            yRotation = 180;
        }
        
        foreach (var item in players)
        {
            if (item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[0]==team.ToString().ToCharArray()[0] && item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[2]== 1.ToString().ToCharArray()[0])
            {
                item.transform.position = Vector3.zero + new Vector3(0, 0.5f, 0);
                item.transform.eulerAngles = new Vector3(0, yRotation, 0);
                
                item.GetComponent<PlayerStates>().view.RPC("ToControlRPC", item.GetComponent<PhotonView>().Owner, 0f, 0f, 0f, 0f);
                controlGiven = true;
            }
            else if(item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[0] == 1.ToString().ToCharArray()[0])
            {
                if (item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[2] == 1.ToString().ToCharArray()[0])
                {
                    item.transform.position = MatchState.instance.player11StartPos.position;
                    item.transform.position = new Vector3(item.transform.position.x, 0.5f, item.transform.position.z);
                }
                else if (item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[2] == 2.ToString().ToCharArray()[0])
                {
                    item.transform.position = MatchState.instance.player12StartPos.position;
                    item.transform.position = new Vector3(item.transform.position.x, 0.5f, item.transform.position.z);
                }
                else if (item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[2] == 3.ToString().ToCharArray()[0])
                {
                    item.transform.position = MatchState.instance.player13StartPos.position;
                    item.transform.position = new Vector3(item.transform.position.x, 0.5f, item.transform.position.z);
                }
                else if (item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[2] == "K".ToCharArray()[0])
                {
                    item.transform.position = MatchState.instance.player1KStartPos.position;
                    item.transform.position = new Vector3(item.transform.position.x, 0.5f, item.transform.position.z);
                }
            }
            else if(item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[0] == 2.ToString().ToCharArray()[0])
            {
                if (item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[2] == 1.ToString().ToCharArray()[0])
                {
                    item.transform.position = MatchState.instance.player21StartPos.position;
                    item.transform.position = new Vector3(item.transform.position.x, 0.5f, item.transform.position.z);
                }
                else if (item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[2] == 2.ToString().ToCharArray()[0])
                {
                    item.transform.position = MatchState.instance.player22StartPos.position;
                    item.transform.position = new Vector3(item.transform.position.x, 0.5f, item.transform.position.z);
                }
                else if (item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[2] == 3.ToString().ToCharArray()[0])
                {
                    item.transform.position = MatchState.instance.player23StartPos.position;
                    item.transform.position = new Vector3(item.transform.position.x, 0.5f, item.transform.position.z);
                }
                else if (item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[2] == "K".ToCharArray()[0])
                {
                    item.transform.position = MatchState.instance.player2KStartPos.position;
                    item.transform.position = new Vector3(item.transform.position.x, 0.5f, item.transform.position.z);
                }
            }
        }
        if (!controlGiven)
        {
            foreach (var item in players)
            {
                if (item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[0] == team.ToString().ToCharArray()[0] && item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[2] == 2.ToString().ToCharArray()[0])
                {
                    item.transform.position = Vector3.zero + new Vector3(0, 0.5f, 0);
                    item.transform.eulerAngles = new Vector3(0, yRotation, 0);
                    item.GetComponent<PlayerStates>().view.RPC("ToControlRPC", item.GetComponent<PhotonView>().Owner, 0f, 0f, 0f, 0f);
                    controlGiven = true;
                }
            }
        }
        if (!controlGiven)
        {
            foreach (var item in players)
            {
                if (item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[0] == team.ToString().ToCharArray()[0] && item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[2] == 3.ToString().ToCharArray()[0])
                {
                    item.transform.eulerAngles = new Vector3(0, yRotation, 0);
                    item.transform.position = Vector3.zero + new Vector3(0, 0.5f, 0);
                    item.GetComponent<PlayerStates>().view.RPC("ToControlRPC", item.GetComponent<PhotonView>().Owner, 0f, 0f, 0f, 0f);
                    controlGiven = true;
                }
            }
        }
        if (!controlGiven)
        {
            Debug.LogError("other team not found");
            foreach (var item in players)
            {
                if (item.GetComponent<PhotonView>().IsMine && PhotonNetwork.IsMasterClient)
                {
                    item.GetComponent<PlayerStates>().view.RPC("ToWalkRPC", RpcTarget.All);
                    item.GetComponent<PlayerStates>().view.RPC("GameContinueRPC", RpcTarget.All);
                    //item.GetComponent<PlayerStates>().view.RPC("GameStopColliderCloseRPC", RpcTarget.All, Ball.instance.whoControlsBall.GetComponent<PlayerStates>().view.ViewID);
                    item.GetComponent<PlayerStates>().view.RPC("WhoControlToNullRPC", RpcTarget.All);
                }
            }
                    
        }
        Ball.instance.GetComponent<Rigidbody>().position = Vector3.zero + new Vector3(0, 0.5f, 0);
        Ball.instance.rb.velocity = Vector3.zero;
        Ball.instance.rb.angularVelocity = Vector3.zero;
        var playersForLook = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in playersForLook)
        {
            if(item.GetComponent<PhotonView>().Owner.NickName==PhotonNetwork.NickName)
                item.transform.LookAt(new Vector3(Ball.instance.transform.position.x, item.transform.position.y, Ball.instance.transform.position.z));
        }
    }
    public Photon.Realtime.Player NickToPlayer(string nick)
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
    public void OpenPanelRPC()
    {
        GameObject.Find("UI").transform.Find("Starting").gameObject.SetActive(false);
    }
    [PunRPC]
    public void ListAddRPC(int id)
    {
        if (!Ball.instance.BallTriggeredPlayers.Contains(PhotonView.Find(id).gameObject))
        {
            Ball.instance.BallTriggeredPlayers.Add(PhotonView.Find(id).gameObject);
        }
    }
    [PunRPC]
    public void ListRemoveRPC(int id)
    {
        if (Ball.instance.BallTriggeredPlayers.Contains(PhotonView.Find(id).gameObject))
        {
            Ball.instance.BallTriggeredPlayers.Remove(PhotonView.Find(id).gameObject);
        }
    }
    public int NickToTeam(string nick)
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
                return int.Parse(item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[2].ToString());
            }
        }
        return 0;
    }
    [PunRPC]
    public void MakeOtherFallRPC(int id)
    {
        PhotonView.Find(id).gameObject.GetComponent<PlayerStates>().Fall();
    }
    [PunRPC]
    public void RedCardRPC(string cardToPlayerName)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().Owner.NickName == cardToPlayerName)
            {
                item.GetComponent<PlayerStates>().isRedCardGiven = true;
            }
        }
        GameObject.Find("UI").transform.Find("RedCard").gameObject.SetActive(true);
        GameObject.Find("UI").transform.Find("RedCard").gameObject.GetComponent<UnityEngine.UI.Text>().text = "Red Card!" + System.Environment.NewLine + cardToPlayerName;
        transform.position = MatchState.instance.SpecPositions[0].position;
        MatchState.instance.SpecPositions.RemoveAt(0);
        gameObject.tag = "Spectator";
        rb.velocity = Vector3.zero;
        transform.Find("Slide").gameObject.SetActive(false);
        playerStateMachine = new Walk();
        animController.SetInteger("State", 0);
    }
    [PunRPC]
    public void YellowCardRPC(string cardToPlayerName)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().Owner.NickName == cardToPlayerName)
            {
                item.GetComponent<PlayerStates>().isYellowCardGiven = true;
            }
        }
        GameObject.Find("UI").transform.Find("YellowCard").gameObject.SetActive(true);
        GameObject.Find("UI").transform.Find("YellowCard").gameObject.GetComponent<UnityEngine.UI.Text>().text = "Yellow Card!" + System.Environment.NewLine + cardToPlayerName;
    }
    [PunRPC]
    public void FaulRPC(string nickForControlPlayer, float x, float z)
    {
        MatchState.instance.isGameStoppedForEvent = true;
        Vector3 FreekickPosition = new Vector3(x, 0.5f, z);
        Ball.instance.transform.position = FreekickPosition;
        Ball.instance.rb.velocity = Vector3.zero;

        Vector3 goalPos = Vector3.zero;
        if (PhotonNetwork.NickName == nickForControlPlayer)
        {
            playerStateMachine = new ControlGameStop(GetComponent<PlayerStates>());
            GetComponent<PhotonView>().RPC("ToControlRPC", NickToPlayer(nickForControlPlayer), FreekickPosition.x, FreekickPosition.z, FreekickPosition.x, FreekickPosition.z);
            Ball.instance.GetComponent<Rigidbody>().position = FreekickPosition;
            
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var item in players)
            {
                if (item.GetComponent<PhotonView>().Owner.NickName == nickForControlPlayer)
                {
                    if (item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[0] == 1.ToString().ToCharArray()[0])
                    {
                        goalPos = MatchState.instance.goal2Transform.position;
                    }
                    else
                    {
                        goalPos = MatchState.instance.goal1Transform.position;
                    }
                }
            }
        }
        else
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var item in players)
            {
                if (item.GetComponent<PhotonView>().Owner.NickName == nickForControlPlayer)
                {
                    if (item.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[0] == 1.ToString().ToCharArray()[0])
                    {
                        goalPos = MatchState.instance.goal2Transform.position;
                    }
                    else
                    {
                        goalPos = MatchState.instance.goal1Transform.position;
                    }
                }
            }

            playerStateMachine = new Walk();
            
            transform.position = FreekickPosition - new Vector3(0, 0, -(goalPos - FreekickPosition).normalized.z) * 45 + new Vector3(Random.Range(-8f, 8f), 0, 0);
            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
            
        }
        var playersForLook = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in playersForLook)
        {
            item.transform.LookAt(new Vector3(Ball.instance.transform.position.x, item.transform.position.y, Ball.instance.transform.position.z));
        }
        
    }
    [PunRPC]
    public void PenaltyRPC(string nickForControlPlayer)
    {
        MatchState.instance.isGameStoppedForEvent = true;
        Vector3 penaltyPositon = Vector3.zero;
        if (NickToTeam(nickForControlPlayer) == 1)
        {
            penaltyPositon = new Vector3(0, 0.5f, -100);
        }
        else
        {
            penaltyPositon = new Vector3(0, 0.5f, 100);
        }
        Ball.instance.rb.velocity = Vector3.zero;
        if (PhotonNetwork.NickName == nickForControlPlayer)
        {
            var playerPos = penaltyPositon - new Vector3(0, 0, penaltyPositon.z / 32f);
            GetComponent<PlayerStates>().playerStateMachine = new ControlGameStop(GetComponent<PlayerStates>());
            GetComponent<PhotonView>().RPC("ToControlRPC", NickToPlayer(nickForControlPlayer), playerPos.x, playerPos.z, penaltyPositon.x, penaltyPositon.z);
            Ball.instance.transform.position = penaltyPositon;
            transform.position = playerPos;
            
        }
        else
        {
            playerStateMachine = new Wait();
            if (NickToTeam(PhotonNetwork.NickName) == 1)
            {
                transform.position = penaltyPositon - new Vector3(0, 0, penaltyPositon.z / 6f) + new Vector3(-10, 0, 0) + new Vector3(NickToTeamInside(PhotonNetwork.NickName) * 2, 0, 0);
            }
            else
            {
                transform.position = penaltyPositon - new Vector3(0, 0, penaltyPositon.z / 6f) + new Vector3(NickToTeamInside(PhotonNetwork.NickName) * 2, 0, 0);
            }

        }

        var playersForLook = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in playersForLook)
        {
            item.transform.LookAt(new Vector3(Ball.instance.transform.position.x, item.transform.position.y, Ball.instance.transform.position.z));
        }
    }
    public void StartTeams()
    {
        MatchState.instance.player11 = (string)PhotonNetwork.CurrentRoom.CustomProperties["1-1"];
        MatchState.instance.player12 = (string)PhotonNetwork.CurrentRoom.CustomProperties["1-2"];
        MatchState.instance.player13 = (string)PhotonNetwork.CurrentRoom.CustomProperties["1-3"];
        MatchState.instance.player21 = (string)PhotonNetwork.CurrentRoom.CustomProperties["2-1"];
        MatchState.instance.player22 = (string)PhotonNetwork.CurrentRoom.CustomProperties["2-2"];
        MatchState.instance.player23 = (string)PhotonNetwork.CurrentRoom.CustomProperties["2-3"];
        MatchState.instance.player1K = (string)PhotonNetwork.CurrentRoom.CustomProperties["1-K"];
        MatchState.instance.player2K = (string)PhotonNetwork.CurrentRoom.CustomProperties["2-K"];


        if (PhotonNetwork.NickName == MatchState.instance.player11)
        {
            
            view.RPC("TeamNumberRPC", RpcTarget.All, view.ViewID, "1-1");
        }
        else if (PhotonNetwork.NickName == MatchState.instance.player12)
        {
            view.RPC("TeamNumberRPC", RpcTarget.All, view.ViewID, "1-2");
        }
        else if (PhotonNetwork.NickName == MatchState.instance.player13)
        {
            view.RPC("TeamNumberRPC", RpcTarget.All, view.ViewID, "1-3");
        }
        else if (PhotonNetwork.NickName == MatchState.instance.player21)
        {
            view.RPC("TeamNumberRPC", RpcTarget.All, view.ViewID, "2-1");
        }
        else if (PhotonNetwork.NickName == MatchState.instance.player22)
        {
            view.RPC("TeamNumberRPC", RpcTarget.All, view.ViewID, "2-2");
        }
        else if (PhotonNetwork.NickName == MatchState.instance.player23)
        {
            view.RPC("TeamNumberRPC", RpcTarget.All, view.ViewID, "2-3");
        }
        else if (PhotonNetwork.NickName == MatchState.instance.player1K)
        {
            view.RPC("TeamNumberRPC", RpcTarget.All, view.ViewID, "1-K");
            gameObject.layer= LayerMask.NameToLayer("GoalKeeper");
        }
        else if (PhotonNetwork.NickName == MatchState.instance.player2K)
        {
            view.RPC("TeamNumberRPC", RpcTarget.All, view.ViewID, "2-K");
            gameObject.layer = LayerMask.NameToLayer("GoalKeeper");
        }


    }
    [PunRPC]
    public void ToWaitRPC()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().IsMine)
            {
                item.GetComponent<PlayerStates>().playerStateMachine = new Wait();
                item.GetComponent<PlayerStates>().rb.velocity = Vector3.zero;
            }
        }
    }
    [PunRPC]
    public void ToWalkRPC()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().IsMine)
            {
                item.GetComponent<PlayerStates>().playerStateMachine = new Walk();
            }
        }
    }
    [PunRPC]
    public void ContinueUIRPC()
    {
        GameObject.Find("UI").transform.Find("Continue").gameObject.SetActive(true);
    }
    [PunRPC]
    public void FaulUIRPC()
    {
        GameObject.Find("UI").transform.Find("Faul").gameObject.SetActive(true);
    }
    [PunRPC]
    public void PenaltyUIRPC()
    {
        GameObject.Find("UI").transform.Find("Penalty").gameObject.SetActive(true);
    }
    [PunRPC]
    public void PenaltyColliderRPC(bool value)
    {
        GameObject.Find("GreenStad").transform.Find("GKColliderPenalty").gameObject.SetActive(value);
    }
    [PunRPC]
    public void KickOffUIRPC()
    {
        GameObject.Find("UI").transform.Find("KickOff").gameObject.SetActive(true);
    }
    [PunRPC]
    public void GoalUIRPC()
    {
        GameObject.Find("UI").transform.Find("Goal").gameObject.SetActive(true);
    }
    [PunRPC]
    public void ToControlRPC(float x, float z, float xBall, float zBall)
    {
        Ball.instance.rb.isKinematic = false;
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().IsMine)
            {
                item.GetComponent<PlayerStates>().ControlStopPosition = new Vector3(x, 0.5f, z);
                item.GetComponent<PlayerStates>().ControlStopBallPosition = new Vector3(xBall, 0.5f, zBall);
                item.GetComponent<Rigidbody>().position = new Vector3(x, 0.5f, z);
                Ball.instance.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                Ball.instance.rb.velocity = Vector3.zero;
                item.GetComponent<PlayerStates>().playerStateMachine = new ControlGameStop(item.GetComponent<PlayerStates>());
                item.GetComponent<PhotonView>().RPC("GameStopColliderOpenRPC", RpcTarget.All, item.GetComponent<PhotonView>().ViewID);
                item.GetComponent<PlayerStates>().rb.velocity = Vector3.zero;
                item.GetComponent<Rigidbody>().isKinematic = true;
                item.GetComponent<PhotonView>().RPC("WhoControlsBallRPC", RpcTarget.All, item.GetComponent<PhotonView>().ViewID);
            }
        }
    }
    [PunRPC]
    public void GameStopRPC()
    {
        //Ball.instance.enabled = false;
        MatchState.instance.isGameStoppedForEvent = true;
        MatchState.instance.ifStopCounter = true;
        if (TeamNumber[2] == "K".ToCharArray()[0])
        {
            if (TeamNumber[0] == "1".ToCharArray()[0])
            {
                transform.position = MatchState.instance.player1KStartPos.position;
            }
            else if(TeamNumber[0] == "2".ToCharArray()[0])
            {
                transform.position = MatchState.instance.player2KStartPos.position;
            }
        }
    }
    [PunRPC]
    public void GameContinueRPC()
    {
        MatchState.instance.isGameStoppedForEvent = false;
        if (Ball.instance.whoControlsBall != null)
        {
            Ball.instance.whoControlsBall.GetComponent<PlayerStates>().view.RPC("IsKinematicRPC", RpcTarget.All, Ball.instance.whoControlsBall.GetComponent<PlayerStates>().view.ViewID, false);
            Ball.instance.whoControlsBall.GetComponent<PlayerStates>().view.RPC("GameStopColliderCloseRPC", RpcTarget.All, Ball.instance.whoControlsBall.GetComponent<PlayerStates>().view.ViewID);
        }
        
        FindObjectOfType<PlayerStates>().view.RPC("PenaltyColliderRPC", RpcTarget.All, false);
        
        Ball.instance.enabled = true;
        MatchState.instance.ifStopCounter = false;
        MatchState.instance.stopCounter = 0;
    }
    [PunRPC]
    public void IsKinematicRPC(int id, bool value)
    {
        PhotonView.Find(id).GetComponent<Rigidbody>().isKinematic = value;
    }
    [PunRPC]
    public void BallKinematicRPC(bool value)
    {
        Ball.instance.rb.isKinematic = value;
        Ball.instance.GetComponent<Collider>().isTrigger = value;
    }
    [PunRPC]
    public void TimeRPC(float min, float sec)
    {
        MatchState.instance.minutesPast = min;
        MatchState.instance.secondPast = sec;
    }
    [PunRPC]
    public void TeamNumberRPC(int id, string number)
    {
        PhotonView.Find(id).GetComponent<PlayerStates>().TeamNumber = number;
    }
    [PunRPC]
    public void FinishGameRPC(int T1Score, int T2Score)
    {
        GameObject.Find("UI").transform.Find("FinishScreen").gameObject.SetActive(true);
        GameObject.Find("UI").transform.Find("FinishScreen").transform.Find("-").GetComponent<TMPro.TextMeshProUGUI>().text = T1Score + "-" + T2Score;
        MatchState.instance.gameSpeed = 0;
        MatchState.instance.isGameFinished = true;
        Cursor.visible = true;
        if (T1Score > T2Score)
        {
            if (TeamNumber.ToCharArray()[0] == 1.ToString().ToCharArray()[0])
            {
                GameObject.Find("UI").transform.Find("FinishScreen").transform.Find("WinLose").GetComponent<TMPro.TextMeshProUGUI>().text = "VICTORY";
            }
            else
            {
                GameObject.Find("UI").transform.Find("FinishScreen").transform.Find("WinLose").GetComponent<TMPro.TextMeshProUGUI>().text = "DEFEAT";
            }
        }
        else if (T2Score > T1Score)
        {
            if (T1Score > T2Score)
            {
                if (TeamNumber.ToCharArray()[0] == 1.ToString().ToCharArray()[0])
                {
                    GameObject.Find("UI").transform.Find("FinishScreen").transform.Find("WinLose").GetComponent<TMPro.TextMeshProUGUI>().text = "DEFEAT";
                }
                else
                {
                    GameObject.Find("UI").transform.Find("FinishScreen").transform.Find("WinLose").GetComponent<TMPro.TextMeshProUGUI>().text = "VICTORY";
                }
            }
        }
        else
        {
            GameObject.Find("UI").transform.Find("FinishScreen").transform.Find("WinLose").GetComponent<TMPro.TextMeshProUGUI>().text = "DRAW";
        }

        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine)
            {
                player.GetComponent<PlayerStates>().rb.velocity = new Vector3(0, 0, 0);
                player.GetComponent<PlayerStates>().enabled = false;
            }
        }
    }
    [PunRPC]
    public void PenaltyCallRPC()
    {
        
        GetComponent<PhotonView>().RPC("PenaltyRPC", RpcTarget.All, PhotonNetwork.NickName);
    }
    [PunRPC]
    public void WeightRPC(float weight, string name)//foot weight
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().Owner.NickName == name)
            {
                item.GetComponent<PlayerStates>().chainIK.weight = weight;
            }
        }
    }
    [PunRPC]
    public void HandsWeightRPC(float weight, string name)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().Owner.NickName == name)
            {
                var bones = item.GetComponent<PlayerStates>().Hands.GetComponentsInChildren<TwoBoneIKConstraint>();
                foreach (var bone in bones)
                {
                    bone.weight = weight;
                }
            }
        }
    }
    void AnimationSet()
    {
        var locVel = transform.InverseTransformDirection(rb.velocity) / 25f;
        locVel.x *= 3;
        animController.SetFloat("xVelocity", locVel.x);
        animController.SetFloat("yVelocity", locVel.z);

        if(playerStateMachine is Fall)
        {
            animController.SetInteger("State", 3);
        }
        else if (playerStateMachine is Slide)
        {
            animController.SetInteger("State", 2);
        }
        else
        {
            /*if((playerStateMachine is BallControl || playerStateMachine is ControlGameStop) && Input.GetKeyUp(KeyCode.Mouse0))
            {
                animController.SetTrigger("Shoot");
            }*/

            if (rb.velocity.magnitude < 0.2f && !animController.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                animController.SetInteger("State", 0);
            }
            else if (rb.velocity.magnitude >= 0.2f && !animController.GetCurrentAnimatorStateInfo(0).IsName("Walk-Run"))
            {
                animController.SetInteger("State", 1);
            }
        }
        view.RPC("AnimRPC", RpcTarget.Others, locVel.x, locVel.z, animController.GetInteger("State"), PhotonNetwork.NickName);

    }
    [PunRPC]
    public void AnimRPC(float xVel, float yVel, int state, string name)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().Owner.NickName == name)
            {
                PlayerStates playerStates = item.GetComponent<PlayerStates>();
                playerStates.animController.SetInteger("State", state);
                playerStates.animController.SetFloat("xVelocity", xVel);
                playerStates.animController.SetFloat("yVelocity", yVel);
            }
        }
    }
    void CheckWeight()
    {
        
        if (chainIKCounter > 0)
        {

            chainIK.weight = Mathf.Clamp((0.4f - chainIKCounter) * 4 + 0.25f, 0.25f, 0.7f);
            chainIKCounter -= Time.deltaTime;
        }
        else
        {
            chainIKCounter = 0;
            chainIK.weight = 0;
        }
        //chainIK.weight = 0.5f;
        view.RPC("WeightRPC", RpcTarget.Others, chainIK.weight, PhotonNetwork.NickName);
    }
    [PunRPC]
    public void FootPosRPC(float x, float y, float z)
    {
        foot.transform.localPosition = new Vector3(x, y, z);
    }
    [PunRPC]
    public void LegPosRPC(float x, float y, float z)
    {
        upperLeg.transform.localPosition = new Vector3(x, y, z);
    }
    [PunRPC]
    public void KeeperColliderRPC(bool isTrue)
    {
        Physics.IgnoreLayerCollision(7, 9, isTrue);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (view.IsMine && TeamNumber[2] == "K".ToCharArray()[0] && collision.gameObject.CompareTag("Ball"))//&& Ball.instance.rb.velocity.magnitude <= 80f
        {
            if (!(playerStateMachine is KeeperHoldingBall) && ballThrowedCounter <= 0)
            {
                BallCatchedGK();
            }
        }
    }
    public void PlayWalkOrRunSound()
    {
        if (WalkCounter <= 0)
        {
            view.RPC("PlayClipPitchRPC", RpcTarget.All, GameAudio.instance.Walk.name, transform.position.x, transform.position.y, transform.position.z, (rb.velocity.magnitude+85f)/100f);
            WalkCounter = GameAudio.instance.Walk.length * 65f / 10f;
        }
        else
        {
            WalkCounter -= Time.deltaTime;
        }
    }
    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKey(KeyCode.H))
        {
            view.RPC("WeightRPC", RpcTarget.Others, 1f, PhotonNetwork.NickName);
        }*/
        if (isRedCardGiven) return;
        Debug.Log(playerStateMachine);


        transform.Find("Target").transform.position = Ball.instance.transform.position;
        if (view.IsMine)
        {
            if (rb.velocity.magnitude > 0.2f && !(playerStateMachine is Slide) && !(playerStateMachine is Fall))
            {
                PlayWalkOrRunSound();
            }

            if (ballThrowedCounter > 0)
            {
                ballThrowedCounter -= Time.deltaTime;
            }
            else
            {
                ballThrowedCounter = 0;
            }

            

            AnimationSet();
            CheckWeight();
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            /*if (Input.GetKeyDown(KeyCode.H))
            {
                GetComponent<PhotonView>().RPC("PenaltyRPC", RpcTarget.All, PhotonNetwork.NickName);
            }*/
            if (letBallGo && !Physics.GetIgnoreCollision(collider, Ball.instance.GetComponent<SphereCollider>()))
            {
                Physics.IgnoreCollision(collider, Ball.instance.GetComponent<SphereCollider>(), true);
            }
            else if(!letBallGo && Physics.GetIgnoreCollision(collider, Ball.instance.GetComponent<SphereCollider>()))
            {
                Physics.IgnoreCollision(collider, Ball.instance.GetComponent<SphereCollider>(), false);
            }
            if (TeamNumber[2] != "K".ToCharArray()[0])
                UIManager.instance.ArrangeStaminaUI(stamina / 100f);
            if (!(playerStateMachine is BallControl))
            {
                dribbleCounter = 0;
            }

            //now written in dostate which means you can press before take control
            if (Input.GetKey(KeyCode.Mouse0))
            {
                KickPower += Time.deltaTime * 100 * 1.75f;
                KickPower = Mathf.Clamp(KickPower, 0, 100);

                KickMouseX += Input.GetAxisRaw("Mouse X");
                KickMouseY += Input.GetAxisRaw("Mouse Y") * 1.7f;
                KickMouseY = Mathf.Clamp(KickMouseY, 0, 25);

                if (TeamNumber[2] != "K".ToCharArray()[0])
                    UIManager.instance.ArrangePowerHeightUI(KickPower, KickMouseY, KickFalso, KickMouseX);
            }
            else if(!Input.GetKeyUp(KeyCode.Mouse0))
            {
                KickPower = 0;

                KickMouseX = 0;
                KickMouseY = 0;
            }

            if (Input.GetKey(KeyCode.Mouse1))
            {
                KickHeight += Time.deltaTime * 100;
                KickHeight = Mathf.Clamp(KickHeight, 0, 100);
            }
            else if (!Input.GetKey(KeyCode.Mouse0) && KickPower<=0)
            {
                KickHeight = 0;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                KickFalso -= Time.deltaTime * 100;
                KickFalso = Mathf.Clamp(KickFalso, -100, 100);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                KickFalso += Time.deltaTime * 100;
                KickFalso = Mathf.Clamp(KickFalso, -100, 100);
            }
            else if (!Input.GetKey(KeyCode.Mouse0) && KickPower <= 0)
            {
                KickFalso = 0;
            }



            if (!((UIManager.instance.isStopScreenOpen || UIManager.instance.MessageInput.isFocused)))
            {
                if (playerStateMachine == null) return;
                playerStateMachine.DoState(this);
                playerStateMachine.CheckStateChange(this);
            }
            else
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
                IncreaseStamina();
            }

        }
    }
    private void LateUpdate()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            item.GetComponent<PlayerStates>().nameUI.transform.LookAt(new Vector3(transform.position.x, item.GetComponent<PlayerStates>().nameUI.transform.position.y, transform.position.z));
        }

        if (view.IsMine)
        {

            if (!((UIManager.instance.isStopScreenOpen || UIManager.instance.MessageInput.isFocused)))
            {
                if (playerStateMachine == null) return;
                playerStateMachine.DoStateLateUpdate(this);
            }

        }
    }
    public void ControlCamera(PlayerStates playerStates)
    {
        float mouseX = Mathf.Clamp(Input.GetAxis("Mouse X"), -2f, 2f) * Time.deltaTime * cameraSpeed * 18;
        float mouseY = Mathf.Clamp(Input.GetAxis("Mouse Y"), -2f, 2f) * Time.deltaTime * cameraSpeed * 12;



        var newCameraXAngle = Camera.main.transform.localEulerAngles.x - mouseY;

        if (newCameraXAngle > 36 && newCameraXAngle < 99)
        {
            newCameraXAngle = 36;
        }
        if (newCameraXAngle < 320 && newCameraXAngle > 300)
        {
            newCameraXAngle = 320;
        }


        if (Input.GetKey(KeyCode.Mouse0))    return;
        
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            Camera.main.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y + mouseX*3/2f, 0);
            //Camera.main.transform.localRotation = Quaternion.Euler(newCameraXAngle, Camera.main.transform.localRotation.y, 0);
        }
        else
        {
            playerStates.transform.Rotate(new Vector3(0, mouseX, 0));//old
            Camera.main.transform.localRotation = Quaternion.Euler(newCameraXAngle, Camera.main.transform.localRotation.y, 0);
        }

    }

    public void IncreaseStamina()
    {
        stamina += Time.deltaTime * 11;
        stamina = Mathf.Clamp(stamina, -1, 100);
        if (stamina > 25)
        {
            canRun = true;
        }
    }
    public void DecraseStamina()
    {
        stamina -= Time.deltaTime * 7;
        stamina = Mathf.Clamp(stamina, -1, 100);
        if (stamina < 1.5f)
        {
            canRun = false;
        }
    }
    public void BallCatchedGK()
    {
        Ball.instance.WhichTeamTouchedLast = int.Parse(TeamNumber[0].ToString());
        GetComponent<PhotonView>().RPC("WhoControlsBallRPC", RpcTarget.All, GetComponent<PhotonView>().ViewID);
        GetComponent<PhotonView>().RPC("BallTransformRPC", RpcTarget.All, GetComponent<PhotonView>().ViewID);
        playerStateMachine = new KeeperHoldingBall(this);
    }
    public IEnumerator StealBarOpen()
    {
        yield return new WaitForSeconds(3);
        canSteal = true;
        GameObject.Find("UI").transform.Find("StealFailed").gameObject.SetActive(false);
    }
    [PunRPC]
    public void GoalHappenedRPC(bool value)
    {
        Ball.instance.goalHappened = value;
    }
    [PunRPC]
    public void GameStopColliderCloseRPC(int id)
    {
        /*var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().ViewID == id)
            {
                item.transform.Find("GameStopCollider").gameObject.SetActive(false);
            }
        }*/
        PhotonView.Find(id).transform.Find("GameStopCollider").gameObject.SetActive(false);
    }
    [PunRPC]
    public void GameStopColliderOpenRPC(int id)
    {
        /*var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().ViewID == id)
            {
                Debug.Log(item.GetComponent<PhotonView>().ViewID);
                item.transform.Find("GameStopCollider").gameObject.SetActive(true);
            }
        }*/
         
        PhotonView.Find(id).transform.Find("GameStopCollider").gameObject.SetActive(true);

    }

    [PunRPC]
    public void LetBallGoTrueRPC(int id)
    {
        PhotonView.Find(id).GetComponent<PlayerStates>().letBallGo = true;
    }
    [PunRPC]
    public void LetBallGoFalseRPC(int id)
    {
        PhotonView.Find(id).GetComponent<PlayerStates>().letBallGo = false;
    }
    [PunRPC]
    public void WhoControlToNullRPC()
    {
        Ball.instance.whoControlsBall = null;
        Physics.IgnoreLayerCollision(6, 7, false);
    }
    [PunRPC]
    public void WhoControlsBallRPC(int id)
    {
        Ball.instance.whoControlsBall = PhotonView.Find(id).gameObject;
        Physics.IgnoreLayerCollision(6, 7, true);
    }
    [PunRPC]
    public void PlayClipRPC(string clipName, float x, float y, float z)
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
            GameAudio.instance.PlayClip(clip, new Vector3(x, y, z));
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
    
    [PunRPC]
    public void PlayClipPitchGlobalRPC(string clipName, float pitch)
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
            GameAudio.instance.PlayClip(clip, pitch);
    }
    [PunRPC]
    public void PlayJumpAnimRPC(string name, bool isLeft)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if(item.GetPhotonView().Owner.NickName == name)
            {
                if(isLeft)
                    item.GetComponent<PlayerStates>().animController.SetTrigger("JumpLeft");
                else
                    item.GetComponent<PlayerStates>().animController.SetTrigger("JumpRight");
            }
        }
    }
    [PunRPC]
    public void OpenSlideObjRPC(int id)
    {
        PhotonView.Find(id).transform.Find("Slide").gameObject.SetActive(true);
    }
    [PunRPC]
    public void CloseSlideObjRPC(int id)
    {
        PhotonView.Find(id).transform.Find("Slide").gameObject.SetActive(false);
    }
    [PunRPC]
    public void BallTransformRPC(int id)
    {
        //Ball.instance.transform.parent = PhotonView.Find(id).GetComponent<PlayerStates>().LeftHand.transform;
    }
    [PunRPC]
    public void BallTransformToNullRPC()
    {
        //Ball.instance.transform.parent = null;
    }
    [PunRPC]
    public void HandPositionRPC(float leftX, float leftY, float leftZ, float rightX, float rightY, float rightZ, string name)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().Owner.NickName == name)
            {
                PlayerStates playerStates = item.GetComponent<PlayerStates>();
                playerStates.HandTransformGK.transform.Find("Right").localPosition = new Vector3(rightX, rightY, rightZ);
                playerStates.HandTransformGK.transform.Find("Left").localPosition = new Vector3(leftX, leftY, leftZ);
            }
        }
    }
    public void HandPositionHandle(PlayerStates playerStates)
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            playerStates.view.RPC("HandsWeightRPC", RpcTarget.All, 1f, PhotonNetwork.NickName);
        }
        else
        {
            playerStates.view.RPC("HandsWeightRPC", RpcTarget.All, 0f, PhotonNetwork.NickName);
        }
        

        if (Input.GetKey(KeyCode.Mouse1))
        {
            playerStates.HandTransformGK.transform.Find("Right").position += Input.GetAxisRaw("Mouse X") / 5f * playerStates.transform.right + Input.GetAxisRaw("Mouse Y") / 5f * playerStates.transform.up;
            playerStates.HandTransformGK.transform.Find("Right").localPosition = new Vector3(Mathf.Clamp(playerStates.HandTransformGK.transform.Find("Right").localPosition.x, -5f+0.4f, 5f+0.4f), Mathf.Clamp(playerStates.HandTransformGK.transform.Find("Right").localPosition.y, -5f, 5f), playerStates.HandTransformGK.transform.Find("Right").localPosition.z);
            playerStates.HandTransformGK.transform.Find("Left").position += Input.GetAxisRaw("Mouse X") / 5f * playerStates.transform.right + Input.GetAxisRaw("Mouse Y") / 5f * playerStates.transform.up;
            playerStates.HandTransformGK.transform.Find("Left").localPosition = new Vector3(Mathf.Clamp(playerStates.HandTransformGK.transform.Find("Left").localPosition.x, -5f-0.4f, 5f-0.4f), Mathf.Clamp(playerStates.HandTransformGK.transform.Find("Left").localPosition.y, -5f, 5f), playerStates.HandTransformGK.transform.Find("Left").localPosition.z);
        }
        else
        {
            playerStates.HandTransformGK.transform.Find("Right").position = playerStates.transform.position + playerStates.transform.up * 3.3f + playerStates.transform.forward * 1f + playerStates.transform.right * 0.4f;
            playerStates.HandTransformGK.transform.Find("Left").position = playerStates.transform.position + playerStates.transform.up * 3.3f + playerStates.transform.forward * 1f + playerStates.transform.right * -0.4f;
        }
        playerStates.view.RPC("HandPositionRPC", RpcTarget.Others,
            playerStates.HandTransformGK.transform.Find("Left").localPosition.x, playerStates.HandTransformGK.transform.Find("Left").localPosition.y, playerStates.HandTransformGK.transform.Find("Left").localPosition.z,
            playerStates.HandTransformGK.transform.Find("Right").localPosition.x, playerStates.HandTransformGK.transform.Find("Right").localPosition.y, playerStates.HandTransformGK.transform.Find("Right").localPosition.z,
            PhotonNetwork.NickName);
    }
}

public interface IPlayerStateMachine
{
    void DoState(PlayerStates playerStates);
    void DoStateLateUpdate(PlayerStates playerStates);
    void CheckStateChange(PlayerStates playerStates);
}

public class KeeperJumping : IPlayerStateMachine
{
    private float jumpStarted;
    public KeeperJumping(PlayerStates playerStates)
    {
        jumpStarted = 0;

        playerStates.view.RPC("HandsWeightRPC", RpcTarget.All, 1f, PhotonNetwork.NickName);
        playerStates.GetComponent<PhotonView>().RPC("BallTransformToNullRPC", RpcTarget.All);
    }
    
    bool CheckBallCatch(PlayerStates playerStates)
    {
        if (playerStates.ballThrowedCounter > 0f)
            return false;
        return ((playerStates.HandTransformGK.transform.Find("Right").position - Ball.instance.transform.position).magnitude < 2.5f) || ((playerStates.HandTransformGK.transform.Find("Left").position - Ball.instance.transform.position).magnitude < 2.5f);
    }
    public void CheckStateChange(PlayerStates playerStates)
    {
        
        /*if (CheckBallCatch(playerStates) && Ball.instance.rb.velocity.magnitude > 65f)
        {
            BallTouchedGK(playerStates);
        }*/
        if (CheckBallCatch(playerStates))
        {
            playerStates.BallCatchedGK();
        }


        if (playerStates.rb.velocity.magnitude < 2 && jumpStarted > 0.25f)
        {
            playerStates.playerStateMachine = new Walk();
        }
        else
        {
            playerStates.rb.velocity = (playerStates.rb.velocity.magnitude - 40f * Time.deltaTime) * playerStates.rb.velocity.normalized;
        }
        jumpStarted += Time.deltaTime;
    }
    public void DoState(PlayerStates playerStates)
    {
        playerStates.HandPositionHandle(playerStates);
    }
    public void DoStateLateUpdate(PlayerStates playerStates)
    {
        playerStates.ControlCamera(playerStates);

        if (playerStates.TeamNumber[2] == "K".ToCharArray()[0] && Input.GetKey(KeyCode.Mouse1))
        {
            playerStates.cameraSpeed = 15;
        }
        else
        {
            playerStates.cameraSpeed = 30;
        }
    }

    
    public void BallTouchedGK(PlayerStates playerStates)
    {
        Ball.instance.WhichTeamTouchedLast = int.Parse(playerStates.TeamNumber[0].ToString());
        playerStates.GetComponent<PhotonView>().RPC("WhoControlsBallRPC", RpcTarget.All, playerStates.GetComponent<PhotonView>().ViewID);
        
    }
    
}
public class KeeperHoldingBall : IPlayerStateMachine
{
    PlayerStates stateForDestructor;
    public KeeperHoldingBall(PlayerStates playerStates)
    {
        Ball.instance.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
        playerStates.view.RPC("BallKinematicRPC", RpcTarget.All, true);
        //Ball.instance.transform.position = new Vector3(0f, 0.5f, 0f);
        Ball.instance.rb.velocity = Vector3.zero;
        stateForDestructor = playerStates;
        playerStates.view.RPC("HandsWeightRPC", RpcTarget.All, 0f, PhotonNetwork.NickName);
        playerStates.view.RPC("KeeperColliderRPC", RpcTarget.All, true);
        playerStates.animController.Play("GKHold");
    }
    
    public void CheckStateChange(PlayerStates playerStates)
    {
        if (Ball.instance.goalHappened)
        {
            ThrowBall(playerStates);
            playerStates.throwPower = 0;
        }
        if (Input.GetKey(KeyCode.Mouse0))
        {
            playerStates.throwPower += Time.deltaTime;
            playerStates.throwPower = Mathf.Clamp(playerStates.throwPower, 0f, 1f);
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            ThrowBall(playerStates);
            playerStates.throwPower = 0;
        }

        if (playerStates.throwPower > 0)
        {
            UIManager.instance.ThrowBar(playerStates.throwPower);
        }
    }
    public void DoState(PlayerStates playerStates)
    {
        WalkWithBall(playerStates);
    }
    public void DoStateLateUpdate(PlayerStates playerStates)
    {
        playerStates.ControlCamera(playerStates);
    }
    public void WalkWithBall(PlayerStates playerStates)
    {
        MoveGK(playerStates);
        playerStates.HandTransformGK.transform.Find("Right").position = playerStates.transform.position + playerStates.transform.up * 3.3f + playerStates.transform.forward * 1f + playerStates.transform.right * 0.4f;
        playerStates.HandTransformGK.transform.Find("Left").position = playerStates.transform.position + playerStates.transform.up * 3.3f + playerStates.transform.forward * 1f + playerStates.transform.right * -0.4f;
        Ball.instance.rb.position = playerStates.transform.position + playerStates.transform.up * 2.75f + playerStates.transform.forward * 0.65f + playerStates.transform.right * -0.125f;
    }
    public void ThrowBall(PlayerStates playerStates)
    {
        playerStates.view.RPC("HandsWeightRPC", RpcTarget.All, 1f, PhotonNetwork.NickName);
        playerStates.view.RPC("KeeperColliderRPC", RpcTarget.All, false);
        playerStates.GetComponent<PhotonView>().RPC("BallTransformToNullRPC", RpcTarget.All);
        playerStates.animController.SetTrigger("Throw");
        playerStates.ballThrowedCounter = 1.25f;
        playerStates.playerStateMachine = new Walk();
        playerStates.view.RPC("BallKinematicRPC", RpcTarget.All, false);

        Ball.instance.rb.velocity = playerStates.transform.forward * playerStates.throwPower * 45f + playerStates.transform.up * playerStates.throwPower * 15f;
        playerStates.GetComponent<PhotonView>().RPC("WhoControlToNullRPC", RpcTarget.All);
    }
    private void MoveGK(PlayerStates playerStates)
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        //vertical = 1;

        var dir = new Vector2(horizontal, vertical).normalized;
        if (dir.y < 0)
        {
            dir.y = dir.y / 3f;
        }
        dir.x = dir.x * 2f / 3f;
        var normalizedVelocity = Quaternion.AngleAxis(playerStates.transform.eulerAngles.y, -Vector3.forward) * dir * playerStates.moveSpeed;
        playerStates.rb.velocity = Vector3.Lerp(playerStates.rb.velocity, new Vector3(normalizedVelocity.x, playerStates.rb.velocity.y, normalizedVelocity.y), Time.deltaTime * 8);
    }
}



public class Walk : IPlayerStateMachine
{
    
    bool CheckBallCatch(PlayerStates playerStates)
    {
        if (playerStates.ballThrowedCounter > 0)
            return false;
        return ((playerStates.HandTransformGK.transform.Find("Right").position - Ball.instance.transform.position).magnitude < 2.5f) || ((playerStates.HandTransformGK.transform.Find("Left").position - Ball.instance.transform.position).magnitude < 2.5f);
    }
    public void CheckStateChange(PlayerStates playerStates)
    {
        if (playerStates.TeamNumber[2] == "K".ToCharArray()[0])
        {
            if (Input.GetKey(KeyCode.Space))
            {
                playerStates.jumpPower += Time.deltaTime * 3f;
                playerStates.jumpPower = Mathf.Clamp(playerStates.jumpPower, 0f, 1f);
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                playerStates.rb.velocity = Input.GetAxisRaw("Horizontal") * Mathf.Log(12 * playerStates.jumpPower, 10) * 40f * playerStates.transform.right + Input.GetAxisRaw("Vertical") * Mathf.Log(12 * playerStates.jumpPower, 10) * 18f * playerStates.transform.forward;
                if (Input.GetAxisRaw("Horizontal") == -1)
                {
                    playerStates.view.RPC("PlayJumpAnimRPC", RpcTarget.All, PhotonNetwork.NickName, true);
                }
                else if (Input.GetAxisRaw("Horizontal") == 1)
                {
                    playerStates.view.RPC("PlayJumpAnimRPC", RpcTarget.All, PhotonNetwork.NickName, false);
                }
                playerStates.playerStateMachine = new KeeperJumping(playerStates);
                playerStates.jumpPower = 0;
            }

            if (playerStates.jumpPower > 0)
            {
                UIManager.instance.JumpBar(playerStates.jumpPower);
            }

            if (CheckBallCatch(playerStates))
            {
                playerStates.BallCatchedGK();
            }
            
            return;
        }

        if (Ball.instance.whoControlsBall != null && Ball.instance.whoControlsBall == playerStates.gameObject && playerStates.TeamNumber.ToCharArray()[2].ToString() != "K")
        {
            playerStates.playerStateMachine = new BallControl();
        }
        if (Input.GetKeyDown(KeyCode.Space) && playerStates.KickPower<=0 && playerStates.stamina>=24 && playerStates.rb.velocity.magnitude > 4f)
        {
            playerStates.stamina -= 24;
            playerStates.slideDirection = playerStates.transform.forward;
            playerStates.slideTime = 1;
            playerStates.playerStateMachine = new Slide();
            playerStates.view.RPC("OpenSlideObjRPC", RpcTarget.All, playerStates.view.ViewID);
            playerStates.view.RPC("PlayClipPitchRPC", RpcTarget.All, GameAudio.instance.Slide.name, playerStates.transform.position.x, playerStates.transform.position.y, playerStates.transform.position.z, Random.Range(0.8f, 1.2f));
            playerStates.rb.velocity = 30 * playerStates.transform.forward;
        }
    }
    
    private void MoveGK(PlayerStates playerStates)
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        //vertical = 1;

        var dir = new Vector2(horizontal, vertical).normalized;
        if (dir.y < 0)
        {
            dir.y = dir.y *4f / 5f;
        }
        var normalizedVelocity = Quaternion.AngleAxis(playerStates.transform.eulerAngles.y, -Vector3.forward) * dir * playerStates.moveSpeed;
        playerStates.rb.velocity = Vector3.Lerp(playerStates.rb.velocity, new Vector3(normalizedVelocity.x, playerStates.rb.velocity.y, normalizedVelocity.y), Time.deltaTime * 8);
    }
    public void DoState(PlayerStates playerStates)
    {
        if (playerStates.TeamNumber[2] == "K".ToCharArray()[0])
        {
            playerStates.HandPositionHandle(playerStates);
            MoveGK(playerStates);
            return;
        }
        if (playerStates.transform.Find("Slide").gameObject.activeSelf)
        {
            playerStates.view.RPC("CloseSlideObjRPC", RpcTarget.All, playerStates.view.ViewID);
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        //vertical = 1;

        var dir = new Vector2(horizontal, vertical).normalized;
        if (dir.y < 0)
        {
            dir.y = dir.y / 3f;
        }
        dir.x = dir.x * 2f / 3f;

        //LetBallGo
        if (Input.GetKey(KeyCode.X))
        {
            playerStates.view.RPC("LetBallGoTrueRPC", RpcTarget.All, playerStates.view.ViewID);
        }
        else
        {
            playerStates.view.RPC("LetBallGoFalseRPC", RpcTarget.All, playerStates.view.ViewID);
        }

        //run
        if (dir.magnitude > 0 && Input.GetKey(KeyCode.LeftShift) && playerStates.stamina > 0 && playerStates.canRun)
        {
            playerStates.moveSpeed = Mathf.Lerp(playerStates.moveSpeed, 21, Time.deltaTime * 2);
            playerStates.DecraseStamina();
        }
        else
        {
            playerStates.moveSpeed = Mathf.Lerp(playerStates.moveSpeed, 10, Time.deltaTime * 2);
            playerStates.IncreaseStamina();
        }


        var normalizedVelocity = Quaternion.AngleAxis(playerStates.transform.eulerAngles.y, -Vector3.forward) * dir * playerStates.moveSpeed;
        playerStates.rb.velocity = Vector3.Lerp(playerStates.rb.velocity, new Vector3(normalizedVelocity.x, playerStates.rb.velocity.y, normalizedVelocity.y), Time.deltaTime * 8);

        Steal(playerStates);
        
        /*if (playerStates.stealBar > 0)
        {
            UIManager.instance.StealBar(playerStates.stealBar);
        }*/
    }
    
    void Steal(PlayerStates playerState)
    {
        if(playerState.canSteal && Input.GetKeyDown(KeyCode.Mouse2) && Ball.instance.whoControlsBall!=null && Ball.instance.BallTriggeredPlayers.Contains(playerState.gameObject) && Ball.instance.whoControlsBall.GetComponent<PlayerStates>().TeamNumber.ToCharArray()[0]!=playerState.TeamNumber.ToCharArray()[0])
        {
            var distanceHimself = (playerState.transform.position - Ball.instance.transform.position).magnitude;
            var distanceWhoControls = (Ball.instance.whoControlsBall.transform.position - Ball.instance.transform.position).magnitude;

            if (distanceHimself<=distanceWhoControls)
            {
                AudioSource.PlayClipAtPoint(GameAudio.instance.Successfull, Camera.main.transform.position + Vector3.up * 3f, PlayerPrefs.GetFloat("Sound"));
                Ball.instance.whoControlsBall.GetComponent<PlayerStates>().playerStateMachine = new Walk();
                AudioSource.PlayClipAtPoint(GameAudio.instance.Successfull, Camera.main.transform.position + Vector3.up * 3f, PlayerPrefs.GetFloat("Sound"));
                playerState.view.RPC("UnSuccessSoundRPC", Ball.instance.whoControlsBall.GetPhotonView().Owner);
                playerState.view.RPC("WhoControlToNullRPC", RpcTarget.All);
            }
            else
            {
                playerState.canSteal = false;
                GameObject.Find("UI").transform.Find("StealFailed").gameObject.SetActive(true);
                AudioSource.PlayClipAtPoint(GameAudio.instance.UnSuccessfull, Camera.main.transform.position + Vector3.up * 3f, PlayerPrefs.GetFloat("Sound"));
                playerState.StartCoroutine(playerState.StealBarOpen());
            }
            
        }
        
    }
    
    public void DoStateLateUpdate(PlayerStates playerStates)
    {
        playerStates.ControlCamera(playerStates);

        if (playerStates.TeamNumber[2] == "K".ToCharArray()[0] && Input.GetKey(KeyCode.Mouse1))
        {
            playerStates.cameraSpeed = 15;
        }
        else
        {
            playerStates.cameraSpeed = 30;
        }

    }
}

public class BallControl : IPlayerStateMachine
{
    
    public void CheckStateChange(PlayerStates playerStates)
    {
        if (Ball.instance.whoControlsBall == null || Ball.instance.whoControlsBall != playerStates.gameObject)
        {
            playerStates.playerStateMachine = new Walk();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0) && Ball.instance.transform.position.y < 2.75f)
        {
            playerStates.playerStateMachine = new Walk();
            playerStates.view.RPC("WhoControlToNullRPC", RpcTarget.All);

            Shoot(playerStates);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            playerStates.playerStateMachine = new Walk();
            playerStates.view.RPC("WhoControlToNullRPC", RpcTarget.All);
            playerStates.view.RPC("LetBallGoTrueRPC", RpcTarget.All, playerStates.view.ViewID);
        }
    }
    void Shoot(PlayerStates playerStates)
    {
        playerStates.animController.SetTrigger("Shoot");
        playerStates.chainIKCounter = 0.4f;
        playerStates.view.RPC("PlayClipPitchRPC", RpcTarget.All, GameAudio.instance.Kick.name, playerStates.transform.position.x, playerStates.transform.position.y, playerStates.transform.position.z, (30f - playerStates.KickPower) / 30f);
        //GameAudio.instance.PlayClip(GameAudio.instance.Kick, playerStates.transform.position, playerStates.KickPower/20f);

        Vector3 directionToShoot = Quaternion.AngleAxis(playerStates.transform.eulerAngles.y, -Vector3.forward) * new Vector2(playerStates.KickMouseX/2f, 15);
        directionToShoot.z = directionToShoot.y;
        directionToShoot.y = 0;

        
        directionToShoot = directionToShoot.normalized;
        Ball.instance.rb.angularVelocity = new Vector3(Ball.instance.rb.angularVelocity.x + playerStates.KickPower/4f, Ball.instance.rb.angularVelocity.y + playerStates.KickFalso /10f * playerStates.KickPower / 10f, Ball.instance.rb.angularVelocity.z);

        Ball.instance.rb.velocity = directionToShoot * playerStates.KickPower *1.7f + new Vector3(0, playerStates.KickMouseY, 0);
        Ball.instance.falso = Mathf.Abs(playerStates.KickFalso) * playerStates.KickPower * playerStates.KickPower /100f / 100f * 15f;
        if (playerStates.KickFalso > 0)
        {
            Ball.instance.falsoAxis = playerStates.transform.right;
        }
        else
        {
            Ball.instance.falsoAxis = -playerStates.transform.right;
        }


        playerStates.dribbleCounter = 0.6f;
        playerStates.KickPower = 0;
        playerStates.KickHeight = 0;
        playerStates.KickMouseX = 0;
        playerStates.KickMouseY = 0;
        playerStates.KickFalso = 0;
    }
    
    void Dribble(PlayerStates playerStates)
    {

        Vector3 distance = (Ball.instance.transform.position - playerStates.transform.position);
        distance.y = 0;
        Vector3 rotatedDistance = playerStates.transform.InverseTransformDirection(Ball.instance.transform.position - playerStates.transform.position);

        if (playerStates.dribbleCounter <= 0)
        {
            if (Input.GetKey(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse0) || playerStates.chainIKCounter > 0.2f) return;
            if (Input.GetKey(KeyCode.Mouse1))//toward yourself
            {
                playerStates.chainIKCounter = 0.4f;
                if (playerStates.rb.velocity.magnitude > 0.05f)
                {
                    playerStates.animController.SetTrigger("DribbleBack");
                }
                else
                {
                    playerStates.animController.SetTrigger("DribbleBackIdle");
                }
                playerStates.view.RPC("PlayClipPitchRPC", RpcTarget.All, GameAudio.instance.DribbleHimself.name, playerStates.transform.position.x, playerStates.transform.position.y, playerStates.transform.position.z, (30f - playerStates.rb.velocity.magnitude) / 30f);

                Ball.instance.rb.velocity = playerStates.rb.velocity.normalized*playerStates.rb.velocity.magnitude/15f + (-distance) * 2.5f;
                playerStates.dribbleCounter = 0.6f;
            }
            else if (Mathf.Abs(rotatedDistance.z) < 2f && playerStates.rb.velocity.magnitude > 0.05f)//toward forward
            {
                playerStates.chainIKCounter = 0.4f;
                playerStates.animController.SetTrigger("Dribble");
                //GameAudio.instance.PlayClip(GameAudio.instance.DribbleForward, playerStates.transform.position);
                if (playerStates.rb.velocity.magnitude > 0.1f)
                {
                    playerStates.view.RPC("PlayClipPitchRPC", RpcTarget.All, GameAudio.instance.DribbleForward.name, playerStates.transform.position.x, playerStates.transform.position.y, playerStates.transform.position.z, (30f - playerStates.rb.velocity.magnitude) / 30f);
                }
                
                distance = distance.normalized;
                Vector3 direction = playerStates.transform.forward - distance;
                Ball.instance.rb.velocity = (playerStates.rb.velocity.magnitude * playerStates.transform.forward * 2f + playerStates.rb.velocity.magnitude / 2f * direction) * 2 / 3f;
                playerStates.dribbleCounter = 0.6f;
            }
            
        }
        else
        {
            playerStates.dribbleCounter -= Time.deltaTime;
        }
    }

    public void DoState(PlayerStates playerStates)
    {
        if (playerStates.transform.Find("Slide").gameObject.activeSelf)
        {
            playerStates.view.RPC("CloseSlideObjRPC", RpcTarget.All, playerStates.view.ViewID);
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        var dir = new Vector2(horizontal, vertical).normalized;
        if (dir.y < 0)
        {
            dir.y = dir.y / 3f;
        }
        dir.x = dir.x / 2f;

        if (dir.magnitude > 0 && Input.GetKey(KeyCode.LeftShift) && playerStates.stamina > 0 && playerStates.canRun)
        {
            playerStates.moveSpeed = Mathf.Lerp(playerStates.moveSpeed, 23, Time.deltaTime * 2);
            playerStates.DecraseStamina();
        }
        else
        {
            playerStates.moveSpeed = Mathf.Lerp(playerStates.moveSpeed, 11, Time.deltaTime * 2);
            playerStates.IncreaseStamina();
        }

        

        

        var normalizedVelocity = Quaternion.AngleAxis(playerStates.transform.eulerAngles.y, -Vector3.forward) * dir * playerStates.moveSpeed;
        playerStates.rb.velocity = Vector3.Lerp(playerStates.rb.velocity, new Vector3(normalizedVelocity.x, playerStates.rb.velocity.y, normalizedVelocity.y), Time.deltaTime * 8);

        Dribble(playerStates);

    }
    public void DoStateLateUpdate(PlayerStates playerStates)
    {
        playerStates.ControlCamera(playerStates);
    }
}

public class Wait : IPlayerStateMachine
{
    public void CheckStateChange(PlayerStates playerStates)
    {
        if (playerStates.TeamNumber[2] == "K".ToCharArray()[0])
        {
            playerStates.playerStateMachine = new Walk();
        }
        if (!MatchState.instance.isGameStoppedForEvent)
        {
            playerStates.playerStateMachine = new Walk();
        }
    }

    public void DoState(PlayerStates playerStates)
    {
        playerStates.rb.velocity = new Vector3(0, playerStates.rb.velocity.y, 0);
    }
    public void DoStateLateUpdate(PlayerStates playerStates)
    {
        playerStates.ControlCamera(playerStates);
    }
}

public class ControlGameStop : IPlayerStateMachine
{
    public ControlGameStop(PlayerStates playerStates)
    {
        Ball.instance.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
        //Ball.instance.transform.position = new Vector3(0f, 0.5f, 0f);
        Ball.instance.rb.velocity = Vector3.zero;
    }
    public void CheckStateChange(PlayerStates playerStates)
    {
        if (!MatchState.instance.isGameStoppedForEvent)
        {
            playerStates.playerStateMachine = new Walk();
        }

        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            playerStates.view.RPC("IsKinematicRPC", RpcTarget.All, playerStates.view.ViewID, false);
            playerStates.view.RPC("GameStopColliderCloseRPC", RpcTarget.All, playerStates.view.ViewID);
            playerStates.view.RPC("GameContinueRPC", RpcTarget.All);
            playerStates.view.RPC("ToWalkRPC", RpcTarget.All);
            //playerStates.playerStateMachine = new BallControl();
            Shoot(playerStates);

        }
        else if(MatchState.instance.isGameStoppedForEvent)
        {
            Ball.instance.rb.velocity = new Vector3(0, 0, 0);
        }
        
    }
    void Shoot(PlayerStates playerStates)
    {
        playerStates.animController.SetTrigger("Shoot");
        playerStates.chainIKCounter = 0.4f;

        playerStates.view.RPC("PlayClipPitchRPC", RpcTarget.All, GameAudio.instance.Kick.name, playerStates.transform.position.x, playerStates.transform.position.y, playerStates.transform.position.z, (30f - playerStates.KickPower)/30f);
        //GameAudio.instance.PlayClip(GameAudio.instance.Kick, playerStates.transform.position, playerStates.KickPower/20f);

        Vector3 directionToShoot = Quaternion.AngleAxis(playerStates.transform.eulerAngles.y, -Vector3.forward) * new Vector2(playerStates.KickMouseX / 2f, 15);
        directionToShoot.z = directionToShoot.y;
        directionToShoot.y = 0;


        directionToShoot = directionToShoot.normalized;
        Ball.instance.rb.angularVelocity = new Vector3(Ball.instance.rb.angularVelocity.x + playerStates.KickPower / 4f, Ball.instance.rb.angularVelocity.y + playerStates.KickFalso / 10f * playerStates.KickPower / 10f, Ball.instance.rb.angularVelocity.z);

        Ball.instance.rb.velocity = directionToShoot * playerStates.KickPower * 1.7f + new Vector3(0, playerStates.KickMouseY, 0);
        Ball.instance.falso = Mathf.Abs(playerStates.KickFalso) * playerStates.KickPower* playerStates.KickPower / 100f / 100f * 15f;
        if (playerStates.KickFalso > 0)
        {
            Ball.instance.falsoAxis = playerStates.transform.right;
        }
        else
        {
            Ball.instance.falsoAxis = -playerStates.transform.right;
        }


        playerStates.dribbleCounter = 0.6f;
        playerStates.KickPower = 0;
        playerStates.KickHeight = 0;
        playerStates.KickMouseX = 0;
        playerStates.KickMouseY = 0;
        playerStates.KickFalso = 0;
    }
    public void DoState(PlayerStates playerStates)
    {
        playerStates.view.RPC("IsKinematicRPC", RpcTarget.All, playerStates.view.ViewID, true);
        playerStates.GetComponent<Rigidbody>().position = playerStates.ControlStopPosition;
        Ball.instance.GetComponent<Rigidbody>().position = playerStates.ControlStopBallPosition;
        if (playerStates.transform.Find("Slide").gameObject.activeSelf)
        {
            playerStates.view.RPC("CloseSlideObjRPC", RpcTarget.All, playerStates.view.ViewID);
        }

    }
    
    public void DoStateLateUpdate(PlayerStates playerStates)
    {
        playerStates.ControlCamera(playerStates);
    }
}

public class Slide : IPlayerStateMachine
{
    
    public void CheckStateChange(PlayerStates playerStates)
    {
        if (playerStates.slideTime <= 0)
        {
            playerStates.playerStateMachine = new Walk();
            playerStates.view.RPC("CloseSlideObjRPC", RpcTarget.All, playerStates.view.ViewID);

        }
        else
        {
            playerStates.slideTime -= Time.deltaTime;
        }
    }

    public void DoState(PlayerStates playerStates)
    {
        if (playerStates.rb.velocity.magnitude > 3)
        {
            playerStates.rb.velocity = playerStates.rb.velocity.magnitude * playerStates.transform.forward - playerStates.transform.forward * Time.deltaTime;
        }
        //faul handled on slide object
    }
    public void DoStateLateUpdate(PlayerStates playerStates)
    {
        
    }
}
public class Fall : IPlayerStateMachine
{
    public void CheckStateChange(PlayerStates playerStates)
    {
        if (playerStates.fallTimer <= 0)
        {
            playerStates.playerStateMachine = new Walk();
        }
        else
        {
            playerStates.fallTimer -= Time.deltaTime;
        }
    }

    public void DoState(PlayerStates playerStates)
    {
        playerStates.rb.velocity = new Vector3(0, playerStates.rb.velocity.y, 0);
    }
    public void DoStateLateUpdate(PlayerStates playerStates)
    {
        playerStates.ControlCamera(playerStates);
    }
}