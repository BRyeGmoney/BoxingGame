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
        bool playerOneStart = Input.GetButtonDown("Submit1");
        bool playerTwoStart = Input.GetButtonDown("Submit2");
        bool playerOneConnect = Input.GetButtonDown("Connect");

        if (currentGameState.Equals(GameState.StartScreen))
            UpdateStartScreenState(playerOneStart);
        else if (currentGameState.Equals(GameState.GameScreen))
            UpdateGameState(playerOneStart, playerTwoStart, playerOneConnect);
    }

    private void UpdateStartScreenState(bool playerOneStart)
    {
        if (playerOneStart)
        {
            Camera.main.gameObject.GetComponent<Animator>().SetTrigger("StartGame");
            currentGameState = GameState.StartToGameAnim;

            GameObject[] startTexts = GameObject.FindGameObjectsWithTag("StartScreenText");

            foreach (GameObject textObj in startTexts)
            {
                //TextMesh pressStart = textObj.GetComponent<TextMesh>();
                textObj.SetActive(false);
            }
        }
    }

    private void UpdateGameState(bool playerOneStart, bool playerTwoStart, bool playerOneConnect)
    {
        if (playerOneStart && !p1Playing)
        {
            startTextLeft.SetActive(false);

            boxer1 = (GameObject)Instantiate(boxerObject, new Vector3(-0.89f, 3.55f, 1.48f), Quaternion.identity);
            boxerScript b1 = boxer1.GetComponent<boxerScript>();
            b1.controlNumStart = 0;
            b1.lifeBar = leftBar;
            p1Playing = true;
        }
        if (playerTwoStart && !p2Playing)
        {
            startTextRight.SetActive(false);

            boxer2 = (GameObject)Instantiate(boxerObject, new Vector3(3.33f, 3.55f, 1.48f), Quaternion.identity);
            boxer2.transform.localEulerAngles = new Vector3(0f, -180f, 0f);
            boxerScript b2 = boxer2.GetComponent<boxerScript>();
            b2.controlNumStart = 4;
            b2.lifeBar = rightBar;
            p2Playing = true;
            PhotonNetwork.Disconnect();
        }

        if (playerOneConnect && !p1Playing && !p2Playing)//only allow a connection if p2 hasn't started playing
        {

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
            b1.controlNumStart = 0;
            b1.lifeBar = leftBar;
            b1.SetPlayerNum(0);
        }
        else
        {
            boxer2 = PhotonNetwork.Instantiate("BoxerRight", new Vector3(3.33f, 3.55f, 1.48f), Quaternion.Euler(0f, 180f, 0f), 0);
            //boxer1 = GameObject.FindGameObjectsWithTag("Player")[0];
            //boxer1.GetComponent<boxerScript>().lifeBar = leftBar;
            //boxer2.GetComponent<SpriteRenderer>().flipX = true;
            boxerScript b2 = boxer2.GetComponent<boxerScript>();
            b2.SetPlayerNum(1);
            //b2.invert = -1;
            //b2.controlNumStart = 0;
            //boxer1.GetComponent<boxerScript>().lifeBar = leftBar;
            //b2.lifeBar = rightBar;
        }
    }
}

public enum GameState
{
    StartScreen,
    StartToGameAnim,
    GameScreen
}
