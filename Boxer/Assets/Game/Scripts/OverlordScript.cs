using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class OverlordScript : MonoBehaviour {

    public GameObject boxer1;
    public GameObject boxer2;
    public GameObject boxerObject;

    public _2dxFX_EnergyBar leftBar;
    public _2dxFX_EnergyBar rightBar;

    public GameObject startTextLeft;
    public GameObject startTextRight;

    public GameState currentGameState = GameState.StartScreen;

    public bool online = false;

    private bool p1Playing = false;
    private bool p2Playing = false;
    private bool p1OnlineControlsJP = true;

    public static OverlordScript instance;

	// Use this for initialization
	void Start () {
        //PhotonNetwork.ConnectUsingSettings("v4.2");
        DOTween.Init();
        PhotonNetwork.ConnectUsingSettings("v4.2");

        instance = this;
    }

    void Awake()
    {
        TextMesh pressStart = GameObject.Find("PressStart").GetComponent<TextMesh>();
        DOTween.ToAlpha(() => pressStart.color, x => pressStart.color = x, 0, 1).SetLoops(-1, LoopType.Yoyo);
    }
	
	// Update is called once per frame
	void Update () {
        

        if (currentGameState.Equals(GameState.StartScreen))
            UpdateStartScreenState();
        else if (currentGameState.Equals(GameState.GameScreen))
            UpdateStartGameState();
    }

    private void UpdateStartScreenState()
    {
        bool playerOneStart = Input.GetButtonDown("SubmitJP1") || Input.GetButtonDown("SubmitKB1");

        if (playerOneStart)
        {
            Camera.main.gameObject.GetComponent<Animator>().SetTrigger("StartGame");
            currentGameState = GameState.StartToGameAnim;

            GameObject[] startTexts = GameObject.FindGameObjectsWithTag("StartScreenText");

            foreach (GameObject textObj in startTexts)
            {
                textObj.SetActive(false);
            }
        }
    }

    private void UpdateStartGameState()
    {
        bool playerOneJPStart = Input.GetButtonDown("SubmitJP1");
        bool playerKBStart = Input.GetButtonDown("SubmitKB1");
        bool playerTwoJPStart = Input.GetButtonDown("SubmitJP2");
        bool playerOneConnectJP = Input.GetButtonDown("ConnectJP");
        bool playerOneConnectKB = Input.GetButtonDown("ConnectKB");

        if ((playerOneJPStart || playerKBStart) && !p1Playing)
        {
            startTextLeft.SetActive(false);

            boxer1 = (GameObject)Instantiate(boxerObject, new Vector3(-0.89f, 3.55f, 1.48f), Quaternion.identity);
            boxerScript b1 = boxer1.GetComponent<boxerScript>();
            if (playerOneJPStart)
                b1.SetPlayerNum(1, "jp", online);
            else if (playerKBStart)
                b1.SetPlayerNum(1, "kb", online);
            p1Playing = true;
        }
        else if ((playerTwoJPStart || (playerKBStart && p1Playing)) && !p2Playing) //if player two starts from the jp, or there is a player one already and 
        {
            startTextRight.SetActive(false);

            boxer2 = (GameObject)Instantiate(boxerObject, new Vector3(3.33f, 3.55f, 1.48f), Quaternion.identity);
            boxer2.transform.localEulerAngles = new Vector3(0f, -180f, 0f);
            boxerScript b2 = boxer2.GetComponent<boxerScript>();

            if (playerTwoJPStart)
                b2.SetPlayerNum(2, "jp", online);
            else
                b2.SetPlayerNum(2, "kb", online);
            p2Playing = true;

            PhotonNetwork.Disconnect();
        }

        if ((playerOneConnectJP || playerOneConnectKB) && !p1Playing && !p2Playing)//only allow a connection if p2 hasn't started playing
        {
            if (playerOneConnectJP)
                p1OnlineControlsJP = true;
            else
                p1OnlineControlsJP = false;

            online = true;
            //try to join game
            PhotonNetwork.JoinOrCreateRoom("BoxingGame", new RoomOptions() { isOpen = true, isVisible = true, maxPlayers = 2 }, TypedLobby.Default);
        }

        if (online && (!boxer1 || !boxer2))
        {
            if (!boxer1)
            {
                GameObject boxer = GameObject.FindGameObjectWithTag("LeftBoxer");
                if (boxer)
                {
                    boxer1 = boxer;
                    boxer1.GetComponent<boxerScript>().lifeBar = leftBar;
                }
            }
            else if (!boxer2)
            {
                GameObject boxer = GameObject.FindGameObjectWithTag("RightBoxer");
                if (boxer)
                {
                    boxer2 = boxer;
                    boxer2.GetComponent<boxerScript>().lifeBar = rightBar;
                }
            }
        }
    }

    void OnDisconnectedFromPhoton()
    {
        PhotonNetwork.offlineMode = true;
    }

    public void ReloadScene()
    {
        PhotonNetwork.LoadLevel(0);
    }

    void OnJoinedRoom()
    {
        Text leftText = startTextLeft.GetComponent<Text>();
        leftText.text = "Connected!";
        leftText.DOFade(0, 0.4f).SetLoops(3);
        startTextRight.SetActive(false);

        if(Array.IndexOf(PhotonNetwork.playerList, PhotonNetwork.player) == 0)
        {
            boxer1 = PhotonNetwork.Instantiate("Boxer", new Vector3(-0.89f, 3.55f, 1.48f), Quaternion.identity, 0);
            boxerScript b1 = boxer1.GetComponent<boxerScript>();

            if (p1OnlineControlsJP)
                b1.SetPlayerNum(1, "jp", online);
            else
                b1.SetPlayerNum(1, "kb", online);
        }
        else
        {
            boxer2 = PhotonNetwork.Instantiate("BoxerRight", new Vector3(3.33f, 3.55f, 1.48f), Quaternion.Euler(0f, 180f, 0f), 0);

            boxerScript b2 = boxer2.GetComponent<boxerScript>();

            if (p1OnlineControlsJP)
                b2.SetPlayerNum(2, "jp", online);
            else
                b2.SetPlayerNum(2, "kb", online);
        }
    }
}

public enum GameState
{
    StartScreen,
    StartToGameAnim,
    GameStartScreen,
    GameScreen
}
