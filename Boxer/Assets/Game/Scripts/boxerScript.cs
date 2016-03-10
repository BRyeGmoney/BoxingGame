using System;
using UnityEngine;
using System.Collections;
using Photon;

public class boxerScript : Photon.MonoBehaviour {

    public float moveSpeed = 50f;//0.6f;
    private Animator boxerAnimator;

    private bool lastPunchLanded;
    private bool punching = false;
    private bool rightJab = false;
    private bool leftJab = false;
    private bool uCut = false;
    private bool isHit = false;

    //public int controlNumStart = -1;
    private Controls[] controlChoice;
    private bool controlsSet = false;
    public float BoxerHealth = 100;
    public float DamageMod = 1f;

    public _2dxFX_EnergyBar lifeBar;
    public ParticleSystem blood;
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
	}

    public void SetPlayerNum(int pNum, string inputType, bool isOnline)
    {
        controlChoice = new Controls[4];

        if (pNum == 1)
        {
            lifeBar = OverlordScript.instance.leftBar;

            if (inputType == "jp")
                SetPlayerOneAsJP();
            else
                SetPlayerOneAsKB();
        }
        else
        { 
            lifeBar = OverlordScript.instance.rightBar;

            if (!isOnline) //if we're on the same screen, you are player two
            {
                if (inputType == "jp")
                    SetPlayerTwoAsJP();
                else
                    SetPlayerTwoAsKB();
            }
            else //if we're online, you're input is still player one
            {
                if (inputType == "jp")
                    SetPlayerOneAsJP();
                else
                    SetPlayerOneAsKB();
            }
        }

        controlsSet = true;
    }
	
	// Update is called once per frame
	void Update () {
        if (OverlordScript.instance.online && !photonView.isMine)
        {
            transform.position = Vector3.Lerp(transform.position, this.correctPlayerPos, Time.deltaTime * 10);
        }


        if (controlsSet)
        {
            
            bool punch = Input.GetButtonDown(controlChoice[2].ToString());//"Punch" + playerNum);
            bool upCut = Input.GetButtonDown(controlChoice[3].ToString());//"UpCut" + playerNum);

            punching = punch || rightJab || leftJab || uCut;

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

    private void ShootBlood()
    {
        //blood.Stop();
        blood.Play();
    }

    void FixedUpdate()
    {
        if (controlsSet)
        {
            float xMove = Input.GetAxis(controlChoice[0].ToString()); //"Horizontal" + playerNum);
            float yMove = Input.GetAxis(controlChoice[1].ToString());//"Vertical" + playerNum);
            Vector3 vel = boxerRb.velocity;

            //clamp velocity
            if (vel.sqrMagnitude > sqrMaxVel) //equivalent to vel.mag > maxVel, but faster this way
            {
                //vector3.normalized returns this vector with a magnitude
                //of 1. This ensures that we're not messing with the 
                //direction of the vector, only its magnitude
                boxerRb.velocity = vel.normalized * maxVelocity;
            }

            if (!punching)
            {
                //gameObject.transform.Translate(new Vector3(xMove, 0, yMove) * invert * moveSpeed);
                boxerRb.AddForce(new Vector3(xMove * moveSpeed, 0, yMove * moveSpeed) * invert, ForceMode.Force);
            }
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

    public void LandedPunch()
    {
        lastPunchLanded = true;
    }

    [PunRPC]
    public void GetHit(int damage, Vector3 punchDir, int playerID)
    {
        if (!isHit)
        {
            isHit = true;
            boxerAnimator.SetTrigger("Hit");
            BoxerHealth -= damage * DamageMod;

            lifeBar.BarProgress = BoxerHealth / 100f;
            boxerRb.AddForce(new Vector3(punchDir.x, 1, 0), ForceMode.Impulse);
            //lifeBarDissolve.SetFloat(BeautifulDissolves.DissolveHelper.dissolveAmountID, 1f - (BoxerHealth / 140f));
            //Debug.Log(BoxerHealth + ", " + (1f - (BoxerHealth / 140f)));
        }
    }


    public void GetHit(int damage, Vector3 punchDir)
    {
        if (!isHit)
        {
            isHit = true;
            boxerAnimator.SetTrigger("Hit");
            BoxerHealth -= damage * DamageMod;

            lifeBar.BarProgress = BoxerHealth / 100f;
            boxerRb.AddForce(new Vector3(punchDir.x, 1, 0), ForceMode.Impulse);
            ShootBlood();
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

    void SetPlayerOneAsJP()
    {
        controlChoice[0] = Controls.HorizontalJP1;
        controlChoice[1] = Controls.VerticalJP1;
        controlChoice[2] = Controls.PunchJP1;
        controlChoice[3] = Controls.UpCutJP1;
    }

    void SetPlayerOneAsKB()
    {
        controlChoice[0] = Controls.HorizontalKB1;
        controlChoice[1] = Controls.VerticalKB1;
        controlChoice[2] = Controls.PunchKB1;
        controlChoice[3] = Controls.UpCutKB1;
    }

    void SetPlayerTwoAsJP()
    {
        controlChoice[0] = Controls.HorizontalJP2;
        controlChoice[1] = Controls.VerticalJP2;
        controlChoice[2] = Controls.PunchJP2;
        controlChoice[3] = Controls.UpCutJP2;
    }

    void SetPlayerTwoAsKB()
    {
        controlChoice[0] = Controls.HorizontalKB2;
        controlChoice[1] = Controls.VerticalKB2;
        controlChoice[2] = Controls.PunchKB1;
        controlChoice[3] = Controls.UpCutKB1;
    }
}

public enum Controls
{
    HorizontalJP1,
    VerticalJP1,
    PunchJP1,
    UpCutJP1,
    HorizontalJP2,
    VerticalJP2,
    PunchJP2,
    UpCutJP2,
    HorizontalKB1,
    VerticalKB1,
    PunchKB1,
    UpCutKB1,
    HorizontalKB2,
    VerticalKB2,
}