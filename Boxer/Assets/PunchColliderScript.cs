using UnityEngine;
using System.Collections;

public class PunchColliderScript : MonoBehaviour {

    private const int HEAD_PUNCH = 5;
    private const int BODY_PUNCH = 1;

    public void OnTriggerEnter(Collider col)
    {
        DealWithHit(col);
    }

    public void OnTriggerStay(Collider col)
    {
        DealWithHit(col);
    }

    private void DealWithHit(Collider col)
    {
        if (gameObject.transform.parent.gameObject.GetComponent<boxerScript>().Punching)
        {
            Vector3 punchDir = (col.transform.position - gameObject.transform.position).normalized;//new Vector3();
            //Debug.Log(string.Format("Type: {0}, Def: {1}, Atacking: {2}", col.name, col.transform.parent.gameObject.name, gameObject.transform.parent.gameObject.name));
            if (col.name == "HeadCollision")
            {
                boxerScript boxer = col.transform.parent.gameObject.GetComponent<boxerScript>();

                if (PhotonNetwork.offlineMode)
                    boxer.GetHit(HEAD_PUNCH, punchDir);
                else
                    boxer.GetComponent<PhotonView>().RPC("GetHit", PhotonTargets.All, HEAD_PUNCH, punchDir, col.transform.parent.gameObject.GetComponent<PhotonView>().owner.ID);
            }
            else if (col.name == "BodyCollision")
            {
                boxerScript boxer = col.transform.parent.gameObject.GetComponent<boxerScript>();

                if (PhotonNetwork.offlineMode)
                    boxer.GetHit(BODY_PUNCH, punchDir);
                else
                    boxer.GetComponent<PhotonView>().RPC("GetHit", PhotonTargets.All, BODY_PUNCH, punchDir, col.transform.parent.gameObject.GetComponent<PhotonView>().owner.ID);
            }
        }
    }
}
