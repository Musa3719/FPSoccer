using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;

public class UIManager : MonoBehaviourPunCallbacks
{
    public bool isStopScreenOpen;
    public static UIManager instance;

    [SerializeField]
    private GameObject MessagePrefab;
    public TMP_InputField MessageInput;
    private Queue<GameObject> MessageQueue;

    private bool isFocusedInputLastFrame;
    private Coroutine lastCoroutine;
    private Coroutine closePowerCoroutine;
    private Coroutine stealCoroutine;
    private Coroutine jumpCoroutine;
    private Coroutine throwCoroutine;

    private void Awake()
    {
        instance = this;
        MessageQueue = new Queue<GameObject>();
        isFocusedInputLastFrame = MessageInput.isFocused;
    }

    // Update is called once per frame
    void Update()
    {
        CheckForBallControl();
        CheckForStopScreen();
        CheckForMessageSent();
        ArrangeTimeUI();
        isFocusedInputLastFrame = MessageInput.isFocused;
    }
    void CheckForBallControl()
    {
        if (Ball.instance.whoControlsBall == null)
        {
            transform.Find("BallControl").gameObject.SetActive(false);
        }
        else if (Ball.instance.whoControlsBall.GetComponent<PhotonView>().IsMine)
        {
            transform.Find("BallControl").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("BallControl").gameObject.SetActive(false);
        }
    }
    public void ArrangeStaminaUI(float value)
    {
        GameObject.Find("Stamina1").GetComponent<Image>().fillAmount = value;
        GameObject.Find("Stamina2").GetComponent<Image>().fillAmount = value;
    }
    public void JumpBar(float jumpValue)
    {
        if (!transform.Find("Jump").gameObject.activeSelf)
        {
            transform.Find("Jump").gameObject.SetActive(true);
        }
        transform.Find("Jump").GetComponent<Image>().fillAmount = jumpValue / 1f;

        if (jumpCoroutine != null)
            StopCoroutine(jumpCoroutine);
        jumpCoroutine = StartCoroutine(JumpCoroutine());
    }
    IEnumerator JumpCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        transform.Find("Jump").gameObject.SetActive(false);
    }
    public void ThrowBar(float throwValue)
    {
        if (!transform.Find("Throw").gameObject.activeSelf)
        {
            transform.Find("Throw").gameObject.SetActive(true);
        }
        transform.Find("Throw").GetComponent<Image>().fillAmount = throwValue / 1f;

        if (throwCoroutine != null)
            StopCoroutine(throwCoroutine);
        throwCoroutine = StartCoroutine(ThrowCoroutine());
    }
    IEnumerator ThrowCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        transform.Find("Throw").gameObject.SetActive(false);
    }
    public void StealBar(float stealValue)
    {
        if (!transform.Find("Steal").gameObject.activeSelf)
        {
            transform.Find("Steal").gameObject.SetActive(true);
        }
        transform.Find("Steal").GetComponent<Image>().fillAmount = stealValue * 2f;

        if (stealCoroutine != null)
            StopCoroutine(stealCoroutine);
        stealCoroutine = StartCoroutine(StealCoroutine());
    }
    IEnumerator StealCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        transform.Find("Steal").gameObject.SetActive(false);   
    }
    public void ArrangePowerHeightUI(float power, float height, float falso, float horizontal)
    {
        
        if (!transform.Find("Power").gameObject.activeSelf || !transform.Find("Height").gameObject.activeSelf || !transform.Find("FalsoBar").gameObject.activeSelf || !transform.Find("Direction").gameObject.activeSelf)
        {
            transform.Find("Power").gameObject.SetActive(true);
            transform.Find("Height").gameObject.SetActive(true);
            transform.Find("FalsoBar").gameObject.SetActive(true);
            transform.Find("Direction").gameObject.SetActive(true);
        }

        transform.Find("Power").GetComponent<Image>().fillAmount = power / 100f;
        transform.Find("Height").GetComponent<Image>().fillAmount = height / 25f;
        var tempPos = GetComponent<RectTransform>().anchoredPosition;
        transform.Find("FalsoBar").transform.Find("Falso").GetComponent<RectTransform>().anchoredPosition = new Vector2(falso, 0);
        float angle = Vector2.SignedAngle(Vector2.up, new Vector2(horizontal, 15));
        transform.Find("Direction").transform.eulerAngles = new Vector3(0, 0, angle);

        if(closePowerCoroutine!=null)
            StopCoroutine(closePowerCoroutine);
        closePowerCoroutine = StartCoroutine(ClosePowerUI());

    }
    IEnumerator ClosePowerUI()
    {
        yield return new WaitForSeconds(1.2f);
        transform.Find("Power").gameObject.SetActive(false);
        transform.Find("Height").gameObject.SetActive(false);
        transform.Find("FalsoBar").gameObject.SetActive(false);
        transform.Find("Direction").gameObject.SetActive(false);
    }
    void ArrangeTimeUI()
    {
        GameObject.Find("Time").GetComponent<TextMeshProUGUI>().text = MatchState.instance.minutesPast.ToString("00") + ":" + MatchState.instance.secondPast.ToString("00") + "  T1 " + MatchState.instance.T1Score.ToString() + "-" + MatchState.instance.T2Score.ToString() + " T2";
    }
    void CheckForMessageSent()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!transform.Find("ChatParent").gameObject.activeSelf)
            {
                transform.Find("ChatParent").gameObject.SetActive(true);
            }

            if (!isFocusedInputLastFrame)
            {
                MessageInput.ActivateInputField();
            }
            else if (isFocusedInputLastFrame && MessageInput.text != string.Empty)
            {
                
                MessageSend(MessageInput.text);
                MessageInput.text = string.Empty;;
            }
            else
            {
                if (lastCoroutine != null)
                    StopCoroutine(lastCoroutine);
                lastCoroutine = StartCoroutine("CloseChatLater");
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
        if (!transform.Find("ChatParent").Find("Chat").gameObject.activeSelf) return;
        if (lastCoroutine!=null)
            StopCoroutine(lastCoroutine);
        transform.Find("ChatParent").gameObject.SetActive(true);
        GameObject newMessage = Instantiate(MessagePrefab, transform.Find("ChatParent").Find("Chat").Find("Viewport").Find("Content"));
        newMessage.GetComponent<TextMeshProUGUI>().text = message;
        MessageQueue.Enqueue(newMessage);
        if (MessageQueue.Count > 18)
        {
            Destroy(MessageQueue.Dequeue());
        }
        lastCoroutine = StartCoroutine("CloseChatLater");
    }
    IEnumerator CloseChatLater()
    {
        yield return new WaitForSeconds(6);
        transform.Find("ChatParent").gameObject.SetActive(false);
    }
    void CheckForStopScreen()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isStopScreenOpen)
            {
                ResumeGame();
            }
            else if(!MatchState.instance.isGameFinished)
            {
                StopGame();
            }
        }
    }
    public void ResumeGame()
    {
        Cursor.visible = false;
        isStopScreenOpen = false;
        transform.Find("StopScreen").gameObject.SetActive(false);
    }
    public void StopGame()
    {
        Cursor.visible = true;
        isStopScreenOpen = true;
        transform.Find("StopScreen").gameObject.SetActive(true);
    }
    public void ExitGame()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("MessageSendRPC", RpcTarget.All, otherPlayer.NickName + " Left The Game...");
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("MessageSendRPC", RpcTarget.All, newPlayer.NickName + " Entered The Game");
        }
    }
}
