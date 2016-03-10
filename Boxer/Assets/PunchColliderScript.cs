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
        boxerScript hittingBoxer = gameObject.transform.parent.gameObject.GetComponent<boxerScript>();
        if (hittingBoxer.Punching)
        {
            //new Vector3();
            int damageDealt = 0;

            //Debug.Log(string.Format("Type: {0}, Def: {1}, Atacking: {2}", col.name, col.transform.parent.gameObject.name, gameObject.transform.parent.gameObject.name));
            if (col.name == "HeadCollision")
                damageDealt = HEAD_PUNCH;
            else if (col.name == "BodyCollision")
                damageDealt = BODY_PUNCH;

            if (damageDealt > 0)
            {
                boxerScript boxerGettingHit = col.transform.parent.gameObject.GetComponent<boxerScript>();
                Vector3 punchDir = (col.transform.position - gameObject.transform.parent.position).normalized;

                if (PhotonNetwork.offlineMode)
                    boxerGettingHit.GetHit(damageDealt, punchDir);
                else
                    boxerGettingHit.GetComponent<PhotonView>().RPC("GetHit", PhotonTargets.All, damageDealt, punchDir, col.transform.parent.gameObject.GetComponent<PhotonView>().owner.ID);

                hittingBoxer.LandedPunch();
            }
        }
    }
}
