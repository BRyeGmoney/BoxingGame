using System;
using UnityEngine;
using System.Collections;
using Photon;

public class boxerScript : Photon.MonoBehaviour {

    public float moveSpeed = 50f;//0.6f;
    private Animator boxerAnimator;

    private bool punching = false;
    private bool rightJab = false;
    private bool leftJab = false;
    private bool uCut = false;
    private bool isHit = false;

    public int controlNumStart = -1;
    public float BoxerHealth = 100;
    public float DamageMod = 1f;

    public _2dxFX_EnergyBar lifeBar;
    public int invert = 1;

    private Vector3 correctPlayerPos;
    private Vector3 correctPlayerRot;

    private Rigidbody boxerRb;
    public float maxVelocity;
    private float sqrMaxVel;

    public bool Punching
    {
        get { return punching; }
    }

	// Use this for initialization
	void Start () {
        boxerAnimator = gameObject.GetComponent<Animator>();
        boxerRb = gameObject.GetComponent<Rigidbody>();
        sqrMaxVel = maxVelocity * maxVelocity;
        //lifeBarDissolve = lifeBar.GetComponent<SpriteRenderer>().material;
	}

    public void SetPlayerNum(int pNum)
    {
        if (pNum == 0)
            lifeBar = OverlordScript.instance.leftBar;
        else
            lifeBar = OverlordScript.instance.rightBar;
    }
	
	// Update is called once per frame
	void Update () {
        if (OverlordScript.instance.online && !photonView.isMine)
        {
            transform.position = Vector3.Lerp(transform.position, this.correctPlayerPos, Time.deltaTime * 10);
        }


        if (controlNumStart > -1)
        {
            float xMove = Input.GetAxis(((Controls)controlNumStart).ToString()); //"Horizontal" + playerNum);
            float yMove = Input.GetAxis(((Controls)controlNumStart + 1).ToString());//"Vertical" + playerNum);
            bool punch = Input.GetButtonDown(((Controls)controlNumStart + 2).ToString());//"Punch" + playerNum);
            bool upCut = Input.GetButtonDown(((Controls)controlNumStart + 3).ToString());//"UpCut" + playerNum);

            punching = rightJab || leftJab || uCut;

            if (!punch && !punching)
            {
                //gameObject.transform.Translate(new Vector3(xMove, 0, yMove) * invert * moveSpeed);
                boxerRb.AddForce(new Vector3(xMove * moveSpeed, 0, yMove * moveSpeed) * invert, ForceMode.Force);
            }


            if (punch && !isHit)
            {
                if (!rightJab) //if we're not punching yet
                {
                    DoRightJab();
                }
                else if (!leftJab)//if we are, then we need a left jab
                {
                    DoLeftJab(true);
                }
                else
                {
                    DoLeftJab(false);
                }
            }

            if (upCut && !punching && !isHit)
            {
                DoUpperCut();
            }
        }

        if (BoxerHealth <= 0)
            OverlordScript.instance.ReloadScene();
	}

    void FixedUpdate()
    {
        Vector3 vel = boxerRb.velocity;
        //clamp velocity
        if (vel.sqrMagnitude > sqrMaxVel) //equivalent to vel.mag > maxVel, but faster this way
        {
            //vector3.normalized returns this vector with a magnitude
            //of 1. This ensures that we're not messing with the 
            //direction of the vector, only its magnitude
            boxerRb.velocity = vel.normalized * maxVelocity;
        }
    }

    private void DoRightJab()
    {
        rightJab = true;
        boxerAnimator.SetTrigger("RightJab");
    }

    private void DoLeftJab(bool doJab)
    {
        leftJab = doJab;
        boxerAnimator.SetBool("LeftJab", doJab);
    }

    private void DoUpperCut()
    {
        uCut = true;
        boxerAnimator.SetTrigger("Uppercut");
    }

    public void DoneRightJab()
    {
        rightJab = false;
    }

    public void DoneLeftJab()
    {
        leftJab = false;
        boxerAnimator.SetBool("LeftJab", false);
    }

    public void DoneUpperCut()
    {
        uCut = false;
    }

    [PunRPC]
    public void GetHit(int damage, int playerID)
    {
        if (!isHit)
        {
            isHit = true;
            boxerAnimator.SetTrigger("Hit");
            BoxerHealth -= damage * DamageMod;

            lifeBar.BarProgress = BoxerHealth / 100f;
            //lifeBarDissolve.SetFloat(BeautifulDissolves.DissolveHelper.dissolveAmountID, 1f - (BoxerHealth / 140f));
            //Debug.Log(BoxerHealth + ", " + (1f - (BoxerHealth / 140f)));
        }
    }


    public void GetHit(int damage)
    {
        if (!isHit)
        {
            isHit = true;
            boxerAnimator.SetTrigger("Hit");
            BoxerHealth -= damage * DamageMod;

            lifeBar.BarProgress = BoxerHealth / 100f;
            //lifeBarDissolve.SetFloat(BeautifulDissolves.DissolveHelper.dissolveAmountID, 1f - (BoxerHealth / 140f));
            //Debug.Log(BoxerHealth + ", " + (1f - (BoxerHealth / 140f)));
        }
    }

    public void DoneHit()
    {
        isHit = false;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            //stream.SendNext(transform.localEulerAngles);
            stream.SendNext(rightJab);
            stream.SendNext(leftJab);
            stream.SendNext(uCut);
        }
        else
        {
            this.correctPlayerPos = (Vector3)stream.ReceiveNext();
            //this.correctPlayerRot = (Vector3)stream.ReceiveNext();

            if ((bool)stream.ReceiveNext())
                DoRightJab();

            DoLeftJab((bool)stream.ReceiveNext());

            if ((bool)stream.ReceiveNext())
                DoUpperCut();
        }
    }
}

public enum Controls
{
    Horizontal1 = 0,
    Vertical1 = 1,
    Punch1 = 2,
    UpCut1 = 3,
    Horizontal2 = 4,
    Vertical2 = 5,
    Punch2 = 6,
    UpCut2 = 7
}