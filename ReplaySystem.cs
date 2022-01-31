using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ReplaySystem : MonoBehaviour
{
	private Queue<MyKeyFrame> keyFrames;
	private Rigidbody rb;
	private GameObject[] players;
	[SerializeField]
	MonoBehaviour script;
	public bool willPlay;
	private float timePass;
	private int frameCount;
	private Dictionary<int, string> playerView›d;
    private void Awake()
    {
		rb = GetComponent<Rigidbody>();
	}
    void Start()
	{
		if (!PhotonNetwork.IsMasterClient) return;

		timePass = 0;
		keyFrames = new Queue<MyKeyFrame>();
		
		willPlay = false;
	}
	public void StartPlayBack()
    {
		willPlay = true;
		playerView›d = new Dictionary<int, string>();
		players = GameObject.FindGameObjectsWithTag("Player");
		foreach (var player in players)
		{
			playerView›d.Add(player.GetInstanceID(), player.GetPhotonView().Controller.NickName);
			player.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
			player.GetComponent<Rigidbody>().isKinematic = true;
			player.GetComponent<ReplaySystem>().script.enabled = false;
			GetComponent<PhotonView>().RPC("PlayBackStartedRPC", RpcTarget.All);
		}
		Ball.instance.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
		Ball.instance.rb.isKinematic = true;
		timePass = 0;
		Ball.instance.GetComponent<ReplaySystem>().script.enabled = false;
		frameCount = keyFrames.Count;
	}
	
	void Update()
	{
		if (!PhotonNetwork.IsMasterClient || !GetComponent<PhotonView>().IsMine) return;
		
		if (Input.GetKeyDown(KeyCode.G))//debug amaÁl˝
        {
			StartPlayBack();
		}
		if (!willPlay)
		{
			Record();
		}
		else
		{
			PlayBack();
		}
	}

	void PlayBack()
	{

		var frame = keyFrames.Dequeue();
        if (keyFrames.Count + 10 >= frameCount)
        {
			transform.position = frame.position;
		}
        else
        {
			transform.position = Vector3.Lerp(transform.position, frame.position, 0.6f);
		}

		if (gameObject.CompareTag("Ball"))
        {
			transform.eulerAngles = new Vector3(transform.eulerAngles.x + frame.angularVel.x/10f, transform.eulerAngles.y + frame.angularVel.y/10f, transform.eulerAngles.z + frame.angularVel.z/10f);
			//transform.eulerAngles -= frame.angularVel.normalized * 0.01f * Time.deltaTime;
        }
		else
        {
			transform.eulerAngles = frame.rotation;
        }



	    if (keyFrames.Count == 0)
        {
			PlayBackEnd();
		}
	}
	void PlayBackEnd()
    {

		GetComponent<PhotonView>().RPC("PlayBackEndedRPC", RpcTarget.All);
		
		Ball.instance.rb.isKinematic = false;
		Ball.instance.GetComponent<ReplaySystem>().script.enabled = true;
		players = GameObject.FindGameObjectsWithTag("Player");
		foreach (var player in players)
		{
			player.GetComponent<ReplaySystem>().script.enabled = true;
			player.GetComponent<Rigidbody>().isKinematic = false;
			string playerName = playerView›d[player.GetInstanceID()];
			Photon.Realtime.Player ply = null;
			foreach (var p in PhotonNetwork.PlayerList)
			{
				if (p.NickName == playerName)
				{
					ply = p;
				}
			}
			if (ply != null)
				player.GetComponent<PhotonView>().TransferOwnership(ply);
			
		}
		
		//MatchState.instance.Kickoff(MatchState.instance.teamWhoTakeGoal);
	}
	void Record()
	{
		timePass += Time.deltaTime;
		rb.isKinematic = false;

		keyFrames.Enqueue(new MyKeyFrame(transform.position, rb.angularVelocity, transform.eulerAngles));
		if (timePass > 8)
        {
			keyFrames.Dequeue();
        }
	}
	[PunRPC]
	public void PlayBackStartedRPC()
    {
		var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().IsMine)
            {
				var renderers = item.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach (var rend in renderers)
				{
					rend.enabled = true;
				}
				item.GetComponent<PlayerStates>().enabled = false;
				GameObject.FindObjectOfType<MatchState>().replayMode = true;
				break;
			}
        }
    }
	[PunRPC]
	public void PlayBackEndedRPC()
	{
		willPlay = false;
		keyFrames = new Queue<MyKeyFrame>();
		Invoke("GoalHappenedFalse", 1);
		var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().IsMine)
            {
				var renderers = item.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach (var rend in renderers)
				{
					if (!item.CompareTag("Legs"))
						rend.enabled = false;
				}
				item.GetComponent<PlayerStates>().enabled = true;
				GameObject.FindObjectOfType<MatchState>().replayMode = false;
				GameObject.FindObjectOfType<MatchState>().isGameStoppedForEvent = true;
			}
			
		}
	}
	public void GoalHappenedFalse()
    {
		var players = GameObject.FindGameObjectsWithTag("Player");
		foreach (var item in players)
		{
			if (item.GetPhotonView().IsMine)
				item.GetComponent<PlayerStates>().view.RPC("GoalHappenedRPC", RpcTarget.All, false);
		}
	}
}

public struct MyKeyFrame
{
	public Vector3 position;
	public Vector3 angularVel;
	public Vector3 rotation;

	public MyKeyFrame(Vector3 pos, Vector3 angular, Vector3 rot)
	{
		position = pos;
		angularVel = angular;
		rotation = rot;
	}

}